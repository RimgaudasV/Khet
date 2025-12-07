// src/components/Laser.js
import React from "react";
import { getLaserSegments } from "../services/game-service";
import "../styles/Laser.css";

export default function Laser({ path, cellSize = 50 }) {
    if (!path || path.length < 2) return null;

    const segments = getLaserSegments(path, cellSize);

    return (
        <svg className="laser-overlay" width={cellSize * 10} height={cellSize * 10}>
            {segments.map((seg, idx) => (
                <line
                    key={idx}
                    x1={seg.x1}
                    y1={seg.y1}
                    x2={seg.x2}
                    y2={seg.y2}
                    className="laser-line"
                />
            ))}
        </svg>
    );
}
