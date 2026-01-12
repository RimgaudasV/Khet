using KhetApi.Models.Board;
using KhetApi.Models.Piece;

namespace KhetApi.Models.Move;

public record ImpactResultModel(BoardModel Board, List<Position> LaserPath, bool GameOver, Player NextPlayer, DestroyedPiece? DestroyedPiece);
