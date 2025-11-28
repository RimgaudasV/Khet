using KhetApi.Entities.Game;
using KhetApi.Entities.Piece;

namespace KhetApi.Entities.Board;

public class BoardEntity
{
   public List<List<PieceEntity?>> Pieces { get; set; } = new();
}
