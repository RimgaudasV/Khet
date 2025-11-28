using KhetApi.Entities;
using KhetApi.Entities.Board;
using KhetApi.Entities.Game;
using KhetApi.Entities.Move;
using KhetApi.Interfaces;
using KhetApi.Requests;
using KhetApi.Responses;
using KhetApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace KhetApi.Controllers;

[ApiController]
public class GameController(IGameService gameService) : ControllerBase
{
    
    [HttpGet("/startGame")]
    public BoardEntity StartGame()
    {
        return gameService.StartGame();
    }

    [HttpPost("/makeMove")]
    public MoveResponse Move(MoveRequest request)
    {
        var move = new MoveEntity
        {
            Player = request.Player,
            PieceId = request.PieceId,

        };

        var result = gameService.MakeMove(move);

        return result;
    }

}
