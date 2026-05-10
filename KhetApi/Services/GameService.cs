using KhetApi.Interfaces;
using KhetApi.Mappers;
using KhetApi.Models;
using KhetApi.Models.Board;
using KhetApi.Models.Move;
using KhetApi.Models.Piece;
using KhetApi.Requests;
using KhetApi.Responses;
using KhetApi.Utils;

namespace KhetApi.Services;

public class GameService(IEvaluationService evaluationService) : IGameService
{
    private static readonly int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
    private static readonly int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

    private int MAX_DEPTH;
    private EvaluationConfig _evalConfig = new();

    private int ALL_MOVES_COUNT = 0;
    private int ALL_ROUTES_COUNT = 0;
    private int EVALUATED_ROUTES_COUNT = 0;
    private const double BEST_MOVE_THRESHOLD = 0.5;


    public GameResponse StartGame()
    {
        var board = new BoardModel();
        board.InitiateBoard();
        return new GameResponse
        {
            Board = board,
            CurrentPlayer = Player.Player1
        };
    }

    public ImpactResultModel MakeMove(MoveRequest request)
    {
        DoMove(request.Board, request.CurrentPosition, request.NewPosition);
        return ApplyImpacts(request.Board, request.Player);
    }

    private UndoState MakeMoveInPlace(BoardModel board, Player player, Move move)
    {
        var undo = new UndoState { From = move.From, To = move.To };

        if (move.Rotation == null)
            undo.Captured = DoMove(board, move.From, move.To);
        else
            undo.OldRotation = DoRotate(board, move.From, move.Rotation.Value);

        var impact = ApplyImpacts(board, player);
        undo.Destroyed = impact.DestroyedPiece;

        return undo;
    }

    private static PieceModel? DoMove(BoardModel board, Position from, Position to)
    {
        var fromCell = board.Cells[from.Y][from.X];
        var toCell   = board.Cells[to.Y][to.X];
        var moving   = fromCell.Piece!;
        var target   = toCell.Piece;

        if (moving.Type == PieceType.Scarab && target != null)
            (toCell.Piece, fromCell.Piece) = (moving, target);
        else
            (toCell.Piece, fromCell.Piece) = (moving, null);

        return target;
    }

    private static Rotation DoRotate(BoardModel board, Position pos, Rotation rotation)
    {
        var piece = board.GetPieceAt(pos)!;
        var old = piece.Rotation;
        piece.Rotation = rotation;
        return old;
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
        DoRotate(request.Board, request.CurrentPosition, request.NewRotation);
        return ApplyImpacts(request.Board, request.Player);
    }

    private ImpactResultModel ApplyImpacts(BoardModel board, Player player)
    {
        var origin = LaserUtil.GetLaserStart(board, player);
        var trace  = LaserUtil.TraceLaserPath(board, origin.Pos, origin.Dir);
        var hit    = trace.HitPiece != null
            ? ResolveHit(board, trace.HitPiece, trace.HitPos!, trace.HitDir)
            : new LaserHitResult(null, false);

        return new ImpactResultModel(board, trace.Path, hit.GameOver, GetNextPlayer(player), hit.Destroyed);
    }

    private static LaserHitResult ResolveHit(BoardModel board, PieceModel piece, Position pos, LaserDirection dir)
    {
        var result = LaserUtil.LaserPieceInteraction(dir, piece);
        if (!result.DestroyPiece) return new LaserHitResult(null, false);

        var destroyed = new DestroyedPiece
        {
            Type     = piece.Type,
            Owner    = piece.Owner,
            Position = pos,
            Rotation = piece.Rotation
        };
        board.RemovePiece(pos);
        return new LaserHitResult(destroyed, result.GameOver);
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

        return moves;

    }

    private List<Move> GenerateAllMoves(BoardModel board, Player player)
    {
        var moves = new List<Move>();

        for (int y = 0; y < board.Cells.Length; y++)
        {
            for (int x = 0; x < board.Cells[y].Length; x++)
            {
                var from = new Position(x, y);
                var piece = board.GetPieceAt(from);

                if (piece == null || piece.Owner != player)
                    continue;

                moves.AddRange(GenerateMoves(board, player, from, piece));
            }
        }

        return moves;
    }

    public GameResponse MoveByAgent(AgentMoveRequest request)
    {
        MAX_DEPTH = request.Depth;
        _evalConfig = request.EvaluationConfig ?? new();
        ALL_MOVES_COUNT = 0;
        ALL_ROUTES_COUNT = 0;
        EVALUATED_ROUTES_COUNT = 0;

        var rootMoves = GenerateAllMoves(request.Board, request.Player);
        ALL_MOVES_COUNT = rootMoves.Count;
        OrderMoves(rootMoves, request.Board);

        var bestMove = FindBestMove(request.Board, request.Player, rootMoves);
        var impact = ApplyMove(request.Board, request.Player, bestMove);

        return new GameResponse
        {
            Board = impact.Board,
            CurrentPlayer = impact.NextPlayer,
            GameEnded = impact.GameOver,
            Laser = impact.LaserPath,
            DestroyedPiece = impact.DestroyedPiece,
            AllMovesCount = ALL_MOVES_COUNT,
            AllRoutesCount = ALL_ROUTES_COUNT,
            EvaluatedRoutesCount = EVALUATED_ROUTES_COUNT,
            Winner = impact.GameOver && impact.DestroyedPiece != null
                ? GetNextPlayer(impact.DestroyedPiece.Owner)
                : null
        };
    }

