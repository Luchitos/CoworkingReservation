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

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new Repository<User>(_context);
            CoworkingSpaces = new Repository<CoworkingSpace>(_context);
            Reservations = new Repository<Reservation>(_context);
            Reviews = new Repository<Review>(_context);
            Addresses = new Repository<Address>(_context);
            Photos = new Repository<Photo>(_context);
        }

        public IRepository<User> Users { get; private set; }
        public IRepository<CoworkingSpace> CoworkingSpaces { get; private set; }
        public IRepository<Reservation> Reservations { get; private set; }
        public IRepository<Review> Reviews { get; private set; }
        public IRepository<Address> Addresses { get; private set; }
        public IRepository<Photo> Photos { get; private set; }

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