using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;

namespace CoworkingReservation.Application.Services
{
    public class SafetyElementService : ISafetyElementService
    {
        private readonly ISafetyElementRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public SafetyElementService(ISafetyElementRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SafetyElement>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<SafetyElement?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
        
        public async Task<SafetyElement> CreateAsync(SafetyElement safetyElement)
        {
            await _unitOfWork.SafetyElements.AddAsync(safetyElement);
            await _unitOfWork.SaveChangesAsync();
            return safetyElement;
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.SafetyElements.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
