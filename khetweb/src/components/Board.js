import React, { useState, useEffect } from "react";
import Piece from "./Piece";
import AgentStats from "./Stats";
import { rotatePiece, isHighlighted } from "../services/game-service";
import { getValidMoves, makeMove, moveByAgent } from "../services/api-service";
import Laser from "./Laser";
import "../styles/Board.css";

const PLAYER_ONE_AGENT = true;
const PLAYER_TWO_AGENT = true;
const PLAYER_ONE_AGENT_DEPTH = 2;
const PLAYER_TWO_AGENT_DEPTH = 2;
const TOTAL_GAMES = 10;

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
    const [gamesCompleted, setGamesCompleted] = useState(0);
    const [allGamesFinished, setAllGamesFinished] = useState(false);
    const [allGamesData, setAllGamesData] = useState([]);
    const [currentGameWinner, setCurrentGameWinner] = useState(null);
    const [perTurnStats, setPerTurnStats] = useState([]);

    
    const [stats, setStats] = useState({
        player1Times: [],
        player2Times: [],
        player1AllMoves: [],
        player2AllMoves: [],
        player1AllRoutes: [],
        player2AllRoutes: [],
        player1EvaluatedRoutes: [],
        player2EvaluatedRoutes: [],
        player1Wins: 0,
        player2Wins: 0
    });


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
            const startTime = performance.now();
            moveByAgent(
                game.board, 
                game.currentPlayer, 
                getAgentDepth(game.currentPlayer)
            ).then(result => {
                const endTime = performance.now();
                const duration = endTime - startTime;
                updateStats(game.currentPlayer, duration, result.allMovesCount, result.allRoutesCount, result.evaluatedRoutesCount);
                setIsProcessing(false);
                handleLaserResult(result);
            }).catch(err => {
                console.error("Agent move failed:", err);
                setIsProcessing(false);
            });
        }
    }

    const updateStats = (player, duration, allMovesCount, allRoutesCount, evaluatedRoutesCount) => {
        setStats(prevStats => {
            const newStats = { ...prevStats };
            if (player === "Player1") {
            newStats.player1Times = [...prevStats.player1Times, duration];
            newStats.player1AllMoves = [...(prevStats.player1AllMoves || []), allMovesCount || 0];
            newStats.player1AllRoutes = [...(prevStats.player1AllRoutes || []), allRoutesCount || 0];
            newStats.player1EvaluatedRoutes = [...(prevStats.player1EvaluatedRoutes || []), evaluatedRoutesCount || 0];
            } else {
            newStats.player2Times = [...prevStats.player2Times, duration];
            newStats.player2AllMoves = [...(prevStats.player2AllMoves || []), allMovesCount || 0];
            newStats.player2AllRoutes = [...(prevStats.player2AllRoutes || []), allRoutesCount || 0];
            newStats.player2EvaluatedRoutes = [...(prevStats.player2EvaluatedRoutes || []), evaluatedRoutesCount || 0];
            }
            return newStats;
        });
        setPerTurnStats(prev => [
            ...prev,
            {
                turn: prev.length + 1,
                routes: allRoutesCount || 0,
                evaluated: evaluatedRoutesCount || 0
            }
        ]);

    };

