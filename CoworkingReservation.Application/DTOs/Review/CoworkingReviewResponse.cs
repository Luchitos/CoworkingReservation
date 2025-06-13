namespace CoworkingReservation.Application.DTOs.Review
{
    public class CoworkingReviewResponse
    {
        public int CoworkingSpaceId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ReviewItemDto> Reviews { get; set; } = new();
    }

    public class ReviewItemDto
    {
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
