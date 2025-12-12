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

    [HttpPost("move")]
    public GameResponse Move(MoveRequest request)
    {
        var results =  gameService.MakeMove(request);
        return new GameResponse
        {
            Board = results.Board,
            Laser = results.LaserPath,
            CurrentPlayer = results.NextPlayer,
            GameEnded = results.GameOver
        };
    }
    [HttpPost("rotate")]
    public GameResponse Rotate(RotationRequest request)
    {
        var results = gameService.Rotate(request);
        return new GameResponse
        {
            Board = results.Board,
            Laser = results.LaserPath,
            CurrentPlayer = results.NextPlayer,
            GameEnded = results.GameOver
        };
    }

    [HttpPost("validMoves")]
    public ValidMovesResponse GetValidMoves([FromBody] ValidMoveRequest request)
    {
        return gameService.GetValidMoves(request.Board, request.Player, request.CurrentPosition);
    }

    [HttpPost("moveByAgent")]

    public GameResponse MoveByAgent([FromBody] AgentMoveRequest request)
    {
        return gameService.MoveByAgent(request);
    }

}
