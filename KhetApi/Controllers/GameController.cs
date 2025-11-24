using Khet.Entities;
using Khet.Services;
using Microsoft.AspNetCore.Mvc;

namespace Khet.Controllers;

[ApiController]
public class GameController : ControllerBase
{
    [HttpGet("/")]
    public GameState Move(MoveEnitity move)
    {
        return GameService.GetState(move);
    }
}
