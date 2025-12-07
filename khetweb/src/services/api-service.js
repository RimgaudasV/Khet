export async function startGame() {
    const response = await fetch("https://localhost:7153/game/startGame"); 
    if (!response.ok) throw new Error("Failed to load");
    return response.json();
}

export async function getValidMoves(currentPosition, player, board) {
    const res = await fetch("https://localhost:7153/game/validMoves", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ currentPosition, player, board })
    });
    if (!res.ok) throw new Error("Failed to fetch valid moves");
    return res.json();
}

export async function makeMove(player, board, currentPosition, newPosition, newRotation = null) {
    const res = await fetch("https://localhost:7153/game/makeMove", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ player, board, currentPosition, newPosition, newRotation })
    });
    if (!res.ok) throw new Error("Failed to make move");
    return res.json();
}