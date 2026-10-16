using System.ComponentModel.DataAnnotations.Schema;

namespace GreenhouseMonitoring.Models;

public partial class SensorReading
{
    [Column("ReadingID")]
    public long ReadingId { get; set; }

    [Column("SensorID")]
    public int SensorId { get; set; }

    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    public virtual Sensor? Sensor { get; set; } = null!;
}