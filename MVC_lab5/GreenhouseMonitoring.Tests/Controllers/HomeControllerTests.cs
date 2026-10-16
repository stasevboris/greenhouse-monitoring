using GreenhouseMonitoring.Controllers;
using GreenhouseMonitoring.Services;
using GreenhouseMonitoring.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GreenhouseMonitoring.Tests.Controllers;

public class HomeControllerTests
{
    private HomeController CreateController(IHomeService? service = null)
    {
        var mockService = service ?? Mock.Of<IHomeService>(s =>
            s.GetHomeViewModel() == new HomeViewModel
            {
                GreenhouseCount = 5,
                SensorCount = 42,
                CriticalEvents = 3,
                PlannedActions = 7
            });

        return new HomeController(mockService);
    }

    [Fact]
    public void Index_ReturnsViewWithHomeViewModel()
    {
        var controller = CreateController();
        var result = controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<HomeViewModel>(viewResult.Model);
        Assert.Equal(5, model.GreenhouseCount);
        Assert.Equal(42, model.SensorCount);
    }

    [Fact]
    public void Privacy_AllowAnonymous_ReturnsView()
    {
        var controller = CreateController();
        var result = controller.Privacy();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_AllowAnonymous_ReturnsView()
    {
        var controller = CreateController();
        var result = controller.Error();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Index_UsesHomeService()
    {
        var mockService = new Mock<IHomeService>();
        mockService.Setup(s => s.GetHomeViewModel())
            .Returns(new HomeViewModel { GreenhouseCount = 999 });

        var controller = CreateController(mockService.Object);
        var result = controller.Index();

        var model = Assert.IsType<HomeViewModel>(((ViewResult)result).Model);
        Assert.Equal(999, model.GreenhouseCount);

        mockService.Verify(s => s.GetHomeViewModel(), Times.Once);
    }
}