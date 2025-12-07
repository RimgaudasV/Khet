using KhetApi.Interfaces;
using KhetApi.Requests;
using KhetApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace KhetApi.Controllers;

[ApiController]
[Route("game")]
public class GameController(IGameService gameService) : ControllerBase
{
    
    [HttpGet("startGame")]
    public GameResponse StartGame()
    {
        return gameService.StartGame();
    }

    [HttpPost("makeMove")]
    public GameResponse Move(MoveRequest request)
    {
        return gameService.MakeMove(request);
    }

    [HttpPost("validMoves")]
    public ValidMovesResponse GetValidMoves([FromBody] ValidMoveRequest request)
    {
        return gameService.GetValidMoves(request);
    }

}
