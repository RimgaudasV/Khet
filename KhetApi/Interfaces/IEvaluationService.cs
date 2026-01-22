using KhetApi.Models;
using KhetApi.Models.Board;

namespace KhetApi.Interfaces
{
    public interface IEvaluationService
    {
        int EvaluateBoard(BoardModel board, bool gameOver, int depth, Player? winner, Player rootPlayer, int maxDepth);
    }
}
