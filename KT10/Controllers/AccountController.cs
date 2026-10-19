using KT10.Models;
using Microsoft.AspNetCore.Mvc;
using KT10.Services;

namespace KT10.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _users;

    public AccountController(IUserService users) => _users = users;

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _users.CreateAsync(model.Username, model.Email, model.Password);

        switch (result.Status)
        {
            case UserOpStatus.UsernameTaken:
                ModelState.AddModelError(nameof(model.Username), "это имя пользователя уже занято.");
                return View(model);

            case UserOpStatus.EmailTaken:
                ModelState.AddModelError(nameof(model.Email), "пользователь с таким email уже зарегистрирован.");
                return View(model);
        }

        TempData["Success"] = $"регистрация прошла успешно. Добро пожаловать, {model.Username}";
        return RedirectToAction(nameof(Register));
    }
}