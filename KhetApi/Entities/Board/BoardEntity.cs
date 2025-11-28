using KhetApi.Entities.Game;
using KhetApi.Entities.Piece;

namespace KhetApi.Entities.Board;

public class BoardEntity
{
    public PieceEntity[,] Pieces { get; set; }
    public BoardEntity()
    {
        Pieces = new PieceEntity[GameConstants.Cols, GameConstants.Rows];
    }
}
