using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }

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

        public string? AuthorName { get; set; }
        public string? AuthorAvatar { get; set; }

        public int PublisherId { get; set; }

        public string? PublisherName { get; set; }

        public int PublishYear { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<ProductImageDTO> ProductImages { get; set; } = new List<ProductImageDTO>();

        public virtual ICollection<ReviewDTO> Reviews { get; set; } = new List<ReviewDTO>();

        public virtual ICollection<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();

        public virtual ICollection<TagDTO> Tags { get; set; } = new List<TagDTO>();

        public decimal Rating { get; set; }

        public int SoldQuantity { get; set; }

    }
}
