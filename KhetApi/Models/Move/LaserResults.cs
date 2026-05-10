using KhetApi.Models.Board;
using KhetApi.Models.Piece;

namespace KhetApi.Models.Move;

public record LaserOrigin(Position Pos, LaserDirection Dir);

public record LaserTraceResult(List<Position> Path, PieceModel? HitPiece, Position? HitPos, LaserDirection HitDir);

public record LaserHitResult(DestroyedPiece? Destroyed, bool GameOver);
