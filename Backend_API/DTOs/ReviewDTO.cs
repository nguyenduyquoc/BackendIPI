using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class ReviewDTO
    {
        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public decimal Rating { get; set; }

        public string? Comment { get; set; }

        public bool? Editable { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual OrderDTO Order { get; set; } = null!;

        public virtual ProductDTO Product { get; set; } = null!;
    }
}
