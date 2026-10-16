// Controllers/CropsController.cs
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Infrastructure.Filters;
using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ExceptionFilter]
[TypeFilter(typeof(TimingLogAttribute))]
public class CropsController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index() =>
        View(await _context.Crops
            .Include(c => c.Greenhouses)
            .Include(c => c.NormativeParameters)
            .ToListAsync());

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Crop crop)
    {
        if (ModelState.IsValid)
        {
            _context.Add(crop);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Культура успешно добавлена";
            return RedirectToAction(nameof(Index));
        }
        return View(crop);
    }

    // GET: Edit
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var crop = await _context.Crops.FindAsync(id);
        if (crop == null) return NotFound();
        return View(crop);
    }

    // POST: Edit — ИСПРАВЛЕНО: убираем проблему с префиксом "crop."
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "")] Crop crop) // КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ
    {
        if (id != crop.CropId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(crop);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Культура успешно обновлена";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Не удалось сохранить изменения: " + ex.Message);
            }
        }
        return View(crop);
    }

    // GET: Delete
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var crop = await _context.Crops
            .Include(c => c.Greenhouses)
            .Include(c => c.NormativeParameters)
            .FirstOrDefaultAsync(c => c.CropId == id);
        if (crop == null) return NotFound();
        return View(crop);
    }

    // POST: Delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var crop = await _context.Crops
            .Include(c => c.Greenhouses)
            .FirstOrDefaultAsync(c => c.CropId == id);

        if (crop != null)
        {
            if (crop.Greenhouses.Any())
            {
                TempData["Error"] = "Нельзя удалить культуру — на неё ссылаются теплицы";
            }
            else
            {
                _context.Crops.Remove(crop);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Культура удалена";
            }
        }
        return RedirectToAction(nameof(Index));
    }
}