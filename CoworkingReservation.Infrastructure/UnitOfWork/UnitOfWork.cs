using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using CoworkingReservation.Infrastructure.Repositories;


namespace CoworkingReservation.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Implementación del patrón Unit of Work para manejar transacciones y repositorios.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context, IUserRepository userRepository)
        {
            _context = context;
            Users = userRepository;
            CoworkingSpaces = new Repository<CoworkingSpace>(_context);
            Reservations = new Repository<Reservation>(_context);
            Reviews = new Repository<Review>(_context);
            Addresses = new Repository<Address>(_context);
            CoworkingSpacePhotos = new Repository<CoworkingSpacePhoto>(_context);
            UserPhotos = new Repository<UserPhoto>(_context); ;

        }
        public IRepository<UserPhoto> UserPhotos { get; private set; }

        public IUserRepository Users { get; private set; }
        public IRepository<CoworkingSpace> CoworkingSpaces { get; private set; }
        public IRepository<Reservation> Reservations { get; private set; }
        public IRepository<Review> Reviews { get; private set; }
        public IRepository<Address> Addresses { get; private set; }
        public IRepository<CoworkingSpacePhoto> CoworkingSpacePhotos { get; private set; }

        /// <summary>
        /// Guarda los cambios pendientes en la base de datos.
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Libera los recursos del contexto.
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}