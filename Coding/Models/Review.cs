using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int ClientId { get; set; }

    public int ServiceId { get; set; }

    public int OrderId { get; set; }

    public int AdminId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? ReviewCreatedAt { get; set; }

    public virtual Admin Admin { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
