using GreenhouseMonitoring.Controllers;
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xunit;

namespace GreenhouseMonitoring.Tests.Controllers;

[Collection("SQLite")]
public class EventsControllerTests : ControllerTestBase
{
    private EventsController Create(ApplicationDbContext ctx) => new EventsController(ctx);

  

    [Fact]
    public async Task Create_Post_Invalid_ReturnsView()
    {
        var c = Create(CreateDbContext());
        c.ModelState.AddModelError("Description", "Описание обязательно");
        var result = await c.Create(new Event());
        Assert.IsType<ViewResult>(result);
    }
}