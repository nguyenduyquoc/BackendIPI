using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class Review
{
    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public decimal Rating { get; set; }

    public string? Comment { get; set; }

    public bool? Editable { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
