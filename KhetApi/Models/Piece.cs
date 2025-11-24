namespace Khet.Models;

public class Piece
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Owner { get; set; }
    public int? Rotation { get; set; } 
}
