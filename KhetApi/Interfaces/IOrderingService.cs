using KhetApi.Models.Board;
using KhetApi.Models.Move;

namespace KhetApi.Interfaces;

public interface IOrderingService
{
    /// <summary>Clears accumulated history scores. Call once at the start of each agent search.</summary>
    void ResetHistory();

    /// <summary>
    /// Orders <paramref name="moves"/> in place by final score (killers → history → tactical).
    /// Scores are snapshotted before sorting to ensure comparator consistency under concurrent history writes.
    /// </summary>
    void OrderMoves(List<Move> moves, BoardModel board, Move?[,] killers, int depth);

    /// <summary>
    /// Records a beta-cutoff move: promotes it to the killer slot for <paramref name="depth"/>
    /// and increments its history score by depth².
    /// </summary>
    void RecordCutoff(Move move, int depth, Move?[,] killers);
}
