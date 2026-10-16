using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Infrastructure;
using GreenhouseMonitoring.Infrastructure.Filters;
using GreenhouseMonitoring.Models;
using GreenhouseMonitoring.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseMonitoring.Controllers;

[ExceptionFilter]
[TypeFilter(typeof(TimingLogAttribute))]
public class NormativeParametersController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index(NormativeFilter filter)
    {
        var queryCropId = Request.Query["cropId"].FirstOrDefault();

        // 1. Пользователь явно что-то выбрал в выпадашке — ЭТО ГЛАВНЫЙ ПРИОРИТЕТ
        if (queryCropId != null) // даже пустая строка — это выбор!
        {
            if (int.TryParse(queryCropId, out int id) && id > 0)
            {
                filter.CropId = id;
                HttpContext.Session.Set("NormativeFilter", filter); // сохраняем конкретную культуру
            }
            else
            {
                filter.CropId = null;
                HttpContext.Session.Remove("NormativeFilter"); // УДАЛЯЕМ ИЗ СЕССИИ — "Все культуры"
            }
        }
        else
        {
            // 2. URL пустой — пришли с редиректа (Create/Edit/Delete)
            var saved = HttpContext.Session.Get<NormativeFilter>("NormativeFilter");
            if (saved?.CropId > 0)
            {
                filter.CropId = saved.CropId;
            }
            // если в сессии null или ничего нет → "Все культуры"
        }

        var query = _context.NormativeParameters
            .Include(np => np.Crop)
            .AsQueryable();

        if (filter.CropId > 0)
            query = query.Where(np => np.CropId == filter.CropId);

        ViewBag.CropId = filter.CropId ?? 0;
        ViewBag.Crops = await _context.Crops
            .Select(c => new SelectListItem
            {
                Value = c.CropId.ToString(),
                Text = c.Name
            })
            .ToListAsync();

        ViewBag.CurrentFilter = filter;

        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Crops = new SelectList(await _context.Crops.ToListAsync(), "CropId", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CropId,Parameter,MinValue,MaxValue,GrowthStage")] NormativeParameter param)
    {
        if (ModelState.IsValid)
        {
            _context.Add(param);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Норматив успешно добавлен";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Crops = new SelectList(await _context.Crops.ToListAsync(), "CropId", "Name", param.CropId);
        return View(param);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var param = await _context.NormativeParameters.FindAsync(id);
        if (param == null) return NotFound();
        ViewBag.Crops = new SelectList(await _context.Crops.ToListAsync(), "CropId", "Name", param.CropId);
        return View(param);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "")] NormativeParameter param)
    {
        if (id != param.ParameterId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(param);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Норматив успешно обновлён";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Ошибка сохранения: " + ex.Message);
            }
        }

        ViewBag.Crops = new SelectList(await _context.Crops.ToListAsync(), "CropId", "Name", param.CropId);
        return View(param);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var param = await _context.NormativeParameters
            .Include(np => np.Crop)
            .FirstOrDefaultAsync(m => m.ParameterId == id);
        if (param == null) return NotFound();
        return View(param);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var param = await _context.NormativeParameters.FindAsync(id);
        if (param != null)
        {
            _context.NormativeParameters.Remove(param);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Норматив удалён";
        }
        return RedirectToAction(nameof(Index));
    }
}