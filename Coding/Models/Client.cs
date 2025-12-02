using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Client
{
    public int ClientId { get; set; }

    public int UserId { get; set; }

    public string? ClientFirstName { get; set; }

    public string? ClientLastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? ClientAddress { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User User { get; set; } = null!;
}
