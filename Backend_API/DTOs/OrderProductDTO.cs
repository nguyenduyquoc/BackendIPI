using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class OrderProductDTO
    {
        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public virtual OrderDTO Order { get; set; } = null!;

        public virtual ProductDTO Product { get; set; } = null!;
    }
}
