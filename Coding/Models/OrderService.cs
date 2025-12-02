using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class OrderService
{
    public int OrderId { get; set; }

    public int ServiceId { get; set; }

    public decimal? Price { get; set; }

    public string? Note { get; set; }

    public string? Image { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
