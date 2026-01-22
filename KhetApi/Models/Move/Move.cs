using KhetApi.Models.Board;
using KhetApi.Models.Piece;

namespace KhetApi.Models.Move;

public struct Move
{
    public Position From;
    public Position To;
    public Rotation? Rotation;
}

public sealed class UndoState
{
    public Position From;
    public Position To;
    public PieceModel? Captured;
    public PieceModel? Swapped;
    public Rotation? OldRotation;
    public DestroyedPiece? Destroyed;

}

public class SearchResult
{
    public int Score;
    public Move BestMove;
}


