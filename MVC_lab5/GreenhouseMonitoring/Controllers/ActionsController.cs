using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Infrastructure;
using GreenhouseMonitoring.Infrastructure.Filters;
using GreenhouseMonitoring.Infrastructure.Helpers;
using GreenhouseMonitoring.Models;
using GreenhouseMonitoring.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ActionEntity = GreenhouseMonitoring.Models.Action;

namespace GreenhouseMonitoring.Controllers;

[ExceptionFilter]
[TypeFilter(typeof(TimingLogAttribute))]
public class ActionsController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    [SetToSession("ActionsFilter")]
    [ResponseCache(Duration = 274)]
    public async Task<IActionResult> Index(ActionsFilter filter)
    {
        // Восстанавливаем только Status из сессии (Page приходит из URL — пагинация работает!)
        if (string.IsNullOrEmpty(filter.Status))
        {
            var saved = HttpContext.Session.Get<ActionsFilter>("ActionsFilter");
            if (saved?.Status != null)
            {
                filter.Status = saved.Status;
            }
        }
        HttpContext.Session.Set("ActionsFilter", filter);
        // Page всегда будет правильным — либо из URL, либо 1
        // После Create/Edit вернёмся на страницу 1 с нужным фильтром — новое действие видно!

        var query = _context.Actions
            .Include(a => a.Event)
                .ThenInclude(e => e!.Sensor!.Greenhouse)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(a => a.Status == filter.Status);

        var total = await query.CountAsync();

        var actions = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((filter.Page - 1) * 10)
            .Take(10)
            .ToListAsync();

        var viewModel = new PageViewModel<ActionEntity>(total, filter.Page, 10)
        {
            Items = actions
        };

        ViewData["Status"] = filter.Status;
        ViewBag.CurrentFilter = filter; // ← Это использует .cshtml для кнопок и пагинации

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var action = await _context.Actions
            .Include(a => a.Event)
                .ThenInclude(e => e!.Sensor!.Greenhouse)
            .FirstOrDefaultAsync(m => m.ActionId == id);
        if (action == null) return NotFound();
        return View(action);
    }

    public async Task<IActionResult> Create(int? eventId = null, string? status = null)
    {
        await PopulateEventsDropDownAsync(eventId);
        var model = new ActionEntity
        {
            EventId = eventId,
            Timestamp = DateTime.Now,
            Status = status ?? "Planned"  // ← Вот оно! Сохраняем текущий фильтр
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EventId,Type,Description,Status")] ActionEntity model)
    {
        ModelState.Remove("Event");
        if (ModelState.IsValid)
        {
            if (model.EventId.HasValue)
            {
                var eventExists = await _context.Events.AnyAsync(e => e.EventId == model.EventId.Value);
                if (!eventExists)
                {
                    ModelState.AddModelError("EventId", "Выбранное событие не существует");
                    await PopulateEventsDropDownAsync(model.EventId);
                    return View(model);
                }
            }

            try
            {
                model.Timestamp = DateTime.Now;
                _context.Actions.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Действие успешно создано";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Ошибка при сохранении: {errorMessage}");
            }
        }
        await PopulateEventsDropDownAsync(model.EventId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var action = await _context.Actions.FindAsync(id);
        if (action == null) return NotFound();
        await PopulateEventsDropDownAsync(action.EventId);
        return View(action);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "")] ActionEntity action)
    {
        action.ActionId = id;
        ModelState.Remove("ActionId");
        ModelState.Remove("Timestamp");

        if (!ModelState.IsValid)
        {
            await PopulateEventsDropDownAsync(action.EventId);
            return View(action);
        }

        try
        {
            var existing = await _context.Actions.FindAsync(id);
            if (existing == null) return NotFound();

            existing.EventId = action.EventId;
            existing.Type = action.Type;
            existing.Description = action.Description;
            existing.Status = action.Status;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Действие успешно обновлено";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Ошибка сохранения: " + ex.Message);
            await PopulateEventsDropDownAsync(action.EventId);
            return View(action);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var action = await _context.Actions
            .Include(a => a.Event)
            .FirstOrDefaultAsync(m => m.ActionId == id);
        if (action == null) return NotFound();
        return View(action);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var action = await _context.Actions.FindAsync(id);
        if (action != null)
        {
            _context.Actions.Remove(action);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Действие удалено";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateEventsDropDownAsync(int? selectedEventId = null)
    {
        var events = await _context.Events
            .OrderByDescending(e => e.Timestamp)
            .Select(e => new SelectListItem
            {
                Value = e.EventId.ToString(),
                Text = $"{e.Timestamp:g} | {e.Criticality} | {e.Description.Truncate(50)}"
            })
            .ToListAsync();
        ViewBag.Events = new SelectList(events, "Value", "Text", selectedEventId);
    }
}