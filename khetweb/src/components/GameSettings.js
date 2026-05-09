import React from "react";
import "../styles/Settings.css";

export default function GameSettings({
    totalGames,
    setTotalGames,
    moveDelay,
    setMoveDelay,
    downloadOverallStats,
    setDownloadOverallStats,
    downloadPerTurnCSV,
    setDownloadPerTurnCSV,
    gameStarted,
    onStartGame,
    onRestart,
    gamesCompleted,
    allGamesFinished,
}) {
    return (
        <div className="game-settings-row">
            <div className="game-settings-inner">
                <label>
                    Total Games:
                    <input
                        type="number"
                        min="1"
                        max="100"
                        value={totalGames}
                        onChange={(e) => {
                            const val = e.target.value;
                            if (val === "") { setTotalGames(""); return; }
                            const num = parseInt(val);
                            if (!isNaN(num)) setTotalGames(num);
                        }}
                        onBlur={(e) => { if (e.target.value === "") setTotalGames(1); }}
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
                            if (val === "") { setMoveDelay(""); return; }
                            const num = parseInt(val);
                            if (!isNaN(num)) setMoveDelay(num);
                        }}
                        onBlur={(e) => { if (e.target.value === "") setMoveDelay(0); }}
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

                {!gameStarted ? (
                    <button className="start-game-button" onClick={onStartGame}>
                        Start Game
                    </button>
                ) : (
                    <div className="game-status">
                        <p className="status-text">
                            {allGamesFinished
                                ? `All ${totalGames} games completed!`
                                : <>Game {gamesCompleted + 1}/{totalGames}</>
                            }
                        </p>
                        {allGamesFinished && (
                            <button className="start-game-button" onClick={onRestart}>
                                Run Again
                            </button>
                        )}
                    </div>
                )}
            </div>
        </div>
    );
}
