// Controllers/SensorsController.cs
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Infrastructure.Filters;
using GreenhouseMonitoring.Models;
using GreenhouseMonitoring.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseMonitoring.Controllers;

[ExceptionFilter]
[TypeFilter(typeof(TimingLogAttribute))]
public class SensorsController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index()
    {
        var sensors = await _context.Sensors
            .Include(s => s.Greenhouse!)
                .ThenInclude(g => g!.Crop)
            .Include(s => s.SensorReadings)
            .Select(s => new SensorViewModel
            {
                SensorId = s.SensorId,
                Type = s.Type,
                Model = s.Model,
                GreenhouseName = s.Greenhouse != null ? s.Greenhouse.NameNumber : "—",
                CropName = s.Greenhouse != null && s.Greenhouse.Crop != null ? s.Greenhouse.Crop.Name : null,
                LocationInGh = s.LocationInGh,
                CreatedAt = s.CreatedAt,
                ReadingsCount = s.SensorReadings.Count
            })
            .ToListAsync();

        // Теперь можно присваивать — set доступен!
        foreach (var s in sensors)
        {
            s.GreenhouseDisplay = s.CropName != null
                ? $"{s.GreenhouseName} ({s.CropName})"
                : s.GreenhouseName;
        }

        return View(sensors.OrderBy(s => s.GreenhouseDisplay).ThenBy(s => s.Type));
    }

    public async Task<IActionResult> Create()
    {
        await PopulateGreenhousesListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Type,Model,LocationInGh,GreenhouseId")] Sensor sensor)
    {
        if (ModelState.IsValid)
        {
            sensor.CreatedAt = DateTime.Now;
            _context.Add(sensor);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Датчик успешно добавлен";
            return RedirectToAction(nameof(Index));
        }

        await PopulateGreenhousesListAsync(sensor.GreenhouseId);
        return View(sensor);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (!id.HasValue) return NotFound();

        var sensor = await _context.Sensors.FindAsync(id.Value);
        if (sensor == null) return NotFound();

        await PopulateGreenhousesListAsync(sensor.GreenhouseId);
        return View(sensor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "")] Sensor sensor)
    {
        if (id != sensor.SensorId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(sensor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Датчик успешно обновлён";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Ошибка сохранения: " + ex.Message);
            }
        }

        await PopulateGreenhousesListAsync(sensor.GreenhouseId);
        return View(sensor);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (!id.HasValue) return NotFound();

        var sensor = await _context.Sensors
            .Include(s => s.Greenhouse)
            .Include(s => s.SensorReadings)
            .FirstOrDefaultAsync(s => s.SensorId == id.Value);

        if (sensor == null) return NotFound();
        return View(sensor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sensor = await _context.Sensors
            .Include(s => s.SensorReadings)
            .FirstOrDefaultAsync(s => s.SensorId == id);

        if (sensor != null)
        {
            if (sensor.SensorReadings.Any())
            {
                TempData["Error"] = $"Нельзя удалить датчик — есть {sensor.SensorReadings.Count} показаний";
            }
            else
            {
                _context.Sensors.Remove(sensor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Датчик удалён";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateGreenhousesListAsync(int? selectedId = null)
    {
        var greenhouses = await _context.Greenhouses
            .Include(g => g.Crop)
            .Select(g => new
            {
                g.GreenhouseId,
                g.NameNumber,
                CropName = g.Crop != null ? g.Crop.Name : null
            })
            .ToListAsync();

        var list = greenhouses.Select(g => new
        {
            g.GreenhouseId,
            Display = g.CropName != null
                ? $"{g.NameNumber} ({g.CropName})"
                : g.NameNumber
        })
        .OrderBy(x => x.Display)
        .ToList();

        ViewBag.Greenhouses = new SelectList(list, "GreenhouseId", "Display", selectedId);
    }
}