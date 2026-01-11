import { rotate } from "./api-service";

export function isHighlighted(moves, x, y) {
    return moves?.some(pos => pos.x === x && pos.y === y);
}

export function isLaserCell(laserPath, x, y) {
    return laserPath?.some(p => p.x === x && p.y === y);
}


export function getLaserSegments(path, cellSize = 50, gap = 2) {
    if (!path || path.length < 2) return [];

    const step = cellSize + gap;
    const half = cellSize / 2;

    const segments = [];

    for (let i = 0; i < path.length - 1; i++) {
        const from = path[i];
        const to = path[i + 1];

        segments.push({
            x1: from.x * step + half,
            y1: from.y * step + half,
            x2: to.x * step + half,
            y2: to.y * step + half
        });
    }

    return segments;
}


export async function rotatePiece(selectedPiece, direction, validRotations, board, currentPlayer) {
    if (!selectedPiece) return;
    const { x, y } = selectedPiece;
    const piece = board.cells[y][x].piece;

    const currentIndex = validRotations.indexOf(piece.rotation);
    if (currentIndex === -1) return;

    const nextIndex =
        (currentIndex + direction + validRotations.length) %
        validRotations.length;

    const nextRotation = validRotations[nextIndex];
    
    return rotate(
        currentPlayer,
        board,
        { x, y },
        nextRotation
    );
}

