using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class Author
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Avatar { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
