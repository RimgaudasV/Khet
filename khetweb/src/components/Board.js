// src/components/Board.js
import React, { useState, useEffect } from "react";
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
    const [destroyedPiece, setDestroyedPiece] = useState(null);
    const [board, setBoard] = useState(null);
    const [currentPlayer, setCurrentPlayer] = useState(null);
    const [explosion, setExplosion] = useState(null);


    useEffect(() => {
        if (!game) return;

        setBoard(game.board);
        setCurrentPlayer(game.currentPlayer);
    }, [game]);


    const CELL_SIZE = 50;
    const GAP = 2;
    const COLS = 10;
    const ROWS = 8;

    const boardWidth  = COLS * CELL_SIZE + (COLS - 1) * GAP;
    const boardHeight = ROWS * CELL_SIZE + (ROWS - 1) * GAP;


    if (!game || !board) return <div>Loading...</div>;

    const rows = board.pieces;

    function clearSelection() {
        setSelectedPiece(null);
        setMoves([]);
        setValidRotations([]);
    }

    function handleLaserResult(data, triggerAgent = true) {
        const LASER_SPEED = 200;
        const LASER_AFTER_DELAY = 1000; // laser stays 1s after traversal

        const laserDuration = (data.laser?.length ?? 0) * LASER_SPEED;

        // 1️⃣ Apply move / rotation immediately
        setBoard(data.board);
        setCurrentPlayer(data.nextPlayer);

        // 2️⃣ Fire laser
        setLaserPath(data.laser ?? []);

        // 3️⃣ Keep destroyed piece visible during traversal
        setDestroyedPiece(data.destroyedPiece ?? null);

        // 4️⃣ END OF TRAVERSAL → destroy piece + explosion
        setTimeout(() => {
            if (data.destroyedPiece) {
                // remove piece immediately
                setDestroyedPiece(null);

                // trigger explosion
                setExplosion(data.destroyedPiece);

                // remove explosion after animation
                setTimeout(() => setExplosion(null), 300);
            }
        }, laserDuration);

        // 5️⃣ AFTER 1s → remove laser & continue game
        setTimeout(() => {
            setLaserPath([]);

            if (data.gameOver) {
                setGameOver(true);
                alert("Game over!");
                return;
            }

            if (triggerAgent && data.nextPlayer === "Player2") {
                AgentsTurn();
            }
        }, laserDuration + LASER_AFTER_DELAY);
    }








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
            const data = await getValidMoves({ x, y }, currentPlayer, board);
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
            const data = await rotatePiece(
                selectedPiece,
                direction,
                validRotations,
                board,
                currentPlayer
            );

            clearSelection();
            handleLaserResult(data);
        } catch (err) {
            console.error(err);
        }

        await AgentsTurn();
    };


    async function AgentsTurn() {
        if (currentPlayer !== "Player2" || gameOver) return;

        try {
            const data = await moveByAgent(board, currentPlayer);
            handleLaserResult(data);
        } catch (err) {
            console.error("Agent move failed:", err);
        }
    }


    const handleMoveClick = async (toX, toY) => {
        if (!selectedPiece) return;

        try {
            const data = await makeMove(
                currentPlayer,
                board,
                { X: selectedPiece.x, Y: selectedPiece.y },
                { X: toX, Y: toY }
            );

            clearSelection();
            handleLaserResult(data);
        } catch (err) {
            console.error(err);
        }

        await AgentsTurn();
    };

    function getHitIndex(laserPath, destroyedPiece) {
        return laserPath.findIndex(
            p =>
                p.x === destroyedPiece.position.x &&
                p.y === destroyedPiece.position.y
        );
    }


    return (
        <div className="board-wrapper" style={{ position: "relative" }}>
            <div className="board" style={{ gridTemplateColumns: `repeat(${rows[0].length}, 50px)` }}>
                {rows.map((row = [], y) =>
                    row.map((cell, x) => {
                        const isMoveTarget = isHighlighted(moves, x, y);
                        const isOwnPiece = cell && cell.owner === currentPlayer;
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
                                    } else if (cell && cell.owner === currentPlayer) {
                                        handlePieceClick(x, y);
                                    }
                                }}
                            >
                                <Piece cell={cell}/>

                                {destroyedPiece &&
                                destroyedPiece.position.x === x &&
                                destroyedPiece.position.y === y && (
                                    <Piece
                                        cell={{
                                            type: destroyedPiece.type,
                                            owner: destroyedPiece.owner,
                                            rotation: destroyedPiece.rotation
                                        }}
                                    />
                                )}
                                {explosion &&
                                explosion.position.x === x &&
                                explosion.position.y === y && (
                                    <div className="explosion" />
                                )}

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

            <Laser
                path={laserPath}
                cellSize={CELL_SIZE}
                gap={GAP}
                width={boardWidth}
                height={boardHeight}
            />

        </div>
    );
}
