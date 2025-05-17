using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoworkingReservation.Infrastructure.Repositories
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Reservation>> GetUserReservationsAsync(int userId)
        {
            // Obtener todas las reservas del usuario con los detalles necesarios
            var reservations = await _context.Reservations
                .Include(r => r.CoworkingSpace)
                .Include(r => r.User)
                .Include(r => r.ReservationDetails)
                    .ThenInclude(rd => rd.CoworkingArea)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Actualizar automáticamente el estado de las reservas completadas
            var now = DateTime.UtcNow;
            var reservationsToUpdate = new List<Reservation>();

            foreach (var reservation in reservations)
            {
                // Si una reserva confirmada ya pasó su fecha de fin, marcarla como completada
                if (reservation.Status == Domain.Enums.ReservationStatus.Confirmed && 
                    reservation.EndDate < now)
                {
                    reservation.Status = Domain.Enums.ReservationStatus.Completed;
                    reservation.UpdatedAt = now;
                    reservationsToUpdate.Add(reservation);
                }
            }

            // Guardar los cambios si hay reservas que actualizar
            if (reservationsToUpdate.Any())
            {
                await _context.SaveChangesAsync();
            }

            return reservations;
        }

        public async Task<bool> CheckAvailabilityAsync(int coworkingSpaceId, DateTime startDate, DateTime endDate, List<int> areaIds)
        {
            // Normalizar las fechas para ignorar la hora
            var normalizedStartDate = startDate.Date;
            var normalizedEndDate = endDate.Date;
            
            // Log para depuración
            Console.WriteLine($"Verificando disponibilidad para espacio {coworkingSpaceId} del {normalizedStartDate.ToString("yyyy-MM-dd")} al {normalizedEndDate.ToString("yyyy-MM-dd")}");
            
            // Obtener todas las reservas que se solapan con las fechas solicitadas
            // Nota: Se comparan solo las fechas, no las horas
            var existingReservations = await _context.Reservations
                .Include(r => r.ReservationDetails)
                .Where(r => r.CoworkingSpaceId == coworkingSpaceId &&
                           r.Status != Domain.Enums.ReservationStatus.Cancelled &&
                           (r.StartDate.Date <= normalizedEndDate && r.EndDate.Date >= normalizedStartDate))
                .ToListAsync();
            
            // Log para depuración
            Console.WriteLine($"Se encontraron {existingReservations.Count} reservas existentes que solapan las fechas");
            
            // Si no hay reservas existentes, todas las áreas están disponibles
            if (!existingReservations.Any())
            {
                return true;
            }
            
            // Verificar cada área solicitada
            foreach (var areaId in areaIds)
            {
                // Si alguna de las reservas existentes incluye este área, no está disponible
                bool isAreaBooked = existingReservations
                    .Any(r => r.ReservationDetails
                        .Any(rd => rd.CoworkingAreaId == areaId));
                
                if (isAreaBooked)
                {
                    Console.WriteLine($"El área {areaId} no está disponible para las fechas seleccionadas");
                    return false; // Al menos un área no está disponible
                }
            }
            
            // Todas las áreas están disponibles
            Console.WriteLine("Todas las áreas solicitadas están disponibles");
            return true;
        }

        public async Task<Reservation> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.CoworkingSpace)
                .Include(r => r.User)
                .Include(r => r.ReservationDetails)
                    .ThenInclude(rd => rd.CoworkingArea)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}

