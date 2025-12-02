using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Tailor
{
    public int TailorId { get; set; }

    public int UserId { get; set; }

    public string? TailorFirstName { get; set; }

    public string? TailorLastName { get; set; }

    public string? TailorPhone { get; set; }

    public string? TailorAddress { get; set; }

    public string? TailorProfilePicture { get; set; }

    public bool? IsApproved { get; set; }

    public virtual ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();

    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public virtual User User { get; set; } = null!;
}
