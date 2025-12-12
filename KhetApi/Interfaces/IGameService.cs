using KhetApi.Models;
using KhetApi.Models.Board;
using KhetApi.Models.Move;
using KhetApi.Requests;
using KhetApi.Responses;

namespace KhetApi.Interfaces;

public interface IGameService
{
    GameResponse StartGame();
    ImpactResultModel MakeMove(MoveRequest request);
    ImpactResultModel Rotate(RotationRequest request);
    ValidMovesResponse GetValidMoves(BoardModel board, Player player, Position position);
    Player GetNextPlayer(Player player);
    GameResponse MoveByAgent(AgentMoveRequest request);
}
