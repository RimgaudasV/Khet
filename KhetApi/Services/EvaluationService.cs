using KhetApi.Interfaces;
using KhetApi.Models;
using KhetApi.Models.Board;
using KhetApi.Models.Piece;
using KhetApi.Utils;

namespace KhetApi.Services;

public class EvaluationService : IEvaluationService
{
    public double EvaluateBoard(BoardModel board, bool gameOver, int depth, Player? winner, Player rootPlayer, int maxDepth, EvaluationConfig evalConfig)
    {
        if (gameOver)
            return EvaluateTerminalState(depth, winner, rootPlayer, maxDepth);

        var boardInfo = GetBoardInfo(board);

        double score = 0;

        if (evalConfig.Weights.TryGetValue("Material", out var matW) && matW != 0)
            score += matW * EvaluateMaterial(boardInfo.Pieces, rootPlayer, evalConfig.PieceValues);

        if (evalConfig.Weights.TryGetValue("PharaohAlignment", out var alignWeight) && alignWeight != 0)
            score += alignWeight * EvaluatePharaohAlignment(boardInfo, rootPlayer);
        if (evalConfig.Weights.TryGetValue("PieceSquareTables", out var pstWeight) && pstWeight != 0)
            score += pstWeight * EvaluatePieceSquareTables(boardInfo.Pieces, rootPlayer);
        if (evalConfig.Weights.TryGetValue("LaserEntry", out var laserEntryWeight) && laserEntryWeight != 0)
            score += laserEntryWeight * EvaluateLaserEntry(board, rootPlayer, boardInfo);
        if (evalConfig.Weights.TryGetValue("Mobility", out var mobilityWeight) && mobilityWeight != 0)
            score += mobilityWeight * EvaluateMobility(board, boardInfo.Pieces, rootPlayer);
        if (evalConfig.Weights.TryGetValue("LaserReflectorAlignment", out var laserReflectorAlignmentWeight) && laserReflectorAlignmentWeight != 0)
            score += laserReflectorAlignmentWeight * EvaluateLaserReflectorAlignment(boardInfo.Pieces, rootPlayer, board);
        if (evalConfig.Weights.TryGetValue("LaserLength", out var laserLengthWeight) && laserLengthWeight != 0)
            score += laserLengthWeight * EvaluateLaserLength(board, rootPlayer);
        if (evalConfig.Weights.TryGetValue("DefensiveRotations", out var defensiveRotationsWeight) && defensiveRotationsWeight != 0)
            score += defensiveRotationsWeight * EvaluateDefensiveRotations(boardInfo, board, rootPlayer);
        if (evalConfig.Weights.TryGetValue("PharaohDefense", out var pdWeight) && pdWeight != 0)
            score += pdWeight * EvaluatePharaohDefense(boardInfo, board, rootPlayer);

        return score;
    }


    private BoardInfo GetBoardInfo(BoardModel board)
    {
        var pieces = new List<(PieceModel, Position)>();
        Position? player1PharaohPos = null;
        Position? player2PharaohPos = null;

        for (int y = 0; y < board.Cells.Length; y++)
        {
            for (int x = 0; x < board.Cells[y].Length; x++)
            {
                var piece = board.Cells[y][x].Piece;
                if (piece == null) continue;

                var pos = new Position(x, y);
                pieces.Add((piece, pos));

                if (piece.Type == PieceType.Pharaoh)
                {
                    if (piece.Owner == Player.Player1)
                        player1PharaohPos = pos;
                    else
                        player2PharaohPos = pos;
                }
            }
        }

        return new BoardInfo
        {
            Pieces = pieces,
            Player1PharaohPos = player1PharaohPos,
            Player2PharaohPos = player2PharaohPos
        };
    }


    private int EvaluateMaterial(List<(PieceModel piece, Position pos)> pieces, Player rootPlayer, PieceValues pieceValues)
    {
        int score = 0;

        foreach (var (piece, _) in pieces)
        {
            int value = piece.Type switch
            {
                PieceType.Pyramid => pieceValues.Pyramid,
                PieceType.Anubis  => pieceValues.Anubis,
                _ => 0
            };

            if (value == 0) continue;

            score += piece.Owner == rootPlayer ? value : -value;
        }

        return score;
    }


