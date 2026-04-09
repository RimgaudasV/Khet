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
    const [playerOneEvalMaterial, setPlayerOneEvalMaterial] = useState(() => {
        const saved = localStorage.getItem('playerOneEvalMaterial');
        return saved !== null ? JSON.parse(saved) : true;
    });
    const [playerTwoEvalMaterial, setPlayerTwoEvalMaterial] = useState(() => {
        const saved = localStorage.getItem('playerTwoEvalMaterial');
        return saved !== null ? JSON.parse(saved) : true;
    });
    const [playerOneEvalAlignment, setPlayerOneEvalAlignment] = useState(() => {
        const saved = localStorage.getItem('playerOneEvalAlignment');
        return saved !== null ? JSON.parse(saved) : true;
    });
    const [playerTwoEvalAlignment, setPlayerTwoEvalAlignment] = useState(() => {
        const saved = localStorage.getItem('playerTwoEvalAlignment');
        return saved !== null ? JSON.parse(saved) : true;
    });
    const [playerOneEvalSphinx, setPlayerOneEvalSphinx] = useState(() => {
        const saved = localStorage.getItem('playerOneEvalSphinx');
        return saved !== null ? JSON.parse(saved) : true;
    });
    const [playerTwoEvalSphinx, setPlayerTwoEvalSphinx] = useState(() => {
        const saved = localStorage.getItem('playerTwoEvalSphinx');
        return saved !== null ? JSON.parse(saved) : true;
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
    const [playerOnePieceValuePharaoh, setPlayerOnePieceValuePharaoh] = useState(() => {
        const saved = localStorage.getItem('playerOnePieceValuePharaoh');
        return saved !== null ? parseInt(saved) : 200;
    });
    const [playerTwoPieceValuePharaoh, setPlayerTwoPieceValuePharaoh] = useState(() => {
        const saved = localStorage.getItem('playerTwoPieceValuePharaoh');
        return saved !== null ? parseInt(saved) : 200;
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
        localStorage.setItem('playerOneEvalMaterial', JSON.stringify(playerOneEvalMaterial));
    }, [playerOneEvalMaterial]);
    useEffect(() => {
        localStorage.setItem('playerTwoEvalMaterial', JSON.stringify(playerTwoEvalMaterial));
    }, [playerTwoEvalMaterial]);
    useEffect(() => {
        localStorage.setItem('playerOneEvalAlignment', JSON.stringify(playerOneEvalAlignment));
    }, [playerOneEvalAlignment]);
    useEffect(() => {
        localStorage.setItem('playerTwoEvalAlignment', JSON.stringify(playerTwoEvalAlignment));
    }, [playerTwoEvalAlignment]);
    useEffect(() => {
        localStorage.setItem('playerOneEvalSphinx', JSON.stringify(playerOneEvalSphinx));
    }, [playerOneEvalSphinx]);
    useEffect(() => {
        localStorage.setItem('playerTwoEvalSphinx', JSON.stringify(playerTwoEvalSphinx));
    }, [playerTwoEvalSphinx]);
    useEffect(() => {
        localStorage.setItem('playerOnePieceValuePyramid', playerOnePieceValuePyramid.toString());
    }, [playerOnePieceValuePyramid]);
    useEffect(() => {
        localStorage.setItem('playerTwoPieceValuePyramid', playerTwoPieceValuePyramid.toString());
    }, [playerTwoPieceValuePyramid]);
    useEffect(() => {
        localStorage.setItem('playerOnePieceValueAnubis', playerOnePieceValueAnubis.toString());
    }, [playerOnePieceValueAnubis]);
    useEffect(() => {
        localStorage.setItem('playerTwoPieceValueAnubis', playerTwoPieceValueAnubis.toString());
    }, [playerTwoPieceValueAnubis]);
    useEffect(() => {
        localStorage.setItem('playerOnePieceValuePharaoh', playerOnePieceValuePharaoh.toString());
    }, [playerOnePieceValuePharaoh]);
    useEffect(() => {
        localStorage.setItem('playerTwoPieceValuePharaoh', playerTwoPieceValuePharaoh.toString());
    }, [playerTwoPieceValuePharaoh]);

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
        totalGames,
        downloadOverallStats,
        downloadPerTurnCSV,
        moveDelay,
        playerOneEvalMaterial,
        playerTwoEvalMaterial,
        playerOneEvalAlignment,
        playerTwoEvalAlignment,
        playerOneEvalSphinx,
        playerTwoEvalSphinx,
        playerOnePieceValuePyramid,
        playerTwoPieceValuePyramid,
        playerOnePieceValueAnubis,
        playerTwoPieceValueAnubis,
        playerOnePieceValuePharaoh,
        playerTwoPieceValuePharaoh,
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
                onRestart={handleRestart}
                gamesCompleted={gamesCompleted}
                allGamesFinished={allGamesFinished}
                playerOneEvalMaterial={playerOneEvalMaterial}
                setPlayerOneEvalMaterial={setPlayerOneEvalMaterial}
                playerTwoEvalMaterial={playerTwoEvalMaterial}
                setPlayerTwoEvalMaterial={setPlayerTwoEvalMaterial}
                playerOneEvalAlignment={playerOneEvalAlignment}
                setPlayerOneEvalAlignment={setPlayerOneEvalAlignment}
                playerTwoEvalAlignment={playerTwoEvalAlignment}
                setPlayerTwoEvalAlignment={setPlayerTwoEvalAlignment}
                playerOneEvalSphinx={playerOneEvalSphinx}
                setPlayerOneEvalSphinx={setPlayerOneEvalSphinx}
                playerTwoEvalSphinx={playerTwoEvalSphinx}
                setPlayerTwoEvalSphinx={setPlayerTwoEvalSphinx}
                playerOnePieceValuePyramid={playerOnePieceValuePyramid}
                setPlayerOnePieceValuePyramid={setPlayerOnePieceValuePyramid}
                playerTwoPieceValuePyramid={playerTwoPieceValuePyramid}
                setPlayerTwoPieceValuePyramid={setPlayerTwoPieceValuePyramid}
                playerOnePieceValueAnubis={playerOnePieceValueAnubis}
                setPlayerOnePieceValueAnubis={setPlayerOnePieceValueAnubis}
                playerTwoPieceValueAnubis={playerTwoPieceValueAnubis}
                setPlayerTwoPieceValueAnubis={setPlayerTwoPieceValueAnubis}
                playerOnePieceValuePharaoh={playerOnePieceValuePharaoh}
                setPlayerOnePieceValuePharaoh={setPlayerOnePieceValuePharaoh}
                playerTwoPieceValuePharaoh={playerTwoPieceValuePharaoh}
                setPlayerTwoPieceValuePharaoh={setPlayerTwoPieceValuePharaoh}
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