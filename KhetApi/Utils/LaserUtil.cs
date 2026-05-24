using KhetApi.Mappers;
using KhetApi.Models;
using KhetApi.Models.Board;
using KhetApi.Models.Move;
using KhetApi.Models.Piece;
using KhetApi.Services;

namespace KhetApi.Utils;

public static class LaserUtil
{
    public static LaserDirection? Reflect(LaserDirection dir, PieceModel piece) =>
        (dir, piece.Rotation, piece.Type) switch
        {
            // Pyramid
            (LaserDirection.Up,    Rotation.LeftDown,  PieceType.Pyramid) => LaserDirection.Left,
            (LaserDirection.Up,    Rotation.RightDown,  PieceType.Pyramid) => LaserDirection.Right,
            (LaserDirection.Down,  Rotation.LeftUp,    PieceType.Pyramid) => LaserDirection.Left,
            (LaserDirection.Down,  Rotation.RightUp,   PieceType.Pyramid) => LaserDirection.Right,
            (LaserDirection.Left,  Rotation.RightDown, PieceType.Pyramid) => LaserDirection.Down,
            (LaserDirection.Left,  Rotation.RightUp,   PieceType.Pyramid) => LaserDirection.Up,
            (LaserDirection.Right, Rotation.LeftDown,  PieceType.Pyramid) => LaserDirection.Down,
            (LaserDirection.Right, Rotation.LeftUp,    PieceType.Pyramid) => LaserDirection.Up,

            // Scarab (always reflects)
            (LaserDirection.Up,    Rotation.RightUp, PieceType.Scarab) => LaserDirection.Left,
            (LaserDirection.Left,  Rotation.RightUp, PieceType.Scarab) => LaserDirection.Up,
            (LaserDirection.Right, Rotation.RightUp, PieceType.Scarab) => LaserDirection.Down,
            (LaserDirection.Down,  Rotation.RightUp, PieceType.Scarab) => LaserDirection.Right,
            (LaserDirection.Up,    Rotation.LeftUp,  PieceType.Scarab) => LaserDirection.Right,
            (LaserDirection.Right, Rotation.LeftUp,  PieceType.Scarab) => LaserDirection.Up,
            (LaserDirection.Left,  Rotation.LeftUp,  PieceType.Scarab) => LaserDirection.Down,
            (LaserDirection.Down,  Rotation.LeftUp,  PieceType.Scarab) => LaserDirection.Left,

            _ => null
        };

    public static int TraceLength(BoardModel board, Player player)
    {
        var pos = player == Player.Player1 ? new Position(9, 7) : new Position(0, 0);
        var dir = RotationMapper.ToLaserDirection(board.GetPieceAt(pos)!.Rotation);
        int cells = 0;

        while (true)
        {
            pos = Step(pos, dir);
            if (!board.IsInsideBoard(pos)) break;
            cells++;

            var piece = board.Cells[pos.Y][pos.X].Piece;
            if (piece == null) continue;

            var reflected = Reflect(dir, piece);
            if (reflected == null) break;
            dir = reflected.Value;
        }

        return cells;
    }

    public static (int dx, int dy) DirectionToDeltas(LaserDirection dir) => dir switch
    {
        LaserDirection.Up    => ( 0, -1),
        LaserDirection.Down  => ( 0,  1),
        LaserDirection.Left  => (-1,  0),
        LaserDirection.Right => ( 1,  0),
        _                    => ( 0,  0)
    };

    private static Position Step(Position pos, LaserDirection dir)
    {
        var (dx, dy) = DirectionToDeltas(dir);
        return new Position(pos.X + dx, pos.Y + dy);
    }


    public static bool WouldKill(LaserDirection dir, PieceModel piece) =>
        Reflect(dir, piece) == null && LaserPieceInteraction(dir, piece).DestroyPiece;

    public static ImpactResult LaserPieceInteraction(LaserDirection laserDir, PieceModel piece)
    {
        return piece.Type switch
        {
            PieceType.Sphinx => new ImpactResult(null, false, false),
            PieceType.Pharaoh => new ImpactResult(null, true, true),
            PieceType.Anubis when IsAnubisShielded(laserDir, piece.Rotation) => new ImpactResult(null, false, false),
            _ => new ImpactResult(null, true, false)
        };
    }

    private static bool IsAnubisShielded(LaserDirection dir, Rotation rot) =>
        (dir, rot) is
            (LaserDirection.Down, Rotation.Up) or
            (LaserDirection.Up, Rotation.Down) or
            (LaserDirection.Left, Rotation.Right) or
            (LaserDirection.Right, Rotation.Left);

    public static LaserOrigin GetLaserStart(BoardModel board, Player player)
    {
        var pos = player == Player.Player1 ? new Position(9, 7) : new Position(0, 0);
        var dir = RotationMapper.ToLaserDirection(board.GetPieceAt(pos)!.Rotation);
        return new LaserOrigin(pos, dir);
    }

    public static LaserTraceResult TraceLaserPath(BoardModel board, Position pos, LaserDirection dir)
    {
        var path = new List<Position> { pos };

        while (true)
        {
            pos = Step(pos, dir);
            if (!board.IsInsideBoard(pos)) break;

            path.Add(pos);

            var cell = board.Cells[pos.Y][pos.X];
            if (cell.IsDisabled && cell.Piece == null) continue;

            var piece = cell.Piece;
            if (piece == null) continue;

            var reflected = Reflect(dir, piece);
            if (reflected == null) return new LaserTraceResult(path, piece, pos, dir);
            dir = reflected.Value;
        }

        return new LaserTraceResult(path, null, null, dir);
    }
}
