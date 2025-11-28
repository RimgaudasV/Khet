using KhetApi.Entities;
using KhetApi.Entities.Board;

namespace KhetApi.Entities.Piece;
public class PieceEntity
{
    public PieceIdentifier Id { get; set; }
    public PieceType Type { get; set; }
    public Player Owner { get; set; }
    public BoardPlacement Position { get; set; }
    public int? Rotation { get; set; } 
}
