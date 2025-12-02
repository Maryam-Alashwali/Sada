using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public int ClientId { get; set; }

    public int? PaymentQuantity { get; set; }

    public decimal? PaymentAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentStatus { get; set; }

    public TimeOnly? PaymentTime { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? PaymentTransactionId { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Order Order { get; set; } = null!;
}
