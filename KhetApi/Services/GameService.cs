using KhetApi.Entities.Board;
using KhetApi.Entities.Game;
using KhetApi.Entities.Move;
using KhetApi.Interfaces;
using KhetApi.Responses;

namespace KhetApi.Services;

public class GameService : IGameService
{
    public BoardEntity StartGame()
    {
        var game = new GameEntity();
        return game.Board;
    }

    public MoveResponse MakeMove(MoveEntity move)
    {
        throw new NotImplementedException();
    }
}

