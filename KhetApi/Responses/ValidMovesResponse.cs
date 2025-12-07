using KhetApi.Models.Board;
using KhetApi.Models.Piece;

namespace KhetApi.Responses;

public class ValidMovesResponse
{
    public List<Position> ValidPositions { get; set; } = new();
    public List<Rotation> ValidRotations { get; set; } = new();

}