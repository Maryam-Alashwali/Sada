using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public int AdminId { get; set; }

    public string Category1 { get; set; } = null!;

    public virtual Admin Admin { get; set; } = null!;

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
