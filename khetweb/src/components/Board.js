// src/components/Board.js
import React, { useState } from "react";
import Piece from "./Piece";
import { rotatePiece, isHighlighted } from "../services/game-service";
import { getValidMoves, makeMove, moveByAgent } from "../services/api-service";
import Laser from "./Laser";
import "../styles/Board.css";

export default function Board({game}) {
    const [moves, setMoves] = useState([]);
    const [selectedPiece, setSelectedPiece] = useState(null);
    const [laserPath, setLaserPath] = useState([]);
    const [validRotations, setValidRotations] = useState([]);
    const [gameOver, setGameOver] = useState(false);


    if (!game) return <div>Loading...</div>;

    const rows = game.board.pieces; // use board directly

    const handlePieceClick = async (x, y) => {
        const piece = rows[y][x];
        if (!piece) return;
        if (selectedPiece && selectedPiece.x === x && selectedPiece.y === y) {
            setSelectedPiece(null);
            setMoves([]);
            setValidRotations([]);
            return;
        }

        setSelectedPiece({ x, y });

        try {
            const data = await getValidMoves({ x, y }, game.currentPlayer, game.board);
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
            game.currentPlayer = data.currentPlayer;
            setLaserPath(data.laser ?? []);
            setSelectedPiece(null);
            setMoves([]);
            setValidRotations([]);

            if (data.gameEnded) {
                setGameOver(true);
                alert("Game over!");
            }

        } catch (err) {
            console.error(err);
        }
        await AgentsTurn(); 
    };

    async function AgentsTurn() {
        if (game.currentPlayer !== "Player2" || gameOver) return;

        try {
            const agentResult = await moveByAgent(game.board, game.currentPlayer);

            game.board = agentResult.board;
            game.currentPlayer = agentResult.currentPlayer;
            setLaserPath(agentResult.laser ?? []);

        } catch (err) {
            console.error("Agent move failed:", err);
        }
    }


    const handleMoveClick = async (toX, toY) => {
        if (!selectedPiece) return;

        try {
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

            if (data.gameEnded) {
                setGameOver(true);
                alert("Game over!");
            }

        } catch (err) {
            console.error(err);
        }
        await AgentsTurn(); 
    };

    return (
        <div className="board-wrapper" style={{ position: "relative" }}>
            <div className="board" style={{ gridTemplateColumns: `repeat(${rows[0].length}, 50px)` }}>
                {rows.map((row = [], y) =>
                    row.map((cell, x) => {
                        const isMoveTarget = isHighlighted(moves, x, y);
                        const isOwnPiece = cell && cell.owner === game.currentPlayer;
                        const isDisabledCell =
                            (!cell) && (
                                x === 0 || x === 9 || 
                                (x === 1 && (y === 0 || y === 7)) || 
                                (x === 8 && (y === 0 || y === 7))
                            );


                        const cellClass = `
                            board-cell
                            ${isMoveTarget ? "highlight" : "default"}
                            ${(isMoveTarget || isOwnPiece) ? "clickable" : ""}
                            ${isDisabledCell ? "disabled" : ""}
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
                                <Piece cell={cell}/>
                                
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
