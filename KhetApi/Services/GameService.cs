using KhetApi.Interfaces;
using KhetApi.Mappers;
using KhetApi.Models;
using KhetApi.Models.Board;
using KhetApi.Models.Move;
using KhetApi.Models.Piece;
using KhetApi.Requests;
using KhetApi.Responses;

namespace KhetApi.Services;

public class GameService : IGameService
{
    private static readonly int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
    private static readonly int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

    private int MAX_DEPTH = 3;

    private readonly Dictionary<PieceType, int> PieceValues = new Dictionary<PieceType, int>{
        { PieceType.Pharaoh, 100 },
        { PieceType.Scarab, 40 },
        { PieceType.Pyramid, 30 },
        { PieceType.Anubis, 50 },
        { PieceType.Sphinx, 0 }
    };


    public GameResponse StartGame()
    {
        var board = new BoardModel();
        return new GameResponse
        {
            Board = board,
            CurrentPlayer = Player.Player1,
            GameEnded = false,
            Laser = new List<Position>()
        };
    }

    public ImpactResultModel MakeMove(MoveRequest request)
    {
        var board = request.Board;
        
        var piece = board.GetPieceAt(request.CurrentPosition)
            ?? throw new InvalidOperationException("No piece found at the current position.");
        var targetPiece = board.Pieces[request.NewPosition.Y][request.NewPosition.X];

        if (piece.Type == PieceType.Scarab && targetPiece is not null)
        {
            board.Pieces[request.CurrentPosition.Y][request.CurrentPosition.X] = targetPiece;
            board.Pieces[request.NewPosition.Y][request.NewPosition.X] = piece;
        }
        else
        {
            board.Pieces[request.NewPosition.Y][request.NewPosition.X] = piece;
            board.Pieces[request.CurrentPosition.Y][request.CurrentPosition.X] = null;
        }

        return ApplyImpacts(request.Board, request.Player);

    }

    public ImpactResultModel Rotate(RotationRequest request)
    {
        var piece = request.Board.GetPieceAt(request.CurrentPosition)
            ?? throw new InvalidOperationException("No piece found at the current position.");
        piece.Rotation = request.NewRotation;

        return ApplyImpacts(request.Board, request.Player);
    }

    private ImpactResultModel ApplyImpacts(BoardModel board, Player player)
    {
        var currentPosition = player == Player.Player1
            ? new Position(9, 7)
            : new Position(0, 0);

        var laserDirection = RotationMapper.ToLaserDirection(board.GetPieceAt(currentPosition).Rotation);

        var laserPath = new List<Position> { currentPosition };

        bool gameOver = false;

        while (true)
        {
            currentPosition = MoveOneStep(currentPosition, laserDirection);

            if (!board.IsInsideBoard(currentPosition))
                break;

            laserPath.Add(currentPosition);

            var piece = board.Pieces[currentPosition.Y][currentPosition.X];

            if (piece != null)
            {
                var impact = CalculateImpact(laserDirection, piece);

                if (impact.GameOver)
                {
                    gameOver = true;
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
                    board.RemovePiece(currentPosition);
                    break;
                }
            }
        }
        var nextPlayer = GetNextPlayer(player);

        return new ImpactResultModel (board, laserPath, gameOver, nextPlayer);
    }

    private Position MoveOneStep(Position pos, LaserDirection dir) => dir switch
    {
        LaserDirection.Up => new Position(pos.X, pos.Y - 1),
        LaserDirection.Right => new Position(pos.X + 1, pos.Y),
        LaserDirection.Down => new Position(pos.X, pos.Y + 1),
        LaserDirection.Left => new Position(pos.X - 1, pos.Y),
        _ => throw new InvalidOperationException("Invalid laser direction.")
    };

