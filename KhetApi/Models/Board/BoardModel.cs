using KhetApi.Models.Piece;

namespace KhetApi.Models.Board;

public class BoardModel
{
    public const int Width = 10;
    public const int Height = 8;

    private static readonly HashSet<(int, int, Player)> DisabledSquares =
    new(){
        (1,0,Player.Player1),(9,0,Player.Player1),(9,1,Player.Player1),(9,2,Player.Player1), (9,3,Player.Player1),
        (9,4,Player.Player1), (9,5,Player.Player1),(9,6,Player.Player1), (1,7,Player.Player1),

        (8,0,Player.Player2),(0,1,Player.Player2),(0,2,Player.Player2), (0,3,Player.Player2), (0,4,Player.Player2),
        (0,5,Player.Player2),(0,6,Player.Player2),(0,7,Player.Player2), (8,7,Player.Player2)
    };

    public Cell[][] Cells { get; set; }

    public BoardModel()
    {
        Cells = new Cell[Height][];

        for (int y = 0; y < Height; y++)
        {
            Cells[y] = new Cell[Width];
            for (int x = 0; x < Width; x++)
                Cells[y][x] = new Cell();
        }

        InitiateBoard();
    }


    private void InitiateBoard()
    {
        // Sphinx
        SetPiece(0, 0, PieceType.Sphinx, Player.Player2, Rotation.Down, false);
        SetPiece(9, 7, PieceType.Sphinx, Player.Player1, Rotation.Up, false);

        // Pharaoh
        SetPiece(5, 0, PieceType.Pharaoh, Player.Player2, Rotation.Down, true);
        SetPiece(4, 7, PieceType.Pharaoh, Player.Player1, Rotation.Up, true);

        // Anubis
        SetPiece(4, 0, PieceType.Anubis, Player.Player2, Rotation.Down, true);
        SetPiece(6, 0, PieceType.Anubis, Player.Player2, Rotation.Down, true);

        SetPiece(3, 7, PieceType.Anubis, Player.Player1, Rotation.Up, true);
        SetPiece(5, 7, PieceType.Anubis, Player.Player1, Rotation.Up, true);

        // Pyramids
        SetPiece(7, 0, PieceType.Pyramid, Player.Player2, Rotation.RightDown, true);
        SetPiece(2, 1, PieceType.Pyramid, Player.Player2, Rotation.LeftDown, true);
        SetPiece(0, 3, PieceType.Pyramid, Player.Player2, Rotation.RightUp, true);
        SetPiece(0, 4, PieceType.Pyramid, Player.Player2, Rotation.RightDown, true);
        SetPiece(7, 3, PieceType.Pyramid, Player.Player2, Rotation.RightDown, true);
        SetPiece(7, 4, PieceType.Pyramid, Player.Player2, Rotation.RightUp, true);
        SetPiece(6, 5, PieceType.Pyramid, Player.Player2, Rotation.RightDown, true);

        SetPiece(2, 7, PieceType.Pyramid, Player.Player1, Rotation.LeftUp, true);
        SetPiece(7, 6, PieceType.Pyramid, Player.Player1, Rotation.RightUp, true);
        SetPiece(9, 4, PieceType.Pyramid, Player.Player1, Rotation.LeftDown, true);
        SetPiece(9, 3, PieceType.Pyramid, Player.Player1, Rotation.LeftUp, true);
        SetPiece(2, 4, PieceType.Pyramid, Player.Player1, Rotation.LeftUp, true);
        SetPiece(2, 3, PieceType.Pyramid, Player.Player1, Rotation.LeftDown, true);
        SetPiece(3, 2, PieceType.Pyramid, Player.Player1, Rotation.LeftUp, true);

        // Scarabs
        SetPiece(4, 3, PieceType.Scarab, Player.Player2, Rotation.RightUp, true);
        SetPiece(5, 3, PieceType.Scarab, Player.Player2, Rotation.LeftUp, true);

        SetPiece(4, 4, PieceType.Scarab, Player.Player1, Rotation.LeftUp, true);
        SetPiece(5, 4, PieceType.Scarab, Player.Player1, Rotation.RightUp, true);

        foreach (var (x, y, p) in DisabledSquares)
        {
            Cells[y][x].IsDisabled = true;
            Cells[y][x].DisabledFor = p;
        }

    }

    private void SetPiece(int x, int y, PieceType type, Player owner, Rotation rot, bool isMovable)
    {
        Cells[y][x].Piece = new PieceModel
        {
            Type = type,
            Owner = owner,
            Rotation = rot,
            IsMovable = isMovable
        };
    }


    public PieceModel? GetPieceAt(Position p)
        => Cells[p.Y][p.X].Piece;

    public bool IsInsideBoard(Position currentPosition)
    {
        return currentPosition.X >= 0 && currentPosition.X < Width &&
               currentPosition.Y >= 0 && currentPosition.Y < Height;
    }

    public void RemovePiece(Position pos)
    {
        Cells[pos.Y][pos.X].Piece = null;
    }


}
