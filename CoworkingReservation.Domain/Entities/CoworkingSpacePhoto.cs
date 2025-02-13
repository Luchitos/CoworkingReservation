using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.Entities
{
    public class CoworkingSpacePhoto
    {
        public int Id { get; set; } // Clave primaria
        public string FilePath { get; set; } // Ruta del archivo de la foto
        public string FileName { get; set; } // Nombre del archivo
        public string MimeType { get; set; } // Tipo MIME (e.g., image/jpeg)
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow; // Fecha de subida
        public bool IsCoverPhoto { get; set; } = false;

        // Relación con `CoworkingSpace`
        [ForeignKey("CoworkingSpace")]
        public int CoworkingSpaceId { get; set; }
        public virtual CoworkingSpace CoworkingSpace { get; set; }
    }
}