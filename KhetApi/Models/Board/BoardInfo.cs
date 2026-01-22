using KhetApi.Models.Piece;

namespace KhetApi.Models.Board;

public class BoardInfo
{
    public List<(PieceModel piece, Position pos)> Pieces { get; init; } = new();
    public Position PharaohPosition { get; init; }
}