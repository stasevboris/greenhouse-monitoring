using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Infrastructure;
using GreenhouseMonitoring.Infrastructure.Filters;
using GreenhouseMonitoring.Models;
using GreenhouseMonitoring.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ActionEntity = GreenhouseMonitoring.Models.Action;

namespace GreenhouseMonitoring.Controllers;

[ExceptionFilter]
[TypeFilter(typeof(TimingLogAttribute))]
public class SensorReadingsController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index(ReadingsFilter filter)
    {
        var querySensorId = Request.Query["sensorId"].ToString();

        // Приоритет — что в URL
        if (Request.Query.ContainsKey("sensorId"))
        {
            if (int.TryParse(querySensorId, out int id) && id > 0)
            {
                filter.SensorId = id;
                HttpContext.Session.Set("ReadingsFilter", filter);
            }
            else
            {
                filter.SensorId = null;
                HttpContext.Session.Remove("ReadingsFilter"); // "— Все датчики —"
            }
        }
        else
        {
            // Пришли с редиректа
            var saved = HttpContext.Session.Get<ReadingsFilter>("ReadingsFilter");
            if (saved?.SensorId > 0)
                filter.SensorId = saved.SensorId;
        }

        // ИСПРАВЛЕНО: убрал опечатку "readings"
        var query = _context.SensorReadings.AsQueryable();

        query = query.Include(sr => sr.Sensor!)
                     .ThenInclude(s => s!.Greenhouse);

        if (filter.SensorId > 0)
            query = query.Where(sr => sr.SensorId == filter.SensorId);

        var readings = await query
            .OrderByDescending(sr => sr.Timestamp)
            .Take(200)
            .ToListAsync();

        // Список датчиков
        var sensorsData = await _context.Sensors
            .Include(s => s.Greenhouse)
            .Select(s => new
            {
                s.SensorId,
                s.Type,
                s.Model,
                GreenhouseName = s.Greenhouse != null ? s.Greenhouse.NameNumber : null
            })
            .ToListAsync();

        var sensorList = sensorsData
            .Select(s => new
            {
                s.SensorId,
                Display = $"{s.Type} ({s.Model ?? "—"}) — {(s.GreenhouseName ?? "Без теплицы")}"
            })
            .OrderBy(x => x.Display)
            .ToList();

        ViewBag.Sensors = new SelectList(sensorList, "SensorId", "Display", filter.SensorId);
        ViewBag.CurrentFilter = filter;

        return View(readings);
    }

    // Остальные методы — без изменений (всё работает идеально)
    public async Task<IActionResult> Create(int? sensorId = null)
    {
        await PopulateSensorsListAsync(sensorId);
        var model = new SensorReading
        {
            SensorId = sensorId ?? 0,
            Timestamp = DateTime.Now
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SensorId,Value,Timestamp")] SensorReading reading)
    {
        if (ModelState.IsValid)
        {
            reading.Timestamp = reading.Timestamp == default ? DateTime.Now : reading.Timestamp;
            _context.SensorReadings.Add(reading);
            await _context.SaveChangesAsync();
            await CheckNormativeAndCreateEventAsync(reading);
            TempData["Success"] = "Показание успешно добавлено";
            return RedirectToAction(nameof(Index));
        }
        await PopulateSensorsListAsync(reading.SensorId);
        return View(reading);
    }

    public async Task<IActionResult> Edit(long? id)
    {
        if (!id.HasValue) return NotFound();
        var reading = await _context.SensorReadings.FindAsync(id.Value);
        if (reading == null) return NotFound();
        await PopulateSensorsListAsync(reading.SensorId);
        return View(reading);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, [Bind(Prefix = "")] SensorReading reading)
    {
        if (id != reading.ReadingId) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(reading);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Показание обновлено";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Ошибка сохранения: " + ex.Message);
            }
        }
        await PopulateSensorsListAsync(reading.SensorId);
        return View(reading);
    }

    public async Task<IActionResult> Delete(long? id)
    {
        if (!id.HasValue) return NotFound();
        var reading = await _context.SensorReadings
            .Include(sr => sr.Sensor!)
            .ThenInclude(s => s!.Greenhouse)
            .FirstOrDefaultAsync(sr => sr.ReadingId == id.Value);
        if (reading == null) return NotFound();
        return View(reading);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var reading = await _context.SensorReadings.FindAsync(id);
        if (reading != null)
        {
            _context.SensorReadings.Remove(reading);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Показание удалено";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateSensorsListAsync(int? selectedId = null)
    {
        var sensorsData = await _context.Sensors
            .Include(s => s.Greenhouse)
            .Select(s => new
            {
                s.SensorId,
                s.Type,
                s.Model,
                GreenhouseName = s.Greenhouse != null ? s.Greenhouse.NameNumber : null
            })
            .ToListAsync();

        var list = sensorsData
            .Select(s => new
            {
                s.SensorId,
                Display = $"{s.Type} ({s.Model ?? "—"}) — {(s.GreenhouseName ?? "Без теплицы")}"
            })
            .OrderBy(x => x.Display)
            .ToList();

        ViewBag.Sensors = new SelectList(list, "SensorId", "Display", selectedId);
    }
    public async Task<IActionResult> Chart(int sensorId)
    {
        var sensor = await _context.Sensors
            .Include(s => s.Greenhouse)
            .FirstOrDefaultAsync(s => s.SensorId == sensorId);

        if (sensor == null)
            return NotFound();

        // Берем последние 100 показаний (или все, если меньше)
        var readings = await _context.SensorReadings
            .Where(sr => sr.SensorId == sensorId)
            .OrderBy(sr => sr.Timestamp)
            .Take(100)
            .Select(sr => new { sr.Timestamp, sr.Value })
            .ToListAsync();

        if (!readings.Any())
        {
            TempData["Error"] = "Нет данных для построения графика";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Sensor = sensor;
        ViewBag.Labels = readings.Select(r => r.Timestamp.ToString("HH:mm:ss")).ToArray();
        ViewBag.Data = readings.Select(r => (double)r.Value).ToArray();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetReadingsData(int sensorId)
    {
        var readings = await _context.SensorReadings
            .Where(sr => sr.SensorId == sensorId)
            .OrderBy(sr => sr.Timestamp)
            .Take(100)
            .Select(sr => new { sr.Timestamp, sr.Value })
            .ToListAsync();

        return Json(new
        {
            labels = readings.Select(r => r.Timestamp.ToString("HH:mm:ss")),
            data = readings.Select(r => r.Value)
        });
    }

    private async Task CheckNormativeAndCreateEventAsync(SensorReading reading)
    {
        var sensor = await _context.Sensors
            .Include(s => s.Greenhouse!)
            .ThenInclude(g => g!.Crop!)
            .ThenInclude(c => c!.NormativeParameters)
            .FirstOrDefaultAsync(s => s.SensorId == reading.SensorId);

        if (sensor?.Greenhouse?.Crop == null) return;

        var paramName = sensor.Type.ToLower();
        var normative = sensor.Greenhouse.Crop.NormativeParameters
            .FirstOrDefault(np => np.Parameter.ToLower() == paramName);

        if (normative == null) return;

        if (reading.Value < normative.MinValue || reading.Value > normative.MaxValue)
        {
            var criticality = reading.Value < normative.MinValue ? "high" : "critical";
            var description = reading.Value < normative.MinValue
                ? $"{sensor.Type} ниже нормы: {reading.Value} (мин: {normative.MinValue})"
                : $"{sensor.Type} выше нормы: {reading.Value} (макс: {normative.MaxValue})";

            var ev = new Event
            {
                SensorId = sensor.SensorId,
                ReadingId = reading.ReadingId,
                Timestamp = reading.Timestamp,
                Description = description,
                Criticality = criticality
            };
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            var action = new ActionEntity
            {
                EventId = ev.EventId,
                Type = "Проверка",
                Description = $"Авто: {description}",
                Status = "Planned",
                Timestamp = DateTime.Now
            };
            _context.Actions.Add(action);
            await _context.SaveChangesAsync();
        }
    }
}