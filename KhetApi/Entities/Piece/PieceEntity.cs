using KhetApi.Entities.Board;

namespace KhetApi.Entities.Piece;
public class PieceEntity
{
    public PieceType Type { get; set; }
    public List<Rotation>? PossibleRotations { get; set; } = new List<Rotation>();
    public Rotation Rotation { get; set; }
    public Player Owner { get; set; }
}
