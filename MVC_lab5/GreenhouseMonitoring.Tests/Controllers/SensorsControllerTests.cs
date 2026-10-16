using GreenhouseMonitoring.Controllers;
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Models;
using GreenhouseMonitoring.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xunit;

namespace GreenhouseMonitoring.Tests.Controllers;

[Collection("SQLite")]
public class SensorsControllerTests : ControllerTestBase
{
    private SensorsController Create(ApplicationDbContext ctx) => new SensorsController(ctx);

    [Fact]
    public async Task Create_Post_Invalid_ReturnsView()
    {
        var c = Create(CreateDbContext());
        c.ModelState.AddModelError("Type", "Тип обязателен");
        var result = await c.Create(new Sensor());
        Assert.IsType<ViewResult>(result);
    }

 

    [Fact] public async Task Edit_Get_InvalidId_ReturnsNotFound() { var c = Create(CreateDbContext()); var r = await c.Edit(999); Assert.IsType<NotFoundResult>(r); }

    [Fact] public async Task Delete_Get_InvalidId_ReturnsNotFound() { var c = Create(CreateDbContext()); var r = await c.Delete(999); Assert.IsType<NotFoundResult>(r); }
}