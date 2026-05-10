using KhetApi.Models.Piece;

namespace KhetApi.Models.Board;

public class BoardInfo
{
    public List<(PieceModel piece, Position pos)> Pieces { get; init; } = new();
    public Position? Player1PharaohPos { get; init; }
    public Position? Player2PharaohPos { get; init; }
    public Dictionary<int, List<(PieceModel piece, Position pos)>> ByRow { get; init; } = new();
    public Dictionary<int, List<(PieceModel piece, Position pos)>> ByCol { get; init; } = new();
}