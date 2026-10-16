using System.ComponentModel.DataAnnotations.Schema;

namespace GreenhouseMonitoring.Models;

public partial class Sensor
{
    [Column("SensorID")]
    public int SensorId { get; set; }

    public string Type { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string LocationInGh { get; set; } = null!;

    [Column("GreenhouseID")]
    public int GreenhouseId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    public virtual Greenhouse? Greenhouse { get; set; } = null!;
    public virtual ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
}