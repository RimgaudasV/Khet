import React, { useState } from "react";

export default function Board({ board, player }) {
    const [moves, setMoves] = useState(null);
    const [selectedPiece, setSelectedPiece] = useState(null);

    if (!board) return <div>Loading...</div>;

    const rows = board.board.pieces;
    const pieceLabels = {
        Sphinx: "Sp",
        Pharaoh: "Ph",
        Anubis: "A",
        Pyramid: "P",
        Scarab: "S"
    };

    const rotationArrows = {
        Up: "↑",
        Down: "↓",
        Left: "←",
        Right: "→",
        LeftUp: "↖",
        RightUp: "↗",
        LeftDown: "↙",
        RightDown: "↘"
    };

    const isHighlighted = (x, y) =>
        moves?.validPositions?.some(pos => pos.x === x && pos.y === y);

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
                    player,
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
            const res = await fetch("https://localhost:7153/game/makeMove", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    player,
                    board: board.board,
                    currentPosition: { x: selectedPiece.x, y: selectedPiece.y },
                    newPosition: { x: toX, y: toY },
                    newRotation: null
                })
            });

            if (!res.ok) throw new Error("Failed to make move");

            const data = await res.json();
            board.board = data.board;
            board.player = data.currentPlayer;

            setSelectedPiece(null);
            setMoves(null);

            if (data.laser) console.log("Laser path:", data.laser);
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
                    const bgColor = isHighlighted(x, y) ? "#aaf" : "#fff";

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
                            {cell ? (pieceLabels[cell.type] || "?") + (rotationArrows[cell.rotation] || "") : ""}
                        </div>
                    );
                })
            )}
        </div>
    );
}
