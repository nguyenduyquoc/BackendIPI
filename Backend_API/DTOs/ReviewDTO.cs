using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class ReviewDTO
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int OrderId { get; set; }

        public string? Fname { get; set; }

        public string? Lname { get; set; }

        public string? Avatar { get; set; }

        public string? Email { get; set; }

        public decimal Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
