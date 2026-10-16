// GreenhouseMonitoring/Services/HomeService.cs
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Models;
using GreenhouseMonitoring.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GreenhouseMonitoring.Services;

public class HomeService(ApplicationDbContext context) : IHomeService
{
    private readonly ApplicationDbContext _context = context;

    public HomeViewModel GetHomeViewModel()
    {
        // Статистика
        var greenhouseCount = _context.Greenhouses.Count();
        var sensorCount = _context.Sensors.Count();
        var criticalEvents = _context.Events.Count(e => e.Criticality == "high");
        var plannedActions = _context.Actions.Count(a => a.Status == "planned");

        // Последние 5 событий (с навигацией)
        var latestEvents = _context.Events
            .Include(e => e.Sensor)
                .ThenInclude(s => s.Greenhouse)
            .OrderByDescending(e => e.Timestamp)
            .Take(5)
            .ToList();

        // Топ-3 культуры (по количеству теплиц)
        var topCrops = _context.Crops
            .Include(c => c.Greenhouses)
            .OrderByDescending(c => c.Greenhouses.Count)
            .Take(3)
            .ToList();

        return new HomeViewModel
        {
            GreenhouseCount = greenhouseCount,
            SensorCount = sensorCount,
            CriticalEvents = criticalEvents,
            PlannedActions = plannedActions,
            LatestEvents = latestEvents,
            TopCrops = topCrops
        };
    }
}