    private Move FindBestMove(BoardModel board, Player player, List<Move> rootMoves)
    {
        var scores = new double[rootMoves.Count];

        var oldestBrotherClone = board.Clone();
        var oldestBrotherUndo = MakeMoveInPlace(oldestBrotherClone, player, rootMoves[0]);

        Interlocked.Increment(ref ALL_ROUTES_COUNT);
        Interlocked.Increment(ref EVALUATED_ROUTES_COUNT);

        scores[0] = AlphaBetaSearch(oldestBrotherClone, GetNextPlayer(player), MAX_DEPTH - 1,
            double.NegativeInfinity, double.PositiveInfinity, IsGameOver(oldestBrotherUndo), player, GetWinner(oldestBrotherUndo));

        double bestScore = scores[0];
        object lockObj = new();

        Parallel.For(1, rootMoves.Count, i =>
        {
            var boardClone = board.Clone();
            var undo = MakeMoveInPlace(boardClone, player, rootMoves[i]);
            Interlocked.Increment(ref ALL_ROUTES_COUNT);
            Interlocked.Increment(ref EVALUATED_ROUTES_COUNT);

            double localAlpha;
            lock (lockObj) { localAlpha = bestScore - BEST_MOVE_THRESHOLD; }

            double score = AlphaBetaSearch(boardClone, GetNextPlayer(player), MAX_DEPTH - 1,
                localAlpha, double.PositiveInfinity, IsGameOver(undo), player, GetWinner(undo));

            scores[i] = score;
            lock (lockObj)
            {
                if (score > bestScore)
                    bestScore = score;
            }
        });

        var candidates = rootMoves
            .Select((move, i) => (move, scores[i]))
            .Where(x => x.Item2 >= bestScore - BEST_MOVE_THRESHOLD)
            .Select(x => x.move)
            .ToList();

        return candidates[Random.Shared.Next(candidates.Count)];
    }

    private ImpactResultModel ApplyMove(BoardModel board, Player player, Move move)
    {
        if (move.Rotation != null)
            DoRotate(board, move.From, move.Rotation.Value);
        else
            DoMove(board, move.From, move.To);
        return ApplyImpacts(board, player);
    }

    private double AlphaBetaSearch(BoardModel board, Player player, int depth, double alpha, double beta, bool gameOver, Player rootPlayer, Player? winner = null)
    {
        if (depth == 0 || gameOver)
            return evaluationService.EvaluateBoard(board, gameOver, depth, winner, rootPlayer, MAX_DEPTH, _evalConfig);

        bool maximizing = player == rootPlayer;
        double bestScore = maximizing ? double.NegativeInfinity : double.PositiveInfinity;

        var allMoves = GenerateAllMoves(board, player);
        OrderMoves(allMoves, board);
        Interlocked.Add(ref ALL_ROUTES_COUNT, allMoves.Count);

        foreach (var move in allMoves)
        {
            Interlocked.Increment(ref EVALUATED_ROUTES_COUNT);

            var undo = MakeMoveInPlace(board, player, move);
            double score = AlphaBetaSearch(board, GetNextPlayer(player), depth - 1, alpha, beta,
                IsGameOver(undo), rootPlayer, GetWinner(undo));
            UndoMove(board, undo);

            if (maximizing)
            {
                bestScore = Math.Max(bestScore, score);
                alpha = Math.Max(alpha, bestScore);
            }
            else
            {
                bestScore = Math.Min(bestScore, score);
                beta = Math.Min(beta, bestScore);
            }

            if (beta <= alpha)
                break;
        }

        return bestScore;
    }

    private static void OrderMoves(List<Move> moves, BoardModel board) =>
        moves.Sort((a, b) => ScoreMove(board, b).CompareTo(ScoreMove(board, a)));

    private static bool IsGameOver(UndoState undo) =>
        undo.Destroyed?.Type == PieceType.Pharaoh;

    private Player? GetWinner(UndoState undo) =>
        IsGameOver(undo) ? GetNextPlayer(undo.Destroyed!.Owner) : null;

    private static int ScoreMove(BoardModel board, Move move)
    {
        var piece = board.GetPieceAt(move.From);
        if (piece == null) return 0;

        if (piece.Type == PieceType.Scarab && move.Rotation == null)
        {
            var target = board.GetPieceAt(move.To);
            if (target != null) return 100;
        }

        if (piece.Type == PieceType.Pharaoh) return -50;

        if (move.Rotation != null && piece.Type == PieceType.Pyramid) return 20;

        return piece.Type switch
        {
            PieceType.Anubis => 10,
            PieceType.Pyramid => 8,
            PieceType.Scarab => 5,
            _ => 0
        };
    }


}


public record ImpactResult(LaserDirection? NewDirection, bool DestroyPiece, bool GameOver);