    private ImpactResult CalculateImpact(LaserDirection laserDir, PieceModel piece)
    {
        return (laserDir, piece.Rotation, piece.Type) switch
        {
            // PYRAMID REFLECTIONS
            (LaserDirection.Up, Rotation.LeftDown, PieceType.Pyramid) => new ImpactResult(LaserDirection.Left, false, false),
            (LaserDirection.Up, Rotation.RightDown, PieceType.Pyramid) => new ImpactResult(LaserDirection.Right, false, false),

            (LaserDirection.Down, Rotation.LeftUp, PieceType.Pyramid) => new ImpactResult(LaserDirection.Left, false, false),
            (LaserDirection.Down, Rotation.RightUp, PieceType.Pyramid) => new ImpactResult(LaserDirection.Right, false, false),

            (LaserDirection.Left, Rotation.RightDown, PieceType.Pyramid) => new ImpactResult(LaserDirection.Down, false, false),
            (LaserDirection.Left, Rotation.RightUp, PieceType.Pyramid) => new ImpactResult(LaserDirection.Up, false, false),

            (LaserDirection.Right, Rotation.LeftDown, PieceType.Pyramid) => new ImpactResult(LaserDirection.Down, false, false),
            (LaserDirection.Right, Rotation.LeftUp, PieceType.Pyramid) => new ImpactResult(LaserDirection.Up, false, false),

            // PYRAMID BACKSIDES
            (LaserDirection.Up, Rotation.Up, PieceType.Pyramid) => new ImpactResult(laserDir, true, false),
            (LaserDirection.Down, Rotation.Down, PieceType.Pyramid) => new ImpactResult(laserDir, true, false),
            (LaserDirection.Left, Rotation.Left, PieceType.Pyramid) => new ImpactResult(laserDir, true, false),
            (LaserDirection.Right, Rotation.Right, PieceType.Pyramid) => new ImpactResult(laserDir, true, false),

            // SCARAB REFLECTIONS
            // RightUp rotation
            (LaserDirection.Up, Rotation.RightUp, PieceType.Scarab) => new ImpactResult(LaserDirection.Left, false, false),
            (LaserDirection.Left, Rotation.RightUp, PieceType.Scarab) => new ImpactResult(LaserDirection.Up, false, false),
            (LaserDirection.Right, Rotation.RightUp, PieceType.Scarab) => new ImpactResult(LaserDirection.Down, false, false),
            (LaserDirection.Down, Rotation.RightUp, PieceType.Scarab) => new ImpactResult(LaserDirection.Right, false, false),

            // LeftUp rotation
            (LaserDirection.Up, Rotation.LeftUp, PieceType.Scarab) => new ImpactResult(LaserDirection.Right, false, false),
            (LaserDirection.Right, Rotation.LeftUp, PieceType.Scarab) => new ImpactResult(LaserDirection.Up, false, false),

            (LaserDirection.Left, Rotation.LeftUp, PieceType.Scarab) => new ImpactResult(LaserDirection.Down, false, false),
            (LaserDirection.Down, Rotation.LeftUp, PieceType.Scarab) => new ImpactResult(LaserDirection.Left, false, false),


            // Sphinx
            (_, _, PieceType.Sphinx) => new ImpactResult(null, false, false),

            // ANUBIS 
            (_, _, PieceType.Anubis) when
                laserDir == LaserDirection.Right && (piece.Rotation == Rotation.Right || piece.Rotation == Rotation.Left) ||
                laserDir == LaserDirection.Left && (piece.Rotation == Rotation.Left || piece.Rotation == Rotation.Right) ||
                laserDir == LaserDirection.Up && (piece.Rotation == Rotation.Up || piece.Rotation == Rotation.Down) ||
                laserDir == LaserDirection.Down && (piece.Rotation == Rotation.Down || piece.Rotation == Rotation.Up)
                => new ImpactResult(null, true, false),

            (_, _, PieceType.Anubis) => new ImpactResult(null, false, false),

            // PHARAOH → game over
            (_, _, PieceType.Pharaoh) => new ImpactResult(null, true, true),

            // ALL OTHER PIECES → destroyed
            (_, _, _) => new ImpactResult(laserDir, true, false)
        };
    }

    public Player GetNextPlayer(Player player)
        => player == Player.Player1 ? Player.Player2 : Player.Player1;

    public ValidMovesResponse GetValidMoves(BoardModel board, Player player, Position position)
    {
        var piece = board.GetPieceAt(position)??
            throw new InvalidOperationException("No piece found at the specified position.");

        var response =  new ValidMovesResponse
        {
            ValidPositions = GetValidPositions(position, piece, board, player),
            ValidRotations = GetValidRotations(piece)
        };
        return response;

    }

