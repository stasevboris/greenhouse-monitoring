using GreenhouseMonitoring.Controllers;
using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity; // ← ЭТОТ using ДОБАВЬ!
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;
using GreenhouseMonitoring.ViewModels;
namespace GreenhouseMonitoring.Tests.Controllers;
[Collection("SQLite")]
public class AccountControllerTests
{
    private AccountController CreateController(
        Mock<UserManager<IdentityUser>>? userManager = null,
        Mock<SignInManager<IdentityUser>>? signInManager = null)
    {
        userManager ??= new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var userClaimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        signInManager ??= new Mock<SignInManager<IdentityUser>>(
            userManager.Object,
            httpContextAccessor.Object,
            userClaimsFactory.Object,
            null!, null!, null!, null!);
        userManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        signInManager.Setup(x => x.PasswordSignInAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        signInManager.Setup(x => x.SignInAsync(It.IsAny<IdentityUser>(), It.IsAny<bool>(), null))
            .Returns(Task.CompletedTask);
        return new AccountController(userManager.Object, signInManager.Object);
    }
    [Fact] public void Login_Get_ReturnsViewResult() { var c = CreateController(); Assert.IsType<ViewResult>(c.Login()); }
    [Fact]
    public async Task Login_Post_ValidCredentials_RedirectsToHome()
    {
        var c = CreateController();
        var model = new LoginViewModel { Email = "test@example.com", Password = "Pass123!" };
        var result = await c.Login(model);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }
    [Fact] public void Register_Get_ReturnsViewResult() { var c = CreateController(); Assert.IsType<ViewResult>(c.Register()); }
    [Fact]
    public async Task Register_Post_Valid_RedirectsToHome()
    {
        var c = CreateController();
        var model = new RegisterViewModel { Email = "new@example.com", Password = "Strong123!" };
        var result = await c.Register(model);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }
    [Fact]
    public async Task Register_Post_Invalid_ReturnsView()
    {
        var c = CreateController();
        c.ModelState.AddModelError("Email", "Ошибка");
        var result = await c.Register(new RegisterViewModel());
        Assert.IsType<ViewResult>(result);
    }
    [Fact]
    public async Task Logout_RedirectsToHome()
    {
        var c = CreateController();
        var result = await c.Logout();
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }
}