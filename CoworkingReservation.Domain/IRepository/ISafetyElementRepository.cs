using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Domain.IRepository
{
    public interface ISafetyElementRepository : IRepository<SafetyElement>
    {
        Task<IEnumerable<SafetyElement>> GetAllAsync();
    }
}
