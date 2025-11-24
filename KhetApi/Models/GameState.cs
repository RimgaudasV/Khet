using Khet.Models;

namespace Khet.Entities;

public class GameState
{
    public int MoveCount { get; set; }
    public Piece[,] Board { get; set; }
}
