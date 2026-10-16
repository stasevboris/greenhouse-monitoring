using GreenhouseMonitoring.Controllers;
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GreenhouseMonitoring.Tests.Controllers;

[Collection("SQLite")]
public class NormativeParametersControllerTests : ControllerTestBase
{
    private NormativeParametersController Create(ApplicationDbContext ctx) => new NormativeParametersController(ctx);

    [Fact] public async Task Edit_Get_InvalidId_ReturnsNotFound() { var c = Create(CreateDbContext()); var r = await c.Edit(999); Assert.IsType<NotFoundResult>(r); }
}