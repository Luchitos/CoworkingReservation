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
    public class BenefitService : IBenefitService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BenefitService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Benefit>> GetAllAsync()
        {
            return await _unitOfWork.Benefits.GetAllAsync();
        }

        public async Task<Benefit> GetByIdAsync(int id)
        {
            return await _unitOfWork.Benefits.GetByIdAsync(id);
        }

        public async Task<Benefit> CreateAsync(Benefit benefit)
        {
            await _unitOfWork.Benefits.AddAsync(benefit);
            await _unitOfWork.SaveChangesAsync();
            return benefit;
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.Benefits.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}