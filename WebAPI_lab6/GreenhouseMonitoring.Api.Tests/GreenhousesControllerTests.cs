using GreenhouseMonitoring.Api.Controllers;
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace GreenhouseMonitoring.Tests;

public class GreenhouseMonitoringControllerTests
{
    private AppDbContext GetInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var context = new AppDbContext(options);
        SeedData(context);
        return context;
    }

    private void SeedData(AppDbContext context)
    {
        if (context.Crops.Any()) return;

        // Культуры
        context.Crops.AddRange(
            new Crop { CropId = 1, Name = "Томат" },
            new Crop { CropId = 2, Name = "Огурец" }
        );

        // Теплицы
        context.Greenhouses.AddRange(
            new Greenhouse { GreenhouseId = 1, NameNumber = "Теплица №1", Location = "Север", CropId = 1, CreatedAt = DateTime.Now.AddDays(-5) },
            new Greenhouse { GreenhouseId = 2, NameNumber = "Теплица №2", Location = "Юг", CropId = 2, CreatedAt = DateTime.Now.AddDays(-3) }
        );

        context.SaveChanges();
    }

    // Вспомогательный метод для извлечения свойств из анонимного объекта
    private T GetProp<T>(object obj, string propName)
    {
        var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(prop);
        return (T)prop.GetValue(obj)!;
    }

    // =====================================
    // CropsController Tests
    // =====================================

    [Fact]
    public async Task Crops_Get_ReturnsAllCrops()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new CropsController(context);

        var result = await controller.GetCrops();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var crops = Assert.IsAssignableFrom<IEnumerable<CropDto>>(okResult.Value);

        Assert.Equal(2, crops.Count());
        Assert.Contains(crops, c => c.Name == "Томат");
    }

    [Fact]
    public async Task Crops_GetById_Existing_ReturnsOk()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new CropsController(context);

        var result = await controller.GetCrop(1);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var crop = Assert.IsType<CropDto>(okResult.Value);

        Assert.Equal(1, crop.CropId);
        Assert.Equal("Томат", crop.Name);
    }

    [Fact]
    public async Task Crops_GetById_NonExisting_ReturnsNotFound()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new CropsController(context);

        var result = await controller.GetCrop(999);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    // =====================================
    // GreenhousesController Tests
    // =====================================

    [Fact]
    public async Task Greenhouses_Get_ReturnsAllGreenhouses_WithCropNames()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var result = await controller.GetGreenhouses();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<GreenhouseDto>>(okResult.Value);

        var greenhouses = list.ToList();
        Assert.Equal(2, greenhouses.Count);
        Assert.Contains(greenhouses, g => g.NameNumber == "Теплица №1" && g.CropName == "Томат");
    }

    [Fact]
    public async Task Greenhouses_GetById_Existing_ReturnsOk()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var result = await controller.GetGreenhouse(1);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var greenhouse = Assert.IsType<GreenhouseDto>(okResult.Value);

        Assert.Equal("Теплица №1", greenhouse.NameNumber);
        Assert.Equal("Томат", greenhouse.CropName);
        Assert.Equal(1, greenhouse.CropId);
    }

    [Fact]
    public async Task Greenhouses_GetById_NonExisting_ReturnsNotFound()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var result = await controller.GetGreenhouse(999);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Greenhouses_Post_ValidDto_CreatesGreenhouseAndSetsCreatedAt()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var dto = new GreenhouseCreateDto
        {
            NameNumber = "Теплица №3",
            Location = "Запад",
            CropId = 1
        };

        var result = await controller.CreateGreenhouse(dto);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var greenhouseDto = Assert.IsType<GreenhouseDto>(createdResult.Value);

        Assert.Equal("Теплица №3", greenhouseDto.NameNumber);
        Assert.Equal("Томат", greenhouseDto.CropName);
        Assert.True(greenhouseDto.CreatedAt.HasValue);
        Assert.InRange(greenhouseDto.CreatedAt.Value, DateTime.Now.AddSeconds(-10), DateTime.Now.AddSeconds(10));

        var inDb = await context.Greenhouses.FirstOrDefaultAsync(g => g.NameNumber == "Теплица №3");
        Assert.NotNull(inDb);
        Assert.Equal(DateTime.Now.Date, inDb!.CreatedAt.Date); // точность до дня
    }

    [Fact]
    public async Task Greenhouses_Post_InvalidCropId_ReturnsBadRequest()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var dto = new GreenhouseCreateDto
        {
            NameNumber = "Невозможная теплица",
            Location = "Нигде",
            CropId = 999 // не существует
        };

        var result = await controller.CreateGreenhouse(dto);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("не существует", badRequest.Value?.ToString());
    }

    [Fact]
    public async Task Greenhouses_Put_Existing_UpdatesFields()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var dto = new GreenhouseCreateDto
        {
            NameNumber = "Обновлённая №1",
            Location = "Центр",
            CropId = 2
        };

        var result = await controller.UpdateGreenhouse(1, dto);
        Assert.IsType<NoContentResult>(result);

        var updated = await context.Greenhouses.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("Обновлённая №1", updated!.NameNumber);
        Assert.Equal("Центр", updated.Location);
        Assert.Equal(2, updated.CropId);
    }

    [Fact]
    public async Task Greenhouses_Put_NonExisting_ReturnsNotFound()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var dto = new GreenhouseCreateDto { NameNumber = "Нет", Location = "Нет", CropId = 1 };
        var result = await controller.UpdateGreenhouse(999, dto);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Greenhouses_Put_InvalidCropId_ReturnsBadRequest()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var dto = new GreenhouseCreateDto { NameNumber = "Плохая", Location = "Там", CropId = 999 };
        var result = await controller.UpdateGreenhouse(1, dto);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Greenhouses_Delete_Existing_WithNoSensors_RemovesGreenhouse()
    {
        // Создаём новую теплицу без датчиков
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var newGh = new Greenhouse
        {
            GreenhouseId = 99,
            NameNumber = "Удаляемая",
            Location = "Мусорка",
            CropId = 1,
            CreatedAt = DateTime.Now
        };
        context.Greenhouses.Add(newGh);
        await context.SaveChangesAsync();

        var controller = new GreenhousesController(context);
        var result = await controller.DeleteGreenhouse(99);
        Assert.IsType<NoContentResult>(result);

        Assert.Null(await context.Greenhouses.FindAsync(99));
    }
    // =====================================
    // Дополнительные тесты для CropsController
    // =====================================



    [Fact]
    public async Task Greenhouses_Post_ZeroCropId_ReturnsBadRequest()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var dto = new GreenhouseCreateDto { NameNumber = "Тест", Location = "Тест", CropId = 0 };
        var result = await controller.CreateGreenhouse(dto);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("не существует", badRequest.Value?.ToString());
    }

    [Fact]
    public async Task Greenhouses_Get_ReturnsOrderedByNameNumber()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        context.Greenhouses.RemoveRange(context.Greenhouses);
        context.Greenhouses.AddRange(new[]
        {
            new Greenhouse { GreenhouseId = 3, NameNumber = "Яблоневая", Location = "Юг", CropId = 1, CreatedAt = DateTime.Now },
            new Greenhouse { GreenhouseId = 4, NameNumber = "Абрикосовая", Location = "Север", CropId = 2, CreatedAt = DateTime.Now }
        });
        await context.SaveChangesAsync();

        var controller = new GreenhousesController(context);
        var result = await controller.GetGreenhouses();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<GreenhouseDto>>(okResult.Value);
        var greenhouses = list.ToList();

        Assert.Equal(2, greenhouses.Count);
        Assert.Equal("Абрикосовая", greenhouses[0].NameNumber);
        Assert.Equal("Яблоневая", greenhouses[1].NameNumber);
    }

    [Fact]
    public async Task Greenhouses_Put_WithSameData_DoesNotChangeAnything()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var original = await context.Greenhouses.FindAsync(1);
        var originalName = original!.NameNumber;
        var originalLocation = original.Location;
        var originalCrop = original.CropId;

        var controller = new GreenhousesController(context);
        var dto = new GreenhouseCreateDto
        {
            NameNumber = originalName,
            Location = originalLocation,
            CropId = originalCrop
        };

        var result = await controller.UpdateGreenhouse(1, dto);
        Assert.IsType<NoContentResult>(result);

        var updated = await context.Greenhouses.FindAsync(1);
        Assert.Equal(originalName, updated!.NameNumber);
        Assert.Equal(originalLocation, updated.Location);
        Assert.Equal(originalCrop, updated.CropId);
    }

    [Fact]
    public async Task Greenhouses_Put_ChangeToSameCropId_StillWorks()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var dto = new GreenhouseCreateDto
        {
            NameNumber = "Тест",
            Location = "Тест",
            CropId = 1 // тот же, что и был
        };

        var result = await controller.UpdateGreenhouse(1, dto);
        Assert.IsType<NoContentResult>(result);

        var updated = await context.Greenhouses.FindAsync(1);
        Assert.Equal(1, updated!.CropId);
    }

    [Fact]
    public async Task Greenhouses_Delete_Existing_WithEmptySensorsList_RemovesGreenhouse()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var greenhouse = new Greenhouse
        {
            GreenhouseId = 98,
            NameNumber = "Пустая",
            Location = "Тест",
            CropId = 1,
            CreatedAt = DateTime.Now,
            Sensors = new List<Sensor>() // пустой список
        };
        context.Greenhouses.Add(greenhouse);
        await context.SaveChangesAsync();

        var controller = new GreenhousesController(context);
        var result = await controller.DeleteGreenhouse(98);
        Assert.IsType<NoContentResult>(result);

        Assert.Null(await context.Greenhouses.FindAsync(98));
    }

    [Fact]
    public async Task Greenhouses_Delete_AfterDeletion_ThrowsNotFound()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var greenhouse = new Greenhouse
        {
            GreenhouseId = 97,
            NameNumber = "Временная",
            Location = "Тест",
            CropId = 1,
            CreatedAt = DateTime.Now
        };
        context.Greenhouses.Add(greenhouse);
        await context.SaveChangesAsync();

        var controller = new GreenhousesController(context);
        await controller.DeleteGreenhouse(97);
        var secondDelete = await controller.DeleteGreenhouse(97);
        Assert.IsType<NotFoundResult>(secondDelete);
    }

    [Fact]
    public async Task Greenhouses_Post_CreatesGreenhouse_WithCorrectCreatedAt()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var before = DateTime.Now;
        var result = await controller.CreateGreenhouse(new GreenhouseCreateDto
        {
            NameNumber = "Временная",
            Location = "Тест",
            CropId = 1
        });
        var after = DateTime.Now;

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<GreenhouseDto>(created.Value);
        Assert.InRange(dto.CreatedAt.Value, before, after);

        var inDb = await context.Greenhouses.FirstAsync(g => g.NameNumber == "Временная");
        Assert.InRange(inDb.CreatedAt, before, after);
    }

    [Fact]
    public async Task Greenhouses_GetById_ReturnsCorrectCreatedAt()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var result = await controller.GetGreenhouse(1);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<GreenhouseDto>(okResult.Value);

        var dbItem = await context.Greenhouses.FindAsync(1);
        Assert.Equal(dbItem!.CreatedAt, dto.CreatedAt);
    }

    [Fact]
    public async Task Crops_Get_ReturnsCrops_WithCorrectIdsAndNames()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new CropsController(context);

        var result = await controller.GetCrops();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var crops = Assert.IsAssignableFrom<IEnumerable<CropDto>>(okResult.Value).ToList();

        var tomato = crops.First(c => c.Name == "Томат");
        var cucumber = crops.First(c => c.Name == "Огурец");

        Assert.Equal(1, tomato.CropId);
        Assert.Equal(2, cucumber.CropId);
    }

    [Fact]
    public async Task Greenhouses_Post_CreatesGreenhouse_WithValidCropId_LinkedCorrectly()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        await controller.CreateGreenhouse(new GreenhouseCreateDto
        {
            NameNumber = "Новая",
            Location = "Центр",
            CropId = 2
        });

        var greenhouse = await context.Greenhouses.FirstAsync(g => g.NameNumber == "Новая");
        Assert.Equal(2, greenhouse.CropId);

        var crop = await context.Crops.FindAsync(2);
        Assert.Equal("Огурец", crop!.Name);
    }

    [Fact]
    public async Task Greenhouses_Put_ChangeCropId_ChangesCropNameInDto()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        // Обновляем теплицу 1 с CropId=1 → на CropId=2
        await controller.UpdateGreenhouse(1, new GreenhouseCreateDto
        {
            NameNumber = "Обновлённая",
            Location = "Центр",
            CropId = 2
        });

        var result = await controller.GetGreenhouse(1);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<GreenhouseDto>(okResult.Value);

        Assert.Equal("Огурец", dto.CropName);
        Assert.Equal(2, dto.CropId);
    }

    [Fact]
    public async Task Greenhouses_Get_ReturnsAll_WhenManyGreenhousesExist()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        context.Greenhouses.RemoveRange(context.Greenhouses);
        for (int i = 1; i <= 10; i++)
        {
            context.Greenhouses.Add(new Greenhouse
            {
                GreenhouseId = 100 + i,
                NameNumber = $"Теплица-{i}",
                Location = $"Место {i}",
                CropId = (i % 2) + 1,
                CreatedAt = DateTime.Now.AddDays(-i)
            });
        }
        await context.SaveChangesAsync();

        var controller = new GreenhousesController(context);
        var result = await controller.GetGreenhouses();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<GreenhouseDto>>(okResult.Value);
        Assert.Equal(10, list.Count());
    }

    [Fact]
    public async Task Greenhouses_Delete_NonExisting_ReturnsNotFound()
    {
        using var context = GetInMemoryContext(Guid.NewGuid().ToString());
        var controller = new GreenhousesController(context);

        var result = await controller.DeleteGreenhouse(999);
        Assert.IsType<NotFoundResult>(result);
    }
}