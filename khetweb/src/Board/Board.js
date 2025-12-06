export default function Board({ board }) {
    if (!board) return <div>Loading...</div>;

    const rows = board.board.pieces;

    // Map piece types to short labels
    const pieceLabels = {
        Sphinx: "Sp",
        Pharaoh: "Ph",
        Anubis: "A",
        Pyramid: "P",
        Scarab: "S"
    };

    // Map rotations to arrows
    const rotationArrows = {
        Up: "↑",
        Down: "↓",
        Left: "←",
        Right: "→"
    };

    return (
        <div style={{
            display: "grid",
            gridTemplateColumns: `repeat(${rows[0].length}, 50px)`,
            gap: "2px",
        }}>
            {rows.flat().map((cell, i) => {
                if (!cell) {
                    return (
                        <div key={i}
                             style={{
                                 width: "50px",
                                 height: "50px",
                                 border: "1px solid black",
                                 background: "#fff",
                             }} />
                    );
                }

                const label = pieceLabels[cell.type] || "?";
                const arrow = rotationArrows[cell.rotation] || "";

                return (
                    <div key={i}
                         style={{
                             width: "50px",
                             height: "50px",
                             border: "1px solid black",
                             background: "#fff",
                             display: "flex",
                             alignItems: "center",
                             justifyContent: "center",
                             fontWeight: "bold"
                         }}>
                        {label}{arrow}
                    </div>
                );
            })}
        </div>
    );
}
