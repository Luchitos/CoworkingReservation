﻿using CoworkingReservation.Domain.Entities;
using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.IRepository
{
    /// <summary>
    /// Define los métodos y repositorios gestionados por el patrón Unit of Work.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ICoworkingSpaceRepository CoworkingSpaces { get; }
        IRepository<Reservation> Reservations { get; }
        IRepository<Review> Reviews { get; }
        IAddressRepository Addresses { get; }
        IRepository<CoworkingSpacePhoto> CoworkingSpacePhotos { get; }
        IAuditLogRepository AuditLogs { get; }
        IRepository<UserPhoto> UserPhotos { get; }
        IRepository<ServiceOffered> Services { get; }
        IRepository<Benefit> Benefits { get; }
        IRepository<SafetyElement> SafetyElements { get; }
        IRepository<SpecialFeature> SpecialFeatures { get; }
        ICoworkingAreaRepository CoworkingAreas { get; }
        ICoworkingAvailabilityRepository CoworkingAvailabilities { get; }
        IFavoriteCoworkingSpaceRepository FavoriteCoworkingSpaces { get; }

        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel);


        /// <summary>
        /// Guarda los cambios pendientes en la base de datos.
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
