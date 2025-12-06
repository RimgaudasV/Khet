using KhetApi.Entities.Board;
using KhetApi.Entities.Move;
using KhetApi.Requests;
using KhetApi.Responses;

namespace KhetApi.Interfaces;

public interface IGameService
{
    public GameResponse StartGame();
    public GameResponse MakeMove(MoveRequest request);
}
