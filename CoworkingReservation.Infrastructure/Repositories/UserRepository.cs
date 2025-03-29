using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoworkingReservation.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<bool> ExistsByEmailOrCuit(string email, string cuit)
        {
            return await _context.Users.AnyAsync(u => u.Email == email || u.Cuit == cuit);
        }

        public async Task<User?> GetByIdentifierWithPhotoAsync(string identifier)
        {
            return await _context.Users
                .Include(u => u.Photo)
                .FirstOrDefaultAsync(u => u.Email == identifier || u.UserName == identifier);
        }
    }
}
