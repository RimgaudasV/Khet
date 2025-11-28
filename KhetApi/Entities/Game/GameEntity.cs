using KhetApi.Entities.Board;

namespace KhetApi.Entities.Game;
public class GameEntity
{
    public int Id { get; set; }
    public BoardEntity Board { get; set; }
    public GameEntity()
    {
        Board = new BoardEntity();
    }
}
