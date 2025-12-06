using KhetApi.Entities.Game;
using KhetApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KhetApi.Controllers;

[ApiController]
[Route("game")]
public class GameController(IGameService gameService) : ControllerBase
{
    
    [HttpGet("startGame")]
    public GameEntity StartGame()
    {
        return gameService.StartGame();
    }

    //[HttpPost("/makeMove")]
    //public MoveResponse Move(MoveRequest request)
    //{

    //}

}
