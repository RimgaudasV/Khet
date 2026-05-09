import React from "react";
import { HEURISTICS } from "../constants";
import "../styles/Settings.css";

export default function PlayerSettings({
    playerLabel,
    isAgent,
    setIsAgent,
    depth,
    setDepth,
    weights,
    setWeights,
    pieceValuePyramid,
    setPieceValuePyramid,
    pieceValueAnubis,
    setPieceValueAnubis,
    gameStarted,
}) {
    const handleWeightChange = (key, raw) => {
        const val = parseFloat(raw);
        setWeights(prev => ({ ...prev, [key]: isNaN(val) ? 0 : Math.min(1, Math.max(0, val)) }));
    };

    const handlePieceValueChange = (setter) => (e) => {
        const val = e.target.value;
        if (val === "") { setter(""); return; }
        const num = parseInt(val);
        if (!isNaN(num)) setter(num);
    };

    return (
        <div className="player-panel">
            <h2>{playerLabel}</h2>
            <div className="settings-section">
                <label className="checkbox-label">
                    <input
                        type="checkbox"
                        checked={isAgent}
                        onChange={(e) => setIsAgent(e.target.checked)}
                        disabled={gameStarted}
                    />
                    AI Agent
                </label>

                {isAgent && (
                    <div className="depth-control">
                        <label>
                            Search Depth:
                            <input
                                type="number"
                                min="1"
                                max="10"
                                value={depth}
                                onChange={(e) => {
                                    const val = e.target.value;
                                    if (val === "") { setDepth(""); return; }
                                    const num = parseInt(val);
                                    if (!isNaN(num)) setDepth(num);
                                }}
                                onBlur={(e) => { if (e.target.value === "") setDepth(1); }}
                                disabled={gameStarted}
                            />
                        </label>

                        <div className="eval-methods">
                            <span className="eval-methods-label">Evaluation</span>

                            {HEURISTICS.map(({ key, label, showPieceValues }) => (
                                <div key={key} className="eval-method-row">
                                    <label className="eval-weight-label">
                                        {label}:
                                        <input
                                            type="number"
                                            min="0"
                                            max="1"
                                            step="0.1"
                                            value={weights[key] ?? 1.0}
                                            onChange={(e) => handleWeightChange(key, e.target.value)}
                                            disabled={gameStarted}
                                        />
                                    </label>
                                    {showPieceValues && (
                                        <div className="piece-values">
                                            <label>
                                                Pyramid:
                                                <input
                                                    type="number"
                                                    min="0"
                                                    value={pieceValuePyramid}
                                                    onChange={handlePieceValueChange(setPieceValuePyramid)}
                                                    onBlur={(e) => { if (e.target.value === "") setPieceValuePyramid(0); }}
                                                    disabled={gameStarted}
                                                />
                                            </label>
                                            <label>
                                                Anubis:
                                                <input
                                                    type="number"
                                                    min="0"
                                                    value={pieceValueAnubis}
                                                    onChange={handlePieceValueChange(setPieceValueAnubis)}
                                                    onBlur={(e) => { if (e.target.value === "") setPieceValueAnubis(0); }}
                                                    disabled={gameStarted}
                                                />
                                            </label>
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}
