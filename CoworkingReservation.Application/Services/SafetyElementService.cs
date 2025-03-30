using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;

namespace CoworkingReservation.Application.Services
{
    public class SafetyElementService : ISafetyElementService
    {
        private readonly ISafetyElementRepository _repository;

        public SafetyElementService(ISafetyElementRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SafetyElement>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<SafetyElement?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}
