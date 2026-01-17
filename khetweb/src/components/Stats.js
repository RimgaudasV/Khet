import React from 'react';
import '../styles/AgentStats.css';

export default function AgentStats({ stats }) {
    if (!stats) return null;
    
    const { player1Times, player2Times, player1Moves, player2Moves } = stats;
    
    const calculateAverage = (arr) => {
        if (!arr || arr.length === 0) return 0;
        return arr.reduce((sum, val) => sum + val, 0) / arr.length;
    };
    
    const allTimes = [...player1Times, ...player2Times];
    const allMoves = [...player1Moves, ...player2Moves];
    
    const avgPlayer1Time = calculateAverage(player1Times);
    const avgPlayer2Time = calculateAverage(player2Times);
    const avgOverallTime = calculateAverage(allTimes);
    const avgOverallMoves = calculateAverage(allMoves);
    
    const formatTime = (ms) => {
        if (ms < 1000) return `${ms.toFixed(0)}ms`;
        return `${(ms / 1000).toFixed(2)}s`;
    };
    
    return (
        <div className="agent-stats-container">
            <h3>Agent Statistics</h3>
            
            <div className="agent-stats-grid">
                <div className="agent-stats-player agent-stats-player1">
                    <h4>Player 1 (Blue)</h4>
                    <div>Avg Time: <strong>{formatTime(avgPlayer1Time)}</strong></div>
                    <div className="agent-stats-moves">
                        Moves: {player1Times.length}
                    </div>
                </div>
                
                <div className="agent-stats-player agent-stats-player2">
                    <h4>Player 2 (Red)</h4>
                    <div>Avg Time: <strong>{formatTime(avgPlayer2Time)}</strong></div>
                    <div className="agent-stats-moves">
                        Moves: {player2Times.length}
                    </div>
                </div>
            </div>
            
            <div className="agent-stats-overall">
                <h4>Overall Statistics</h4>
                <div>Avg Time per Move: <strong>{formatTime(avgOverallTime)}</strong></div>
                <div>Avg Moves Evaluated: <strong>{avgOverallMoves.toFixed(1)}</strong></div>
                <div className="agent-stats-moves">
                    Total Agent Moves: {allTimes.length}
                </div>
            </div>
        </div>
    );
}