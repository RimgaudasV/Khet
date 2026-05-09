export const HEURISTICS = [
    { key: "Material",          label: "Material",           showPieceValues: true },
    { key: "PharaohAlignment",  label: "Pharaoh alignment" },
    { key: "PieceSquareTables", label: "Piece-square tables" },
    { key: "LaserEntry",        label: "Laser entry" },
];

export const DEFAULT_WEIGHTS = Object.fromEntries(HEURISTICS.map(h => [h.key, 1.0]));
