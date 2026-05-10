export const HEURISTICS = [
    { key: "Material",          label: "Material",           showPieceValues: true },
    { key: "PharaohAlignment",  label: "Pharaoh alignment" },
    { key: "PieceSquareTables", label: "Piece-square tables" },
    { key: "LaserEntry",        label: "Laser entry" },
    { key: "Mobility",          label: "Mobility" },
    { key: "LaserReflectorAlignment", label: "Laser reflector alignment" },
    { key: "LaserLength",             label: "Laser length" },
    { key: "DefensiveRotations",      label: "Defensive rotations" },
];

export const DEFAULT_WEIGHTS = Object.fromEntries(HEURISTICS.map(h => [h.key, 1.0]));
