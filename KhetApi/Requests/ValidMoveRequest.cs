using KhetApi.Entities;
using KhetApi.Entities.Board;

namespace KhetApi.Requests;

public class ValidMoveRequest
{
    public Position CurrentPosition { get; set; }
    public Player Player { get; set; }
    public BoardEntity Board { get; set; }
}
