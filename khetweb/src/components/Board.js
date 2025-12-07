// src/components/Board.js
import React, { useState } from "react";
import Piece from "./Piece";
import { isHighlighted } from "../services/game-service";
import { getValidMoves, makeMove } from "../services/api-service";
import Laser from "./Laser";
import "../styles/Board.css";

export default function Board({game}) {
    const [moves, setMoves] = useState(null);
    const [selectedPiece, setSelectedPiece] = useState(null);
    const [laserPath, setLaserPath] = useState([]);

    if (!game) return <div>Loading...</div>;

    const rows = game.board.pieces; // use board directly

    const handlePieceClick = async (x, y) => {
        const piece = rows[y][x];
        if (!piece || !piece.isMovable) return;
        //if (piece.owner !== game.currentPlayer) return; 

        // Clicking the same piece again removes highlights
        if (selectedPiece && selectedPiece.x === x && selectedPiece.y === y) {
            setSelectedPiece(null);
            setMoves(null);
            return;
        }

        setSelectedPiece({ x, y });

        try {
            const data = await getValidMoves({ x, y }, game.currentPlayer, game.board);
            setMoves(data);
        } catch (err) {
            console.error(err);
        }
    };

    const handleMoveClick = async (toX, toY) => {
        if (!selectedPiece) return;

        try {
            const data = await makeMove(
                game.currentPlayer,
                game.board,
                { x: selectedPiece.x, y: selectedPiece.y },
                { x: toX, y: toY }
            );
            game.board = data.board;
            game.currentPlayer = data.currentPlayer;
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
                        const isMoveTarget = isHighlighted(moves, x, y);
                        const isOwnPiece = cell && cell.owner === game.currentPlayer;

                        const cellClass = `
                            board-cell
                            ${isMoveTarget ? "highlight" : "default"}
                            ${(isMoveTarget || isOwnPiece) ? "clickable" : ""}
                        `;

                        return (
                            <div
                                key={`${x}-${y}`}
                                className={cellClass}
                                onClick={() => {
                                    if (isHighlighted(moves, x, y)) {
                                        handleMoveClick(x, y);
                                    } else if (cell && cell.owner === game.currentPlayer) {
                                        handlePieceClick(x, y);
                                    }
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
