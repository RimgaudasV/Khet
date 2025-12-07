import React, { useState, useEffect } from "react";
import { startGame } from "./services/api-service";
import Board from "./components/Board";
import './App.css';

function App() {
    const [board, setBoard] = useState(null);

    useEffect(() => {
        startGame().then(setBoard).catch(console.error);
    }, []);

    return (
        <div style={{ padding: 20 }}>
            <h1>Khet Board</h1>
            <Board board={board} />
        </div>
    );
}

export default App;
