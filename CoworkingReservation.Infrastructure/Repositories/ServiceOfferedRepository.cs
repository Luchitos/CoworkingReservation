using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;

namespace CoworkingReservation.Infrastructure.Repositories
{
    public class ServiceOfferedRepository : Repository<ServiceOffered>, IServiceOfferedRepository
    {
        public ServiceOfferedRepository(ApplicationDbContext context) : base(context) { }
    }
}
