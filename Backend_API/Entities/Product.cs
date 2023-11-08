using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class Product
{
    public int Id { get; set; }

    public int Status { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string Thumbnail { get; set; } = null!;

    public string? Description { get; set; }

    public string? Detail { get; set; }

    public decimal Price { get; set; }

    public decimal VatRate { get; set; }

    public decimal? DiscountAmount { get; set; }

    public int Quantity { get; set; }

    public int AuthorId { get; set; }

    public int PublisherId { get; set; }

    public int PublishYear { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Author Author { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual Publisher Publisher { get; set; } = null!;

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
