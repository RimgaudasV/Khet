using KhetApi.Models.Board;

namespace KhetApi.Models.Piece;

public class DestroyedPiece
{
    public PieceType Type { get; set; }
    public Player Owner { get; set; }
    public Position Position { get; set; }
    public Rotation Rotation { get; set; }
}
