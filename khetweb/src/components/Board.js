import React, { useState, useEffect } from "react";
import Piece from "./Piece";
import { rotatePiece, isHighlighted } from "../services/game-service";
import { getValidMoves, makeMove, moveByAgent } from "../services/api-service";
import Laser from "./Laser";
import "../styles/Board.css";

const PLAYER_ONE_AGENT = true;
const PLAYER_TWO_AGENT = true;
const PLAYER_ONE_AGENT_DEPTH = 2;
const PLAYER_TWO_AGENT_DEPTH = 2;

export default function Board({game}) {
    const [moves, setMoves] = useState([]);
    const [selectedPiece, setSelectedPiece] = useState(null);
    const [laserPath, setLaserPath] = useState([]);
    const [validRotations, setValidRotations] = useState([]);
    const [gameStarted, setGameStarted] = useState(false);
    const [gameOver, setGameOver] = useState(false);
    const [destroyedPiece, setDestroyedPiece] = useState(null);
    const [board, setBoard] = useState(null);
    const [currentPlayer, setCurrentPlayer] = useState(null);
    const [explosion, setExplosion] = useState(null);
    const [isProcessing, setIsProcessing] = useState(false);

    const bothAgents = PLAYER_ONE_AGENT && PLAYER_TWO_AGENT;

    const LASER_SPEED = bothAgents ? 0 : 100;
    const LASER_AFTER_DELAY = bothAgents ? 0 : 500;
    const EXPLOSION_DURATION = bothAgents ? 100 : 300;

    const isCurrentPlayerAgent = (player) => {
        return player === "Player1" ? PLAYER_ONE_AGENT : PLAYER_TWO_AGENT;
    };

    const getAgentDepth = (player) => {
        return player === "Player1" ? PLAYER_ONE_AGENT_DEPTH : PLAYER_TWO_AGENT_DEPTH;
    };

    function handleStartGame() {
        setGameStarted(true);
        if (game && isCurrentPlayerAgent(game.currentPlayer)) {
            setIsProcessing(true);
            moveByAgent(
                game.board, 
                game.currentPlayer, 
                getAgentDepth(game.currentPlayer)
            ).then(result => {
                setIsProcessing(false);
                handleLaserResult(result);
            }).catch(err => {
                console.error("Agent move failed:", err);
                setIsProcessing(false);
            });
        }
    }

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
        const laserDuration = (data.laser?.length ?? 0) * LASER_SPEED;

        setBoard(data.board);
        setCurrentPlayer(data.currentPlayer);
        setLaserPath(data.laser ?? []);
        setDestroyedPiece(data.destroyedPiece ?? null);

        setTimeout(() => {
            if (data.destroyedPiece) {
                setDestroyedPiece(null);
                setExplosion(data.destroyedPiece);
                setTimeout(() => setExplosion(null), EXPLOSION_DURATION);
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

            if (!bothAgents) {
                setLaserPath([]);
            }

            const shouldAgentMove = isCurrentPlayerAgent(data.currentPlayer);

            if (shouldAgentMove) {
                setIsProcessing(true);
                try {
                    const agentResult = await moveByAgent(
                        data.board, 
                        data.currentPlayer, 
                        getAgentDepth(data.currentPlayer)
                    );
                    setIsProcessing(false);
                    handleLaserResult(agentResult);
                } catch (err) {
                    console.error("Agent move failed:", err);
                    setIsProcessing(false);
                }
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
                    {!gameStarted ? (
                        <div className="start-game-container">
                            <button className="start-game-button" onClick={handleStartGame}>
                                Start Game
                            </button>
                        </div>
                    ) : (
                        <div className="current-player-display">
                            {(currentPlayer === "Player1" ? "Blue" : "Red")}'s turn{" "}
                            {isCurrentPlayerAgent(currentPlayer) ? "(Agent)" : "(Human)"}
                            {isProcessing && " - Thinking..."}
                        </div>
                    )}
                    
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
                                    const isHumanTurn = !isCurrentPlayerAgent(currentPlayer);
                                    const cellClass = `
                                        board-cell
                                        ${cell.isDisabled ? "disabled" : ""}
                                        ${cell.isDisabled ? cell.disabledFor.toLowerCase() : ""}
                                        ${isMoveTarget ? "highlight" : "default"}
                                        ${(isMoveTarget || (isOwnPiece && !cell.isDisabled && isHumanTurn)) ? "clickable" : ""}
                                    `;

                                    return (
                                        <div
                                            key={`${x}-${y}`}
                                            className={cellClass}
                                            onClick={() => {
                                                if (!gameStarted || isProcessing) return;
                                                
                                                if (!isHumanTurn) return;
                                                
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
                            animated={!bothAgents}
                        />
                    </div>

                    {selectedPiece && validRotations.length > 0 && !isCurrentPlayerAgent(currentPlayer) && gameStarted && !isProcessing && (
                        <div className="rotation-controls">
                            <span className="rotation-label">Rotate piece:</span>
                            <button onClick={() => handleRotate(-1)}>⟲ Counter-clockwise</button>
                            <button onClick={() => handleRotate(1)}>⟳ Clockwise</button>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}