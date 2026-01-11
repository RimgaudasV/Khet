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
    const pendingAgentMoveRef = React.useRef(null);

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

    const rows = board.cells;

    function clearSelection() {
        setSelectedPiece(null);
        setMoves([]);
        setValidRotations([]);
    }

    function handleLaserResult(data) {
        const LASER_SPEED = 100;
        const LASER_AFTER_DELAY = 500;

        const laserDuration = (data.laser?.length ?? 0) * LASER_SPEED;

        setBoard(data.board);
        setCurrentPlayer(data.currentPlayer);
        setLaserPath(data.laser ?? []);
        setDestroyedPiece(data.destroyedPiece ?? null);

        setTimeout(() => {
            if (data.destroyedPiece) {
                setDestroyedPiece(null);
                setExplosion(data.destroyedPiece);
                setTimeout(() => setExplosion(null), 300);
            }
        }, laserDuration);

        setTimeout(async () => {
            if (data.gameEnded) {
                setTimeout(() => {
                    setGameOver(true);
                    alert("Game over!");
                }, 500);
                return;
            }

            setLaserPath([]);

            if (data.currentPlayer === "Player2") {
                pendingAgentMoveRef.current = moveByAgent(data.board, data.currentPlayer)
                    .then(result => {
                        return result;
                    });
            } else {
                pendingAgentMoveRef.current = null;
            }

            if (pendingAgentMoveRef.current) {
                const agentResult = await pendingAgentMoveRef.current;
                pendingAgentMoveRef.current = null;
                handleLaserResult(agentResult);
            }
        }, laserDuration + LASER_AFTER_DELAY);
    }



    const handlePieceClick = async (x, y) => {
        const cell = rows[y][x];
        const piece = cell.piece;
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
    };



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
    };


    return (
        <div className="board-container">
            <div className="board-with-axes">
                <div className="y-axis">
                    {Array.from({ length: ROWS }, (_, i) => (
                        <div key={i} className="y-label">
                            {i}
                        </div>
                    ))}
                </div>

                <div className="board-and-top">
                    <div className="x-axis">
                        {Array.from({ length: COLS }, (_, i) => (
                            <div key={i} className="x-label">
                                {i}
                            </div>
                        ))}
                    </div>

                    <div className="board-wrapper" style={{ position: "relative" }}>
                        <div className="board" style={{ gridTemplateColumns: `repeat(${rows[0].length}, 50px)` }}>
                            {rows.map((row = [], y) =>
                                row.map((cell, x) => {
                                    const piece = cell.piece;
                                    const isMoveTarget = isHighlighted(moves, x, y);
                                    const isOwnPiece = piece && piece.owner === currentPlayer;
                                    const cellClass = `
                                        board-cell
                                        ${cell.isDisabled ? "disabled" : ""}
                                        ${cell.isDisabled ? cell.disabledFor.toLowerCase() : ""}
                                        ${isMoveTarget ? "highlight" : "default"}
                                        ${(isMoveTarget || (isOwnPiece && !cell.isDisabled)) ? "clickable" : ""}
                                    `;

                                    return (
                                        <div
                                            key={`${x}-${y}`}
                                            className={cellClass}
                                            onClick={() => {
                                                if (isHighlighted(moves, x, y)) {
                                                    handleMoveClick(x, y);
                                                } else if (cell.piece && cell.piece.owner === currentPlayer) {
                                                    handlePieceClick(x, y);
                                                }
                                            }}
                                        >
                                            <Piece piece={cell.piece} disabled={cell.isDisabled} disabledFor={cell.disabledFor} />


                                            {destroyedPiece &&
                                            destroyedPiece.position.x === x &&
                                            destroyedPiece.position.y === y && (
                                                <Piece
                                                    piece={{
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
                </div>
            </div>
        </div>
    );
}