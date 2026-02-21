import React, { useState, useEffect } from "react";
import { startGame } from "./services/api-service";
import Board from "./components/Board";
import Settings from "./components/Settings";
import './App.css';

function App() {
    const [game, setGame] = useState(null);
    const [gameStarted, setGameStarted] = useState(false);
    const [gamesCompleted, setGamesCompleted] = useState(0);
    const [allGamesFinished, setAllGamesFinished] = useState(false);
    
    const [playerOneAgent, setPlayerOneAgent] = useState(() => {
        const saved = localStorage.getItem('playerOneAgent');
        return saved !== null ? JSON.parse(saved) : true;
    });
    const [playerTwoAgent, setPlayerTwoAgent] = useState(() => {
        const saved = localStorage.getItem('playerTwoAgent');
        return saved !== null ? JSON.parse(saved) : true;
    });
    const [playerOneDepth, setPlayerOneDepth] = useState(() => {
        const saved = localStorage.getItem('playerOneDepth');
        return saved !== null ? parseInt(saved) : 2;
    });
    const [playerTwoDepth, setPlayerTwoDepth] = useState(() => {
        const saved = localStorage.getItem('playerTwoDepth');
        return saved !== null ? parseInt(saved) : 2;
    });
    const [totalGames, setTotalGames] = useState(() => {
        const saved = localStorage.getItem('totalGames');
        return saved !== null ? parseInt(saved) : 10;
    });
    const [downloadOverallStats, setDownloadOverallStats] = useState(() => {
        const saved = localStorage.getItem('downloadOverallStats');
        return saved !== null ? JSON.parse(saved) : true;
    });
    const [downloadPerTurnCSV, setDownloadPerTurnCSV] = useState(() => {
        const saved = localStorage.getItem('downloadPerTurnCSV');
        return saved !== null ? JSON.parse(saved) : false;
    });
    const [moveDelay, setMoveDelay] = useState(() => {
        const saved = localStorage.getItem('moveDelay');
        return saved !== null ? parseInt(saved) : 0;
    });

    useEffect(() => {
        localStorage.setItem('playerOneAgent', JSON.stringify(playerOneAgent));
    }, [playerOneAgent]);

    useEffect(() => {
        localStorage.setItem('playerTwoAgent', JSON.stringify(playerTwoAgent));
    }, [playerTwoAgent]);

    useEffect(() => {
        localStorage.setItem('playerOneDepth', playerOneDepth.toString());
    }, [playerOneDepth]);

    useEffect(() => {
        localStorage.setItem('playerTwoDepth', playerTwoDepth.toString());
    }, [playerTwoDepth]);

    useEffect(() => {
        localStorage.setItem('totalGames', totalGames.toString());
    }, [totalGames]);

    useEffect(() => {
        localStorage.setItem('downloadOverallStats', JSON.stringify(downloadOverallStats));
    }, [downloadOverallStats]);

    useEffect(() => {
        localStorage.setItem('downloadPerTurnCSV', JSON.stringify(downloadPerTurnCSV));
    }, [downloadPerTurnCSV]);

    useEffect(() => {
        localStorage.setItem('moveDelay', moveDelay.toString());
    }, [moveDelay]);

    useEffect(() => {
        startGame().then(setGame).catch(console.error);
    }, []);

    const handleStartGame = () => {
        setGameStarted(true);
        setGamesCompleted(0);
        setAllGamesFinished(false);
    };

    const settings = {
        playerOneAgent,
        playerTwoAgent,
        playerOneDepth,
        playerTwoDepth,
        totalGames,
        downloadOverallStats,
        downloadPerTurnCSV,
        moveDelay
    };

return (
    <div className="app-container">
        <div className="game-layout">

            <Settings
                playerOneAgent={playerOneAgent}
                setPlayerOneAgent={setPlayerOneAgent}
                playerTwoAgent={playerTwoAgent}
                setPlayerTwoAgent={setPlayerTwoAgent}
                playerOneDepth={playerOneDepth}
                setPlayerOneDepth={setPlayerOneDepth}
                playerTwoDepth={playerTwoDepth}
                setPlayerTwoDepth={setPlayerTwoDepth}
                totalGames={totalGames}
                setTotalGames={setTotalGames}
                downloadOverallStats={downloadOverallStats}
                setDownloadOverallStats={setDownloadOverallStats}
                downloadPerTurnCSV={downloadPerTurnCSV}
                setDownloadPerTurnCSV={setDownloadPerTurnCSV}
                moveDelay={moveDelay}
                setMoveDelay={setMoveDelay}
                gameStarted={gameStarted}
                onStartGame={handleStartGame}
                gamesCompleted={gamesCompleted}
                allGamesFinished={allGamesFinished}
            />

            <div className="board-section">
                <Board 
                    game={game} 
                    settings={settings}
                    gameStarted={gameStarted}
                    setGameStarted={setGameStarted}
                    gamesCompleted={gamesCompleted}
                    setGamesCompleted={setGamesCompleted}
                    allGamesFinished={allGamesFinished}
                    setAllGamesFinished={setAllGamesFinished}
                />
            </div>

        </div>
    </div>
);
}

export default App;