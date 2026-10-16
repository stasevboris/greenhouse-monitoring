using GreenhouseMonitoring.Controllers;
using GreenhouseMonitoring.Data;
using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ActionEntity = GreenhouseMonitoring.Models.Action;
namespace GreenhouseMonitoring.Tests.Controllers;
[Collection("SQLite")]
public class AdminControllerTests
{
    private AdminController CreateController(
        UserManager<IdentityUser>? userManager = null,
        RoleManager<IdentityRole>? roleManager = null)
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        userManager ??= new Mock<UserManager<IdentityUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!).Object;
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        roleManager ??= new Mock<RoleManager<IdentityRole>>(roleStore.Object, null!, null!, null!, null!).Object;
        // По умолчанию возвращаем список пользователей
        Mock.Get(userManager).Setup(x => x.Users)
            .Returns(new List<IdentityUser>
            {
                new IdentityUser { Id = "1", UserName = "admin@example.com", Email = "admin@example.com" },
                new IdentityUser { Id = "2", UserName = "user@example.com", Email = "user@example.com" }
            }.AsQueryable());
        Mock.Get(userManager).Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync((IdentityUser u) => u.Email == "admin@example.com" ? new[] { "Administrator" } : new[] { "User" });
        return new AdminController(userManager, roleManager);
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Email", "Ошибка");
        var model = new RegisterViewModel { Email = "", Password = "" };
        var result = await controller.Create(model);
        Assert.IsType<ViewResult>(result);
    }
    [Fact]
    public async Task Edit_Get_ValidId_ReturnsViewWithUser()
    {
        var userManager = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        var user = new IdentityUser { Id = "123", Email = "test@example.com" };
        userManager.Setup(x => x.FindByIdAsync("123")).ReturnsAsync(user);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new[] { "User" });
        var controller = CreateController(userManager: userManager.Object);
        var result = await controller.Edit("123");
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EditUserViewModel>(viewResult.Model);
        Assert.Equal("test@example.com", model.Email);
    }
    [Fact]
    public async Task Edit_Get_InvalidId_ReturnsNotFound()
    {
        var controller = CreateController();
        var result = await controller.Edit("999");
        Assert.IsType<NotFoundResult>(result);
    }
    [Fact]
    public async Task Edit_Post_UserNotFound_ReturnsNotFound()
    {
        var userManager = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        userManager.Setup(x => x.FindByIdAsync("999")).ReturnsAsync((IdentityUser)null!);
        var controller = CreateController(userManager: userManager.Object);
        var model = new EditUserViewModel { Id = "999", Email = "test@example.com", Role = "User" };
        var result = await controller.Edit(model);
        Assert.IsType<NotFoundResult>(result);
    }
    [Fact]
    public async Task Delete_Post_UserNotFound_ReturnsNotFound()
    {
        var userManager = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        userManager.Setup(x => x.FindByIdAsync("999")).ReturnsAsync((IdentityUser)null!);
        var controller = CreateController(userManager: userManager.Object);
        var result = await controller.Delete("999");
        Assert.IsType<NotFoundResult>(result);
    }
}