using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public int CategoryId { get; set; }

    public int TailorId { get; set; }

    public string? ServiceName { get; set; }

    public string? ServiceDescription { get; set; }

    public decimal? BasePrice { get; set; }

    public int? Duration { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderService> OrderServices { get; set; } = new List<OrderService>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Tailor Tailor { get; set; } = null!;
}
