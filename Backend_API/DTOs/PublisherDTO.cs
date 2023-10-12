using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class PublisherDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public List<ProductDTO>? Products { get; set; }
    }
}
