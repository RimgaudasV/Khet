using KhetApi.Entities;
using KhetApi.Entities.Board;
using KhetApi.Entities.Piece;

namespace KhetApi.Requests;

public class MoveRequest
{
    public Player Player { get; set; }
    public PieceIdentifier PieceId { get; set; }
    public BoardPlacement? Position {get; set;}
    public int? NewRotation { get; set; }
}
