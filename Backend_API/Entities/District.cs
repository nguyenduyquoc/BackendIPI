using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class District
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int ProvinceId { get; set; }

    public virtual ICollection<DeliveryService> DeliveryServices { get; set; } = new List<DeliveryService>();

    public virtual Province Province { get; set; } = null!;

    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
}
