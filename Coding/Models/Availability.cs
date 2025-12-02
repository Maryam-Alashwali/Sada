using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Availability
{
    public int AvailabilityId { get; set; }

    public int TailorId { get; set; }

    public string? DayOfWeek { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public bool IsAvailable { get; set; }

    public virtual Tailor Tailor { get; set; } = null!;
}
