using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class UserAddress
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Address { get; set; }

    public int DistrictId { get; set; }

    public virtual District District { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
