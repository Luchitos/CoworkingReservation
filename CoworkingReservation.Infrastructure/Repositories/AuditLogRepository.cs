using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;

namespace CoworkingReservation.Infrastructure.Repositories
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context) { }

        public async Task LogAsync(AuditLog log)
        {
            await _dbSet.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}