using System;
using System.Collections.Generic;

namespace GreenhouseMonitoring.Models;

public partial class Event
{
    public int EventId { get; set; }

    public int SensorId { get; set; }

    public long ReadingId { get; set; }

    public DateTime Timestamp { get; set; }

    public string Description { get; set; } = null!;

    public string Criticality { get; set; } = null!;

    public virtual ICollection<Action> Actions { get; set; } = new List<Action>();

    public virtual SensorReading? Reading { get; set; } = null!;

    public virtual Sensor? Sensor { get; set; } = null!;
}