const logGameStats = () => {
    const allLegalMoves = [
        ...(stats.player1AllMoves || []),
        ...(stats.player2AllMoves || [])
    ];

    const allRoutes = [
        ...(stats.player1AllRoutes || []),
        ...(stats.player2AllRoutes || [])
    ];

    const evaluatedRoutes = [
        ...(stats.player1EvaluatedRoutes || []),
        ...(stats.player2EvaluatedRoutes || [])
    ];

    const avgLegalMoves =
        allLegalMoves.length > 0
            ? allLegalMoves.reduce((s, v) => s + v, 0) / allLegalMoves.length
            : 0;

    const avgRoutes =
        allRoutes.length > 0
            ? allRoutes.reduce((s, v) => s + v, 0) / allRoutes.length
            : 0;

    const avgEvaluatedRoutes =
        evaluatedRoutes.length > 0
            ? evaluatedRoutes.reduce((s, v) => s + v, 0) / evaluatedRoutes.length
            : 0;

    const pruningPercentage =
        avgRoutes > 0
            ? ((avgRoutes - avgEvaluatedRoutes) / avgRoutes) * 100
            : 0;

    const turnsTaken =
        stats.player1Times.length + stats.player2Times.length;

    const avgPlayer1Time =
        stats.player1Times.length > 0
            ? stats.player1Times.reduce((s, v) => s + v, 0) / stats.player1Times.length
            : 0;

    const avgPlayer2Time =
        stats.player2Times.length > 0
            ? stats.player2Times.reduce((s, v) => s + v, 0) / stats.player2Times.length
            : 0;


    const gameData = {
        gameNumber: gamesCompleted + 1,
        winner: currentGameWinner,
        avgLegalMovesPerTurn: Number(avgLegalMoves.toFixed(1)),
        avgRoutes: Number(avgRoutes.toFixed(1)),
        avgEvaluatedRoutes: Number(avgEvaluatedRoutes.toFixed(1)),
        pruningPercentage: Number(pruningPercentage.toFixed(1)),
        avgPlayer1Time: Number(avgPlayer1Time.toFixed(1)),
        avgPlayer2Time: Number(avgPlayer2Time.toFixed(1)),
        turnsTaken
    };


    setAllGamesData(prev => [...prev, gameData]);
};


