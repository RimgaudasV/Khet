using KhetApi.Entities.Board;
using KhetApi.Entities.Move;
using KhetApi.Responses;

namespace KhetApi.Interfaces;

public interface IGameService
{
    public BoardEntity StartGame();
    public MoveResponse MakeMove(MoveEntity move);
}
