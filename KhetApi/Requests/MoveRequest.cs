using KhetApi.Models;
using KhetApi.Models.Board;

namespace KhetApi.Requests;

public class MoveRequest
{
    public Player Player { get; set; }
    public BoardModel Board { get; set; }
    public Position CurrentPosition { get; set; }
    public Position NewPosition { get; set; }
}
