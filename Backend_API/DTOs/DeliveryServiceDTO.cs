namespace Backend_API.DTOs
{
    public class DeliveryServiceDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public decimal? Fee { get; set; }

        public string? Type { get; set; }

        public string? EstimatedTime { get; set; }

        public int? EstimatedTimeValue { get; set; }
    }
}
