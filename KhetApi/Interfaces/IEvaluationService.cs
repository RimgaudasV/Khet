using KhetApi.Models;
using KhetApi.Models.Board;

namespace KhetApi.Interfaces
{
    public interface IEvaluationService
    {
        double EvaluateBoard(BoardModel board, bool gameOver, int depth, Player? winner, Player rootPlayer, int maxDepth, EvaluationConfig evalConfig);
    }
}
