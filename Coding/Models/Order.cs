using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int ClientId { get; set; }

    public int TailorId { get; set; }

    public string? OrderStatus { get; set; }

    public string? OrderAddress { get; set; }

    public string? ClientNotes { get; set; }

    public string? ClientUploadedImage { get; set; }

    public decimal? TotalPrice { get; set; }

    public decimal? PlatformCommission { get; set; }

    public decimal? TailorPayout { get; set; }

    public DateTime? ScheduledPick { get; set; }

    public DateTime? ScheduledVisitDate { get; set; }

    public DateTime? CompletionDate { get; set; }

    public DateTime? DateCreated { get; set; }

    public string? ServiceType { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<OrderService> OrderServices { get; set; } = new List<OrderService>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Tailor Tailor { get; set; } = null!;
}
