using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;

namespace CoworkingReservation.Infrastructure.Repositories
{
    public class FavoriteCoworkingSpaceRepository : Repository<FavoriteCoworkingSpace>, IFavoriteCoworkingSpaceRepository
    {
        public FavoriteCoworkingSpaceRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
