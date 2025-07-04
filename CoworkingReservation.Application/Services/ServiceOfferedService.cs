using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;

namespace CoworkingReservation.Application.Services
{
    public class ServiceOfferedService : IServiceOfferedService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceOfferedService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ServiceOffered>> GetAllAsync()
        {
            return await _unitOfWork.Services.GetAllAsync();
        }

        public async Task<ServiceOffered> GetByIdAsync(int id)
        {
            return await _unitOfWork.Services.GetByIdAsync(id);
        }

        public async Task<ServiceOffered> CreateAsync(ServiceOffered service)
        {
            await _unitOfWork.Services.AddAsync(service);
            await _unitOfWork.SaveChangesAsync();
            return service;
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.Services.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}