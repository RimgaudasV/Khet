// src/components/Piece.js
import React from "react";
import { getPieceContent, getPyramidClipPath } from "../services/game-service";
import "../styles/Pieces.css";

export default function Piece({ cell }) {
    if (!cell) return null;

    const content = getPieceContent(cell);
    const playerClass = cell.owner === 1 ? "player1" : "player2";
    //debugger;

    const style = cell.type === "Pyramid"
        ? { clipPath: getPyramidClipPath(cell.rotation) }
        : {};

    return (
        <div className={`piece ${cell.type.toLowerCase()} ${playerClass}`} style={style}>
            {content}
        </div>
    );
}

