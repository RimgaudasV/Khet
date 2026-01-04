using KhetApi.Interfaces;
using KhetApi.Mappers;
using KhetApi.Models;
using KhetApi.Models.Board;
using KhetApi.Models.Move;
using KhetApi.Models.Piece;
using KhetApi.Requests;
using KhetApi.Responses;
using System.Numerics;

namespace KhetApi.Services;

public class GameService : IGameService
{
    private static readonly int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
    private static readonly int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

    private int MAX_DEPTH = 3;

    private readonly Dictionary<PieceType, int> PieceValues = new Dictionary<PieceType, int>{
        { PieceType.Pharaoh, 10 },
        { PieceType.Scarab, 2 },
        { PieceType.Pyramid, 1 },
        { PieceType.Anubis, 3 }
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

    private UndoState MakeMoveInPlace(BoardModel board, Player player, Move move)
    {
        var undo = new UndoState
        {
            From = move.From,
            To = move.To
        };

        if (move.Rotation == null)
        {
            var oldPosition = board.Pieces[move.From.Y][move.From.X];
            var newPosition = board.Pieces[move.To.Y][move.To.X];

            undo.Captured = newPosition;

            board.Pieces[move.To.Y][move.To.X] = oldPosition;
            board.Pieces[move.From.Y][move.From.X] = null;
        }
        else
        {
            var piece = board.GetPieceAt(move.From)!;
            undo.OldRotation = piece.Rotation;
            piece.Rotation = move.Rotation.Value;
        }

        var impact = ApplyImpacts(board, player);

        undo.Destroyed = impact.DestroyedPiece;

        return undo;
    }

    private void UndoMove(BoardModel board, UndoState undo)
    {
        if (undo.Destroyed != null)
        {
            var destroyedPiece = undo.Destroyed;
            board.Pieces[destroyedPiece.Position.Y][destroyedPiece.Position.X] = new PieceModel
            {
                Type = destroyedPiece.Type,
                Owner = destroyedPiece.Owner,
                Rotation = destroyedPiece.Rotation,
                IsMovable = true
            };
        }

        if (undo.OldRotation != null)
        {
            var piece = board.GetPieceAt(undo.From)!;
            piece.Rotation = undo.OldRotation.Value;
            return;
        }

        var moving = board.Pieces[undo.To.Y][undo.To.X];
        board.Pieces[undo.From.Y][undo.From.X] = moving;
        board.Pieces[undo.To.Y][undo.To.X] = undo.Captured;
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
        DestroyedPiece? destroyedPiece = null;

        while (true)
        {
            currentPosition = MoveOneStep(currentPosition, laserDirection);

            if (!board.IsInsideBoard(currentPosition))
                break;

            laserPath.Add(currentPosition);

            var piece = board.Pieces[currentPosition.Y][currentPosition.X];

            if (piece != null && piece.Type != PieceType.Disabled)
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
                if (impact.DestroyPiece)
                {
                    destroyedPiece = new DestroyedPiece { 
                        Type = piece.Type,
                        Owner = piece.Owner,
                        Position = currentPosition,
                        Rotation = piece.Rotation
                    };
                    board.RemovePiece(currentPosition);
                    break;
                }

                laserDirection = impact.NewDirection.Value;
            }
        }
        var nextPlayer = GetNextPlayer(player);

        return new ImpactResultModel (board, laserPath, gameOver, nextPlayer, destroyedPiece);
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

            if (targetPiece == null || (targetPiece.Type == PieceType.Disabled && player == targetPiece.Owner) ||
               (piece.Type == PieceType.Scarab &&(targetPiece.Type == PieceType.Pyramid || targetPiece.Type == PieceType.Anubis)))
            {
                validPositions.Add(cell);
            }
        }

