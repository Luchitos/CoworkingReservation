using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; } // Identificador único
        public int UserId { get; set; } // Relación con usuario
        public User User { get; set; }
        public int CoworkingSpaceId { get; set; } // Relación con espacio
        public CoworkingSpace CoworkingSpace { get; set; }
        public int Rating { get; set; } // Calificación (1-5 estrellas)
        public string Comment { get; set; } // Comentario
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Fecha de creación
    }
}
