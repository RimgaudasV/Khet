using KhetApi.Entities;
using KhetApi.Entities.Board;
using KhetApi.Entities.Piece;

namespace KhetApi.Responses;

public class MoveResponse
{
    public BoardEntity Board { get; set; }
    public PieceEntity? DefeatedPiece { get; set; }
    public Player NextPlayer { get; set; }
}

