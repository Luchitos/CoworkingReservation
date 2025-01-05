using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(int userId, string email, string role);
    }
}
