// GreenhouseMonitoring/ViewModels/HomeViewModel.cs
using GreenhouseMonitoring.Models;
using System.Collections.Generic;

namespace GreenhouseMonitoring.ViewModels;

public class HomeViewModel
{
    public int GreenhouseCount { get; set; }
    public int SensorCount { get; set; }
    public int CriticalEvents { get; set; }
    public int PlannedActions { get; set; }

    public List<Event> LatestEvents { get; set; } = new();
    public List<Crop> TopCrops { get; set; } = new();
}