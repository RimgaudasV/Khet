using KhetApi.Models.Board;

namespace KhetApi.Models.Move;

public record ImpactResultModel(BoardModel Board, List<Position> LaserPath, bool GameOver, Player NextPlayer);
