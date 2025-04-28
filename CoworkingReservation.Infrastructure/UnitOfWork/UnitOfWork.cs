using System.Data;
using System.Security.AccessControl;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using CoworkingReservation.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace CoworkingReservation.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Implementación del patrón Unit of Work para manejar transacciones y repositorios.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context, IUserRepository userRepository, ICoworkingSpaceRepository coworkingSpaceRepository, IAuditLogRepository auditLogRepository, IAddressRepository addressRepository,
            ICoworkingAreaRepository coworkingAreaRepository,
            ICoworkingAvailabilityRepository coworkingAvailabilityRepository)
        {
            _context = context;
            Users = userRepository;
            Reservations = new Repository<Reservation>(_context);
            Reviews = new Repository<Review>(_context);
            CoworkingSpacePhotos = new Repository<CoworkingSpacePhoto>(_context);
            UserPhotos = new Repository<UserPhoto>(_context);
            Addresses = addressRepository; 
            AuditLogs = auditLogRepository;
            CoworkingSpaces = coworkingSpaceRepository;
            Services = new ServiceOfferedRepository(context);
            Benefits = new BenefitRepository(context);
            SafetyElements = new SafetyElementRepository(context);
            SpecialFeatures = new SpecialFeatureRepository(context);
            FavoriteCoworkingSpaces = new FavoriteCoworkingSpaceRepository(_context);
            CoworkingAreas = coworkingAreaRepository;
            CoworkingAvailabilities = coworkingAvailabilityRepository;

        }
        public IRepository<UserPhoto> UserPhotos { get; private set; }

        public IUserRepository Users { get; private set; }
        public ICoworkingSpaceRepository CoworkingSpaces { get; private set; }
        public IRepository<Reservation> Reservations { get; private set; }
        public IRepository<Review> Reviews { get; private set; }
        public IAddressRepository Addresses { get; private set; }
        public IRepository<CoworkingSpacePhoto> CoworkingSpacePhotos { get; private set; }
        public IAuditLogRepository AuditLogs { get; private set; }
        public IRepository<ServiceOffered> Services { get; private set; }
        public IRepository<Benefit> Benefits { get; private set; }
        public IRepository<SpecialFeature> SpecialFeatures { get; private set; }
        public IRepository<SafetyElement> SafetyElements { get; private set; }
        public ICoworkingAreaRepository CoworkingAreas { get; private set; }
        public ICoworkingAvailabilityRepository CoworkingAvailabilities { get; private set; }
        public IFavoriteCoworkingSpaceRepository FavoriteCoworkingSpaces { get; private set; }


        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            return await _context.Database.BeginTransactionAsync(isolationLevel);
        }

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