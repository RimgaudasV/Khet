using KhetApi.Models.Piece;

namespace KhetApi.Models.Board;

public class Cell
{
    public PieceModel? Piece { get; set; }
    public bool IsDisabled { get; set; }
    public Player? DisabledFor { get; set; }
}
