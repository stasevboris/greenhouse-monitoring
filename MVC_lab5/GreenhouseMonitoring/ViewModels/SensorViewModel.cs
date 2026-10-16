namespace GreenhouseMonitoring.ViewModels;

public class SensorViewModel
{
    public int SensorId { get; set; }
    public string Type { get; set; } = null!;
    public string? Model { get; set; }
    public string? LocationInGh { get; set; }
    public DateTime? CreatedAt { get; set; }

    public string GreenhouseName { get; set; } = "—";
    public string? CropName { get; set; }

    // ← Должно быть с set, иначе нельзя присваивать!
    public string GreenhouseDisplay { get; set; } = "—";

    public int ReadingsCount { get; set; }
}