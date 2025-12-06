using KhetApi.Entities.Board;
using KhetApi.Entities.Game;
using KhetApi.Entities.Move;
using KhetApi.Entities.Piece;
using KhetApi.Interfaces;
using KhetApi.Responses;

namespace KhetApi.Services;

public class GameService : IGameService
{
    public GameEntity StartGame()
    {
        var game = new GameEntity();
        return game;
    }

    public MoveResponse MakeMove(MoveEntity move)
    {
        throw new NotImplementedException();
    }
}

