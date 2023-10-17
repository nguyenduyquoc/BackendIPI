using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class CategoryDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public int? ParentId { get; set; }

        public string? ParentName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public List<CategoryDTO>? InverseParent { get; set; }

        public List<ProductDTO>? Products { get; set; }
    }
}
