using KhetApi.Entities.Board;
using KhetApi.Entities.Game;
using KhetApi.Entities.Move;
using KhetApi.Responses;

namespace KhetApi.Interfaces;

public interface IGameService
{
    public GameEntity StartGame();
    public MoveResponse MakeMove(MoveEntity move);
}
