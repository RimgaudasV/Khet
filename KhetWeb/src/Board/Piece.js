// Piece.js
import React from "react";

export default function Piece({ cell }) {
    if (!cell) return null;

    const pieceLetters = {
        Anubis: "A",
        Scarab: "S",
        Pharaoh: "PH",
        Pyramid: "PY",
        Sphinx: "SP"
    };

    const arrow = {
        Up: "↑",
        Down: "↓",
        Left: "←",
        Right: "→"
    };

    const containerStyle = {
        width: "100%",
        height: "100%",
        borderRadius: "8px",
        background: "#d9d9d9",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        fontSize: "14px",
        fontWeight: "bold",
        position: "relative"
    };

    const arrowStyle = {
        position: "absolute",
        bottom: "2px",
        fontSize: "14px"
    };

    return React.createElement(
        "div",
        { style: containerStyle },
        [
            React.createElement(
                "div",
                { key: "letter" },
                pieceLetters[cell.Type]
            ),
            React.createElement(
                "div",
                { key: "arrow", style: arrowStyle },
                arrow[cell.Rotation]
            )
        ]
    );
}
