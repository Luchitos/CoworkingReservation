using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.DTOs
{
    public class PhotoResponseDTO
    {
        public string FileName { get; set; }
        public bool IsCoverPhoto { get; set; }
        public string ContentType { get; set; }
        public string FilePath { get; set; }
    }
}
