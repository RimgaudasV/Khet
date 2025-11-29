using Microsoft.AspNetCore.Mvc;

namespace KhetApi.Controllers;

public class HomeController : ControllerBase
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        return Ok("Khet API is running.");
    }
}
