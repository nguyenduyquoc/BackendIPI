using Backend_API.DTOs;

namespace Backend_API.ViewModels
{
    public class ProductCreateModel
    {
        public string Name { get; set; } = null!;

        public int Status { get; set; }

        public string Thumbnail { get; set; } = null!;

        public string? Description { get; set; }

        public string? Detail { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountAmount { get; set; }

        public int Quantity { get; set; }

        public int AuthorId { get; set; }

        public int PublisherId { get; set; }

        public int PublishYear { get; set; }

    }
}
