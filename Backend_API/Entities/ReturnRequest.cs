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

    public string? Response { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<ReturnRequestImage> ReturnRequestImages { get; set; } = new List<ReturnRequestImage>();
}
