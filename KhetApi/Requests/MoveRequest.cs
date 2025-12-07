using KhetApi.Models.Board;
using KhetApi.Models.Piece;
using KhetApi.Models.Player;

namespace KhetApi.Requests;

public class MoveRequest
{
    public Player Player { get; set; }
    public BoardModel Board { get; set; }
    public Position CurrentPosition { get; set; }
    public Rotation? NewRotation { get; set; }
    public Position? NewPosition { get; set; }
}
