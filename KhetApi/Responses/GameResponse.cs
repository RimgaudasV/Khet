using KhetApi.Entities;
using KhetApi.Entities.Board;

namespace KhetApi.Responses;
public class GameResponse
{
    public BoardEntity Board { get; set; }
    public Player CurrentPlayer { get; set; }
    public bool GameEnded { get; set; } = false;
    public List<Position> Laser { get; internal set; }
}
