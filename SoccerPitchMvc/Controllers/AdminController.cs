using Microsoft.AspNetCore.Mvc;

namespace SoccerPitchMvc.Controllers;

public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