    private int EvaluatePharaohAlignment(BoardInfo boardInfo, Player rootPlayer)
    {
        var opponent      = rootPlayer == Player.Player1 ? Player.Player2 : Player.Player1;
        var oppPharaohPos  = rootPlayer == Player.Player1 ? boardInfo.Player2PharaohPos : boardInfo.Player1PharaohPos;
        var rootPharaohPos = rootPlayer == Player.Player1 ? boardInfo.Player1PharaohPos : boardInfo.Player2PharaohPos;

        return PharaohAlignmentScore(boardInfo.Pieces, rootPlayer, oppPharaohPos)
             - PharaohAlignmentScore(boardInfo.Pieces, opponent,   rootPharaohPos);
    }

    private static int PharaohAlignmentScore(List<(PieceModel piece, Position pos)> pieces, Player owner, Position? pharaohPos)
    {
        if (pharaohPos == null) return 0;
        int score = 0;

        foreach (var (piece, pos) in pieces)
        {
            if (piece.Owner != owner) continue;
            if (piece.Type != PieceType.Pyramid && piece.Type != PieceType.Scarab) continue;

            int proximity = Math.Max(Math.Abs(pos.X - pharaohPos.X), Math.Abs(pos.Y - pharaohPos.Y));

            score += proximity switch
            {
                <= 1 => piece.Type == PieceType.Scarab ? 10 : 8,
                <= 4 => piece.Type == PieceType.Scarab ? 6 : 4,
                _    => 0
            };
        }

        return score;
    }

    private static readonly int[,] AnubisPst =
    {
        { 0, -6,  0,  0,  0,  0,  0,  0,  6, 0 },
        { 0, -6,  0,  0,  0,  0,  0, 12, 12, 0 },
        { 0, -6,  0,  0,  0,  0,  6, 12, 12, 0 },
        { 0, -6,  0,  0,  0,  0,  6, 12, 12, 0 },
        { 0, -6,  0,  0,  0,  6, 14, 14, 14, 0 },
        { 0, -6,  0, 10, 10, 12, 14, 14, 14, 0 },
        { 0, -6, 12, 12, 12, 12, 14, 14, 14, 0 },
        { 0, -6, 12, 12, 12, 12, 12, 12, 12, 0 },
    };

    private static readonly int[,] PharaohPst =
    {
        {  0,  -6, -6, -6, -6, -6, -6, -6, -6, -6 },
        {  0,  -6,  0,  0,  0,  0,  0,  0, 12, 12 },
        {  0,  -6,  0,  0,  0,  0,  0,  6, 18, 12 },
        {  0,  -6,  0,  0,  0,  0,  6,  6, 18, 12 },
        {  0,  -6,  0,  0,  0,  0,  6, 12, 24, 12 },
        {  0,  -6,  0,  0,  6, 12, 12, 18, 24, 12 },
        {  0,  -6,  9, 10, 12, 18, 18, 24, 30, 12 },
        {  0,  -6,  9, 18, 18, 18, 12, 12, 12,  0 },
    };

