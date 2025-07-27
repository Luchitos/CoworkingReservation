using System;
using System.Collections.Generic;

namespace CoworkingReservation.Domain.Entities
{
    public class User
    {
        public int Id { get; set; } // Identificador único del usuario
        public string Name { get; set; } // Nombre
        public string Lastname { get; set; } // Apellido
        public string UserName { get; set; } // Nombre de usuario
        public string Cuit { get; set; } // CUIT o documento identificador (opcional)
        public string Email { get; set; } // Correo electrónico
        public string PasswordHash { get; set; } // Contraseña en formato hash
        public DateTime CreationDate { get; set; } = DateTime.UtcNow; // Fecha de creación
        public bool IsActive { get; set; } = true; // Usuario activo/deshabilitado
        public string Role { get; set; } = "Client"; // Rol inicial ("Client", "Hoster", "Admin")
        public bool IsHosterRequestPending { get; set; } = false; // Indica si el usuario solicitó ser hoster
        
        // Nuevos campos para información personal
        public string? Phone { get; set; } // Teléfono del usuario (opcional)
        public string? Address { get; set; } // Dirección del usuario (opcional)

        // Relación con CoworkingSpaces si es Hoster
        public ICollection<CoworkingSpace> CoworkingSpaces { get; set; } = new List<CoworkingSpace>();

        // Relación con Favoritos
        public ICollection<FavoriteCoworkingSpace> FavoriteCoworkingSpaces { get; set; } = new List<FavoriteCoworkingSpace>();

        // Relación con Reservas
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();

        // Relación con Reseñas
        public List<Review> Reviews { get; set; } = new List<Review>();

        // Relación con Métodos de Pago
        public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();

        // Relación con Transacciones
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        // Foto de perfil (archivo externo)
        public int? PhotoId { get; set; } // Clave foránea (nullable)
        public UserPhoto Photo { get; set; }
    }
}
