using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int PaymentMethodId { get; set; }
        
        public int? ReservationId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "ARS";
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } // pending, completed, failed, refunded
        
        [MaxLength(200)]
        public string Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        [MaxLength(100)]
        public string? ExternalTransactionId { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        // Relaciones
        public User User { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public Reservation? Reservation { get; set; }
    }
} 