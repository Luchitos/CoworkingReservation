using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Domain.IRepository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> ExistsByEmailOrCuit(string email, string cuit);
        Task<User?> GetByIdentifierWithPhotoAsync(string identifier);
    }

}
