using Backend_API.Entities;

namespace Backend_API.DTOs
{
    public class OrderDTO
    {
        public int? Id { get; set; }

        public string Code { get; set; } = null!;

        public int Status { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Phone { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string District { get; set; } = null!;

        public string Province { get; set; } = null!;

        public string Country { get; set; } = null!;

        public decimal Subtotal { get; set; }

        public decimal DeliveryFee { get; set; }

        public string? CouponCode { get; set; }

        public decimal? CouponAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public string? Note { get; set; }

        public string PaymentMethod { get; set; } = null!;

        public string? CancelReason { get; set; }

        public int? UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<OrderProduct>? OrderProducts { get; set; }

        public virtual ReturnRequestDTO? ReturnRequest { get; set; }

        public List<ReviewDTO>? Reviews { get; set; }

        public virtual UserDTO? User { get; set; }
    }
}
