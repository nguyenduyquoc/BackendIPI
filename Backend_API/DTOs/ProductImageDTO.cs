using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class ProductImageDTO
    {
        public int? Id { get; set; }

        public string? Url { get; set; }

        public int ProductId { get; set; }

        public string?  ProductName { get; set; }
    }
}