        return validPositions;
    }

    public List<Rotation> GetValidRotations(PieceModel piece)
    {
        var allPositions = piece.Type switch
        {
            PieceType.Disabled => new List<Rotation>(),
            PieceType.Scarab => new List<Rotation> { Rotation.LeftUp, Rotation.RightUp },
            PieceType.Pyramid => new List<Rotation> { Rotation.RightUp, Rotation.RightDown, Rotation.LeftDown, Rotation.LeftUp },
            PieceType.Anubis => new List<Rotation> { Rotation.Up, Rotation.Right, Rotation.Down, Rotation.Left },
            PieceType.Sphinx => piece.Owner == Player.Player1
                ? new List<Rotation> { Rotation.Up, Rotation.Left }
                : new List<Rotation> { Rotation.Down, Rotation.Right },
            _ => new List<Rotation> { Rotation.Up, Rotation.Right, Rotation.Down, Rotation.Left }
        };

        if (allPositions.Count == 0)
            return new List<Rotation>();

        var index = allPositions.IndexOf(piece.Rotation);
        var count = allPositions.Count;

        var previous = allPositions[(index - 1 + count) % count];
        var current = allPositions[index];
        var next = allPositions[(index + 1) % count];

        return new List<Rotation> { previous, current, next };
    }


    private IEnumerable<Move> GenerateMoves(BoardModel board, Player player, Position from, PieceModel piece)
    {
        var valid = GetValidMoves(board, player, from);

        foreach (var to in valid.ValidPositions)
        {
            yield return new Move
            {
                From = from,
                To = to,
                Rotation = null
            };
        }

        foreach (var rot in valid.ValidRotations)
        {
            if (piece.Rotation == rot) continue;

            yield return new Move
            {
                From = from,
                To = from,
                Rotation = rot
            };
        }
    }


    public GameResponse MoveByAgent(AgentMoveRequest request)
    {
        var search = AlphaBetaSearch( request.Board, request.Player, MAX_DEPTH,int.MinValue,int.MaxValue);

        var chosen = search.BestMoves[Random.Shared.Next(search.BestMoves.Count)];

        ImpactResultModel result = chosen.Rotation != null
            ? Rotate(new RotationRequest
            {
                Board = request.Board,
                Player = request.Player,
                CurrentPosition = chosen.From,
                NewRotation = chosen.Rotation.Value
            })
            : MakeMove(new MoveRequest
            {
                Board = request.Board,
                Player = request.Player,
                CurrentPosition = chosen.From,
                NewPosition = chosen.To
            });

        return new GameResponse
        {
            Board = result.Board,
            CurrentPlayer = result.NextPlayer,
            GameEnded = result.GameOver,
            Laser = result.LaserPath,
            DestroyedPiece = result.DestroyedPiece
        };
    }




    private SearchResult AlphaBetaSearch(BoardModel board, Player player, int depth, int alpha,int beta)
    {
        if (depth == 0)
            return new SearchResult { Score = EvaluateBoard(board) };

        bool maximizing = player == Player.Player2;
        int bestScore = maximizing ? int.MinValue : int.MaxValue;
        var bestMoves = new List<Move>();

        for (int y = 0; y < board.Pieces.Length; y++)
            for (int x = 0; x < board.Pieces[y].Length; x++)
            {
                var from = new Position(x, y);
                var piece = board.GetPieceAt(from);
                if (piece == null || piece.Owner != player) continue;

                foreach (var move in GenerateMoves(board, player, from, piece))
                {
                    var undo = MakeMoveInPlace(board, player, move);

                    int score = AlphaBetaSearch(board, GetNextPlayer(player), depth - 1, alpha, beta).Score;

                    UndoMove(board, undo);

                    if (maximizing)
                    {
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMoves.Clear();
                        }
                        if (score == bestScore)
                            bestMoves.Add(move);
                        alpha = Math.Max(alpha, score);
                    }
                    else
                    {
                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestMoves.Clear();
                        }
                        if (score == bestScore)
                            bestMoves.Add(move);
                        beta = Math.Min(beta, score);
                    }

                    if (beta <= alpha)
                        break;
                }

                if (beta <= alpha) break;
            }

        return new SearchResult
        {
            Score = bestScore,
            BestMoves = bestMoves
        };
    }


    private int EvaluateBoard(BoardModel board)
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
                    if (piece.Owner == Player.Player2)
                        score += value;
                    else
                        score -= value;
                }
            }
        }

        return score;
    }

}








public record ImpactResult(LaserDirection? NewDirection, bool DestroyPiece, bool GameOver);
