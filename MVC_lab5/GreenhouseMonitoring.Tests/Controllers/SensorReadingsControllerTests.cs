using GreenhouseMonitoring.Controllers;
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xunit;

namespace GreenhouseMonitoring.Tests.Controllers;

[Collection("SQLite")]
public class SensorReadingsControllerTests : ControllerTestBase
{
    private SensorReadingsController Create(ApplicationDbContext ctx) => new SensorReadingsController(ctx);


    [Fact]
    public async Task Create_Post_Invalid_ReturnsView()
    {
        var c = Create(CreateDbContext());
        c.ModelState.AddModelError("Value", "Значение обязательно");
        var result = await c.Create(new SensorReading());
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ValidId_ReturnsView()
    {
        var ctx = CreateDbContext();
        ctx.SensorReadings.Add(new SensorReading { ReadingId = 10, Value = 25.0m });
        ctx.SaveChanges();

        var c = Create(ctx);
        var result = await c.Edit(10);
        Assert.IsType<ViewResult>(result);
    }

    [Fact] public async Task Edit_Get_InvalidId_ReturnsNotFound() { var c = Create(CreateDbContext()); var r = await c.Edit(999); Assert.IsType<NotFoundResult>(r); }

   
    [Fact] public async Task Delete_Get_InvalidId_ReturnsNotFound() { var c = Create(CreateDbContext()); var r = await c.Delete(999); Assert.IsType<NotFoundResult>(r); }
}