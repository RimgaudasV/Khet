// src/components/Board.js
import React, { useState } from "react";
import Piece from "./Piece";
import { rotatePiece, isHighlighted } from "../services/game-service";
import { getValidMoves, makeMove } from "../services/api-service";
import Laser from "./Laser";
import "../styles/Board.css";

export default function Board({game}) {
    const [moves, setMoves] = useState([]);
    const [selectedPiece, setSelectedPiece] = useState(null);
    const [laserPath, setLaserPath] = useState([]);
    const [validRotations, setValidRotations] = useState([]);


    if (!game) return <div>Loading...</div>;

    const rows = game.board.pieces; // use board directly

    const handlePieceClick = async (x, y) => {
        const piece = rows[y][x];
        if (!piece || !piece.isMovable) return;
        if (selectedPiece && selectedPiece.x === x && selectedPiece.y === y) {
            setSelectedPiece(null);
            setMoves([]);
            setValidRotations([]);
            return;
        }

        setSelectedPiece({ x, y });

        try {
            const data = await getValidMoves({ x, y }, game.currentPlayer, game.board);
            debugger;
            setMoves(data.validPositions);
            const validRotationsAsStrings = data.validRotations.map(r => r.toString());
            setValidRotations(validRotationsAsStrings);
        } catch (err) {
            console.error(err);
        }
    };

    const handleRotate = async (direction) => {
        if (!selectedPiece) return;

        try {
            const data = await rotatePiece(selectedPiece, direction, validRotations, game);
            game.board = data.board;
            setLaserPath(data.laser ?? []);
            setSelectedPiece(null);
            setMoves([]);
            setValidRotations([]);
        } catch (err) {
            console.error(err);
        }
    };


    const handleMoveClick = async (toX, toY) => {
        if (!selectedPiece) return;

        try {
            debugger;
            const data = await makeMove(
                game.currentPlayer,
                game.board,
                { X: selectedPiece.x, Y: selectedPiece.y },
                { X: toX, Y: toY }
            );
            game.board = data.board;
            game.currentPlayer = data.currentPlayer;
            setSelectedPiece(null);
            setMoves(null);
            setValidRotations([]);
            setLaserPath(data.laser ?? []);

            if (data.gameEnded) alert("Game over!");
        } catch (err) {
            console.error(err);
        }
    };

    return (
        <div className="board-wrapper" style={{ position: "relative" }}>
            <div className="board" style={{ gridTemplateColumns: `repeat(${rows[0].length}, 50px)` }}>
                {rows.map((row = [], y) =>
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
                                
                                {
                                selectedPiece?.x === x && selectedPiece?.y === y && validRotations.length > 0 && (
                                    <div className="rotation-overlay">
                                        <button onClick={() => handleRotate(-1)}>⟲</button>
                                        <button onClick={() => handleRotate(1)}>⟳</button>
                                    </div>
                                )}
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
