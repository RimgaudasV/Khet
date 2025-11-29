using KhetApi.Entities;
using KhetApi.Entities.Board;
using KhetApi.Entities.Piece;

namespace KhetApi.Requests;

public class MoveRequest
{
    public Rotation Rotation { get; set; }
    public BoardPlacement? OldPosition {get; set;}
    public BoardPlacement? NewPosition { get; set; }
}
