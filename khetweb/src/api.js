export async function startGame() {
    const response = await fetch("https://localhost:7153/game/startGame"); 
    if (!response.ok) throw new Error("Failed to load");
    return response.json();
}
