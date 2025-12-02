using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int? AdminId { get; set; }

    public int? TailorId { get; set; }

    public int? ClientId { get; set; }

    public string? Message { get; set; }

    public DateTime? Date { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual Client? Client { get; set; }

    public virtual Tailor? Tailor { get; set; }
}
