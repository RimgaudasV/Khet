using KhetApi.Models.Board;

namespace KhetApi.Models.Move;

public class MoveModel
{
    public Player Player { get; set; }
    public Position? Position { get; set; }
    public int? NewRotation { get; set; }
}
