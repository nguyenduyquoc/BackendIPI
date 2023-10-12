using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class Admin
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
