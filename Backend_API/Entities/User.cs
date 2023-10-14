using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Fname { get; set; } = null!;

    public string Lname { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Avatar { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? VerificationCode { get; set; }

    public bool Subscribe { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
