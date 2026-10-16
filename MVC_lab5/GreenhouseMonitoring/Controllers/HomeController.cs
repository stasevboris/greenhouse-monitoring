using GreenhouseMonitoring.Infrastructure.Filters;
using GreenhouseMonitoring.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenhouseMonitoring.Controllers;

[ExceptionFilter]
[TypeFilter(typeof(TimingLogAttribute))]
public class HomeController(IHomeService homeService) : Controller
{
    private readonly IHomeService _homeService = homeService;

    // Доступ только авторизованным пользователям
    [ResponseCache(Duration = 274)]
    public IActionResult Index()
    {
        var viewModel = _homeService.GetHomeViewModel();
        return View(viewModel);
    }

    // Доступен всем (анонимно)
    [AllowAnonymous]
    public IActionResult Privacy() => View();

    // Доступен всем (анонимно)
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}