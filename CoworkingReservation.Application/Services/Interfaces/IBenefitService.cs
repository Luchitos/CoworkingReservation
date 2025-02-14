using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface IBenefitService
    {
        Task<IEnumerable<Benefit>> GetAllAsync();
        Task<Benefit> GetByIdAsync(int id);
        Task<Benefit> CreateAsync(Benefit benefit);
        Task DeleteAsync(int id);
    }
}
