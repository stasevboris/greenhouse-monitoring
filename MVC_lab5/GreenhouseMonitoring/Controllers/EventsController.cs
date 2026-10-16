using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Infrastructure;           // ← Для SessionExtensions.Get<T>()
using GreenhouseMonitoring.Infrastructure.Filters;
using GreenhouseMonitoring.Models;
using GreenhouseMonitoring.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseMonitoring.Controllers;

[ExceptionFilter]
[TypeFilter(typeof(TimingLogAttribute))]
public class EventsController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    [SetToSession("EventsFilter")]
    public async Task<IActionResult> Index(EventsFilter filter)
    {
        // Восстанавливаем только Criticality из сессии
        // Page НЕ трогаем — он приходит из URL (пагинация работает!)
        if (string.IsNullOrEmpty(filter.Criticality))
        {
            var saved = HttpContext.Session.Get<EventsFilter>("EventsFilter");
            if (saved?.Criticality != null)
            {
                filter.Criticality = saved.Criticality;
            }
        }
        HttpContext.Session.Set("EventsFilter", filter);

        filter.Page = filter.Page > 0 ? filter.Page : 1;

        // Если Page не пришёл — ставим 1 (после редиректа)
        // Это нормально — после создания события возвращаемся на первую страницу с нужным фильтром
        // → новое событие видно сразу!
        filter.Page = filter.Page > 0 ? filter.Page : 1;

        var query = _context.Events
            .Include(e => e.Sensor!)
                .ThenInclude(s => s!.Greenhouse)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Criticality))
            query = query.Where(e => e.Criticality == filter.Criticality);

        var total = await query.CountAsync();

        var events = await query
            .OrderByDescending(e => e.Timestamp)
            .Skip((filter.Page - 1) * 10)
            .Take(10)
            .ToListAsync();

        var viewModel = new PageViewModel<Event>(total, filter.Page, 10)
        {
            Items = events
        };

        ViewData["Criticality"] = filter.Criticality;
        ViewBag.CurrentFilter = filter; // ← Это использует твой .cshtml!

        return View(viewModel);
    }

    // Остальные методы — без изменений
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();
        ViewBag.Sensors = await GetSensorsListAsync();
        return View(ev);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "")] Event ev)
    {
        if (id != ev.EventId) return NotFound();

        if (!await _context.Sensors.AnyAsync(s => s.SensorId == ev.SensorId))
            ModelState.AddModelError("SensorId", "Датчик не найден");
        if (!await _context.SensorReadings.AnyAsync(r => r.ReadingId == ev.ReadingId))
            ModelState.AddModelError("ReadingId", "Показание не найдено");

        if (!ModelState.IsValid)
        {
            ViewBag.Sensors = await GetSensorsListAsync();
            return View(ev);
        }

        try
        {
            _context.Update(ev);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Событие успешно обновлено";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Ошибка сохранения: " + ex.Message);
            ViewBag.Sensors = await GetSensorsListAsync();
            return View(ev);
        }
    }

    public async Task<IActionResult> Create(string? criticality = null)
    {
        ViewBag.Sensors = await GetSensorsListAsync();
        var model = new Event
        {
            Criticality = criticality ?? "low"  // ← Сохраняем текущую критичность
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SensorId,ReadingId,Description,Criticality")] Event ev)
    {
        if (ModelState.IsValid)
        {
            var sensorExists = await _context.Sensors.AnyAsync(s => s.SensorId == ev.SensorId);
            var readingExists = await _context.SensorReadings.AnyAsync(r => r.ReadingId == ev.ReadingId);

            if (!sensorExists)
                ModelState.AddModelError("SensorId", "Выбранный датчик не существует");
            if (!readingExists)
                ModelState.AddModelError("ReadingId", "Указанное показание не существует");

            if (ModelState.IsValid)
            {
                ev.Timestamp = DateTime.Now;
                _context.Events.Add(ev);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Событие успешно создано";
                return RedirectToAction(nameof(Index));
            }
        }
        ViewBag.Sensors = await GetSensorsListAsync();
        return View(ev);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var ev = await _context.Events
            .Include(e => e.Sensor!)
                .ThenInclude(s => s!.Greenhouse)
            .Include(e => e.Actions)
            .FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null) return NotFound();
        return View(ev);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ev = await _context.Events.Include(e => e.Actions).FirstOrDefaultAsync(e => e.EventId == id);
        if (ev != null)
        {
            if (ev.Actions.Any())
            {
                TempData["Error"] = "Нельзя удалить событие — на него ссылаются действия";
            }
            else
            {
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Событие удалено";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetSensorsListAsync()
    {
        return await _context.Sensors
            .Include(s => s.Greenhouse)
            .OrderBy(s => s.Greenhouse!.NameNumber)
            .ThenBy(s => s.Type)
            .Select(s => new SelectListItem
            {
                Value = s.SensorId.ToString(),
                Text = $"{s.Greenhouse!.NameNumber} — {s.Type} ({s.LocationInGh})"
            })
            .ToListAsync();
    }
}