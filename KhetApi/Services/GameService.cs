using Khet.Entities;
using Khet.Models;

namespace Khet.Services;

using System;

public static class GameService
{
    private static readonly GameConstants constants = new GameConstants();
    private static GameState _state;

    static GameService()
    {
        _state = CreateInitialBoard();
    }

    /// <summary>
    /// Returns the current state after performing a move.
    /// </summary>
    public static GameState GetState(MoveEnitity move)
    {
        if (move == null)
            throw new ArgumentNullException(nameof(move));

        // Handle rotation (no movement)
        if (move.NewRotation.HasValue)
        {
            RotatePiece(move);
        }
        else
        {
            MovePiece(move);
        }

        _state.MoveCount++;
        return _state;
    }

    // -------------------------------
    // Internal game logic
    // -------------------------------

    private static void MovePiece(MoveEnitity move)
    {
        if (!IsInBounds(move.FromRow, move.FromCol) ||
            !IsInBounds(move.ToRow, move.ToCol))
            throw new Exception("Move out of bounds.");

        var piece = _state.Board[move.FromRow, move.FromCol];
        if (piece == null)
            throw new Exception("No piece at source position.");

        if (_state.Board[move.ToRow, move.ToCol] != null)
            throw new Exception("Target cell is occupied.");

        // Move piece
        _state.Board[move.ToRow, move.ToCol] = piece;
        _state.Board[move.FromRow, move.FromCol] = null;
    }

    private static void RotatePiece(MoveEnitity move)
    {
        if (!IsInBounds(move.FromRow, move.FromCol))
            throw new Exception("Position out of bounds.");

        var piece = _state.Board[move.FromRow, move.FromCol];
        if (piece == null)
            throw new Exception("No piece to rotate.");

        piece.Rotation = ((move.NewRotation ?? 0) % 360 + 360) % 360;
    }

    private static bool IsInBounds(int r, int c)
    {
        return r >= 0 && c >= 0 && r < constants.Rows && c < constants.Cols;
    }

    private static GameState CreateInitialBoard()
    {
        var state = new GameState
        {
            Board = new Piece[constants.Rows, constants.Cols],
            MoveCount = 0
        };

        // Example setup: some red and blue pieces
        state.Board[0, 0] = new Piece { Id = "R1", Type = "L", Owner = "red", Rotation = 0 };
        state.Board[0, 1] = new Piece { Id = "R2", Type = "M", Owner = "red", Rotation = 90 };
        state.Board[7, 9] = new Piece { Id = "B1", Type = "S", Owner = "blue", Rotation = 180 };
        state.Board[7, 8] = new Piece { Id = "B2", Type = "E", Owner = "blue", Rotation = 270 };

        return state;
    }
}

