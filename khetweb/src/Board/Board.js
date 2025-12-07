import React, { useState } from "react";
import {renderPiece} from "./Piece.js";

export default function Board({ board, player }) {
    const [moves, setMoves] = useState(null);
    const [selectedPiece, setSelectedPiece] = useState(null);
    const [laserPath, setLaserPath] = useState([]);


    if (!board) return <div>Loading...</div>;

    const rows = board.board.pieces;

    const isHighlighted = (x, y) =>
        moves?.validPositions?.some(pos => pos.x === x && pos.y === y);

    const isLaserCell = (x, y) =>
        laserPath?.some(p => p.x === x && p.y === y);


    const handlePieceClick = async (x, y) => {
        const piece = rows[y][x];
        if (!piece || !piece.isMovable) return;

        setSelectedPiece({ x, y });

        try {
            const res = await fetch("https://localhost:7153/game/validMoves", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    currentPosition: { x, y },
                    player: player,
                    board: board.board
                })
            });

            if (!res.ok) throw new Error("Failed to fetch valid moves");

            const data = await res.json();
            setMoves(data);

        } catch (err) {
            console.error(err);
        }
    };

    const handleMoveClick = async (toX, toY) => {
        if (!selectedPiece) return;

        try {
            debugger;
            const res = await fetch("https://localhost:7153/game/makeMove", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    player: player,
                    board: board.board,
                    currentPosition: { x: selectedPiece.x, y: selectedPiece.y },
                    newPosition: { x: toX, y: toY },
                    newRotation: null
                })
            });

            if (!res.ok) throw new Error("Failed to make move");

            const data = await res.json();
            debugger;
            board.board = data.board;
            board.player = data.currentPlayer;
            console.log(board.player);


            setSelectedPiece(null);
            setMoves(null);

            // ✅ Save laser path
            setLaserPath(data.laser ?? []);

            if (data.gameEnded) alert("Game over!");

        } catch (err) {
            console.error(err);
        }
    };

    return (
        <div style={{
            display: "grid",
            gridTemplateColumns: `repeat(${rows[0].length}, 50px)`,
            gap: "2px",
        }}>
            {rows.map((row, y) =>
                row.map((cell, x) => {
                    const bgColor = isLaserCell(x, y)
                        ? "#f88"          // red laser
                        : isHighlighted(x, y)
                            ? "#aaf"      // blue valid move
                            : "#fff";


                    return (
                        <div key={`${x}-${y}`}
                             onClick={() => {
                                 if (isHighlighted(x, y)) {
                                     handleMoveClick(x, y);
                                 } else if (cell) {
                                     handlePieceClick(x, y);
                                 }
                             }}
                             style={{
                                 width: "50px",
                                 height: "50px",
                                 border: "1px solid black",
                                 background: bgColor,
                                 display: "flex",
                                 alignItems: "center",
                                 justifyContent: "center",
                                 fontWeight: "bold",
                                 cursor: isHighlighted(x, y) || cell ? "pointer" : "default"
                             }}>
                            {renderPiece(cell)}
                        </div>
                    );
                })
            )}
        </div>
    );
}
