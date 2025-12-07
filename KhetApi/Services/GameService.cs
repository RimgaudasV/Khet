using KhetApi.Entities;
using KhetApi.Entities.Board;
using KhetApi.Entities.Piece;
using KhetApi.Interfaces;
using KhetApi.Requests;
using KhetApi.Responses;

namespace KhetApi.Services;

public class GameService : IGameService
{
    private BoardEntity _board;
    private bool _gameOver;

    public GameResponse StartGame()
    {
        var board = new BoardEntity();
        return new GameResponse
        {
            Board = board,
            CurrentPlayer = Player.Player1,
            GameEnded = false,
            Laser = new List<Position>()
        };
    }

    public GameResponse MakeMove(MoveRequest request)
    {
        _board = request.Board;

        DoMove(request);

        var laser = LaserMovement(request.Player);

        return new GameResponse
        {
            Board = _board,
            Laser = laser,
            CurrentPlayer = GetNextPlayer(request.Player),
            GameEnded = _gameOver
        };
    }

    private void DoMove(MoveRequest request)
    {
        var piece = _board.GetPieceAt(request.CurrentPosition)
            ?? throw new InvalidOperationException("No piece found at the current position.");

        if (request.NewRotation is not null)
            piece.Rotation = request.NewRotation.Value;

        if (request.NewPosition is not null)
        {
            var targetPiece = _board.GetPieceAt(request.NewPosition);
            if (piece.Type == PieceType.Scarab && targetPiece is not null)
            {
                _board.Pieces[request.CurrentPosition.Y][request.CurrentPosition.X] = targetPiece;
                _board.Pieces[request.NewPosition.Y][request.NewPosition.X] = piece;

            }
            else
            {
                _board.Pieces[request.NewPosition.Y][request.NewPosition.X] = piece;
                _board.Pieces[request.CurrentPosition.Y][request.CurrentPosition.X] = null;
            }
        }
    }

    private List<Position> LaserMovement(Player player)
    {
        var currentPosition = player == Player.Player1
            ? new Position(9, 7)
            : new Position(0, 0);

        var laserDirection = player == Player.Player1
            ? LaserDirection.Up
            : LaserDirection.Down;

        var laserPath = new List<Position> { currentPosition };

        while (true)
        {
            currentPosition = MoveOneStep(currentPosition, laserDirection);

            if (!_board.IsInsideBoard(currentPosition))
                break;

            laserPath.Add(currentPosition);

            var piece = _board.GetPieceAt(currentPosition);

            if (piece != null)
            {
                var impact = CalculateImpact(laserDirection, piece);

                if (impact.GameOver)
                {
                    _gameOver = true;
                    break;
                }

                if(impact.NewDirection is null)
                {
                    break;
                }
                else
                {
                    laserDirection = impact.NewDirection.Value;
                }

                if (impact.DestroyPiece)
                {
                    _board.RemovePiece(currentPosition);
                    break;
                }
            }
        }

        return laserPath;
    }

    private Position MoveOneStep(Position pos, LaserDirection dir) => dir switch
    {
        LaserDirection.Up => new Position(pos.X, pos.Y - 1),
        LaserDirection.Right => new Position(pos.X + 1, pos.Y),
        LaserDirection.Down => new Position(pos.X, pos.Y + 1),
        LaserDirection.Left => new Position(pos.X - 1, pos.Y),
        _ => throw new InvalidOperationException("Invalid laser direction.")
    };

