using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class Order
{
    public int Id { get; set; }

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

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual ReturnRequest? ReturnRequest { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User? User { get; set; }
}
