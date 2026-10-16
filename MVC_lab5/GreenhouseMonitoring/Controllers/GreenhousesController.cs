// Controllers/GreenhousesController.cs
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Infrastructure.Filters;
using GreenhouseMonitoring.Models;
using GreenhouseMonitoring.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseMonitoring.Controllers;

[ExceptionFilter]
[TypeFilter(typeof(TimingLogAttribute))]
public class GreenhousesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IReportService _reportService;

    // Обновленный конструктор с IReportService
    public GreenhousesController(ApplicationDbContext context, IReportService reportService)
    {
        _context = context;
        _reportService = reportService;
    }

    // GET: Greenhouses
    public async Task<IActionResult> Index()
    {
        var greenhouses = await _context.Greenhouses
            .Include(g => g.Crop)
            .Include(g => g.Sensors)
            .OrderBy(g => g.NameNumber)
            .ToListAsync();

        return View(greenhouses);
    }

    // GET: Greenhouses/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Crops = new SelectList(await _context.Crops.ToListAsync(), "CropId", "Name");
        return View();
    }

    // POST: Greenhouses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NameNumber,Location,CropId")] Greenhouse greenhouse)
    {
        if (ModelState.IsValid)
        {
            greenhouse.CreatedAt = DateTime.Now;
            _context.Add(greenhouse);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Теплица успешно добавлена";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Crops = new SelectList(await _context.Crops.ToListAsync(), "CropId", "Name", greenhouse.CropId);
        return View(greenhouse);
    }

    // GET: Greenhouses/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var greenhouse = await _context.Greenhouses.FindAsync(id);
        if (greenhouse == null) return NotFound();

        ViewBag.Crops = new SelectList(await _context.Crops.ToListAsync(), "CropId", "Name", greenhouse.CropId);
        return View(greenhouse);
    }

    // POST: Greenhouses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "")] Greenhouse greenhouse)
    {
        if (id != greenhouse.GreenhouseId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(greenhouse);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Теплица успешно обновлена";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Greenhouses.Any(e => e.GreenhouseId == id))
                    return NotFound();
                throw;
            }
        }

        ViewBag.Crops = new SelectList(await _context.Crops.ToListAsync(), "CropId", "Name", greenhouse.CropId);
        return View(greenhouse);
    }

    // GET: Greenhouses/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var greenhouse = await _context.Greenhouses
            .Include(g => g.Crop)
            .Include(g => g.Sensors)
            .FirstOrDefaultAsync(m => m.GreenhouseId == id);

        if (greenhouse == null) return NotFound();

        return View(greenhouse);
    }

    // POST: Greenhouses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var greenhouse = await _context.Greenhouses
            .Include(g => g.Sensors)
            .FirstOrDefaultAsync(g => g.GreenhouseId == id);

        if (greenhouse != null)
        {
            if (greenhouse.Sensors.Any())
            {
                TempData["Error"] = "Нельзя удалить теплицу — в ней есть датчики (" + greenhouse.Sensors.Count + " шт.)";
            }
            else
            {
                _context.Greenhouses.Remove(greenhouse);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Теплица удалена";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // НОВЫЙ МЕТОД: Отчет по эффективности
    public async Task<IActionResult> Report(int id)
    {
        var report = await _reportService.GetEfficiencyReportAsync(id);
        ViewBag.Greenhouse = await _context.Greenhouses.FindAsync(id);
        return View(report);
    }
}