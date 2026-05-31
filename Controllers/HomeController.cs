using Microsoft.AspNetCore.Mvc;

namespace WallpaperApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
