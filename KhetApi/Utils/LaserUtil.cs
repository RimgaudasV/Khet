using KhetApi.Mappers;
using KhetApi.Models;
using KhetApi.Models.Board;
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

    public static Position Step(Position pos, LaserDirection dir) => dir switch
    {
        LaserDirection.Up    => new Position(pos.X, pos.Y - 1),
        LaserDirection.Down  => new Position(pos.X, pos.Y + 1),
        LaserDirection.Left  => new Position(pos.X - 1, pos.Y),
        LaserDirection.Right => new Position(pos.X + 1, pos.Y),
        _                    => pos
    };


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


}
