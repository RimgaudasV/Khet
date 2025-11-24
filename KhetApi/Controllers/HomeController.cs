using Microsoft.AspNetCore.Mvc;

namespace Khet.Controllers;

public class HomeController : ControllerBase
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        return Ok("Khet API is running.");
    }


}
