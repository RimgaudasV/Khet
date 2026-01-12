using KhetApi.Models.Piece;

namespace KhetApi.Mappers;

public static class RotationMapper
{
    public static LaserDirection ToLaserDirection(Rotation rotation) =>
    rotation switch
    {
        Rotation.Up => LaserDirection.Up,
        Rotation.Down => LaserDirection.Down,
        Rotation.Right => LaserDirection.Right,
        Rotation.Left => LaserDirection.Left,
        _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null)
    };
}
