using KhetApi.Entities.Board;
using KhetApi.Entities.Game;
using KhetApi.Entities.Move;
using KhetApi.Entities.Piece;
using KhetApi.Interfaces;
using KhetApi.Responses;

namespace KhetApi.Services;

public class GameService : IGameService
{
    public BoardEntity StartGame()
    {
        var game = new GameEntity();
        var board = game.Board;

        var dto = new BoardEntity();

        for (int y = 0; y < GameConstants.Rows; y++)
        {
            var row = new List<PieceEntity?>();

            for (int x = 0; x < GameConstants.Cols; x++)
            {
                row.Add(board.Pieces[y][x]);
            }

            dto.Pieces.Add(row);
        }

        return dto;
    }

    public MoveResponse MakeMove(MoveEntity move)
    {
        throw new NotImplementedException();
    }
}

