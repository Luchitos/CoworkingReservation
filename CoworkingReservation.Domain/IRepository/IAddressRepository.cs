using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Domain.IRepository
{
    public interface IAddressRepository : IRepository<Address>
    {
        Task<bool> ExistsAsync(Expression<Func<Address, bool>> predicate);
    }

}
