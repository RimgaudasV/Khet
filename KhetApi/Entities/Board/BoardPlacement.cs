namespace KhetApi.Entities.Board;


public class BoardPlacement
{
    public int X { get; set; }
    public int Y { get; set; }
    public BoardPlacement(int x, int y)
    {
        X = x;
        Y = y;
    }
}
