import React from "react";
import "../styles/Settings.css";

export default function Settings({
    playerOneAgent,
    setPlayerOneAgent,
    playerTwoAgent,
    setPlayerTwoAgent,
    playerOneDepth,
    setPlayerOneDepth,
    playerTwoDepth,
    setPlayerTwoDepth,
    totalGames,
    setTotalGames,
    downloadOverallStats,
    setDownloadOverallStats,
    downloadPerTurnCSV,
    setDownloadPerTurnCSV,
    moveDelay,
    setMoveDelay,
    gameStarted,
    onStartGame,
    onRestart,
    gamesCompleted,
    allGamesFinished,
    playerOneEvalMaterial,
    setPlayerOneEvalMaterial,
    playerTwoEvalMaterial,
    setPlayerTwoEvalMaterial,
    playerOneEvalAlignment,
    setPlayerOneEvalAlignment,
    playerTwoEvalAlignment,
    setPlayerTwoEvalAlignment,
    playerOneEvalSphinx,
    setPlayerOneEvalSphinx,
    playerTwoEvalSphinx,
    setPlayerTwoEvalSphinx,
    playerOnePieceValuePyramid,
    setPlayerOnePieceValuePyramid,
    playerTwoPieceValuePyramid,
    setPlayerTwoPieceValuePyramid,
    playerOnePieceValueAnubis,
    setPlayerOnePieceValueAnubis,
    playerTwoPieceValueAnubis,
    setPlayerTwoPieceValueAnubis,
    playerOnePieceValuePharaoh,
    setPlayerOnePieceValuePharaoh,
    playerTwoPieceValuePharaoh,
    setPlayerTwoPieceValuePharaoh,
}) {
    const makePieceValueHandler = (setter) => (e) => {
        const val = e.target.value;
        if (val === '') { setter(''); return; }
        const num = parseInt(val);
        if (!isNaN(num)) setter(num);
    };
    const makePieceValueBlurHandler = (setter) => (e) => {
        if (e.target.value === '') setter(0);
    };
    return (
        <div className="settings-sidebar">
            <h2>Game Configuration</h2>
            
            <div className="settings-section">
                <h3>Player 1 (Blue)</h3>
                <label className="checkbox-label">
                    <input
                        type="checkbox"
                        checked={playerOneAgent}
                        onChange={(e) => setPlayerOneAgent(e.target.checked)}
                        disabled={gameStarted}
                    />
                    AI Agent
                </label>
                {playerOneAgent && (
                    <div className="depth-control">
                        <label>
                            Search Depth:
                            <input
                                type="number"
                                min="1"
                                max="10"
                                value={playerOneDepth}
                                onChange={(e) => {
                                    const val = e.target.value;
                                    if (val === '') {
                                        setPlayerOneDepth('');
                                    } else {
                                        const num = parseInt(val);
                                        if (!isNaN(num)) setPlayerOneDepth(num);
                                    }
                                }}
                                onBlur={(e) => {
                                    if (e.target.value === '') {
                                        setPlayerOneDepth(1);
                                    }
                                }}
                                disabled={gameStarted}
                            />
                        </label>
                        <div className="eval-methods">
                            <span className="eval-methods-label">Evaluation methods:</span>
                            <label className="checkbox-label">
                                <input type="checkbox" checked={playerOneEvalMaterial}
                                    onChange={(e) => setPlayerOneEvalMaterial(e.target.checked)}
                                    disabled={gameStarted} />
                                Material score
                            </label>
                            {playerOneEvalMaterial && (
                                <div className="piece-values">
                                    <label>Pyramid: <input type="number" min="0" value={playerOnePieceValuePyramid} onChange={makePieceValueHandler(setPlayerOnePieceValuePyramid)} onBlur={makePieceValueBlurHandler(setPlayerOnePieceValuePyramid)} disabled={gameStarted} /></label>
                                    <label>Anubis: <input type="number" min="0" value={playerOnePieceValueAnubis} onChange={makePieceValueHandler(setPlayerOnePieceValueAnubis)} onBlur={makePieceValueBlurHandler(setPlayerOnePieceValueAnubis)} disabled={gameStarted} /></label>
                                    <label>Pharaoh: <input type="number" min="0" value={playerOnePieceValuePharaoh} onChange={makePieceValueHandler(setPlayerOnePieceValuePharaoh)} onBlur={makePieceValueBlurHandler(setPlayerOnePieceValuePharaoh)} disabled={gameStarted} /></label>
                                </div>
                            )}
                            <label className="checkbox-label">
                                <input type="checkbox" checked={playerOneEvalAlignment}
                                    onChange={(e) => setPlayerOneEvalAlignment(e.target.checked)}
                                    disabled={gameStarted} />
                                Pharaoh alignment
                            </label>
                            <label className="checkbox-label">
                                <input type="checkbox" checked={playerOneEvalSphinx}
                                    onChange={(e) => setPlayerOneEvalSphinx(e.target.checked)}
                                    disabled={gameStarted} />
                                Sphinx support
                            </label>
                        </div>
                    </div>
                )}
            </div>

            <div className="settings-section">
                <h3>Player 2 (Red)</h3>
                <label className="checkbox-label">
                    <input
                        type="checkbox"
                        checked={playerTwoAgent}
                        onChange={(e) => setPlayerTwoAgent(e.target.checked)}
                        disabled={gameStarted}
                    />
                    AI Agent
                </label>
                {playerTwoAgent && (
                    <div className="depth-control">
                        <label>
                            Search Depth:
                            <input
                                type="number"
                                min="1"
                                max="10"
                                value={playerTwoDepth}
                                onChange={(e) => {
                                    const val = e.target.value;
                                    if (val === '') {
                                        setPlayerTwoDepth('');
                                    } else {
                                        const num = parseInt(val);
                                        if (!isNaN(num)) setPlayerTwoDepth(num);
                                    }
                                }}
                                onBlur={(e) => {
                                    if (e.target.value === '') {
                                        setPlayerTwoDepth(1);
                                    }
                                }}
                                disabled={gameStarted}
                            />
                        </label>
                        <div className="eval-methods">
                            <span className="eval-methods-label">Evaluation methods:</span>
                            <label className="checkbox-label">
                                <input type="checkbox" checked={playerTwoEvalMaterial}
                                    onChange={(e) => setPlayerTwoEvalMaterial(e.target.checked)}
                                    disabled={gameStarted} />
                                Material score
                            </label>
                            {playerTwoEvalMaterial && (
                                <div className="piece-values">
                                    <label>Pyramid: <input type="number" min="0" value={playerTwoPieceValuePyramid} onChange={makePieceValueHandler(setPlayerTwoPieceValuePyramid)} onBlur={makePieceValueBlurHandler(setPlayerTwoPieceValuePyramid)} disabled={gameStarted} /></label>
                                    <label>Anubis: <input type="number" min="0" value={playerTwoPieceValueAnubis} onChange={makePieceValueHandler(setPlayerTwoPieceValueAnubis)} onBlur={makePieceValueBlurHandler(setPlayerTwoPieceValueAnubis)} disabled={gameStarted} /></label>
                                    <label>Pharaoh: <input type="number" min="0" value={playerTwoPieceValuePharaoh} onChange={makePieceValueHandler(setPlayerTwoPieceValuePharaoh)} onBlur={makePieceValueBlurHandler(setPlayerTwoPieceValuePharaoh)} disabled={gameStarted} /></label>
                                </div>
                            )}
                            <label className="checkbox-label">
                                <input type="checkbox" checked={playerTwoEvalAlignment}
                                    onChange={(e) => setPlayerTwoEvalAlignment(e.target.checked)}
                                    disabled={gameStarted} />
                                Pharaoh alignment
                            </label>
                            <label className="checkbox-label">
                                <input type="checkbox" checked={playerTwoEvalSphinx}
                                    onChange={(e) => setPlayerTwoEvalSphinx(e.target.checked)}
                                    disabled={gameStarted} />
                                Sphinx support
                            </label>
                        </div>
                    </div>
                )}
            </div>

            <div className="settings-section">
                <h3>Game Settings</h3>
                <label>
                    Total Games:
                    <input
                        type="number"
                        min="1"
                        max="100"
                        value={totalGames}
                        onChange={(e) => {
                            const val = e.target.value;
                            if (val === '') {
                                setTotalGames('');
                            } else {
                                const num = parseInt(val);
                                if (!isNaN(num)) setTotalGames(num);
                            }
                        }}
                        onBlur={(e) => {
                            if (e.target.value === '') {
                                setTotalGames(1);
                            }
                        }}
                        disabled={gameStarted}
                    />
                </label>

                <label>
                    Move Delay (ms):
                    <input
                        type="number"
                        min="0"
                        max="500"
                        step="10"
                        value={moveDelay}
                        onChange={(e) => {
                            const val = e.target.value;
                            if (val === '') {
                                setMoveDelay('');
                            } else {
                                const num = parseInt(val);
                                if (!isNaN(num)) setMoveDelay(num);
                            }
                        }}
                        onBlur={(e) => {
                            if (e.target.value === '') {
                                setMoveDelay(0);
                            }
                        }}
                        disabled={gameStarted}
                    />
                </label>
                
                <label className="checkbox-label">
                    <input
                        type="checkbox"
                        checked={downloadOverallStats}
                        onChange={(e) => setDownloadOverallStats(e.target.checked)}
                    />
                    Download overall stats (.txt)
                </label>

                <label className="checkbox-label">
                    <input
                        type="checkbox"
                        checked={downloadPerTurnCSV}
                        onChange={(e) => setDownloadPerTurnCSV(e.target.checked)}
                    />
                    Download per-turn routes (.csv)
                </label>
            </div>

            {!gameStarted ? (
                <button className="start-game-button" onClick={onStartGame}>
                    Start Game
                </button>
            ) : (
                <div className="game-status">
                    <p className="status-text">
                        {allGamesFinished ? (
                            `All ${totalGames} games completed!`
                        ) : (
                            <>Game {gamesCompleted + 1}/{totalGames}</>
                        )}
                    </p>
                    {allGamesFinished && (
                        <button className="start-game-button" onClick={onRestart}>
                            Run Again
                        </button>
                    )}
                </div>
            )}
        </div>
    );
}
