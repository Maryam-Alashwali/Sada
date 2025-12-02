using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Advertisement
{
    public int AdsId { get; set; }

    public int AdminId { get; set; }

    public int? TailorId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? Image { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual Admin Admin { get; set; } = null!;

    public virtual Tailor? Tailor { get; set; }
}
