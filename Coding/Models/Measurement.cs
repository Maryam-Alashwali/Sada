using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Measurement
{
    public int MeasurementId { get; set; }

    public int ClientId { get; set; }

    public decimal? Chest { get; set; }

    public decimal? Waist { get; set; }

    public decimal? Hips { get; set; }

    public decimal? Length { get; set; }

    public decimal? SleeveLength { get; set; }

    public string? OtherNotes { get; set; }

    public virtual Client Client { get; set; } = null!;
}
