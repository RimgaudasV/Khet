using KhetApi.Interfaces;
using KhetApi.Mappers;
using KhetApi.Models;
using KhetApi.Models.Board;
using KhetApi.Models.Move;
using KhetApi.Models.Piece;
using KhetApi.Requests;
using KhetApi.Responses;
using System.Diagnostics;

namespace KhetApi.Services;

public class GameService : IGameService
{
    private static readonly int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
    private static readonly int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

    private int MAX_DEPTH;

    private readonly Dictionary<PieceType, int> PieceValues = new Dictionary<PieceType, int>{
        { PieceType.Pyramid, 10 },
        { PieceType.Anubis, 15 },
        { PieceType.Pharaoh, 100 }
    };

    private int ALL_MOVES_COUNT = 0;
    private int MAX_MOVES_COUNT = 0;


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
                    board.RemovePiece(currentPosition);
                    if (impact.GameOver)
                    {
                        return new ImpactResultModel(board, laserPath, true, GetNextPlayer(player), destroyedPiece);
                    }
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


    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }




    public GameResponse MoveByAgent(AgentMoveRequest request)
    {
        MAX_DEPTH = request.Depth;
        var search = AlphaBetaSearch(request.Board, request.Player, MAX_DEPTH, int.MinValue, int.MaxValue, false, request.Player);

        var chosen = search.BestMoves[Random.Shared.Next(search.BestMoves.Count)];
        Console.WriteLine($"Chosen: {chosen.From} -> {chosen.To}, Rot: {chosen.Rotation}");

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

        if (result.DestroyedPiece != null)
            Console.WriteLine($"Agent ({request.Player}) destroyed {result.DestroyedPiece?.Owner} piece");

        return new GameResponse
        {
            Board = result.Board,
            CurrentPlayer = result.NextPlayer,
            GameEnded = result.GameOver,
            Laser = result.LaserPath,
            DestroyedPiece = result.DestroyedPiece,
            AllMovesCount = ALL_MOVES_COUNT,
            MaxMovesCount = MAX_MOVES_COUNT,
            Winner = result.GameOver && result.DestroyedPiece != null
                ? GetNextPlayer(result.DestroyedPiece.Owner)
                : null
        };
    }

    private SearchResult AlphaBetaSearch(BoardModel board, Player player, int depth, int alpha, int beta, bool gameOver, Player rootPlayer, Player? winner = null)
    {
        if (depth == 0 || gameOver)
            return new SearchResult { Score = EvaluateBoard(board, gameOver, depth, winner, rootPlayer) };

        bool maximizing = player == rootPlayer;
        int bestScore = maximizing ? int.MinValue : int.MaxValue;
        var bestMoves = new List<Move>();

        bool shouldPrune = false;

        var allMoves = GenerateAllMoves(board, player);

        //Shuffle moves to increase variaty of moves
        //Random rng = new Random();
        //int n = allMoves.Count;
        //for (int i = n - 1; i > 0; i--)
        //{
        //    int j = rng.Next(i + 1);
        //    var temp = allMoves[i];
        //    allMoves[i] = allMoves[j];
        //    allMoves[j] = temp;
        //}

        int totalMoves = allMoves.Count;

        if (depth == MAX_DEPTH)
            ALL_MOVES_COUNT += totalMoves;

        foreach (var move in allMoves)
        {
            var undoInformation = MakeMoveInPlace(board, player, move);
            bool moveResultsInGameOver = undoInformation.Destroyed?.Type == PieceType.Pharaoh;

            Player? winnerPlayer = null;
            if (moveResultsInGameOver)
            {
                winnerPlayer = GetNextPlayer(undoInformation.Destroyed.Owner);
            }

            int score = AlphaBetaSearch(board, GetNextPlayer(player), depth - 1, alpha, beta, moveResultsInGameOver,
                rootPlayer, winnerPlayer).Score;

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

        MAX_MOVES_COUNT = Math.Max(MAX_MOVES_COUNT, totalMoves);


        return new SearchResult
        {
            Score = bestScore,
            BestMoves = bestMoves
        };
    }

    private int EvaluateBoard(BoardModel board, bool gameOver, int depth, Player? winner, Player rootPlayer)
    {
        if (gameOver)
            return EvaluateTerminalState(depth, winner, rootPlayer);

        var boardInfo = GetBoardInfo(board);
        var (rootCount, oppCount, total) = CountPieces(boardInfo.Pieces, rootPlayer);

        var phase = DetermineGamePhase(total, rootCount, oppCount);

        int score = 0;

        score += EvaluateMaterial(boardInfo.Pieces, rootPlayer);
        score += EvaluatePhaseSpecific(board, boardInfo.Pieces, rootPlayer, phase);
        score += EvaluatePharaohThreats(boardInfo.PharaohPosition, board, rootPlayer);

        return score;
    }



    private int EvaluatePhaseSpecific(BoardModel board,List<(PieceModel piece, Position pos)> pieces, Player rootPlayer, GamePhase phase)
    {
        int score = 0;

        foreach (var (piece, pos) in pieces)
        {
            if (piece.Type != PieceType.Pharaoh)
                continue;

            int defence = CheckPharaohDefence(pos, board, piece.Owner);

            int phaseMultiplier = phase switch
            {
                GamePhase.Start => 2,
                GamePhase.Middlegame => 1,
                GamePhase.NearEnd => 1,
                GamePhase.EndGame => -1,
                _ => 0
            };

            int value = defence * phaseMultiplier;

            score += piece.Owner == rootPlayer ? value : -value;
        }

        return score;
    }



    private int EvaluateTerminalState(int depth, Player? winner, Player rootPlayer)
    {
        int bonus = (MAX_DEPTH - depth) * 10;

        if (winner == rootPlayer)
            return int.MaxValue - bonus;
        else
            return int.MinValue + bonus;
    }



    private GamePhase DetermineGamePhase(int totalPieces, int rootPieces, int opponentPieces)
    {

        if (totalPieces > 22 && (rootPieces >= 10 || opponentPieces >= 10))
            return GamePhase.Start;

        else if (totalPieces >= 18 && (rootPieces >= 8 || opponentPieces >= 8))
            return GamePhase.Middlegame;

        else if (totalPieces >= 12 && (rootPieces > 5 || opponentPieces > 5))
            return GamePhase.NearEnd;
        else 
            return GamePhase.EndGame;

    }


    private (int rootCount, int opponentCount, int total)
    CountPieces(List<(PieceModel piece, Position pos)> pieces, Player rootPlayer)
    {
        int root = 0;
        int opp = 0;

        foreach (var (piece, _) in pieces)
        {
            if (piece.Owner == rootPlayer) root++;
            else opp++;
        }

        return (root, opp, root + opp);
    }

    private BoardInfo GetBoardInfo(BoardModel board)
    {
        var pieces = new List<(PieceModel, Position)>();
        Position? pharaohPosition = null;

        for (int y = 0; y < board.Cells.Length; y++)
        {
            for (int x = 0; x < board.Cells[y].Length; x++)
            {
                var piece = board.Cells[y][x].Piece;
                if (piece == null) continue;

                var pos = new Position(x, y);
                pieces.Add((piece, pos));

                if (piece.Type == PieceType.Pharaoh)
                    pharaohPosition = pos;
            }
        }

        if (pharaohPosition == null)
            throw new InvalidOperationException("Pharaoh not found on board.");

        return new BoardInfo
        {
            Pieces = pieces,
            PharaohPosition = pharaohPosition
        };
    }


    private int EvaluateMaterial(List<(PieceModel piece, Position pos)> pieces, Player rootPlayer)
    {
        int score = 0;

        foreach (var (piece, _) in pieces)
        {
            if (!PieceValues.ContainsKey(piece.Type)) continue;

            int value = PieceValues[piece.Type];
            score += piece.Owner == rootPlayer ? value : -value;
        }

        return score;
    }



    private int CheckPharaohDefence(Position pharaohPos, BoardModel board, Player owner)
    {
        int score = 0;

        for (int y = 0; y < board.Cells.Length; y++)
        {
            for (int x = 0; x < board.Cells[y].Length; x++)
            {
                var piece = board.Cells[y][x].Piece;
                if (piece == null || piece.Owner != owner)
                    continue;

                var piecePos = new Position(x, y);
                var side = GetSideOfPharaoh(pharaohPos, piecePos);

                if (side == null)
                    continue;

                score += piece.Type switch
                {
                    PieceType.Anubis => AnubisDefendPharoah(side.Value, piece.Rotation),
                    PieceType.Pyramid => PyramidDefendPharoah(side.Value, piece.Rotation),
                    _ => 0
                };
            }
        }

        return score;
    }



    private Rotation? GetSideOfPharaoh(Position pharaohPos, Position piecePos)
    {
        int dx = piecePos.X - pharaohPos.X;
        int dy = piecePos.Y - pharaohPos.Y;

        if (dx == 0)
        {
            if (dy < 0) return Rotation.Up;
            if (dy > 0) return Rotation.Down;
        }

        if (dy == 0)
        {
            if (dx < 0) return Rotation.Left;
            if (dx > 0) return Rotation.Right;
        }

        return null;
    }

    private int EvaluatePharaohThreats(Position pharaohPos, BoardModel board, Player rootPlayer)
    {
        int score = 0;

        var directions = new (int dx, int dy)[]
        {
            (0,-1),(0,1),(-1,0),(1,0)
        };

        foreach (var (dx, dy) in directions)
        {

            int step = 1;
            
            while(true)
            {
                var pos = new Position(pharaohPos.X + dx * step, pharaohPos.Y + dy * step);
                if (!board.IsInsideBoard(pos)) break;

                var piece = board.GetPieceAt(pos);
                if (piece == null || (piece.Owner == rootPlayer && piece.Type == PieceType.Scarab)) 
                    break;

                if(piece.Owner == rootPlayer)
                {
                    if(piece.Type == PieceType.Pyramid)
                    {
                        var side = GetSideOfPharaoh(pharaohPos, pos);
                        if (side != null && PyramidDefendPharoah(side.Value, piece.Rotation) == 0)
                        {
                            score -= 2;
                        }
                    }

                    if (piece.Type == PieceType.Scarab)
                    {
                        score -= 2;
                        break;
                    }
                }

                else
                {
                    int threat = piece.Type switch
                    {
                        PieceType.Scarab => 8,
                        PieceType.Pyramid => 6,
                        _ => 0
                    };
                    score -= threat;
                }

                step++;
            }
        }

        return score;
    }




    private int AnubisDefendPharoah(Rotation sideOfPharoah, Rotation rotation)
        => sideOfPharoah == rotation ? 4 : 2;

    private int PyramidDefendPharoah(Rotation sideOfPharoah, Rotation rotation)
    {
        var PyramidScore = 2;
        return (sideOfPharoah, rotation) switch
        {
            (Rotation.Up, Rotation.LeftUp) => PyramidScore,
            (Rotation.Up, Rotation.RightUp) => PyramidScore,
            (Rotation.Down, Rotation.LeftDown) => PyramidScore,
            (Rotation.Down, Rotation.RightDown) => PyramidScore,
            (Rotation.Left, Rotation.LeftUp) => PyramidScore,
            (Rotation.Left, Rotation.LeftDown) => PyramidScore,
            (Rotation.Right, Rotation.RightUp) => PyramidScore,
            (Rotation.Right, Rotation.RightDown) => PyramidScore,
            _ => 0
        };
    }

}








public record ImpactResult(LaserDirection? NewDirection, bool DestroyPiece, bool GameOver);