    private ImpactResult CalculateImpact(LaserDirection laserDir, PieceEntity piece)
    {
        return (laserDir, piece.Rotation, piece.Type) switch
        {
            // ===============================
            // PYRAMID REFLECTIONS
            // ===============================
            (LaserDirection.Up, Rotation.LeftDown, PieceType.Pyramid)
                => new ImpactResult(LaserDirection.Left, false, false),
            (LaserDirection.Up, Rotation.RightDown, PieceType.Pyramid)
                => new ImpactResult(LaserDirection.Right, false, false),

            (LaserDirection.Down, Rotation.LeftUp, PieceType.Pyramid)
                => new ImpactResult(LaserDirection.Left, false, false),
            (LaserDirection.Down, Rotation.RightUp, PieceType.Pyramid)
                => new ImpactResult(LaserDirection.Right, false, false),

            (LaserDirection.Left, Rotation.RightDown, PieceType.Pyramid)
                => new ImpactResult(LaserDirection.Down, false, false),
            (LaserDirection.Left, Rotation.RightUp, PieceType.Pyramid)
                => new ImpactResult(LaserDirection.Up, false, false),

            (LaserDirection.Right, Rotation.LeftDown, PieceType.Pyramid)
                => new ImpactResult(LaserDirection.Down, false, false),
            (LaserDirection.Right, Rotation.LeftUp, PieceType.Pyramid)
                => new ImpactResult(LaserDirection.Up, false, false),

            // PYRAMID BACKSIDES → destroy
            (LaserDirection.Up, Rotation.Up, PieceType.Pyramid)
                => new ImpactResult(laserDir, true, false),
            (LaserDirection.Down, Rotation.Down, PieceType.Pyramid)
                => new ImpactResult(laserDir, true, false),
            (LaserDirection.Left, Rotation.Left, PieceType.Pyramid)
                => new ImpactResult(laserDir, true, false),
            (LaserDirection.Right, Rotation.Right, PieceType.Pyramid)
                => new ImpactResult(laserDir, true, false),

            // ===============================
            // SCARAB REFLECTIONS
            // ===============================
            // RightUp rotation
            (LaserDirection.Up, Rotation.RightUp, PieceType.Scarab)
            or (LaserDirection.Left, Rotation.RightUp, PieceType.Scarab)
                => new ImpactResult(LaserDirection.Right, false, false),

            (LaserDirection.Right, Rotation.RightUp, PieceType.Scarab)
            or (LaserDirection.Down, Rotation.RightUp, PieceType.Scarab)
                => new ImpactResult(LaserDirection.Up, false, false),

            // LeftUp rotation
            (LaserDirection.Up, Rotation.LeftUp, PieceType.Scarab)
            or (LaserDirection.Right, Rotation.LeftUp, PieceType.Scarab)
                => new ImpactResult(LaserDirection.Left, false, false),

            (LaserDirection.Left, Rotation.LeftUp, PieceType.Scarab)
            or (LaserDirection.Down, Rotation.LeftUp, PieceType.Scarab)
                => new ImpactResult(LaserDirection.Up, false, false),

            // ===============================
            // ANUBIS / SIDE BLOCKS → end move
            // ===============================
            (_, _, PieceType.Anubis)
                => new ImpactResult(null, false, false),

            (_, _, PieceType.Pharaoh)
                => new ImpactResult(null, true, true),

            // ===============================
            // ALL OTHER PIECES → destroyed
            // ===============================
            (_, _, _)
                => new ImpactResult(laserDir, true, false)
        };
    }




    public Player GetNextPlayer(Player player)
        => player == Player.Player1 ? Player.Player2 : Player.Player1;

    public ValidMovesResponse GetValidMoves(ValidMoveRequest request)
    {
        _board = request.Board;
        var piece = _board.GetPieceAt(request.CurrentPosition);
        return new ValidMovesResponse
        {
            ValidPositions = GetValidPositions(request.CurrentPosition, piece),
            ValidRotations = GetValidRotations(piece)
        };

    }

    public List<Position> GetValidPositions(Position position, PieceEntity piece)
    {
        var validPositions = new List<Position>();

        if (piece == null || !piece.IsMovable)
            return validPositions;

        // Directions: N, NE, E, SE, S, SW, W, NW
        int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
        int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

        for (int i = 0; i < 8; i++)
        {
            int newX = position.X + dx[i];
            int newY = position.Y + dy[i];

            // Check if inside board
            if (!_board.IsInsideBoard(new Position(newX, newY)))
                continue;

            var targetPiece = _board.GetPieceAt(new Position(newX, newY));

            if (targetPiece == null)
            {
                // Empty square, always valid
                validPositions.Add(new Position(newX, newY));
            }
            else if (piece.Type == PieceType.Scarab)
            {
                // Scarab can swap with Pyramid or Anubis, any color
                if (targetPiece.Type == PieceType.Pyramid || targetPiece.Type == PieceType.Anubis)
                {
                    validPositions.Add(new Position(newX, newY));
                }
                // Scarab cannot move into Pharaoh or another Scarab
            }
            else
            {
                // Other pieces cannot move into occupied squares
                continue;
            }
        }

        return validPositions;
    }


    public List<Rotation> GetValidRotations(PieceEntity piece)
    {
        return piece.Type switch
        {
            PieceType.Scarab => new List<Rotation> { Rotation.LeftUp, Rotation.RightUp },
            PieceType.Pyramid => new List<Rotation> { Rotation.RightUp, Rotation.LeftUp, Rotation.RightDown, Rotation.LeftDown },
            PieceType.Sphinx => new List<Rotation>(),
            _ => new List<Rotation> { Rotation.Up, Rotation.Right, Rotation.Down, Rotation.Left }
        };
    }


}

public record ImpactResult(LaserDirection? NewDirection, bool DestroyPiece, bool GameOver);
