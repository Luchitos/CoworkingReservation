using CoworkingReservation.Application.DTOs.Reservation;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoworkingReservation.Application.Services.Interfaces;

namespace CoworkingReservation.Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReservationRepository _reservationRepository;

        public ReservationService(IUnitOfWork unitOfWork, IReservationRepository reservationRepository)
        {
            _unitOfWork = unitOfWork;
            _reservationRepository = reservationRepository;
        }

        public async Task<ReservationResponseDTO> CreateReservationAsync(CreateReservationDTO dto, int userId)
        {
            // Validar que el usuario no sea el dueño del espacio
            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(dto.CoworkingSpaceId);
            if (coworkingSpace == null)
                throw new KeyNotFoundException("El espacio de coworking no existe.");

            if (coworkingSpace.HosterId == userId)
                throw new InvalidOperationException("No puedes reservar tu propio espacio de coworking.");

            // Validar fechas
            if (dto.StartDate < DateTime.Today)
                throw new InvalidOperationException("La fecha de inicio no puede ser en el pasado.");

            if (dto.EndDate <= dto.StartDate)
                throw new InvalidOperationException("La fecha de fin debe ser posterior a la fecha de inicio.");

            // Verificar disponibilidad
            var isAvailable = await CheckAvailabilityAsync(dto.CoworkingSpaceId, dto.StartDate, dto.EndDate, dto.AreaIds);
            if (!isAvailable)
                throw new InvalidOperationException("No hay disponibilidad para las fechas y áreas seleccionadas.");

            // Calcular precio total
            var totalPrice = await CalculateTotalPriceAsync(dto.CoworkingSpaceId, dto.AreaIds, dto.StartDate, dto.EndDate);

            // Crear la reserva
            var reservation = new Reservation
            {
                UserId = userId,
                CoworkingSpaceId = dto.CoworkingSpaceId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = ReservationStatus.Pending,
                TotalPrice = totalPrice,
                PaymentMethod = dto.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                ReservationDetails = new List<ReservationDetail>()
            };

            // Agregar detalles de la reserva
            foreach (var areaId in dto.AreaIds)
            {
                var area = await _unitOfWork.CoworkingAreas.GetByIdAsync(areaId);
                if (area == null)
                    throw new KeyNotFoundException($"El área con ID {areaId} no existe.");

                reservation.ReservationDetails.Add(new ReservationDetail
                {
                    CoworkingAreaId = areaId,
                    PricePerDay = area.PricePerDay
                });
            }

            await _unitOfWork.Reservations.AddAsync(reservation);
            await _unitOfWork.SaveChangesAsync();

            return await GetReservationByIdAsync(reservation.Id, userId);
        }

        public async Task<ReservationResponseDTO> GetReservationByIdAsync(int id, int userId)
        {
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(id);
            if (reservation == null)
                throw new KeyNotFoundException("La reserva no existe.");

            if (reservation.UserId != userId)
                throw new UnauthorizedAccessException("No tienes permiso para ver esta reserva.");

            return MapToResponseDTO(reservation);
        }

        public async Task<IEnumerable<ReservationResponseDTO>> GetUserReservationsAsync(int userId)
        {
            var reservations = await _reservationRepository.GetUserReservationsAsync(userId);
            return reservations.Select(MapToResponseDTO);
        }

        public async Task<ReservationResponseDTO> CancelReservationAsync(int id, int userId)
        {
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(id);
            if (reservation == null)
                throw new KeyNotFoundException("La reserva no existe.");

            if (reservation.UserId != userId)
                throw new UnauthorizedAccessException("No tienes permiso para cancelar esta reserva.");

            if (reservation.Status == ReservationStatus.Cancelled)
                throw new InvalidOperationException("La reserva ya está cancelada.");

            if (reservation.Status == ReservationStatus.Completed)
                throw new InvalidOperationException("No se puede cancelar una reserva completada.");

            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Reservations.UpdateAsync(reservation);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponseDTO(reservation);
        }

        public async Task<bool> CheckAvailabilityAsync(int coworkingSpaceId, DateTime startDate, DateTime endDate, List<int> areaIds)
        {
            return await _reservationRepository.CheckAvailabilityAsync(coworkingSpaceId, startDate, endDate, areaIds);
        }

        public async Task<decimal> CalculateTotalPriceAsync(int coworkingSpaceId, List<int> areaIds, DateTime startDate, DateTime endDate)
        {
            var areas = await _unitOfWork.CoworkingAreas.GetAllAsync(a => areaIds.Contains(a.Id));
            var totalDays = (endDate - startDate).Days + 1;
            return areas.Sum(a => a.PricePerDay * totalDays);
        }

        private ReservationResponseDTO MapToResponseDTO(Reservation reservation)
        {
            return new ReservationResponseDTO
            {
                Id = reservation.Id,
                CoworkingSpaceName = reservation.CoworkingSpace.Name,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                Status = reservation.Status,
                TotalPrice = reservation.TotalPrice,
                PaymentMethod = reservation.PaymentMethod,
                CreatedAt = reservation.CreatedAt,
                Details = reservation.ReservationDetails.Select(d => new ReservationDetailDTO
                {
                    Id = d.Id,
                    CoworkingAreaId = d.CoworkingAreaId,
                    AreaType = d.CoworkingArea.Type.ToString(),
                    PricePerDay = d.PricePerDay
                }).ToList()
            };
        }
    }
} 