using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.Entities
{
    public class UserPhoto
    {
        public int Id { get; set; } // Clave primaria
        public string FilePath { get; set; } // Ruta del archivo de la foto
        public string FileName { get; set; } // Nombre del archivo
        public string MimeType { get; set; } // Tipo MIME (e.g., image/jpeg)
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow; // Fecha de subida

        // Relación con User
        public int UserId { get; set; }
        public User User { get; set; } // Relación inversa
    }
}
