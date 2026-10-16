// ViewModels/FilterStateViewModels.cs
namespace GreenhouseMonitoring.ViewModels;

public class ActionsFilter
{
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
}

public class EventsFilter
{
    public string? Criticality { get; set; }
    public int Page { get; set; } = 1;
}

public class ReadingsFilter
{
    public int? SensorId { get; set; }
}

public class NormativeFilter
{
    public int? CropId { get; set; }
}