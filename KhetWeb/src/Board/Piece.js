import React from "react";

export function renderPiece(cell) {
    if (!cell) return null;

    const isWhite = cell.isWhite;
    const color = isWhite ? "#eee" : "#222";

    const size = 46;

    const getPyramidClipPath = () => {
        switch (cell.rotation) {
            case "LeftDown":
                // NUKERPAM apatinį KAIRĮ trikampį
                return "polygon(100% 0%, 100% 100%, 0% 0%)";
            case "RightDown":
                // NUKERPAM apatinį DEŠINĮ trikampį
                return "polygon(0% 0%, 0% 100%, 100% 0%)";
            case "LeftUp":
                // NUKERPAM viršutinį KAIRĮ trikampį
                return "polygon(100% 0%, 0% 100%, 100% 100%)";
            case "RightUp":
                // NUKERPAM viršutinį DEŠINĮ trikampį
                return "polygon(0% 0%, 100% 100%, 0% 100%)";
            default:
                return "polygon(0% 0%, 100% 0%, 100% 100%)";
        }
    };

    switch (cell.type) {
        case "Pyramid":
            return (
                <div style={{
                    width: size,
                    height: size,
                    background: color,
                    clipPath: getPyramidClipPath()
                }} />
            );

        case "Scarab":
            return (
                <div style={{
                    width: size,
                    height: size,
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    fontSize: "32px",
                    color
                }}>
                    {cell.rotation === "LeftUp" || cell.rotation === "RightDown" ? "\\" : "/"}
                </div>
            );

        case "Pharaoh":
            return (
                <div style={{
                    width: size,
                    height: size,
                    borderRadius: "50%",
                    background: color
                }} />
            );

        case "Anubis":
            return (
                <div style={{
                    width: 12,
                    height: size,
                    background: color
                }} />
            );

        case "Sphinx":
            return (
                <div style={{
                    width: size,
                    height: size / 2,
                    background: color
                }} />
            );

        default:
            return null;
    }
}
