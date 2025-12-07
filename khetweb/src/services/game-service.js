// src/services/game-service.js
export function isHighlighted(moves, x, y) {
    return moves?.validPositions?.some(pos => pos.x === x && pos.y === y);
}

export function isLaserCell(laserPath, x, y) {
    return laserPath?.some(p => p.x === x && p.y === y);
}


// src/services/game-service.js

export function getPieceClass(cell) {
    if (!cell) return "";

    const classes = ["piece"];

    switch (cell.type) {
        case "Pyramid": classes.push("pyramid"); break;
        case "Scarab": classes.push("scarab"); break;
        case "Pharaoh": classes.push("pharaoh"); break;
        case "Anubis": classes.push("anubis"); break;
        case "Sphinx": classes.push("sphinx"); break;
        default: break;
    }

    classes.push(cell.isWhite ? "white" : "black");

    return classes.join(" ");
}

// Only for Scarab content
export function getPieceContent(cell) {
    if (!cell) return null;

    if (cell.type === "Scarab") {
        return cell.rotation === "LeftUp" || cell.rotation === "RightDown" ? "\\" : "/";
    }

    return null;
}


export function getPyramidClipPath(rotation) {
    switch (rotation) {
        case "LeftDown": return "polygon(100% 0%, 100% 100%, 0% 0%)";
        case "RightDown": return "polygon(0% 0%, 0% 100%, 100% 0%)";
        case "LeftUp": return "polygon(100% 0%, 0% 100%, 100% 100%)";
        case "RightUp": return "polygon(0% 0%, 100% 100%, 0% 100%)";
        default: return "polygon(0% 0%, 100% 0%, 100% 100%)";
    }
}

export function getLaserSegments(path, cellSize = 50) {
    if (!path || path.length < 2) return [];
    
    const segments = [];
    for (let i = 0; i < path.length - 1; i++) {
        const from = path[i];
        const to = path[i + 1];

        segments.push({
            x1: from.x * cellSize + cellSize / 2,
            y1: from.y * cellSize + cellSize / 2,
            x2: to.x * cellSize + cellSize / 2,
            y2: to.y * cellSize + cellSize / 2
        });
    }
    return segments;
}
