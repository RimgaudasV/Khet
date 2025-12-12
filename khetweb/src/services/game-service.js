import { rotate } from "./api-service";

export function isHighlighted(moves, x, y) {
    return moves?.some(pos => pos.x === x && pos.y === y);
}

export function isLaserCell(laserPath, x, y) {
    return laserPath?.some(p => p.x === x && p.y === y);
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

export async function rotatePiece(selectedPiece, direction, validRotations, game) {
    if (!selectedPiece) return;

    const { x, y } = selectedPiece;
    const piece = game.board.pieces[y][x];
    const currentIndex = validRotations.indexOf(piece.rotation);
    if (currentIndex === -1) return;

    const nextIndex = (currentIndex + direction + validRotations.length) % validRotations.length;
    const nextRotation = validRotations[nextIndex];

    const data = await rotate(
        game.currentPlayer,
        game.board,
        { x, y },
        nextRotation
    );

    return data;
}
