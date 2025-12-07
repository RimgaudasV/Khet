using KhetApi.Requests;
using KhetApi.Responses;

namespace KhetApi.Interfaces;

public interface IGameService
{
    GameResponse StartGame();
    GameResponse MakeMove(MoveRequest request);
    GameResponse Rotate(RotationRequest request);
    ValidMovesResponse GetValidMoves(ValidMoveRequest request);
}
