using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SoccerPitchMvc.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
