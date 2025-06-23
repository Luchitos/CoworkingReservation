namespace CoworkingReservation.Application.DTOs.Review
{
    public class ReviewValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int StatusCode { get; set; }

        public static ReviewValidationResult Success()
        {
            return new ReviewValidationResult { IsValid = true };
        }

        public static ReviewValidationResult Failure(string message, int statusCode = 400)
        {
            return new ReviewValidationResult 
            { 
                IsValid = false, 
                ErrorMessage = message, 
                StatusCode = statusCode 
            };
        }
    }
} 