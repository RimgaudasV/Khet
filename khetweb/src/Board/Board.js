export default function Board({ board }) {
    if (!board) return <div>Loading...</div>;

    const rows = board.board.pieces; // <-- Important fix

    return (
        <div style={{
            display: "grid",
            gridTemplateColumns: `repeat(${rows[0].length}, 50px)`,
            gap: "2px",
        }}>
            {rows.flat().map((cell, i) => (
                <div key={i}
                     style={{
                         width: "50px",
                         height: "50px",
                         border: "1px solid black",
                         background: cell ? "#ececec" : "white"
                     }} />
            ))}
        </div>
    );
}
