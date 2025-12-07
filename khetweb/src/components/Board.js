// src/components/Board.js
import React, { useState } from "react";
import Piece from "./Piece";
import { isHighlighted, isLaserCell } from "../services/game-service";
import { getValidMoves, makeMove } from "../services/api-service";
import Laser from "./Laser";
import "../styles/Board.css";

export default function Board({ board, player }) {
    const [moves, setMoves] = useState(null);
    const [selectedPiece, setSelectedPiece] = useState(null);
    const [laserPath, setLaserPath] = useState([]);

    if (!board) return <div>Loading...</div>;

    const rows = board.board.pieces;

    const handlePieceClick = async (x, y) => {
        const piece = rows[y][x];
        if (!piece || !piece.isMovable) return;

        setSelectedPiece({ x, y });

        try {
            const data = await getValidMoves({ x, y }, player, board.board);
            setMoves(data);
        } catch (err) {
            console.error(err);
        }
    };

    const handleMoveClick = async (toX, toY) => {
        if (!selectedPiece) return;

        try {
            const data = await makeMove(player, board.board, { x: selectedPiece.x, y: selectedPiece.y }, { x: toX, y: toY });
            
            board.board = data.board;
            board.player = data.currentPlayer;

            setSelectedPiece(null);
            setMoves(null);
            setLaserPath(data.laser ?? []);

            if (data.gameEnded) alert("Game over!");
        } catch (err) {
            console.error(err);
        }
    };

    return (
       <div className="board-wrapper" style={{ position: "relative" }}>
        <div className="board" style={{ gridTemplateColumns: `repeat(${rows[0].length}, 50px)` }}>
            {rows.map((row, y) =>
                row.map((cell, x) => {
                    const cellClass = isHighlighted(moves, x, y) ? "board-cell highlight"
                                    : "board-cell default";

                    return (
                        <div
                            key={`${x}-${y}`}
                            className={cellClass}
                            onClick={() => {
                                if (isHighlighted(moves, x, y)) handleMoveClick(x, y);
                                else if (cell) handlePieceClick(x, y);
                            }}
                        >
                            <Piece cell={cell} />
                        </div>
                    );
                })
            )}
        </div>

        {/* Laser overlay */}
        <Laser path={laserPath} cellSize={50} />
    </div>
    );
}
