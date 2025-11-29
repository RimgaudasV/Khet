using KhetApi.Entities.Piece;

namespace KhetApi.Entities.Board;

public class BoardEntity
{
    public const int Width = 10;
    public const int Height = 8;

    public PieceEntity?[][] Pieces { get; set; }

    public BoardEntity()
    {
        // Allocate jagged array
        Pieces = new PieceEntity?[Height][];
        for (int y = 0; y < Height; y++)
            Pieces[y] = new PieceEntity?[Width];

        InitiateBoard();
    }

    private void InitiateBoard()
    {
        // Sphinx
        SetPiece(0, 0, PieceType.Sphinx, Player.Player1, Rotation.Down, new List<Rotation>{Rotation.Down, Rotation.Right});
        SetPiece(9, 7, PieceType.Sphinx, Player.Player2, Rotation.Up, new List<Rotation> { Rotation.Up, Rotation.Left });

        // Pharaoh
        SetPiece(5, 0, PieceType.Pharaoh, Player.Player1, Rotation.Down);
        SetPiece(4, 7, PieceType.Pharaoh, Player.Player2, Rotation.Up);

        // Anubis
        SetPiece(4, 0, PieceType.Anubis, Player.Player1, Rotation.Down);
        SetPiece(6, 0, PieceType.Anubis, Player.Player1, Rotation.Down);
        
        SetPiece(3, 7, PieceType.Anubis, Player.Player2, Rotation.Up);
        SetPiece(5, 7, PieceType.Anubis, Player.Player2, Rotation.Up);


        // Pyramids
        SetPiece(7, 0, PieceType.Pyramid, Player.Player1, Rotation.Right);
        SetPiece(2, 1, PieceType.Pyramid, Player.Player1, Rotation.Up);
        SetPiece(0, 3, PieceType.Pyramid, Player.Player1, Rotation.Up);
        SetPiece(0, 4, PieceType.Pyramid, Player.Player1, Rotation.Left);
        SetPiece(7, 3, PieceType.Pyramid, Player.Player1, Rotation.Right);
        SetPiece(7, 4, PieceType.Pyramid, Player.Player2, Rotation.Left);
        SetPiece(6, 5, PieceType.Pyramid, Player.Player2, Rotation.Down);

        SetPiece(2, 7, PieceType.Pyramid, Player.Player2, Rotation.Left);
        SetPiece(7, 6, PieceType.Pyramid, Player.Player2, Rotation.Down);
        SetPiece(9, 4, PieceType.Pyramid, Player.Player2, Rotation.Down);
        SetPiece(9, 3, PieceType.Pyramid, Player.Player2, Rotation.Right);
        SetPiece(2, 4, PieceType.Pyramid, Player.Player2, Rotation.Down);
        SetPiece(2, 3, PieceType.Pyramid, Player.Player2, Rotation.Down);
        SetPiece(3, 2, PieceType.Pyramid, Player.Player2, Rotation.Right);



        // Scarabs
        SetPiece(4, 3, PieceType.Scarab, Player.Player1, Rotation.Up);
        SetPiece(5, 3, PieceType.Scarab, Player.Player1, Rotation.Right);
        
        SetPiece(4, 4, PieceType.Scarab, Player.Player2, Rotation.Down);
        SetPiece(5, 4, PieceType.Scarab, Player.Player2, Rotation.Left);
    }

    private void SetPiece(int x, int y, PieceType type, Player owner, Rotation rot, List<Rotation>? possibleRotations = null)
    {
        Pieces[y][x] = new PieceEntity
        {
            Type = type,
            Owner = owner,
            Rotation = rot,
            PossibleRotations = possibleRotations
        };
    }
}
