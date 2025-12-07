using KhetApi.Models.Board;
using KhetApi.Models.Player;

namespace KhetApi.Responses;
public class GameResponse
{
    public BoardModel Board { get; set; }
    public Player CurrentPlayer { get; set; }
    public bool GameEnded { get; set; } = false;
    public List<Position> Laser { get; internal set; }
}
