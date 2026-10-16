using GreenhouseMonitoring.Controllers;
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Models;
using GreenhouseMonitoring.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GreenhouseMonitoring.Tests.Controllers;

[Collection("SQLite")]
public class GreenhousesControllerTests : ControllerTestBase
{
    private GreenhousesController Create(ApplicationDbContext ctx)
    {
        var mockReportService = new Mock<IReportService>();
        return new GreenhousesController(ctx, mockReportService.Object);
    }
    [Fact]
    public async Task Index_ReturnsViewResult()
    {
        var c = Create(CreateDbContext());
        var r = await c.Index();
        Assert.IsType<ViewResult>(r);
    }


    [Fact]
    public async Task Index_EmptyDatabase_ReturnsEmptyList()
    {
        var c = Create(CreateDbContext());
        var result = await c.Index() as ViewResult;
        var model = Assert.IsType<List<Greenhouse>>(result!.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task Index_ReturnsDefaultViewName()
    {
        var c = Create(CreateDbContext());
        var result = await c.Index() as ViewResult;
        Assert.Null(result!.ViewName); // или Assert.Equal("Index", result.ViewName);
    }

    [Fact]
    public async Task Create_Post_Invalid_ReturnsView()
    {
        var c = Create(CreateDbContext());
        c.ModelState.AddModelError("NameNumber", "Обязательное поле");
        var result = await c.Create(new Greenhouse());
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_ValidModel_ReturnsViewWithSameModel_WhenInvalid()
    {
        var c = Create(CreateDbContext());
        c.ModelState.AddModelError("Location", "Укажите местоположение");

        var gh = new Greenhouse { NameNumber = "Т-99" };
        var result = await c.Create(gh) as ViewResult;

        Assert.Equal(gh, result!.Model);
    }

    [Fact]
    public async Task Edit_Get_InvalidId_ReturnsNotFound()
    {
        var c = Create(CreateDbContext());
        var r = await c.Edit(999);
        Assert.IsType<NotFoundResult>(r);
    }

    [Fact]
    public async Task Edit_Get_ValidId_ReturnsViewWithCorrectModel()
    {
        var ctx = CreateDbContext();
        ctx.Greenhouses.Add(new Greenhouse
        {
            GreenhouseId = 7,
            NameNumber = "Теплица 7",
            Location = "Южный сектор"
        });
        await ctx.SaveChangesAsync();

        var c = Create(ctx);
        var result = await c.Edit(7) as ViewResult;
        var model = Assert.IsType<Greenhouse>(result!.Model);
        Assert.Equal("Теплица 7", model.NameNumber);
    }
    // 1. Проверка возвращения корректной View при существующем Id
    [Fact]
    public async Task Edit_Get_ExistingId_ReturnsViewWithCorrectModel()
    {
        var ctx = CreateDbContext();
        ctx.Greenhouses.Add(new Greenhouse
        {
            GreenhouseId = 15,
            NameNumber = "Теплица №15",
            Location = "Северный корпус"
        });
        await ctx.SaveChangesAsync();

        var controller = Create(ctx);
        var result = await controller.Edit(15) as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsType<Greenhouse>(result.Model);
        Assert.Equal("Теплица №15", model.NameNumber);
        Assert.Equal("Северный корпус", model.Location);
    }

    // 2. Проверка возвращения NotFound при несуществующем Id
    [Fact]
    public async Task Edit_Get_NonExistentId_ReturnsNotFound()
    {
        var ctx = CreateDbContext();
        var controller = Create(ctx);

        var result = await controller.Edit(999) as NotFoundResult;

        Assert.NotNull(result);
        Assert.Equal(404, result!.StatusCode);
    }

    // 3. Проверка возвращения NotFound при отрицательном Id
    [Fact]
    public async Task Edit_Get_NegativeId_ReturnsNotFound()
    {
        var ctx = CreateDbContext();
        ctx.Greenhouses.AddRange(
            new Greenhouse { GreenhouseId = 1, NameNumber = "Теплица 1", Location = "Восток" },
            new Greenhouse { GreenhouseId = 2, NameNumber = "Теплица 2", Location = "Запад" }
        );
        await ctx.SaveChangesAsync();

        var controller = Create(ctx);
        var result = await controller.Edit(-5) as NotFoundResult;

        Assert.NotNull(result);
        Assert.Equal(404, result!.StatusCode);
    }

    // 4. Проверка, что возвращается правильная View с полностью заполненной моделью
    [Fact]
    public async Task Edit_Get_ValidId_ReturnsViewWithFullyPopulatedModel()
    {
        var ctx = CreateDbContext();
        var expected = new Greenhouse
        {
            GreenhouseId = 42,
            NameNumber = "Экспериментальная теплица А-42",
            Location = "Лабораторный блок",
        };
        ctx.Greenhouses.Add(expected);
        await ctx.SaveChangesAsync();

        var controller = Create(ctx);
        var result = await controller.Edit(42) as ViewResult;

        Assert.NotNull(result);
        Assert.Equal("Edit", result.ViewName ?? "Edit"); // если ViewName не указана явно, по умолчанию "Edit"

        var model = Assert.IsType<Greenhouse>(result.Model);
        Assert.Equal(expected.GreenhouseId, model.GreenhouseId);
        Assert.Equal(expected.NameNumber, model.NameNumber);
        Assert.Equal(expected.Location, model.Location);
    }
    [Fact]
    public async Task Edit_Get_NullId_ReturnsNotFound()
    {
        var c = Create(CreateDbContext());
        var result = await c.Edit(null);
        Assert.IsType<NotFoundResult>(result);
    }


    [Fact]
    public async Task Delete_Get_InvalidId_ReturnsNotFound()
    {
        var c = Create(CreateDbContext());
        var r = await c.Delete(999);
        Assert.IsType<NotFoundResult>(r);
    }


    [Fact]
    public async Task Delete_Get_NullId_ReturnsNotFound()
    {
        var c = Create(CreateDbContext());
        var result = await c.Delete(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact] public Task Controller_CanBeInstantiated() { var c = Create(CreateDbContext()); Assert.NotNull(c); return Task.CompletedTask; }


    [Fact]
    public void Controller_DerivesFromControllerBase()
    {
        var c = Create(CreateDbContext());
        Assert.IsAssignableFrom<Controller>(c);
    }

}