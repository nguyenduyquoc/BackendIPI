using System.ComponentModel.DataAnnotations;

namespace Backend_API.ViewModels
{
    public class OrderCreateModel
    {

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

        [Required(ErrorMessage = "Payment method is required.")]
        [RegularExpression("^(PAYPAL|VNPAY|COD)$", ErrorMessage = "Invalid payment method.")]
        public string PaymentMethod { get; set; } = null!;

        public int? UserId { get; set; }

        public virtual ICollection<OrderProductCreateModel> OrderProducts { get; set; } = new List<OrderProductCreateModel>();
    }

    public class OrderProductCreateModel
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }
    }
}
