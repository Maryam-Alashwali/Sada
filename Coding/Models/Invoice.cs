using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int OrderId { get; set; }

    public int ClientId { get; set; }

    public int? PaymentId { get; set; }

    public decimal? InvoiceTotalAmount { get; set; }

    public string? PaymentStatus { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual Payment? Payment { get; set; }
}
