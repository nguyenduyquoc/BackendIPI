using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class AuthorDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public string? Avatar { get; set; }

        public List<ProductDTO>? Products { get; set; }
    }
}
