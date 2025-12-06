using KhetApi.Entities.Board;
using KhetApi.Entities.Piece;

namespace KhetApi.Entities.Move;

public class MoveEntity
{
    public Player Player { get; set; }
    public Position? Position { get; set; }
    public int? NewRotation { get; set; }
}
