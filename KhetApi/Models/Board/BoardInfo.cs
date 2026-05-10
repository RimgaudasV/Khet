using KhetApi.Models.Piece;

namespace KhetApi.Models.Board;

public class BoardInfo
{
    public List<(PieceModel piece, Position pos)> Pieces { get; init; } = new();
    public Position? Player1PharaohPos { get; init; }
    public Position? Player2PharaohPos { get; init; }
}