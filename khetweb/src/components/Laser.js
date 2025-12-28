import React, { useEffect, useState } from "react";
import { getLaserSegments } from "../services/game-service";
import "../styles/Laser.css";

export default function Laser({ path, cellSize, gap, width, height }) {
    const [visibleCount, setVisibleCount] = useState(0);

    const segments =
        path && path.length >= 2
            ? getLaserSegments(path, cellSize, gap)
            : [];

    useEffect(() => {
        if (segments.length === 0) return;

        setVisibleCount(0);

        let i = 0;
        const interval = setInterval(() => {
            i++;
            setVisibleCount(i);

            if (i >= segments.length) {
                clearInterval(interval);
            }
        }, 200);

        return () => clearInterval(interval);
    }, [path, segments.length]);

    if (segments.length === 0) return null;

    return (
        <svg
            className="laser-overlay"
            width={width}
            height={height}
        >
            {segments.slice(0, visibleCount).map((seg, idx) => (
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
