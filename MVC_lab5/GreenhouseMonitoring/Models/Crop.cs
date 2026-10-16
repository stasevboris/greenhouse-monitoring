using System;
using System.Collections.Generic;

namespace GreenhouseMonitoring.Models;

public partial class Crop
{
    public int CropId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Greenhouse> Greenhouses { get; set; } = new List<Greenhouse>();

    public virtual ICollection<NormativeParameter> NormativeParameters { get; set; } = new List<NormativeParameter>();
}
