using System.ComponentModel.DataAnnotations;

namespace CoworkingReservation.Domain.Entities
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } // credit_card, debit_card, bank_transfer, digital_wallet
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(16)]
        public string? CardNumber { get; set; }
        
        [MaxLength(4)]
        public string? Last4 { get; set; }
        
        [MaxLength(2)]
        public string? ExpiryMonth { get; set; }
        
        [MaxLength(4)]
        public string? ExpiryYear { get; set; }
        
        [MaxLength(4)]
        public string? Cvv { get; set; }
        
        [MaxLength(100)]
        public string? BankName { get; set; }
        
        [MaxLength(50)]
        public string? AccountNumber { get; set; }
        
        [MaxLength(50)]
        public string? WalletType { get; set; } // mercadopago, paypal, other
        
        public bool IsDefault { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Relación con User
        public User User { get; set; }
        
        // Relación con Transactions
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
} 