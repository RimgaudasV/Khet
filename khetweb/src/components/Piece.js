// src/components/Piece.js
import React from "react";
import "../styles/Pieces.css";

export default function Piece({ cell }) {
    if (!cell) return null;
    const playerClass = cell.owner === "Player1" ? "player1" : "player2";
    const rotationClass = cell.rotation ? cell.rotation.toLowerCase() : "";

    return (
        <div className={`piece ${cell.type.toLowerCase()} ${playerClass} ${rotationClass}`} />
    );
}