const downloadPerTurnStats = () => {
    if (perTurnStats.length === 0) return;

    let content = "Turn,TotalRoutes,EvaluatedRoutes\n";
    perTurnStats.forEach(row => {
        content += `${row.turn},${row.routes},${row.evaluated}\n`;
    });

    const blob = new Blob([content], { type: "text/csv" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "per_turn_routes.csv";
    a.click();
    URL.revokeObjectURL(url);
};



    const downloadGameStats = () => {
        if (allGamesData.length === 0) return;
        
        const totalAvgLegalMoves =
            allGamesData.reduce((s, g) => s + g.avgLegalMovesPerTurn, 0) / allGamesData.length;

        const totalAvgRoutes =
            allGamesData.reduce((s, g) => s + g.avgRoutes, 0) / allGamesData.length;

        const totalAvgEvaluatedRoutes =
            allGamesData.reduce((s, g) => s + g.avgEvaluatedRoutes, 0) / allGamesData.length;

        const totalAvgPruning =
            allGamesData.reduce((s, g) => s + g.pruningPercentage, 0) / allGamesData.length;

        const totalAvgTurns =
            allGamesData.reduce((s, g) => s + g.turnsTaken, 0) / allGamesData.length;

        const totalAvgPlayer1Time =
            allGamesData.reduce((s, g) => s + g.avgPlayer1Time, 0) / allGamesData.length;

        const totalAvgPlayer2Time =
            allGamesData.reduce((s, g) => s + g.avgPlayer2Time, 0) / allGamesData.length;

        let fileContent = '';
        allGamesData.forEach(game => {
            fileContent +=
                `Game ${game.gameNumber}: ` +
                `Winner: ${game.winner}, ` +
                `Avg legal moves: ${game.avgLegalMovesPerTurn}, ` +
                `Avg routes: ${game.avgRoutes}, ` +
                `Avg evaluated routes: ${game.avgEvaluatedRoutes}, ` +
                `Pruning: ${game.pruningPercentage}%, ` +
                `P1 avg time: ${game.avgPlayer1Time}ms, ` +
                `P2 avg time: ${game.avgPlayer2Time}ms, ` +
                `Turns: ${game.turnsTaken}\n`;
        });


        const totalGames = allGamesData.length;

        const wins = allGamesData.reduce(
            (acc, game) => {
                acc[game.winner] = (acc[game.winner] || 0) + 1;
                return acc;
            },
            {}
        );

        const player1Wins = wins.Player1 || 0;
        const player2Wins = wins.Player2 || 0;

        const player1WinRate = totalGames > 0
            ? ((player1Wins / totalGames) * 100).toFixed(1)
            : "0.0";

        const player2WinRate = totalGames > 0
            ? ((player2Wins / totalGames) * 100).toFixed(1)
            : "0.0";



        fileContent += `\nSummary (${totalGames} games):\n`;
        fileContent += `Player1 win rate: ${player1WinRate}% (${player1Wins} wins)\n`;
        fileContent += `Player2 win rate: ${player2WinRate}% (${player2Wins} wins)\n`;
        fileContent += `Avg legal moves per turn: ${totalAvgLegalMoves.toFixed(1)}\n`;
        fileContent += `Avg routes per turn: ${totalAvgRoutes.toFixed(1)}\n`;
        fileContent += `Avg evaluated routes per turn: ${totalAvgEvaluatedRoutes.toFixed(1)}\n`;
        fileContent += `Avg pruning percentage: ${totalAvgPruning.toFixed(1)}%\n`;
        fileContent += `Avg Player1 time per turn: ${totalAvgPlayer1Time.toFixed(1)}ms\n`;
        fileContent += `Avg Player2 time per turn: ${totalAvgPlayer2Time.toFixed(1)}ms\n`;
        fileContent += `Avg turns per game: ${totalAvgTurns.toFixed(1)}\n`;

        const blob = new Blob([fileContent], { type: 'text/plain' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${PLAYER_ONE_AGENT_DEPTH} vs ${PLAYER_TWO_AGENT_DEPTH}, ${TOTAL_GAMES} partiju.txt`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    };

    const resetGame = () => {
        setGameOver(false);
        setStats({
            player1Times: [],
            player2Times: [],
            player1AllMoves: [],
            player2AllMoves: [],
            player1AllRoutes: [],
            player2AllRoutes: [],
            player1EvaluatedRoutes: [],
            player2EvaluatedRoutes: [],
            player1Wins: 0,
            player2Wins: 0
        });
        setMoves([]);
        setSelectedPiece(null);
        setLaserPath([]);
        setValidRotations([]);
        setDestroyedPiece(null);
        setExplosion(null);
        setIsProcessing(false);
        
        if (game) {
            setBoard(game.board);
            setCurrentPlayer(game.currentPlayer);
        }
    };

    useEffect(() => {
        if (gameOver && (stats.player1Times.length > 0 || stats.player2Times.length > 0)) {
            logGameStats();
            
            const newGamesCompleted = gamesCompleted + 1;
            setGamesCompleted(newGamesCompleted);
            
            if (newGamesCompleted < TOTAL_GAMES) {
                setTimeout(() => {
                    resetGame();
                    setTimeout(() => {
                        handleStartGame();
                    }, 100);
                }, 500);
            } else {
                setAllGamesFinished(true);
                console.log(`All ${TOTAL_GAMES} games completed!`);
            }
        }
    }, [gameOver]);

    useEffect(() => {
        if (!game) return;

        setBoard(game.board);
        setCurrentPlayer(game.currentPlayer);
    }, [game]);

    useEffect(() => {
    if (allGamesFinished && allGamesData.length === TOTAL_GAMES) {
        downloadGameStats();
        downloadPerTurnStats();
        alert(`All ${TOTAL_GAMES} games completed! Stats file downloaded.`);
    }
}, [allGamesFinished, allGamesData]);


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
            setCurrentGameWinner(data.winner);

            setTimeout(() => {
                setGameOver(true);
                if (gamesCompleted + 1 >= TOTAL_GAMES) {
                    alert("Game over!");
                }
            }, 500);
            return;
        }

            if (!bothAgents) {
                setLaserPath([]);
            }

            const shouldAgentMove = isCurrentPlayerAgent(data.currentPlayer);

            if (shouldAgentMove) {
                setIsProcessing(true);
                const startTime = performance.now();
                try {
                    const agentResult = await moveByAgent(
                        data.board, 
                        data.currentPlayer, 
                        getAgentDepth(data.currentPlayer)
                    );
                    const endTime = performance.now();
                    const duration = endTime - startTime;
                    updateStats(data.currentPlayer, duration, agentResult.allMovesCount, agentResult.allRoutesCount, agentResult.evaluatedRoutesCount);
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
                            {allGamesFinished ? (
                                `All ${TOTAL_GAMES} games completed!`
                            ) : (
                                <>
                                    Game {gamesCompleted + 1}/{TOTAL_GAMES} - {(currentPlayer === "Player1" ? "Blue" : "Red")}'s turn{" "}
                                    {isCurrentPlayerAgent(currentPlayer) ? "(Agent)" : "(Human)"}
                                    {isProcessing && " - Thinking..."}
                                </>
                            )}
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
                    
                    {gameStarted && (stats.player1Times.length > 0 || stats.player2Times.length > 0) && (
                        <AgentStats stats={stats} />
                    )}
                </div>
            </div>
        </div>
    );
}