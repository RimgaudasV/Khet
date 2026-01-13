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

    private int MAX_DEPTH = 2;

    private readonly Dictionary<PieceType, int> PieceValues = new Dictionary<PieceType, int>{
        { PieceType.Pyramid, 1 },
        { PieceType.Anubis, 2 }
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
        var targetPiece = board.Cells[request.NewPosition.Y][request.NewPosition.X].Piece;

        if (piece.Type == PieceType.Scarab && targetPiece is not null)
        {
            board.Cells[request.CurrentPosition.Y][request.CurrentPosition.X].Piece = targetPiece;
            board.Cells[request.NewPosition.Y][request.NewPosition.X].Piece = piece;
        }
        else
        {
            board.Cells[request.NewPosition.Y][request.NewPosition.X].Piece = piece;
            board.Cells[request.CurrentPosition.Y][request.CurrentPosition.X].Piece = null;
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
            var fromCell = board.Cells[move.From.Y][move.From.X];
            var toCell = board.Cells[move.To.Y][move.To.X];

            var movingPiece = fromCell.Piece;
            var targetPiece = toCell.Piece;

            undo.Captured = targetPiece;

            if (movingPiece.Type == PieceType.Scarab && targetPiece != null)
            {
                toCell.Piece = movingPiece;
                fromCell.Piece = targetPiece;
            }
            else
            {
                toCell.Piece = movingPiece;
                fromCell.Piece = null;
            }
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
            board.Cells[destroyedPiece.Position.Y][destroyedPiece.Position.X].Piece = new PieceModel
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

        var fromCell = board.Cells[undo.From.Y][undo.From.X];
        var toCell = board.Cells[undo.To.Y][undo.To.X];

        fromCell.Piece = toCell.Piece;
        toCell.Piece = undo.Captured;
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

            var cell = board.Cells[currentPosition.Y][currentPosition.X];
            var piece = cell.Piece;

            if (cell.IsDisabled && piece == null)
                continue;

            if (piece != null)
            {
                var impact = CalculateImpact(laserDirection, piece);

                if (impact.DestroyPiece)
                {
                    destroyedPiece = new DestroyedPiece { 
                        Type = piece.Type,
                        Owner = piece.Owner,
                        Position = currentPosition,
                        Rotation = piece.Rotation
                    };
                    if (impact.GameOver)
                    {
                        return new ImpactResultModel(board, laserPath, true, GetNextPlayer(player), destroyedPiece);
                    }
                    board.RemovePiece(currentPosition);
                    break;
                }

                if (impact.NewDirection is null)
                {
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
            (LaserDirection.Down, Rotation.Up, PieceType.Anubis) => new ImpactResult(null, false, false),
            (LaserDirection.Up, Rotation.Down, PieceType.Anubis) => new ImpactResult(null, false, false),
            (LaserDirection.Left, Rotation.Right, PieceType.Anubis) => new ImpactResult(null, false, false),
            (LaserDirection.Right, Rotation.Left, PieceType.Anubis) => new ImpactResult(null, false, false),

            //Not destoyed but no reflection either
            (_, _, PieceType.Anubis) => new ImpactResult(null, true, false),

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

            var targetCell = board.Cells[newY][newX];

            if (targetCell.IsDisabled && targetCell.DisabledFor != player)
                continue;

            var targetPiece = targetCell.Piece;

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
        var allPositions = piece.Type switch
        {
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
        var moves = new List<Move>();

        foreach (var to in valid.ValidPositions)
        {
            moves.Add(new Move
            {
                From = from,
                To = to,
                Rotation = null
            });
        }

        foreach (var rot in valid.ValidRotations)
        {
            if (piece.Rotation == rot) continue;

            moves.Add(new Move
            {
                From = from,
                To = from,
                Rotation = rot
            });
        }

        return moves.OrderBy(_ => Random.Shared.Next());
    }


    public GameResponse MoveByAgent(AgentMoveRequest request)
    {
        var search = AlphaBetaSearch( request.Board, request.Player, MAX_DEPTH, int.MinValue, int.MaxValue, false);

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

        if(result.DestroyedPiece != null )
            Console.WriteLine($"Agent ({request.Player}) destroyed {result.DestroyedPiece?.Owner} piece");


        return new GameResponse
        {
            Board = result.Board,
            CurrentPlayer = result.NextPlayer,
            GameEnded = result.GameOver,
            Laser = result.LaserPath,
            DestroyedPiece = result.DestroyedPiece
        };
    }




    private SearchResult AlphaBetaSearch(BoardModel board, Player player, int depth, int alpha, int beta, bool gameOver)
    {
        if (depth == 0 || gameOver)
            return new SearchResult { Score = EvaluateBoard(board, gameOver, GetNextPlayer(player), depth) };

        bool maximizing = player == Player.Player2;
        int bestScore = maximizing ? int.MinValue : int.MaxValue;
        var bestMoves = new List<Move>();

        for (int y = 0; y < board.Cells.Length; y++)
            for (int x = 0; x < board.Cells[y].Length; x++)
            {
                var from = new Position(x, y);
                var piece = board.GetPieceAt(from);
                if (piece == null || piece.Owner != player) continue;

                foreach (var move in GenerateMoves(board, player, from, piece))
                {
                    var undoInformation = MakeMoveInPlace(board, player, move);
                    gameOver = undoInformation.Destroyed?.Type == PieceType.Pharaoh;

                    int score = AlphaBetaSearch(board, GetNextPlayer(player), depth - 1, alpha, beta, gameOver).Score;

                    UndoMove(board, undoInformation);

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


    private int EvaluateBoard(BoardModel board, bool gameOver, Player player, int depth)
    {
        int score = 0;

        if (gameOver) {
            return player == Player.Player2 ? int.MinValue + depth * 10 : int.MaxValue - depth * 10;
        }

        for (int y = 0; y < board.Cells.Length; y++)
        {
            for (int x = 0; x < board.Cells[y].Length; x++)
            {
                var piece = board.Cells[y][x].Piece;
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

        //if (IsPharaohExposed(board, Player.Player2))
        //    score -= 5000;

        //if (IsPharaohExposed(board, Player.Player1))
        //    score += 5000;

        return score;
    }


    private bool IsPharaohExposed(BoardModel board, Player player)
    {
        Position start = player == Player.Player1
            ? new Position(9, 7)
            : new Position(0, 0);

        var sphinx = board.GetPieceAt(start);
        if (sphinx == null) return false;

        var dir = RotationMapper.ToLaserDirection(sphinx.Rotation);
        var pos = start;

        while (true)
        {
            pos = MoveOneStep(pos, dir);
            if (!board.IsInsideBoard(pos))
                return false;

            var piece = board.GetPieceAt(pos);
            if (piece == null)
                continue;

            if (piece.Type == PieceType.Pharaoh && piece.Owner == player)
                return true;

            var impact = CalculateImpact(dir, piece);

            if (impact.GameOver || impact.NewDirection == null)
                return false;

            dir = impact.NewDirection.Value;
        }
    }

}








public record ImpactResult(LaserDirection? NewDirection, bool DestroyPiece, bool GameOver);
