using System;
using System.Collections.Generic;

namespace GreenhouseMonitoring.Models;

public partial class Action
{
    public int ActionId { get; set; }

    public int? EventId { get; set; }

    public string Type { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public string Description { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual Event? Event { get; set; }
}
