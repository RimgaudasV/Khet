export const HEURISTICS = [
    { key: "Material",          label: "Material",           showPieceValues: true },
    { key: "PharaohDefense",    label: "Pharaoh defense" },
    { key: "Mobility",          label: "Mobility" },
    { key: "PharaohAlignment",  label: "Pharaoh alignment" },
    { key: "LaserEntry",        label: "Laser entry" },
    { key: "LaserReflectorAlignment", label: "Laser reflector alignment" },
    { key: "DefensiveRotations",      label: "Defensive rotations" },
    { key: "PieceSquareTables",       label: "Piece-square tables" },
    { key: "LaserLength",             label: "Laser length" }

];

export const DEFAULT_WEIGHTS = Object.fromEntries(HEURISTICS.map(h => [h.key, 1.0]));
