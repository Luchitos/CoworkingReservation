using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; } // Identificador único
        public string Action { get; set; } // Acción realizada (ej. "ToggleActiveStatus", "UpdateCoworkingSpace")
        public int? UserId { get; set; } // Usuario que realizó la acción (puede ser null si es un proceso automático)
        public string UserRole { get; set; } // Rol del usuario (ej. "Admin", "Hoster", "Client")
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Fecha y hora del evento
        public bool Success { get; set; } // Indica si la acción fue exitosa
        public string Description { get; set; } // Mensaje de error o descripción de la acción
    }
}
