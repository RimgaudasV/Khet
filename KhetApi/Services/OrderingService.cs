using KhetApi.Interfaces;
using KhetApi.Models.Board;
using KhetApi.Models.Move;
using KhetApi.Models.Piece;

namespace KhetApi.Services;

public class OrderingService : IOrderingService
{
    private int[,,,] _moveHistory = new int[10, 8, 10, 8]; // [fromX, fromY, toX, toY]
    private int[,,]  _rotHistory  = new int[10, 8, 9];     // [fromX, fromY, rotationIndex]

    public void ResetHistory()
    {
        Array.Clear(_moveHistory, 0, _moveHistory.Length);
        Array.Clear(_rotHistory,  0, _rotHistory.Length);
    }

    public void OrderMoves(List<Move> moves, BoardModel board, Move?[,] killers, int depth)
    {
        var orderedMoves = moves
            .Select(move => new OrderedMove
            {
                Move          = move,
                KillerScore   = ComputeKillerScore(move, killers, depth),
                HistoryScore  = ComputeHistoryScore(move),
                TacticalScore = ComputeTacticalScore(board, move)
            })
            .OrderByDescending(om => om.FinalScore)
            .Select(om => om.Move)
            .ToList();

        moves.Clear();
        moves.AddRange(orderedMoves);
    }

    public void RecordCutoff(Move move, int depth, Move?[,] killers)
    {
        StoreKiller(killers, depth, move);
        StoreHistory(move, depth);
    }

    private int ComputeKillerScore(Move move, Move?[,] killers, int depth)
    {
        var primaryKiller   = killers[depth, 0];
        var secondaryKiller = killers[depth, 1];

        if (primaryKiller.HasValue   && MovesAreEqual(move, primaryKiller.Value))   return 2;
        if (secondaryKiller.HasValue && MovesAreEqual(move, secondaryKiller.Value)) return 1;

        return 0;
    }

    private int ComputeHistoryScore(Move move) =>
        move.Rotation.HasValue
            ? _rotHistory[move.From.X, move.From.Y, (int)move.Rotation.Value]
            : _moveHistory[move.From.X, move.From.Y, move.To.X, move.To.Y];

    private static int ComputeTacticalScore(BoardModel board, Move move)
    {
        var piece = board.GetPieceAt(move.From);

        if (piece == null)
            return 0;

        bool isScarabSwap =
            piece.Type == PieceType.Scarab &&
            move.Rotation == null &&
            board.GetPieceAt(move.To) != null;

        if (isScarabSwap)
            return 10;

        bool isPharaohMove =
            piece.Type == PieceType.Pharaoh;

        if (isPharaohMove)
            return -1;

        bool isPyramidRotation =
            move.Rotation != null &&
            piece.Type == PieceType.Pyramid;

        if (isPyramidRotation)
            return 3;

        return piece.Type switch
        {
            PieceType.Anubis => 1,
            PieceType.Pyramid => 2,
            PieceType.Scarab => 2,
            _ => 0
        };
    }

    private static void StoreKiller(Move?[,] killers, int depth, Move move)
    {
        var currentPrimaryKiller = killers[depth, 0];
        if (currentPrimaryKiller.HasValue && MovesAreEqual(currentPrimaryKiller.Value, move))
            return;

        killers[depth, 1] = currentPrimaryKiller;
        killers[depth, 0] = move;
    }

    private void StoreHistory(Move move, int depth)
    {
        int bonus = depth * depth;
        if (move.Rotation.HasValue)
            Interlocked.Add(ref _rotHistory[move.From.X, move.From.Y, (int)move.Rotation.Value], bonus);
        else
            Interlocked.Add(ref _moveHistory[move.From.X, move.From.Y, move.To.X, move.To.Y], bonus);
    }

    private static bool MovesAreEqual(Move a, Move b) =>
        a.From.X == b.From.X && a.From.Y == b.From.Y &&
        a.To.X   == b.To.X   && a.To.Y   == b.To.Y   &&
        a.Rotation == b.Rotation;
}
