using KhetApi.Entities;
using KhetApi.Entities.Board;
using KhetApi.Entities.Piece;

namespace KhetApi.Requests;

public class MoveRequest
{
    public Player Player { get; set; }
    public BoardEntity Board { get; set; }
    public Position CurrentPosition { get; set; }
    public Rotation? NewRotation { get; set; }
    public Position? NewPosition { get; set; }
}
