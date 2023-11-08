using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class DeliveryService
{
    public int Id { get; set; }

    public int DistrictId { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Fee { get; set; }

    public string? Type { get; set; }

    public string? EstimatedTime { get; set; }

    public int? EstimatedTimeValue { get; set; }

    public virtual District District { get; set; } = null!;
}