    public List<Position> GetValidPositions(Position currentPosition, PieceModel piece, BoardModel board, Player player)
    {
        if (!piece.IsMovable)
            return new List<Position>();

        var validPositions = new List<Position>();

        for (int i = 0; i < 8; i++)
        {
            int newX = currentPosition.X + dx[i];
            int newY = currentPosition.Y + dy[i];

            var cell = new Position(newX, newY);

            if (!board.IsInsideBoard(cell))
                continue;

            var targetPiece = board.Pieces[newY][newX];

            if(!IsPlayableTile(cell, player))
            {
                continue;
            }

            if (targetPiece == null ||
               (piece.Type == PieceType.Scarab &&(targetPiece.Type == PieceType.Pyramid || targetPiece.Type == PieceType.Anubis)))
            {
                validPositions.Add(cell);
            }
        }

        return validPositions;
    }

    public List<Rotation> GetValidRotations(PieceModel piece)
    {
        return piece.Type switch
        {
            PieceType.Scarab => new List<Rotation> { Rotation.LeftUp, Rotation.RightUp },
            PieceType.Pyramid => new List<Rotation> { Rotation.RightUp, Rotation.RightDown, Rotation.LeftDown, Rotation.LeftUp },
            PieceType.Anubis => new List<Rotation> { Rotation.Up, Rotation.Right, Rotation.Down, Rotation.Left },
            PieceType.Sphinx => piece.Owner == Player.Player1
                ? new List<Rotation> { Rotation.Up, Rotation.Left }
                : new List<Rotation> { Rotation.Down, Rotation.Right },
            _ => new List<Rotation> { Rotation.Up, Rotation.Right, Rotation.Down, Rotation.Left }
        };
    }

    public bool IsPlayableTile(Position cell, Player player)
    {
        if (player == Player.Player1)
        {
            if (cell.X == 0 || (cell.X == 8 && (cell.Y == 0 || cell.Y == 7)))
                return false;
        }
        else
        {
            if (cell.X == 9 || (cell.X == 1 && (cell.Y == 0 || cell.Y == 7)))
                return false;
        }

        return true;
    }

    public GameResponse MoveByAgent(AgentMoveRequest request)
    {
        var best = AlphaBetaSearch(request.Board, request.Player, MAX_DEPTH, int.MinValue, int.MaxValue, false);

        ImpactResultModel result;

        if (best.MoveTo != null && best.MoveTo != best.MoveFrom)
        {
            result = MakeMove(new MoveRequest
            {
                Board = request.Board,
                Player = request.Player,
                CurrentPosition = best.MoveFrom,
                NewPosition = best.MoveTo
            });
        }
        else if (best.Rotation != null)
        {
            result = Rotate(new RotationRequest
            {
                Board = request.Board,
                Player = request.Player,
                CurrentPosition = best.MoveFrom,
                NewRotation = best.Rotation.Value
            });
        }
        else
        {
            throw new InvalidOperationException("Invalid move returned from AI.");
        }

        return new GameResponse
        {
            Board = result.Board,
            CurrentPlayer = result.NextPlayer,
            GameEnded = result.GameOver,
            Laser = result.LaserPath
        };
    }

