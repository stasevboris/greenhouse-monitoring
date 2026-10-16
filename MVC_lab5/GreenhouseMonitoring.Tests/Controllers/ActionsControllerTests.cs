using GreenhouseMonitoring.Controllers;
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ActionEntity = GreenhouseMonitoring.Models.Action;

namespace GreenhouseMonitoring.Tests.Controllers;

[Collection("Database")]
public class ActionsControllerTests : ControllerTestBase
{
    private ActionsController Create(ApplicationDbContext ctx) => new ActionsController(ctx);


    [Fact]
    public async Task Details_ValidId_ReturnsView()
    {
        var ctx = CreateDbContext();
        ctx.Actions.Add(new ActionEntity
        {
            ActionId = 1,
            Type = "Test",
            Description = "Test",
            Status = "Planned",
            Timestamp = DateTime.Now
        });
        ctx.SaveChanges();

        var c = Create(ctx);
        var r = await c.Details(1);
        Assert.IsType<ViewResult>(r);
    }
    [Fact]
    public async Task Details_ValidId_ReturnsViewWithCorrectModel()
    {
        // Arrange
        var ctx = CreateDbContext();
        var action = new ActionEntity
        {
            ActionId = 1,
            Type = "Полив",
            Description = "Тестовое действие",
            Status = "Planned",
            Timestamp = DateTime.Now
        };
        ctx.Actions.Add(action);
        await ctx.SaveChangesAsync();

        var controller = Create(ctx);

        // Act
        var result = await controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ActionEntity>(viewResult.Model);
        Assert.Equal(1, model.ActionId);
        Assert.Equal("Полив", model.Type);
        Assert.Equal("Planned", model.Status);
    }

    [Fact]
    public async Task Details_InvalidId_ReturnsNotFound1()
    {
        // Arrange
        var ctx = CreateDbContext();
        var controller = Create(ctx);

        // Act
        var result = await controller.Details(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_NullId_ReturnsNotFound()
    {
        // Arrange
        var ctx = CreateDbContext();
        var controller = Create(ctx);

        // Act
        var result = await controller.Details(null);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    [Fact] public async Task Details_InvalidId_ReturnsNotFound() { var c = Create(CreateDbContext()); var r = await c.Details(999); Assert.IsType<NotFoundResult>(r); }

    [Fact]
    public async Task Create_Post_Invalid_ReturnsView()
    {
        var c = Create(CreateDbContext());
        c.ModelState.AddModelError("Type", "Тип обязателен");
        var result = await c.Create(new ActionEntity());
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ValidId_ReturnsView()
    {
        var ctx = CreateDbContext();
        ctx.Actions.Add(new ActionEntity
        {
            ActionId = 1,
            Type = "Test",
            Description = "Test",
            Status = "Planned",
            Timestamp = DateTime.Now
        });
        ctx.SaveChanges();

        var c = Create(ctx);
        var result = await c.Edit(1);
        Assert.IsType<ViewResult>(result);
    }

    [Fact] public async Task Edit_Get_InvalidId_ReturnsNotFound1() { var c = Create(CreateDbContext()); var r = await c.Edit(999); Assert.IsType<NotFoundResult>(r); }

    [Fact]
    public async Task Edit_Post_Invalid_ReturnsView()
    {
        var c = Create(CreateDbContext());
        c.ModelState.AddModelError("Type", "Ошибка");
        var result = await c.Edit(1, new ActionEntity());
        Assert.IsType<ViewResult>(result);
    }
    [Fact]
    public async Task Edit_Get_ValidId_ReturnsViewWithCorrectModel()
    {
        // Arrange
        var ctx = CreateDbContext();
        var action = new ActionEntity
        {
            ActionId = 5,
            Type = "Полив",
            Description = "Тестовое действие",
            Status = "Planned",
            Timestamp = DateTime.Now
        };
        ctx.Actions.Add(action);
        await ctx.SaveChangesAsync();

        var controller = Create(ctx);

        // Act
        var result = await controller.Edit(5);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ActionEntity>(viewResult.Model);
        Assert.Equal(5, model.ActionId);
        Assert.Equal("Полив", model.Type);
    }

    [Fact]
    public async Task Edit_Get_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var controller = Create(CreateDbContext());

        // Act
        var result = await controller.Edit(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_IdMismatch_ReturnsNotFound()
    {
        var controller = Create(CreateDbContext());
        var action = new ActionEntity { ActionId = 999 };

        var result = await controller.Edit(1, action); // id в маршруте ≠ id в модели

        Assert.IsType<NotFoundResult>(result);
    }
    [Fact]
    public async Task Delete_Get_ValidId_ReturnsView()
    {
        var ctx = CreateDbContext();
        ctx.Actions.Add(new ActionEntity
        {
            ActionId = 1,
            Type = "Test",
            Description = "Test",
            Status = "Planned",
            Timestamp = DateTime.Now
        });
        ctx.SaveChanges();

        var c = Create(ctx);
        var result = await c.Delete(1);
        Assert.IsType<ViewResult>(result);
    }

    [Fact] public async Task Delete_Get_InvalidId_ReturnsNotFound() { var c = Create(CreateDbContext()); var r = await c.Delete(999); Assert.IsType<NotFoundResult>(r); }


    [Fact]
    public async Task DeleteConfirmed_InvalidId_Redirects()
    {
        var c = Create(CreateDbContext());
        var result = await c.DeleteConfirmed(999);
        Assert.IsType<RedirectToActionResult>(result);
    }
    [Fact]
    public async Task DeleteConfirmed_InvalidId_DoesNotThrow_AndRedirectsToIndex()
    {
        // Arrange
        var ctx = CreateDbContext();

        // Убедимся, что в БД вообще нет записи с ID = 999
        var existsBefore = await ctx.Actions.AnyAsync(a => a.ActionId == 999);
        Assert.False(existsBefore);

        var controller = Create(ctx);

        // Act
        var result = await controller.DeleteConfirmed(999);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Null(redirectResult.ControllerName);

        // Дополнительно: убеждаемся, что ничего не сломалось и БД осталась чистой
        var stillNoRecord = await ctx.Actions.AnyAsync(a => a.ActionId == 999);
        Assert.False(stillNoRecord);
    }
}