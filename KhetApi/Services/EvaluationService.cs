using KhetApi.Interfaces;
using KhetApi.Models;
using KhetApi.Models.Board;
using KhetApi.Models.Piece;

namespace KhetApi.Services;

public class EvaluationService : IEvaluationService
{
    private int MAX_DEPTH = 0;

    private readonly Dictionary<PieceType, int> PieceValues = new Dictionary<PieceType, int>{
        { PieceType.Pyramid, 10 },
        { PieceType.Anubis, 15 },
        { PieceType.Pharaoh, 200 }
    };


    public int EvaluateBoard(BoardModel board, bool gameOver, int depth, Player? winner, Player rootPlayer, int maxDepth)
    {
        MAX_DEPTH = maxDepth;

        //if (gameOver)
        //    return EvaluateTerminalState(depth, winner, rootPlayer);

        var boardInfo = GetBoardInfo(board);
        //var (rootCount, oppCount, total) = CountPieces(boardInfo.Pieces, rootPlayer);

        //var phase = DetermineGamePhase(total, rootCount, oppCount);

        int score = 0;

        score += EvaluateMaterial(boardInfo.Pieces, rootPlayer);
        //score += EvaluatePhaseSpecific(board, boardInfo.Pieces, rootPlayer, phase);
        score += EvaluatePharaohAlignment(boardInfo.Pieces, rootPlayer);
        score += EvaluateSphinxSupport(board, rootPlayer, boardInfo.Pieces);
        //score += EvaluatePharaohThreats(boardInfo.PharaohPosition, board, rootPlayer);

        score += Random.Shared.Next(-1, 2);

        return score;
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


    private int EvaluatePharaohAlignment(List<(PieceModel piece, Position pos)> pieces, Player rootPlayer)
    {
        int score = 0;

        Position? rootPharaohPos = null;
        Position? oppPharaohPos = null;

        foreach (var (piece, pos) in pieces)
        {
            if (piece.Type == PieceType.Pharaoh)
            {
                if (piece.Owner == rootPlayer)
                    rootPharaohPos = pos;
                else
                    oppPharaohPos = pos;
            }
        }

        if (oppPharaohPos == null)
            return 0;

        foreach (var (piece, pos) in pieces)
        {
            if (piece.Owner != rootPlayer)
                continue;

            if (piece.Type != PieceType.Scarab && piece.Type != PieceType.Pyramid)
                continue;

            bool xAligned = pos.X == oppPharaohPos.X ||
                            pos.X == oppPharaohPos.X - 1 ||
                            pos.X == oppPharaohPos.X + 1;

            bool yAligned = pos.Y == oppPharaohPos.Y ||
                            pos.Y == oppPharaohPos.Y - 1 ||
                            pos.Y == oppPharaohPos.Y + 1;

            int alignmentBonus = 0;


            bool close = false;
            
            if (xAligned)
            {
                close = Math.Abs(pos.Y - oppPharaohPos.Y) <= 4;
                alignmentBonus = piece.Type == PieceType.Scarab ? 3 : 2;
            }
            if (yAligned)
            {
                close = Math.Abs(pos.X - oppPharaohPos.X) <= 4;
                alignmentBonus = piece.Type == PieceType.Scarab ? 3 : 2;
            }

            if (close)
                alignmentBonus += 2;

            score += alignmentBonus;
        }

        return score;
    }


    private int EvaluateSphinxSupport(BoardModel board, Player rootPlayer, List<(PieceModel piece, Position pos)> pieces)
    {
        int XaxisSupportScore = 0,
            YaxisSupportScore = 0;

        foreach (var (piece, pos) in pieces)
        {
            if (piece.Type == PieceType.Pharaoh && piece.Owner != rootPlayer)
            {
                XaxisSupportScore = EvaluateAxisSupport(pos.X, board, Axis.X, rootPlayer);
                YaxisSupportScore = EvaluateAxisSupport(pos.Y, board, Axis.Y, rootPlayer);
                break;
            }
        }

        return XaxisSupportScore + YaxisSupportScore;
    }



    private int EvaluateAxisSupport(int enemyCoord, BoardModel board, Axis axis, Player rootPlayer)
    {
        Position sphinxPos = rootPlayer == Player.Player1? new Position(9, 7) : new Position(0, 0);
        var stepDirection = rootPlayer == Player.Player1 ? -1 : 1;

        var step = 1;
        int score = 0;


        while (true)
        {
            var currentPosition = new Position(
                axis == Axis.X ? sphinxPos.X + stepDirection * step : sphinxPos.X,
                axis == Axis.Y ? sphinxPos.Y + stepDirection * step : sphinxPos.Y
            );

            if(!board.IsInsideBoard(currentPosition))
            {
                break;
            }

            var piece = board.GetPieceAt(currentPosition);

            if (piece == null)
            {
                step++;
                continue;
            }

            if (piece.Owner != rootPlayer)
                break;

            if (piece.Type == PieceType.Pyramid || piece.Type == PieceType.Scarab) 
            {
                var pressureScore = AxisPressure(
                    axis == Axis.X ? currentPosition.X : currentPosition.Y,
                    enemyCoord
                );

                score = 2 + pressureScore - TooClosePenalty(axis, sphinxPos, currentPosition);
                break;
            }

            step++;

        }

        return score;
    }


    private int AxisPressure(int supportCoord, int enemyCoord)
    {
        int distance = Math.Abs(supportCoord - enemyCoord);
        return Math.Max(0, 6 - distance);
    }

    private int TooClosePenalty(Axis axis, Position sphinx, Position piece)
    {
        if (axis == Axis.Y)
        {
            int dist = Math.Abs(sphinx.Y - piece.Y);
            return dist < 3 ? 3 : 0;
        }
        else
        {
            int dist = Math.Abs(sphinx.X - piece.X);
            return dist < 3 ? 3 : 0;
        }
    }





    // Deprecated

    //private int PyramidDefendPharoah(Rotation sideOfPharoah, Rotation rotation)
    //{
    //    var PyramidScore = 1;
    //    return (sideOfPharoah, rotation) switch
    //    {
    //        (Rotation.Up, Rotation.LeftUp) => PyramidScore,
    //        (Rotation.Up, Rotation.RightUp) => PyramidScore,
    //        (Rotation.Down, Rotation.LeftDown) => PyramidScore,
    //        (Rotation.Down, Rotation.RightDown) => PyramidScore,
    //        (Rotation.Left, Rotation.LeftUp) => PyramidScore,
    //        (Rotation.Left, Rotation.LeftDown) => PyramidScore,
    //        (Rotation.Right, Rotation.RightUp) => PyramidScore,
    //        (Rotation.Right, Rotation.RightDown) => PyramidScore,
    //        _ => 0
    //    };
    //}

    //private int AnubisDefendPharoah(Rotation sideOfPharoah, Rotation rotation)
    //    => sideOfPharoah == rotation ? 2 : 1;


    //private int EvaluatePharaohThreats(Position pharaohPos, BoardModel board, Player rootPlayer)
    //{
    //    int score = 0;

    //    var directions = new (int dx, int dy)[]
    //    {
    //        (0,-1),(0,1),(-1,0),(1,0)
    //    };

    //    foreach (var (dx, dy) in directions)
    //    {

    //        int step = 1;

    //        while (true)
    //        {
    //            var pos = new Position(pharaohPos.X + dx * step, pharaohPos.Y + dy * step);
    //            if (!board.IsInsideBoard(pos)) break;

    //            var piece = board.GetPieceAt(pos);
    //            if (piece == null)
    //                break;

    //            if (piece.Owner == rootPlayer)
    //            {
    //                if (piece.Type == PieceType.Pyramid)
    //                {
    //                    var side = GetSideOfPharaoh(pharaohPos, pos);
    //                    if (side != null && PyramidDefendPharoah(side.Value, piece.Rotation) == 0)
    //                    {
    //                        score -= 2;
    //                    }
    //                }

    //                if (piece.Type == PieceType.Scarab)
    //                {
    //                    score -= 2;
    //                }
    //                break;
    //            }

    //            else
    //            {
    //                int threat = piece.Type switch
    //                {
    //                    PieceType.Scarab => 8,
    //                    PieceType.Pyramid => 6,
    //                    _ => 0
    //                };
    //                score -= threat;
    //            }

    //            step++;
    //        }
    //    }

    //    return score;
    //}




    //private int EvaluatePhaseSpecific(BoardModel board, List<(PieceModel piece, Position pos)> pieces, Player rootPlayer, GamePhase phase)
    //{
    //    int score = 0;

    //    foreach (var (piece, pos) in pieces)
    //    {
    //        if (piece.Type != PieceType.Pharaoh)
    //            continue;

    //        int defence = CheckPharaohDefence(pos, board, piece.Owner);

    //        int phaseMultiplier = phase switch
    //        {
    //            GamePhase.Start => 1,
    //            GamePhase.Middlegame => 1,
    //            GamePhase.NearEnd => 0,
    //            GamePhase.EndGame => 0,
    //            _ => 0
    //        };

    //        int value = defence * phaseMultiplier;

    //        score += piece.Owner == rootPlayer ? value : -value;
    //    }

    //    return score;
    //}


    //private int CheckPharaohDefence(Position pharaohPos, BoardModel board, Player owner)
    //{
    //    int score = 0;

    //    for (int y = 0; y < board.Cells.Length; y++)
    //    {
    //        for (int x = 0; x < board.Cells[y].Length; x++)
    //        {
    //            var piece = board.Cells[y][x].Piece;
    //            if (piece == null || piece.Owner != owner)
    //                continue;

    //            var piecePos = new Position(x, y);
    //            var side = GetSideOfPharaoh(pharaohPos, piecePos);

    //            if (side == null)
    //                continue;

    //            score += piece.Type switch
    //            {
    //                PieceType.Anubis => 1,
    //                PieceType.Pyramid => PyramidDefendPharoah(side.Value, piece.Rotation),
    //                _ => 0
    //            };
    //        }
    //    }

    //    return score;
    //}



    //private Rotation? GetSideOfPharaoh(Position pharaohPos, Position piecePos)
    //{
    //    int dx = piecePos.X - pharaohPos.X;
    //    int dy = piecePos.Y - pharaohPos.Y;

    //    if (dx == 0)
    //    {
    //        if (dy < 0) return Rotation.Up;
    //        if (dy > 0) return Rotation.Down;
    //    }

    //    if (dy == 0)
    //    {
    //        if (dx < 0) return Rotation.Left;
    //        if (dx > 0) return Rotation.Right;
    //    }

    //    return null;
    //}







    //private int EvaluateTerminalState(int depth, Player? winner, Player rootPlayer)
    //{
    //    int bonus = (MAX_DEPTH - depth) * 10;

    //    if (winner == rootPlayer)
    //        return int.MaxValue - bonus;
    //    else
    //        return int.MinValue + bonus;
    //}



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
