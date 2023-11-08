using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class Review
{
    public int Id { get; set; }

    public int OrderProductId { get; set; }

    public decimal Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual OrderProduct OrderProduct { get; set; } = null!;
}
