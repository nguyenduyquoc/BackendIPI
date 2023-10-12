using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class ReturnRequest
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int Status { get; set; }

    public string ReturnReason { get; set; } = null!;

    public decimal RefundAmount { get; set; }

    public string Media1 { get; set; } = null!;

    public string? Media2 { get; set; }

    public string? Media3 { get; set; }

    public string? Media4 { get; set; }

    public string? Media5 { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
