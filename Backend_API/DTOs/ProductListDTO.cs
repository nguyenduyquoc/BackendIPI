namespace Backend_API.DTOs
{
    public class ProductListDTO
    {
        public List<ProductDTO>? Products { get; set; }

        public int? TotalPages { get; set; }

        public int TotalItems { get; set; }
    }
}
