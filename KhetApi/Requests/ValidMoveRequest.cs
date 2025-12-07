using KhetApi.Models.Board;
using KhetApi.Models.Player;

namespace KhetApi.Requests;

public class ValidMoveRequest
{
    public Position CurrentPosition { get; set; }
    public Player Player { get; set; }
    public BoardModel Board { get; set; }
}
