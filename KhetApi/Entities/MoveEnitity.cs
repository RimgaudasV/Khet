using Khet.Models;

namespace Khet.Entities;

public class MoveEnitity
{
    public Player Player { get; set; }
    public string PieceId { get; set; }
    public int FromRow { get; set; }
    public int FromCol { get; set; }
    public int ToRow { get; set; }
    public int ToCol { get; set; }
    public int? NewRotation { get; set; }
}
