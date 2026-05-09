import React, { useState, useEffect } from "react";
import { startGame } from "./services/api-service";
import Board from "./components/Board";
import PlayerSettings from "./components/PlayerSettings";
import GameSettings from "./components/GameSettings";
import AgentStats from "./components/Stats";
import { DEFAULT_WEIGHTS } from "./constants";
import './App.css';

function App() {
    const [game, setGame] = useState(null);
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

    const [playerOneWeights, setPlayerOneWeights] = useState(() => {
        const saved = localStorage.getItem('playerOneWeights');
        return saved ? { ...DEFAULT_WEIGHTS, ...JSON.parse(saved) } : { ...DEFAULT_WEIGHTS };
    });
    const [playerTwoWeights, setPlayerTwoWeights] = useState(() => {
        const saved = localStorage.getItem('playerTwoWeights');
        return saved ? { ...DEFAULT_WEIGHTS, ...JSON.parse(saved) } : { ...DEFAULT_WEIGHTS };
    });

    const [playerOnePieceValuePyramid, setPlayerOnePieceValuePyramid] = useState(() => {
        const saved = localStorage.getItem('playerOnePieceValuePyramid');
        return saved !== null ? parseInt(saved) : 10;
    });
    const [playerTwoPieceValuePyramid, setPlayerTwoPieceValuePyramid] = useState(() => {
        const saved = localStorage.getItem('playerTwoPieceValuePyramid');
        return saved !== null ? parseInt(saved) : 10;
    });
    const [playerOnePieceValueAnubis, setPlayerOnePieceValueAnubis] = useState(() => {
        const saved = localStorage.getItem('playerOnePieceValueAnubis');
        return saved !== null ? parseInt(saved) : 15;
    });
    const [playerTwoPieceValueAnubis, setPlayerTwoPieceValueAnubis] = useState(() => {
        const saved = localStorage.getItem('playerTwoPieceValueAnubis');
        return saved !== null ? parseInt(saved) : 15;
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

    useEffect(() => { localStorage.setItem('playerOneAgent', JSON.stringify(playerOneAgent)); }, [playerOneAgent]);
    useEffect(() => { localStorage.setItem('playerTwoAgent', JSON.stringify(playerTwoAgent)); }, [playerTwoAgent]);
    useEffect(() => { localStorage.setItem('playerOneDepth', playerOneDepth.toString()); }, [playerOneDepth]);
    useEffect(() => { localStorage.setItem('playerTwoDepth', playerTwoDepth.toString()); }, [playerTwoDepth]);
    useEffect(() => { localStorage.setItem('playerOneWeights', JSON.stringify(playerOneWeights)); }, [playerOneWeights]);
    useEffect(() => { localStorage.setItem('playerTwoWeights', JSON.stringify(playerTwoWeights)); }, [playerTwoWeights]);
    useEffect(() => { localStorage.setItem('playerOnePieceValuePyramid', playerOnePieceValuePyramid.toString()); }, [playerOnePieceValuePyramid]);
    useEffect(() => { localStorage.setItem('playerTwoPieceValuePyramid', playerTwoPieceValuePyramid.toString()); }, [playerTwoPieceValuePyramid]);
    useEffect(() => { localStorage.setItem('playerOnePieceValueAnubis', playerOnePieceValueAnubis.toString()); }, [playerOnePieceValueAnubis]);
    useEffect(() => { localStorage.setItem('playerTwoPieceValueAnubis', playerTwoPieceValueAnubis.toString()); }, [playerTwoPieceValueAnubis]);
    useEffect(() => { localStorage.setItem('totalGames', totalGames.toString()); }, [totalGames]);
    useEffect(() => { localStorage.setItem('downloadOverallStats', JSON.stringify(downloadOverallStats)); }, [downloadOverallStats]);
    useEffect(() => { localStorage.setItem('downloadPerTurnCSV', JSON.stringify(downloadPerTurnCSV)); }, [downloadPerTurnCSV]);
    useEffect(() => { localStorage.setItem('moveDelay', moveDelay.toString()); }, [moveDelay]);

    useEffect(() => {
        startGame().then(setGame).catch(console.error);
    }, []);

    const handleStartGame = () => {
        setGameStarted(true);
        setGamesCompleted(0);
        setAllGamesFinished(false);
    };

    const handleRestart = () => {
        startGame().then(setGame).catch(console.error);
        setGameStarted(false);
        setGamesCompleted(0);
        setAllGamesFinished(false);
    };

    const settings = {
        playerOneAgent,
        playerTwoAgent,
        playerOneDepth,
        playerTwoDepth,
        playerOneWeights,
        playerTwoWeights,
        playerOnePieceValuePyramid,
        playerTwoPieceValuePyramid,
        playerOnePieceValueAnubis,
        playerTwoPieceValueAnubis,
        totalGames,
        downloadOverallStats,
        downloadPerTurnCSV,
        moveDelay,
    };

    return (
        <div className="app-container">
            <div className="game-row">
                <PlayerSettings
                    playerLabel="Player 1 (Blue)"
                    isAgent={playerOneAgent}
                    setIsAgent={setPlayerOneAgent}
                    depth={playerOneDepth}
                    setDepth={setPlayerOneDepth}
                    weights={playerOneWeights}
                    setWeights={setPlayerOneWeights}
                    pieceValuePyramid={playerOnePieceValuePyramid}
                    setPieceValuePyramid={setPlayerOnePieceValuePyramid}
                    pieceValueAnubis={playerOnePieceValueAnubis}
                    setPieceValueAnubis={setPlayerOnePieceValueAnubis}
                    gameStarted={gameStarted}
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
                        stats={stats}
                        setStats={setStats}
                    />
                </div>
                <PlayerSettings
                    playerLabel="Player 2 (Red)"
                    isAgent={playerTwoAgent}
                    setIsAgent={setPlayerTwoAgent}
                    depth={playerTwoDepth}
                    setDepth={setPlayerTwoDepth}
                    weights={playerTwoWeights}
                    setWeights={setPlayerTwoWeights}
                    pieceValuePyramid={playerTwoPieceValuePyramid}
                    setPieceValuePyramid={setPlayerTwoPieceValuePyramid}
                    pieceValueAnubis={playerTwoPieceValueAnubis}
                    setPieceValueAnubis={setPlayerTwoPieceValueAnubis}
                    gameStarted={gameStarted}
                />
            </div>
            <GameSettings
                totalGames={totalGames}
                setTotalGames={setTotalGames}
                moveDelay={moveDelay}
                setMoveDelay={setMoveDelay}
                downloadOverallStats={downloadOverallStats}
                setDownloadOverallStats={setDownloadOverallStats}
                downloadPerTurnCSV={downloadPerTurnCSV}
                setDownloadPerTurnCSV={setDownloadPerTurnCSV}
                gameStarted={gameStarted}
                onStartGame={handleStartGame}
                onRestart={handleRestart}
                gamesCompleted={gamesCompleted}
                allGamesFinished={allGamesFinished}
            />
            {(stats.player1Times.length > 0 || stats.player2Times.length > 0) && (
                <AgentStats stats={stats} />
            )}
        </div>
    );
}

export default App;
