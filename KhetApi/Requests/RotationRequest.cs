using KhetApi.Models;
using KhetApi.Models.Board;
using KhetApi.Models.Piece;

namespace KhetApi.Requests;

public class RotationRequest
{
    public Player Player { get; set; }
    public BoardModel Board { get; set; }
    public Position CurrentPosition { get; set; }
    public Rotation NewRotation { get; set; }
}