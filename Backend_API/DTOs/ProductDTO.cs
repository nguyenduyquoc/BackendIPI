using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class ProductDTO
    {
        public int? Id { get; set; }

        public int Status { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public string Thumbnail { get; set; } = null!;

        public string? Description { get; set; }

        public string? Detail { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountAmount { get; set; }

        public int Quantity { get; set; }

        public int AuthorId { get; set; }

        public int PublisherId { get; set; }

        public int PublishYear { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public virtual AuthorDTO Author { get; set; } = null!;

        public List<OrderProductDTO>? OrderProducts { get; set; }

        public List<ProductImageDTO>? ProductImages { get; set; }

        public virtual Publisher Publisher { get; set; } = null!;

        public List<ReviewDTO>? Reviews { get; set; }

        public List<CategoryDTO>? Categories { get; set; }

        public List<TagDTO>? Tags { get; set; }

        public List<UserDTO>? Users { get; set; }
    }
}
