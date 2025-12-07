// src/components/Piece.js
import React from "react";
import { getPieceClass, getPieceContent, getPyramidClipPath } from "../services/game-service";
import "../styles/Pieces.css";

export default function Piece({ cell }) {
    if (!cell) return null;

    const className = getPieceClass(cell);
    const content = getPieceContent(cell);

    // Only Pyramid needs dynamic inline style for clipPath
    const style = cell.type === "Pyramid"
        ? { clipPath: getPyramidClipPath(cell.rotation) }
        : {};

    return (
        <div className={className} style={style}>
            {content}
        </div>
    );
}
