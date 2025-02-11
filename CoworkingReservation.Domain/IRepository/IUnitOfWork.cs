using CoworkingReservation.Domain.Entities;
using System;
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
        IRepository<User> Users { get; }
        IRepository<CoworkingSpace> CoworkingSpaces { get; }
        IRepository<Reservation> Reservations { get; }
        IRepository<Review> Reviews { get; }
        IRepository<Address> Addresses { get; }
        //IRepository<CoworkingSpacePhoto> CoworkingSpacePhotos { get; }

        /// <summary>
        /// Guarda los cambios pendientes en la base de datos.
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
