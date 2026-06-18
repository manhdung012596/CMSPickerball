using Microsoft.AspNetCore.Mvc;

namespace SoccerPitchMvc.Controllers;

public class CustomerController : Controller
{
    public IActionResult Profile()
    {
        return View();
    }
}
