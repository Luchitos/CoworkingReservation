namespace CoworkingReservation.Application.DTOs.PaymentMethod
{
    public class PaymentMethodDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string? CardNumber { get; set; }
        public string? Last4 { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? Cvv { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? WalletType { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreatePaymentMethodDTO
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string? CardNumber { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? Cvv { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? WalletType { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdatePaymentMethodDTO
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string? CardNumber { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? Cvv { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? WalletType { get; set; }
        public bool IsDefault { get; set; }
    }
} 