    private AlphaBetaResultModel AlphaBetaSearch(BoardModel board, Player currentPlayer, int depth, int alpha, int beta, bool gameOver)
    {
        if (depth == 0 || gameOver)
        {
            return new AlphaBetaResultModel { Score = EvaluateBoard(board, Player.Player2) };
        }

        Position? bestMoveFrom = null;
        Position? bestMoveTo = null;
        Rotation? bestRotation = null;

        bool isMaximizing = currentPlayer == Player.Player2;
        int bestEval = isMaximizing ? int.MinValue : int.MaxValue;

        for (int y = 0; y < board.Pieces.Length; y++)
        {
            for (int x = 0; x < board.Pieces[y].Length; x++)
            {
                var position = new Position(x, y);
                var piece = board.GetPieceAt(position);

                if (piece == null || piece.Owner != currentPlayer)
                    continue;

                var validMoves = GetValidMoves(board, currentPlayer, position);

                foreach (var newPosition in validMoves.ValidPositions)
                {
                    var boardCopy = CloneBoard(board);
                    var result = MakeMove(new MoveRequest
                    {
                        Board = boardCopy,
                        Player = currentPlayer,
                        CurrentPosition = position,
                        NewPosition = newPosition
                    });

                    int eval;
                    if (result.GameOver)
                        eval = isMaximizing ? -10000 : 10000;
                    else
                         eval = AlphaBetaSearch(result.Board, result.NextPlayer, depth - 1, alpha, beta, result.GameOver).Score;

                    if (isMaximizing)
                    {
                        if (eval > bestEval)
                        {
                            Console.WriteLine($"New best move: ({position.X},{position.Y}) -> ({newPosition.X},{newPosition.Y}), eval: {eval}");
                            bestEval = eval;
                            bestMoveFrom = position;
                            bestMoveTo = newPosition;
                            bestRotation = null;
                        }
                        alpha = Math.Max(alpha, eval);
                    }
                    else
                    {
                        if (eval < bestEval)
                        {
                            Console.WriteLine($"New best move: ({position.X},{position.Y}) -> ({newPosition.X},{newPosition.Y}), eval: {eval}");
                            bestEval = eval;
                            bestMoveFrom = position;
                            bestMoveTo = newPosition;
                            bestRotation = null;
                        }
                        beta = Math.Min(beta, eval);
                    }

                    if (beta <= alpha)
                        break;
                }

                if (beta <= alpha)
                    break;

                foreach (var rotation in validMoves.ValidRotations)
                {
                    if (piece.Rotation == rotation)
                        continue;

                    var boardCopy = CloneBoard(board);
                    var result = Rotate(new RotationRequest
                    {
                        Board = boardCopy,
                        Player = currentPlayer,
                        CurrentPosition = position,
                        NewRotation = rotation
                    });

                    int eval;
                    if (result.GameOver)
                        eval = isMaximizing ? 10000 : -10000;
                    else
                        eval = AlphaBetaSearch(result.Board, result.NextPlayer, depth - 1, alpha, beta, result.GameOver).Score;

                    if (isMaximizing)
                    {
                        if (eval > bestEval)
                        {
                            Console.WriteLine($"New best rotation: ({position.X},{position.Y}) -> {rotation}, eval: {eval}");
                            bestEval = eval;
                            bestMoveFrom = position;
                            bestMoveTo = position;
                            bestRotation = rotation;
                        }
                        alpha = Math.Max(alpha, eval);
                    }
                    else
                    {
                        if (eval < bestEval)
                        {
                            Console.WriteLine($"New best rotation: ({position.X},{position.Y}) -> {rotation}, eval: {eval}");
                            bestEval = eval;
                            bestMoveFrom = position;
                            bestMoveTo = position;
                            bestRotation = rotation;
                        }
                        beta = Math.Min(beta, eval);
                    }

                    if (beta <= alpha)
                        break;
                }

                if (beta <= alpha)
                    break;
            }

            if (beta <= alpha)
                break;
        }

        return new AlphaBetaResultModel
        {
            Score = bestEval,
            MoveFrom = bestMoveFrom,
            MoveTo = bestMoveTo,
            Rotation = bestRotation
        };
    }

    private int EvaluateBoard(BoardModel board, Player maximizingPlayer)
    {
        int score = 0;

        for (int y = 0; y < board.Pieces.Length; y++)
        {
            for (int x = 0; x < board.Pieces[y].Length; x++)
            {
                var piece = board.Pieces[y][x];
                if (piece != null && PieceValues.ContainsKey(piece.Type))
                {
                    int value = PieceValues[piece.Type];
                    if (piece.Owner == maximizingPlayer)
                        score += value;
                    else
                        score -= value;
                }
            }
        }

        Console.WriteLine($"Eval for Player {maximizingPlayer}: {score}");
        return score;
    }






    private BoardModel CloneBoard(BoardModel original)
    {
        var cloned = new BoardModel();

        for (int y = 0; y < original.Pieces.Length; y++)
        {
            for (int x = 0; x < original.Pieces[y].Length; x++)
            {
                var piece = original.Pieces[y][x];
                if (piece != null)
                {
                    cloned.Pieces[y][x] = new PieceModel
                    {
                        Type = piece.Type,
                        Owner = piece.Owner,
                        Rotation = piece.Rotation,
                        IsMovable = piece.IsMovable
                    };
                }
            }
        }

        return cloned;
    }


}








public record ImpactResult(LaserDirection? NewDirection, bool DestroyPiece, bool GameOver);
