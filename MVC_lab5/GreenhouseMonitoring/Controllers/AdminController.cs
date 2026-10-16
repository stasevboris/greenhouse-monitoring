// Controllers/AdminController.cs
using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseMonitoring.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        return View(users);
    }

    public async Task<IActionResult> Create()
    {
        await LoadRolesAsync();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }

                TempData["Success"] = $"Пользователь {model.Email} успешно создан.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        await LoadRolesAsync();
        return View(model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        var currentRole = userRoles.FirstOrDefault() ?? "User";

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email!,
            Role = currentRole
        };

        await LoadRolesAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        if (ModelState.IsValid)
        {
            user.Email = model.Email;
            user.UserName = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            else
            {
                // Удаляем все текущие роли
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                }
                else if (!string.IsNullOrEmpty(model.Role))
                {
                    var addResult = await _userManager.AddToRoleAsync(user, model.Role);
                    if (!addResult.Succeeded)
                    {
                        foreach (var error in addResult.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                if (ModelState.IsValid)
                {
                    TempData["Success"] = "Пользователь обновлён.";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        await LoadRolesAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id)) return BadRequest();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // Нельзя удалить самого себя
        if (user.Email == User.Identity?.Name)
        {
            TempData["Error"] = "Нельзя удалить текущего пользователя!";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = $"Пользователь {user.Email} удалён.";
        }
        else
        {
            TempData["Error"] = "Ошибка при удалении пользователя.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRolesAsync()
    {
        var roles = new[] { "Administrator", "Manager", "User" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        ViewBag.Roles = new SelectList(roles);
    }
}