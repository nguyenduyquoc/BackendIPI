using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class OrderProductDTO
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public string ProductSlug { get; set; } = null!;

        public string ProductThumbnail { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal VatRate { get; set; }

        public int? ReturnQuantity { get; set; }

        public ReviewDTO? Review { get; set; }
    }
}
