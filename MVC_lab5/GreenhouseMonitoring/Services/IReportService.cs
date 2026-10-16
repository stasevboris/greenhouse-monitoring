using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreenhouseMonitoring.Services;

public interface IReportService
{
    Task<Dictionary<string, double>> GetEfficiencyReportAsync(int greenhouseId);
}