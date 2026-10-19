using Microsoft.AspNetCore.Mvc;

namespace KT10.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Privacy() => View();
}