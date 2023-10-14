using System;
using System.Collections.Generic;

namespace Backend_API.Entities;

public partial class ReturnRequestImage
{
    public int Id { get; set; }

    public int RequestId { get; set; }

    public string Url { get; set; } = null!;

    public virtual ReturnRequest Request { get; set; } = null!;
}
