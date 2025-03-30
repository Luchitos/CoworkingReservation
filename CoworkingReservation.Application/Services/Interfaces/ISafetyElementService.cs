using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface ISafetyElementService
    {
        Task<IEnumerable<SafetyElement>> GetAllAsync();
        Task<SafetyElement?> GetByIdAsync(int id);
    }
}
