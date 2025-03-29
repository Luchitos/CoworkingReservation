using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.Entities
{
    public class SpecialFeature
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        // Relationship with CoworkingSpaces
        public ICollection<CoworkingSpace> CoworkingSpaces { get; set; } = new List<CoworkingSpace>();
    }
}
