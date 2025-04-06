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
        private readonly IUnitOfWork _unitOfWork;

        public SpecialFeatureService(ISpecialFeatureRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SpecialFeature>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<SpecialFeature?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
        
        public async Task<SpecialFeature> CreateAsync(SpecialFeature specialFeature)
        {
            await _unitOfWork.SpecialFeatures.AddAsync(specialFeature);
            await _unitOfWork.SaveChangesAsync();
            return specialFeature;
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.SpecialFeatures.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}