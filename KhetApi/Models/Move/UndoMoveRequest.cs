using KhetApi.Models.Board;
using KhetApi.Models.Piece;

namespace KhetApi.Models.Move;
struct UndoMoveRequest
{
    public Position From;
    public Position To;
    public PieceModel? CapturedPiece;
    public Rotation? OldRotation;
}
