using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface ISafetyElementService
    {
        Task<IEnumerable<SafetyElement>> GetAllAsync();
        Task<SafetyElement?> GetByIdAsync(int id);
        Task<SafetyElement> CreateAsync(SafetyElement safetyElement);
        Task DeleteAsync(int id);
    }
}
