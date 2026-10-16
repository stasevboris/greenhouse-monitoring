using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseMonitoring.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CropsController : ControllerBase
{
    private readonly AppDbContext _context;
    public CropsController(AppDbContext context) => _context = context;

    // GET: api/crops
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CropDto>>> GetCrops()
    {
        var crops = await _context.Crops
            .Select(c => new CropDto
            {
                CropId = c.CropId,
                Name = c.Name
            })
            .ToListAsync();

        return Ok(crops);
    }

    // GET: api/crops/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CropDto>> GetCrop(int id)
    {
        var crop = await _context.Crops
            .Where(c => c.CropId == id)
            .Select(c => new CropDto
            {
                CropId = c.CropId,
                Name = c.Name
            })
            .FirstOrDefaultAsync();

        if (crop == null) return NotFound();
        return Ok(crop);
    }

    // POST: api/crops
    [HttpPost]
    public async Task<ActionResult<CropDto>> CreateCrop(CropCreateDto dto)
    {
        var crop = new Crop
        {
            Name = dto.Name
        };

        _context.Crops.Add(crop);
        await _context.SaveChangesAsync();

        var result = new CropDto
        {
            CropId = crop.CropId,
            Name = crop.Name
        };

        return Created($"/api/crops/{crop.CropId}", result);
    }
}

public class CropDto
{
    public int CropId { get; set; }
    public string Name { get; set; } = null!;
}

public class CropCreateDto
{
    public string Name { get; set; } = null!;
}
