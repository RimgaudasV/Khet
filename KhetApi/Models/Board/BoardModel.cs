using KhetApi.Models.Piece;

namespace KhetApi.Models.Board;

public class BoardModel
{
    public const int Width = 10;
    public const int Height = 8;

    private static readonly HashSet<(int x, int y)> DisabledCellsPlayerOne = new()
    {
        (8, 0), (9, 0), (9, 1), (9, 2), (9, 5), (9, 6), (8, 7),
    };
    private static readonly HashSet<(int x, int y)> DisabledCellsPlayerTwo = new()
    {
        (1, 0), (0, 1), (0, 2), (0, 5), (0, 6), (0, 7), (1, 7),
    };

    private bool IsDisabledCell(Position pos, out Player owner)
    {
        if (DisabledCellsPlayerOne.Contains((pos.X, pos.Y)))
        {
            owner = Player.Player1;
            return true;
        }

        if (DisabledCellsPlayerTwo.Contains((pos.X, pos.Y)))
        {
            owner = Player.Player2;
            return true;
        }

        owner = default;
        return false;
    }



    public PieceModel?[][] Pieces { get; set; }

    public BoardModel()
    {
        Pieces = new PieceModel?[Height][];
        for (int y = 0; y < Height; y++)
            Pieces[y] = new PieceModel?[Width];

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

        //Disabled
        SetPiece(8, 0, PieceType.Disabled, Player.Player1, Rotation.None, false);
        SetPiece(9, 0, PieceType.Disabled, Player.Player1, Rotation.None, false);
        SetPiece(9, 1, PieceType.Disabled, Player.Player1, Rotation.None, false);
        SetPiece(9, 2, PieceType.Disabled, Player.Player1, Rotation.None, false);
        SetPiece(9, 5, PieceType.Disabled, Player.Player1, Rotation.None, false);
        SetPiece(9, 6, PieceType.Disabled, Player.Player1, Rotation.None, false);
        SetPiece(8, 7, PieceType.Disabled, Player.Player1, Rotation.None, false);

        SetPiece(1, 0, PieceType.Disabled, Player.Player2, Rotation.None, false);
        SetPiece(0, 1, PieceType.Disabled, Player.Player2, Rotation.None, false);
        SetPiece(0, 2, PieceType.Disabled, Player.Player2, Rotation.None, false);
        SetPiece(0, 5, PieceType.Disabled, Player.Player2, Rotation.None, false);
        SetPiece(0, 6, PieceType.Disabled, Player.Player2, Rotation.None, false);
        SetPiece(0, 7, PieceType.Disabled, Player.Player2, Rotation.None, false);
        SetPiece(1, 7, PieceType.Disabled, Player.Player2, Rotation.None, false);

    }

    private void SetPiece(int x, int y, PieceType type, Player owner, Rotation rot, bool? isMovable)
    {
        Pieces[y][x] = new PieceModel
        {
            Type = type,
            Owner = owner,
            Rotation = rot,
            IsMovable = isMovable ?? false
        };
    }

    public PieceModel GetPieceAt(Position currentPosition)
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
        if (IsDisabledCell(currentPosition, out var owner))
        {
            Pieces[currentPosition.Y][currentPosition.X] = new PieceModel
            {
                Type = PieceType.Disabled,
                Owner = owner,
                Rotation = Rotation.None,
                IsMovable = false
            };
        }
        else
        {
            Pieces[currentPosition.Y][currentPosition.X] = null;
        }
    }


}