    private static readonly int[,] ScarabPst =
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 8, 8, 8, 8, 8, 8, 0, 0 },
        { 0, 0, 8, 8, 8, 8, 8, 8, 0, 0 },
        { 0, 0, 8, 8, 8, 8, 8, 8, 0, 0 },
        { 0, 0, 8, 8, 8, 8, 8, 8, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    };

    private int EvaluatePieceSquareTables(List<(PieceModel piece, Position pos)> pieces, Player rootPlayer)
    {
        int score = 0;
        foreach (var (piece, pos) in pieces)
        {
            int[,]? pst = piece.Type switch
            {
                PieceType.Anubis  => AnubisPst,
                PieceType.Pharaoh => PharaohPst,
                PieceType.Scarab => ScarabPst,
                _ => null
            };
            if (pst == null) continue;

            int py = piece.Owner == Player.Player1 ? pos.Y : 7 - pos.Y;
            int px = piece.Owner == Player.Player1 ? pos.X : 9 - pos.X;

            int value = pst[py, px];
            score += piece.Owner == rootPlayer ? value : -value;
        }
        return score;
    }


    private int EvaluateLaserEntry(BoardModel board, Player rootPlayer, BoardInfo boardInfo)
    {
        var opponent = rootPlayer == Player.Player1 ? Player.Player2 : Player.Player1;
        return LaserEntryScore(board, rootPlayer, boardInfo) - LaserEntryScore(board, opponent, boardInfo);
    }

    private int LaserEntryScore(BoardModel board, Player player, BoardInfo boardInfo)
    {
        var enemyPharaohPos = player == Player.Player1
            ? boardInfo.Player2PharaohPos
            : boardInfo.Player1PharaohPos;
        if (enemyPharaohPos == null) return 0;

        int score = 0;

        foreach (var axis in new[] { Axis.X, Axis.Y })
        {
            var (first, second) = FindSupportersOnAxis(board, player, axis);
            var incoming = GetIncomingDirection(player, axis);

            if (first != null)
            {
                var piece = board.GetPieceAt(first)!;
                score += 14
                      + (IsCorrectRotation(piece, incoming, first, enemyPharaohPos) ? 9 : 0)
                      + PharaohAlignBonus(first, enemyPharaohPos, axis);
            }

            if (second != null)
            {
                var piece = board.GetPieceAt(second)!;
                score += 14
                      + (IsCorrectRotation(piece, incoming, second, enemyPharaohPos) ? 9 : 0)
                      + PharaohAlignBonus(second, enemyPharaohPos, axis);
            }
        }

        return score;
    }

    private (Position? first, Position? second) FindSupportersOnAxis(BoardModel board, Player player, Axis axis)
    {
        Position sphinxPos = player == Player.Player1 ? new Position(9, 7) : new Position(0, 0);
        int stepDir = player == Player.Player1 ? -1 : 1;
        Position? first = null;
        int piecesEncountered = 0;
        int step = 1;

        while (true)
        {
            var pos = new Position(
                axis == Axis.X ? sphinxPos.X + stepDir * step : sphinxPos.X,
                axis == Axis.Y ? sphinxPos.Y + stepDir * step : sphinxPos.Y
            );
            if (!board.IsInsideBoard(pos)) break;

            var piece = board.GetPieceAt(pos);
            if (piece == null) { step++; continue; }
            if (piece.Owner != player) break;

            piecesEncountered++;

            if (piece.Type == PieceType.Pyramid)
            {
                if (first == null) { first = pos; }
                else if (piecesEncountered == 2) { return (first, pos); }
                else break;
            }

            step++;
        }

        return (first, null);
    }

    private static LaserDirection GetIncomingDirection(Player player, Axis axis) =>
        (player, axis) switch
        {
            (Player.Player1, Axis.X) => LaserDirection.Left,
            (Player.Player1, Axis.Y) => LaserDirection.Up,
            (Player.Player2, Axis.X) => LaserDirection.Right,
            _                        => LaserDirection.Down,
        };

    private static bool IsCorrectRotation(PieceModel pyramid, LaserDirection incoming,
        Position pyramidPos, Position enemyPharaohPos)
    {
        var reflected = LaserUtil.Reflect(incoming, pyramid);
        return reflected switch
        {
            LaserDirection.Up    => enemyPharaohPos.Y < pyramidPos.Y,
            LaserDirection.Down  => enemyPharaohPos.Y > pyramidPos.Y,
            LaserDirection.Left  => enemyPharaohPos.X < pyramidPos.X,
            LaserDirection.Right => enemyPharaohPos.X > pyramidPos.X,
            _                    => false,
        };
    }

    private static int PharaohAlignBonus(Position pyramidPos, Position enemyPharaohPos, Axis axis)
    {
        int dist = axis == Axis.X
            ? Math.Abs(pyramidPos.X - enemyPharaohPos.X)
            : Math.Abs(pyramidPos.Y - enemyPharaohPos.Y);
        return 2 * Math.Max(0, 4 - dist);
    }


    private static readonly int[] MobilityDx = { -1, -1, -1,  0, 0,  1, 1, 1 };
    private static readonly int[] MobilityDy = { -1,  0,  1, -1, 1, -1, 0, 1 };

    private double EvaluateMobility(BoardModel board, List<(PieceModel piece, Position pos)> pieces, Player rootPlayer)
    {
        double score = 0;
        foreach (var (piece, pos) in pieces)
        {
            if (!piece.IsMovable) continue;

            int mobility = 0;
            for (int i = 0; i < 8; i++)
            {
                var neighbor = new Position(pos.X + MobilityDx[i], pos.Y + MobilityDy[i]);
                if (!board.IsInsideBoard(neighbor)) continue;

                var cell = board.Cells[neighbor.Y][neighbor.X];
                if (cell.IsDisabled && cell.DisabledFor != piece.Owner) continue;

                var occupant = cell.Piece;
                if (occupant == null)
                    mobility++;
                else if (piece.Type == PieceType.Scarab
                      && occupant.Owner != piece.Owner
                      && (occupant.Type == PieceType.Pyramid || occupant.Type == PieceType.Anubis))
                    mobility++;
            }
            double bonus = mobility / 5;
            score += piece.Owner == rootPlayer ? bonus : -bonus;
        }
        return score;
    }

    private int EvaluatePharaohDefense(BoardInfo boardInfo, BoardModel board, Player rootPlayer)
    {
        var opponent      = rootPlayer == Player.Player1 ? Player.Player2 : Player.Player1;
        var rootPharaohPos = rootPlayer == Player.Player1 ? boardInfo.Player1PharaohPos : boardInfo.Player2PharaohPos;
        var oppPharaohPos  = rootPlayer == Player.Player1 ? boardInfo.Player2PharaohPos : boardInfo.Player1PharaohPos;

        int rootDefense = rootPharaohPos != null ? PharaohDefenseScore(rootPharaohPos, board, rootPlayer) : 0;
        int oppDefense  = oppPharaohPos  != null ? PharaohDefenseScore(oppPharaohPos,  board, opponent)   : 0;

        return rootDefense - oppDefense;
    }

    private static readonly (int dx, int dy, int baseScore)[] NeighbourOffsets =
    [
        ( 0, -1, 2), ( 0,  1, 2), (-1,  0, 2), ( 1,  0, 2),
        (-1, -1, 2), ( 1, -1, 2), (-1,  1, 2), ( 1,  1, 2),
    ];

    private int PharaohDefenseScore(Position pharaohPos, BoardModel board, Player pharaohOwner)
    {
        int score = 0;

        if (pharaohPos.X == 0) score += 3;
        if (pharaohPos.X == 9) score += 3;
        if (pharaohPos.Y == 0) score += 3;
        if (pharaohPos.Y == 7) score += 3;


        foreach (var (dx, dy, baseScore) in NeighbourOffsets)
        {
            int x = pharaohPos.X + dx;
            int y = pharaohPos.Y + dy;
            if ((uint)x >= 10 || (uint)y >= 8) continue;

            var piece = board.Cells[y][x].Piece;
            if (piece == null) continue;

            if (piece.Owner == pharaohOwner)
            {
                var side = GetSideOfPharaoh(pharaohPos, new Position(x, y));

                score += piece.Type switch
                {
                    PieceType.Anubis  => baseScore + (side != null && side.Value == piece.Rotation ? 2 : 0),
                    PieceType.Pyramid => baseScore + (side == null ? 2 : 0),
                    _                 => 0
                };
            }
        }

        return score;
    }

    //private static int PyramidDefendPharoah(Rotation sideOfPharoah, Rotation rotation)
    //    => (sideOfPharoah, rotation) switch
    //    {
    //        (Rotation.Up,    Rotation.LeftUp)    => 1,
    //        (Rotation.Up,    Rotation.RightUp)   => 1,
    //        (Rotation.Down,  Rotation.LeftDown)  => 1,
    //        (Rotation.Down,  Rotation.RightDown) => 1,
    //        (Rotation.Left,  Rotation.LeftUp)    => 1,
    //        (Rotation.Left,  Rotation.LeftDown)  => 1,
    //        (Rotation.Right, Rotation.RightUp)   => 1,
    //        (Rotation.Right, Rotation.RightDown) => 1,
    //        _ => 0
    //    };

    private static Rotation? GetSideOfPharaoh(Position pharaohPos, Position piecePos)
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







    private static int EvaluateLaserReflectorAlignment(List<(PieceModel piece, Position pos)> pieces, Player rootPlayer, BoardModel board)
    {
        int score = 0;

        for (int i = 0; i < pieces.Count; i++)
        {
            var (piece, pos) = pieces[i];
            if (piece.Type != PieceType.Pyramid && piece.Type != PieceType.Scarab) continue;

            int alignCount = 0;
            for (int j = i + 1; j < pieces.Count; j++)
            {
                var (other, otherPos) = pieces[j];
                if (other.Type != PieceType.Pyramid && other.Type != PieceType.Scarab) continue;
                if (other.Owner != piece.Owner) continue;

                if (otherPos.Y == pos.Y || otherPos.X == pos.X)
                {
                    if (CountPiecesBetween(board, pos, otherPos) <= 1)
                        alignCount++;
                }
            }

            score += piece.Owner == rootPlayer ? alignCount : -alignCount;
        }

        return score;
    }


    private static double EvaluateLaserLength(BoardModel board, Player rootPlayer)
    {
        var opponent = rootPlayer == Player.Player1 ? Player.Player2 : Player.Player1;
        return 0.5 * (LaserUtil.TraceLength(board, rootPlayer) - LaserUtil.TraceLength(board, opponent));
    }


    private static int EvaluateDefensiveRotations(BoardInfo boardInfo, BoardModel board, Player rootPlayer)
    {
        int score = 0;

        foreach (var (piece, pos) in boardInfo.Pieces)
        {
            if (piece.Type != PieceType.Pyramid && piece.Type != PieceType.Scarab) continue;

            int penalty = 0;

            foreach (LaserDirection dir in AllLaserDirections)
            {
                var reflected = LaserUtil.Reflect(dir, piece);
                if (reflected == null) continue;

                var threatenedPiece = GetVulnerableFriendlyInLine(
                    pos,
                    reflected.Value,
                    piece.Owner,
                    board);

                if (threatenedPiece == null)
                    continue;

                penalty += threatenedPiece.Type == PieceType.Pharaoh
                    ? 10
                    : 3;
            }

            score += piece.Owner == rootPlayer ? -penalty : penalty;
        }

        return score;
    }

    private static readonly LaserDirection[] AllLaserDirections =
        [LaserDirection.Up, LaserDirection.Down, LaserDirection.Left, LaserDirection.Right];

    private static PieceModel? GetVulnerableFriendlyInLine(Position from, LaserDirection dir, Player owner, BoardModel board)
    {
        var (dx, dy) = LaserUtil.DirectionToDeltas(dir);

        int x = from.X + dx;
        int y = from.Y + dy;

        int encounteredPieces = 0;

        while (board.IsInsideBoard(new Position(x, y)))
        {
            var piece = board.Cells[y][x].Piece;

            if (piece != null)
            {
                encounteredPieces++;

                bool vulnerableFriendly =
                    piece.Owner == owner &&
                    LaserUtil.WouldKill(dir, piece);

                if (vulnerableFriendly)
                    return piece;

                if (encounteredPieces >= 2)
                    return null;
            }

            x += dx;
            y += dy;
        }

        return null;
    }

    private static int CountPiecesBetween(BoardModel board, Position a, Position b)
    {
        int count = 0;

        if (a.X == b.X)
        {
            int minY = Math.Min(a.Y, b.Y);
            int maxY = Math.Max(a.Y, b.Y);

            for (int y = minY + 1; y < maxY; y++)
            {
                if (board.Cells[y][a.X].Piece != null)
                    count++;
            }
        }
        else
        {
            int minX = Math.Min(a.X, b.X);
            int maxX = Math.Max(a.X, b.X);

            for (int x = minX + 1; x < maxX; x++)
            {
                if (board.Cells[a.Y][x].Piece != null)
                    count++;
            }
        }

        return count;
    }


    private int EvaluateTerminalState(int depth, Player? winner, Player rootPlayer, int maxDepth)
    {
        int bonus = (maxDepth - depth) * 10;

        if (winner == rootPlayer)
            return int.MaxValue - bonus;
        else
            return int.MinValue + bonus;
    }



    //private GamePhase DetermineGamePhase(int totalPieces, int rootPieces, int opponentPieces)
    //{

    //    if (totalPieces > 22 && (rootPieces >= 10 || opponentPieces >= 10))
    //        return GamePhase.Start;

    //    else if (totalPieces >= 18 && (rootPieces >= 8 || opponentPieces >= 8))
    //        return GamePhase.Middlegame;

    //    else if (totalPieces >= 12 && (rootPieces > 5 || opponentPieces > 5))
    //        return GamePhase.NearEnd;
    //    else
    //        return GamePhase.EndGame;

    //}
}
