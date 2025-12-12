using KhetApi.Models.Board;
using KhetApi.Models.Piece;

namespace KhetApi.Models;

public class AlphaBetaResultModel
{
    public int Score { get; set; }
    public Position? MoveFrom { get; set; }
    public Position? MoveTo { get; set; }
    public Rotation? Rotation { get; set; }
}
