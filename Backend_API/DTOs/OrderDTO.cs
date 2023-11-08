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

        public decimal Vat { get; set; }

        public decimal DeliveryFee { get; set; }

        public string DeliveryService { get; set; } = null!;

        public DateTime? DeliveryEstimate { get; set; }

        public string? CouponCode { get; set; }

        public decimal? CouponAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public string? Note { get; set; }

        public string PaymentMethod { get; set; } = null!;

        public string? CancelReason { get; set; }

        public int? UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<OrderProductDTO> OrderProducts { get; set; } = new List<OrderProductDTO>();

        public int? ReturnRequestId { get; set; }

        public int? ReturnRequestStatus { get; set; }

        public string? paymentUrl { get; set; }

    }
}
