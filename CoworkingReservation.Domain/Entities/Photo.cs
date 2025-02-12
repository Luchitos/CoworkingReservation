using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.Entities
{
    public class Photo
    {
        [Key]
        public int Id { get; set; } // Identificador único de la foto

        [Required]
        public string FileName { get; set; } // Nombre del archivo

        [Required]
        public byte[] FotoData { get; set; } // 📌 Debe ser `byte[]`, no `string` 

        public bool IsCoverPhoto { get; set; } // Indica si es la foto de portada

        [Required]
        public string ContentType { get; set; } // Tipo MIME (ej: "image/jpeg")

        // Relación con `CoworkingSpace`
        [ForeignKey("CoworkingSpace")]
        public int CoworkingSpaceId { get; set; }
        public virtual CoworkingSpace CoworkingSpace { get; set; }
    }
}
