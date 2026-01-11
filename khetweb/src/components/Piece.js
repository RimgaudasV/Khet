
import React from "react";
import "../styles/Pieces.css";

export default function Piece({ piece, disabled, disabledFor }) {
    if (!piece) return null;

    const playerClass =
        piece.owner === "Player1" ? "player1" : "player2";

    const rotationClass =
        piece.rotation ? piece.rotation.toLowerCase() : "";

    const classes = [
        "piece",
        piece.type.toLowerCase(),
        playerClass,
        rotationClass
    ];

    // Optional: visual hint for disabled tiles
    if (disabled) classes.push("disabled-tile");

    return <div className={classes.join(" ")} />;
}

