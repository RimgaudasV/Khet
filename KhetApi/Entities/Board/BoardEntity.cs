using KhetApi.Entities.Piece;

namespace KhetApi.Entities.Board;

public class BoardEntity
{
    public const int Width = 10;
    public const int Height = 8;

    public PieceEntity?[][] Pieces { get; set; }

    public BoardEntity()
    {
        Pieces = new PieceEntity?[Height][];
        for (int y = 0; y < Height; y++)
            Pieces[y] = new PieceEntity?[Width];

        InitiateBoard();
    }

    private void InitiateBoard()
    {
        // Sphinx
        SetPiece(0, 0, PieceType.Sphinx, Player.Player1, Rotation.Down, false);
        SetPiece(9, 7, PieceType.Sphinx, Player.Player2, Rotation.Up, false);

        // Pharaoh
        SetPiece(5, 0, PieceType.Pharaoh, Player.Player1, Rotation.Down, true);
        SetPiece(4, 7, PieceType.Pharaoh, Player.Player2, Rotation.Up, true);

        // Anubis
        SetPiece(4, 0, PieceType.Anubis, Player.Player1, Rotation.Down, true);
        SetPiece(6, 0, PieceType.Anubis, Player.Player1, Rotation.Down, true);
        
        SetPiece(3, 7, PieceType.Anubis, Player.Player2, Rotation.Up, true);
        SetPiece(5, 7, PieceType.Anubis, Player.Player2, Rotation.Up, true);

        // Pyramids
        SetPiece(7, 0, PieceType.Pyramid, Player.Player1, Rotation.RightDown, true);
        SetPiece(2, 1, PieceType.Pyramid, Player.Player1, Rotation.LeftDown, true);
        SetPiece(0, 3, PieceType.Pyramid, Player.Player1, Rotation.RightUp, true);
        SetPiece(0, 4, PieceType.Pyramid, Player.Player1, Rotation.RightDown, true);
        SetPiece(7, 3, PieceType.Pyramid, Player.Player1, Rotation.RightDown, true);
        SetPiece(7, 4, PieceType.Pyramid, Player.Player2, Rotation.RightUp, true);
        SetPiece(6, 5, PieceType.Pyramid, Player.Player2, Rotation.RightDown, true);

        SetPiece(2, 7, PieceType.Pyramid, Player.Player2, Rotation.LeftUp, true);
        SetPiece(7, 6, PieceType.Pyramid, Player.Player2, Rotation.RightUp, true);
        SetPiece(9, 4, PieceType.Pyramid, Player.Player2, Rotation.LeftDown, true);
        SetPiece(9, 3, PieceType.Pyramid, Player.Player2, Rotation.LeftUp, true);
        SetPiece(2, 4, PieceType.Pyramid, Player.Player2, Rotation.LeftUp, true);
        SetPiece(2, 3, PieceType.Pyramid, Player.Player2, Rotation.LeftDown, true);
        SetPiece(3, 2, PieceType.Pyramid, Player.Player2, Rotation.LeftUp, true);

        // Scarabs
        SetPiece(4, 3, PieceType.Scarab, Player.Player1, Rotation.RightUp, true);
        SetPiece(5, 3, PieceType.Scarab, Player.Player1, Rotation.LeftUp, true);
        
        SetPiece(4, 4, PieceType.Scarab, Player.Player2, Rotation.LeftUp, true);
        SetPiece(5, 4, PieceType.Scarab, Player.Player2, Rotation.RightUp, true);
    }

    private void SetPiece(int x, int y, PieceType type, Player owner, Rotation rot, bool isMovable)
    {
        Pieces[y][x] = new PieceEntity
        {
            Type = type,
            Owner = owner,
            Rotation = rot,
            IsMovable = isMovable
        };
    }

    public PieceEntity GetPieceAt(Position currentPosition)
    {
        return Pieces[currentPosition.Y][currentPosition.X]!;
    }

    public bool IsInsideBoard(Position currentPosition)
    {
        return currentPosition.X >= 0 && currentPosition.X < Width &&
               currentPosition.Y >= 0 && currentPosition.Y < Height;
    }

    public void RemovePiece(Position currentPosition)
    {
        Pieces[currentPosition.Y][currentPosition.X] = null;
    }
}
