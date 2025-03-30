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
    public class SpecialFeatureService : ISpecialFeatureService
    {
        private readonly ISpecialFeatureRepository _repository;

        public SpecialFeatureService(ISpecialFeatureRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SpecialFeature>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<SpecialFeature?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}