using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;

namespace CoworkingReservation.Infrastructure.Repositories
{
    public class BenefitRepository : Repository<Benefit>, IBenefitRepository
    {
        public BenefitRepository(ApplicationDbContext context) : base(context) { }
    }
}