using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface IServiceOfferedService
    {
        Task<IEnumerable<ServiceOffered>> GetAllAsync();
        Task<ServiceOffered> GetByIdAsync(int id);
        Task<ServiceOffered> CreateAsync(ServiceOffered service);
        Task DeleteAsync(int id);
    }
}