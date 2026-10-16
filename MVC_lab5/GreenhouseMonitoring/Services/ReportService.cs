using GreenhouseMonitoring.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreenhouseMonitoring.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, double>> GetEfficiencyReportAsync(int greenhouseId)
    {
        var greenhouse = await _context.Greenhouses
            .Include(g => g.Crop)
                .ThenInclude(c => c.NormativeParameters)
            .FirstOrDefaultAsync(g => g.GreenhouseId == greenhouseId);

        if (greenhouse?.Crop == null)
            return new Dictionary<string, double>();

        var sensors = await _context.Sensors
            .Where(s => s.GreenhouseId == greenhouseId)
            .ToListAsync();

        var report = new Dictionary<string, double>();
        foreach (var sensor in sensors)
        {
            var paramName = sensor.Type.ToLower();
            var norm = greenhouse.Crop.NormativeParameters
                .FirstOrDefault(n => n.Parameter.ToLower() == paramName);

            if (norm == null) continue;

            var readings = await _context.SensorReadings
                .Where(sr => sr.SensorId == sensor.SensorId)
                .ToListAsync();

            if (!readings.Any()) continue;

            var inNormCount = readings.Count(r => r.Value >= norm.MinValue && r.Value <= norm.MaxValue);
            var efficiency = (double)inNormCount / readings.Count * 100;
            report[sensor.Type] = System.Math.Round(efficiency, 1);
        }

        return report;
    }
}