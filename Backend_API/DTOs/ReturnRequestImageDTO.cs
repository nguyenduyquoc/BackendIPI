using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class ReturnRequestImageDTO
    {
        public int Id { get; set; }

        public int RequestId { get; set; }

        public string Url { get; set; } = null!;
    }
}
