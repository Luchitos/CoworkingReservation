namespace CoworkingReservation.Application.DTOs.Transaction
{
    public class TransactionDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PaymentMethodId { get; set; }
        public int? ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ExternalTransactionId { get; set; }
        public string? Notes { get; set; }
        
        // Información relacionada
        public string PaymentMethodName { get; set; }
        public string? ReservationDescription { get; set; }
    }

    public class CreateTransactionDTO
    {
        public int UserId { get; set; }
        public int PaymentMethodId { get; set; }
        public int? ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "ARS";
        public string Description { get; set; }
        public string? ExternalTransactionId { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateTransactionStatusDTO
    {
        public string Status { get; set; }
        public string? ExternalTransactionId { get; set; }
        public string? Notes { get; set; }
    }
} 