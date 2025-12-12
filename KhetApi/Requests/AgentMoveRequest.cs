using KhetApi.Models;
using KhetApi.Models.Board;

namespace KhetApi.Requests;

public class AgentMoveRequest
{
    public BoardModel Board { get; set; }
    public Player Player { get; set; }
}