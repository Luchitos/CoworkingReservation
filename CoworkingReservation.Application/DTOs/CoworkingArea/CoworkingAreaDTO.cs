public class CoworkingAreaDTO
{
    public int Id { get; set; }
    public int Type { get; set; } 
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerDay { get; set; }
    public bool Available { get; set; }
    public int CoworkingSpaceId { get; set; }
}
