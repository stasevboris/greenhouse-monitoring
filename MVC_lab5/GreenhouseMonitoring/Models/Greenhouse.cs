using System.Collections.Generic;

namespace GreenhouseMonitoring.Models;

public partial class Greenhouse
{
    public int GreenhouseId { get; set; }

    public string NameNumber { get; set; } = null!;

    public string Location { get; set; } = null!;

    public int CropId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Crop? Crop { get; set; } // Навигационное свойство
    public virtual ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
}

