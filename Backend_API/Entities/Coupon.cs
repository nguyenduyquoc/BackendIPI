using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class Coupon
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal Discount { get; set; }

    public decimal? MaxReduction { get; set; }

    public int Quantity { get; set; }

    public decimal MinimumRequire { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
