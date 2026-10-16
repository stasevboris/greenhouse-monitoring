using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseMonitoring.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GreenhousesController : ControllerBase
{
    private readonly AppDbContext _context;

    public GreenhousesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/greenhouses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GreenhouseDto>>> GetGreenhouses()
    {
        var data = await _context.Greenhouses
            .Include(g => g.Crop)
            .Select(g => new GreenhouseDto
            {
                GreenhouseId = g.GreenhouseId,
                NameNumber = g.NameNumber,
                Location = g.Location,
                CreatedAt = g.CreatedAt,
                CropId = g.CropId,
                CropName = g.Crop != null ? g.Crop.Name : null
            })
            .OrderBy(g => g.NameNumber)
            .ToListAsync();

        return Ok(data);
    }

    // GET: api/greenhouses/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GreenhouseDto>> GetGreenhouse(int id)
    {
        var greenhouse = await _context.Greenhouses
            .Include(g => g.Crop)
            .Where(g => g.GreenhouseId == id)
            .Select(g => new GreenhouseDto
            {
                GreenhouseId = g.GreenhouseId,
                NameNumber = g.NameNumber,
                Location = g.Location,
                CreatedAt = g.CreatedAt,
                CropId = g.CropId,
                CropName = g.Crop != null ? g.Crop.Name : null
            })
            .FirstOrDefaultAsync();

        if (greenhouse == null) return NotFound();
        return Ok(greenhouse);
    }

    // POST: api/greenhouses
    [HttpPost]
    public async Task<ActionResult<GreenhouseDto>> CreateGreenhouse(GreenhouseCreateDto dto)
    {
        // Защита от несуществующего CropId
        bool cropExists = await _context.Crops.AnyAsync(c => c.CropId == dto.CropId);
        if (!cropExists)
            return BadRequest($"Культура с ID {dto.CropId} не существует. Сначала создайте её через POST /api/crops");

        var greenhouse = new Greenhouse
        {
            NameNumber = dto.NameNumber,
            Location = dto.Location,
            CropId = dto.CropId,
            CreatedAt = DateTime.Now
        };

        _context.Greenhouses.Add(greenhouse);
        await _context.SaveChangesAsync();

        // Возвращаем полный DTO с CropName
        var result = await _context.Greenhouses
            .Include(g => g.Crop)
            .Where(g => g.GreenhouseId == greenhouse.GreenhouseId)
            .Select(g => new GreenhouseDto
            {
                GreenhouseId = g.GreenhouseId,
                NameNumber = g.NameNumber,
                Location = g.Location,
                CreatedAt = g.CreatedAt,
                CropId = g.CropId,
                CropName = g.Crop != null ? g.Crop.Name : null
            })
            .FirstAsync();

        return CreatedAtAction(nameof(GetGreenhouse), new { id = greenhouse.GreenhouseId }, result);
    }

    // PUT: api/greenhouses/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGreenhouse(int id, GreenhouseCreateDto dto)
    {
        var greenhouse = await _context.Greenhouses.FindAsync(id);
        if (greenhouse == null) return NotFound();

        // Проверка существования нового CropId
        bool cropExists = await _context.Crops.AnyAsync(c => c.CropId == dto.CropId);
        if (!cropExists)
            return BadRequest($"Культура с ID {dto.CropId} не существует");

        greenhouse.NameNumber = dto.NameNumber;
        greenhouse.Location = dto.Location;
        greenhouse.CropId = dto.CropId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/greenhouses/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGreenhouse(int id)
    {
        var greenhouse = await _context.Greenhouses
            .Include(g => g.Sensors)
            .FirstOrDefaultAsync(g => g.GreenhouseId == id);

        if (greenhouse == null) return NotFound();

        if (greenhouse.Sensors.Any())
            return BadRequest("Нельзя удалить теплицу — в ней есть датчики");

        _context.Greenhouses.Remove(greenhouse);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

// DTO
public class GreenhouseDto
{
    public int GreenhouseId { get; set; }
    public string NameNumber { get; set; } = null!;
    public string Location { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public int CropId { get; set; }
    public string? CropName { get; set; }
}

public class GreenhouseCreateDto
{
    public string NameNumber { get; set; } = null!;
    public string Location { get; set; } = null!;
    public int CropId { get; set; }
}