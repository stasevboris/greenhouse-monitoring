using GreenhouseMonitoring.Controllers;
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xunit;

namespace GreenhouseMonitoring.Tests.Controllers;

[Collection("SQLite")]
public class CropsControllerTests : ControllerTestBase
{
    private CropsController Create(ApplicationDbContext ctx) => new CropsController(ctx);

    [Fact] public async Task Index_ReturnsViewResult() { var c = Create(CreateDbContext()); var r = await c.Index(); Assert.IsType<ViewResult>(r); }

    [Fact]
    public async Task Create_Post_Invalid_ReturnsView()
    {
        var c = Create(CreateDbContext());
        c.ModelState.AddModelError("Name", "Название обязательно");
        var result = await c.Create(new Crop());
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ValidId_ReturnsView()
    {
        var ctx = CreateDbContext();
        ctx.Crops.Add(new Crop { CropId = 1, Name = "Огурец" });
        ctx.SaveChanges();
        var c = Create(ctx);
        var result = await c.Edit(1);
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ValidId_ReturnsView()
    {
        var ctx = CreateDbContext();
        ctx.Crops.Add(new Crop { CropId = 1, Name = "Удаляемая" });
        ctx.SaveChanges();
        var c = Create(ctx);
        var result = await c.Delete(1);
        Assert.IsType<ViewResult>(result);
    }
    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsView()
    {
        var ctx = CreateDbContext();
        var c = Create(ctx);
        c.ModelState.AddModelError("Name", "Название обязательно");
        var result = await c.Edit(1, new Crop { CropId = 1 });
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Edit_Get_InvalidId_ReturnsNotFound()
    {
        var c = Create(CreateDbContext());
        var result = await c.Edit(999);
        Assert.IsType<NotFoundResult>(result);
    }


    [Fact]
    public async Task Edit_Post_IdMismatch_ReturnsNotFound()
    {
        var ctx = CreateDbContext();
        ctx.Crops.Add(new Crop { CropId = 1, Name = "Огурец" });
        ctx.SaveChanges();

        var c = Create(ctx);
        var result = await c.Edit(2, new Crop { CropId = 1, Name = "Помидор" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Post_EmptyName_ReturnsView()
    {
        var c = Create(CreateDbContext());
        c.ModelState.AddModelError("Name", "The Name field is required.");
        var result = await c.Create(new Crop { Name = "" });
        Assert.IsType<ViewResult>(result);
    }
    [Fact]
    public async Task Create_Post_NullName_ReturnsView()
    {
        var c = Create(CreateDbContext());
        c.ModelState.AddModelError("Name", "The Name field is required.");
        var result = await c.Create(new Crop { Name = null! });
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_WhitespaceName_ReturnsView()
    {
        var c = Create(CreateDbContext());
        c.ModelState.AddModelError("Name", "The Name field is required.");
        var result = await c.Create(new Crop { Name = "   " });
        Assert.IsType<ViewResult>(result);
    }
    [Fact]
    public async Task Index_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var ctx = CreateDbContext();
        // База данных пустая — ничего не добавляем
        var controller = Create(ctx);

        // Act
        var result = await controller.Index() as ViewResult;

        // Assert
        Assert.NotNull(result);
        var model = Assert.IsType<List<Crop>>(result.Model);
        Assert.Empty(model); // список пустой, но ViewResult вернулся
        Assert.Equal("Index", result.ViewName ?? "Index"); // явно или по умолчанию
    }
    [Fact]
    public async Task Index_ReturnsListOfCrops1()
    {
        // Arrange
        var ctx = CreateDbContext();
        ctx.Crops.AddRange(new[]
        {
        new Crop { CropId = 1, Name = "Томат" },
        new Crop { CropId = 2, Name = "Огурец" },
        new Crop { CropId = 3, Name = "Перец" }
    });
        await ctx.SaveChangesAsync();

        var controller = Create(ctx);

        // Act
        var result = await controller.Index() as ViewResult;

        // Assert
        Assert.NotNull(result);
        var model = Assert.IsType<List<Crop>>(result.Model);
        Assert.Equal(3, model.Count);
        Assert.Contains(model, c => c.Name == "Огурец");
    }
    [Fact]
    public async Task Index_ReturnsListOfCrops()
    {
        var ctx = CreateDbContext();
        ctx.Crops.AddRange(new[]
        {
        new Crop { CropId = 1, Name = "Первый" },
        new Crop { CropId = 2, Name = "Второй" }
    });
        ctx.SaveChanges();

        var c = Create(ctx);
        var result = await c.Index() as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsType<List<Crop>>(result.Model);
        Assert.Equal(2, model.Count);
    }
    [Fact] public async Task Delete_Get_InvalidId_ReturnsNotFound() { var c = Create(CreateDbContext()); var r = await c.Delete(999); Assert.IsType<NotFoundResult>(r); }
}