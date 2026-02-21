import React from 'react';
import '../styles/AgentStats.css';

export default function AgentStats({ stats }) {
    if (!stats) return null;
    
    const { 
        player1Times = [], 
        player2Times = [],
        player1AllMoves = [], 
        player2AllMoves = [],
        player1AllRoutes = [],
        player2AllRoutes = [],
        player1EvaluatedRoutes = [], 
        player2EvaluatedRoutes = []
    } = stats;
    
    const calculateAverage = (arr) => {
        if (!arr || arr.length === 0) return 0;
        return arr.reduce((sum, val) => sum + val, 0) / arr.length;
    };
    
    const allTimes = [...player1Times, ...player2Times];
    const allMoves = [...player1AllMoves, ...player2AllMoves];
    const allRoutes = [...player1AllRoutes, ...player2AllRoutes];
    const allEvaluatedRoutes = [...player1EvaluatedRoutes, ...player2EvaluatedRoutes];
    
    const avgPlayer1Time = calculateAverage(player1Times);
    const avgPlayer2Time = calculateAverage(player2Times);
    const avgOverallTime = calculateAverage(allTimes);
    
    const avgPossibleMovesPerTurn = calculateAverage(allMoves);
    
    const avgTotalRoutesPerTurn = calculateAverage(allRoutes);
    
    const avgEvaluatedRoutesPerTurn = calculateAverage(allEvaluatedRoutes);
    
    const pruningEfficiency = avgTotalRoutesPerTurn > 0 
        ? ((avgTotalRoutesPerTurn - avgEvaluatedRoutesPerTurn) / avgTotalRoutesPerTurn * 100)
        : 0;
    
    const formatTime = (ms) => {
        if (ms < 1000) return `${ms.toFixed(0)}ms`;
        return `${(ms / 1000).toFixed(2)}s`;
    };
    
    const formatNumber = (num, decimals = 1) => {
        return num.toLocaleString('en-US', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        });
    };
    
    return (
        <div className="agent-stats-container">
            <h3>Agent Statistics</h3>
            
            <div className="agent-stats-grid">
                <div className="agent-stats-player agent-stats-player1">
                    <h4>Player 1 (Blue)</h4>
                    <div>Avg Time: <strong>{formatTime(avgPlayer1Time)}</strong></div>
                    <div className="agent-stats-moves">
                        Turns: {player1Times.length}
                    </div>
                </div>
                
                <div className="agent-stats-player agent-stats-player2">
                    <h4>Player 2 (Red)</h4>
                    <div>Avg Time: <strong>{formatTime(avgPlayer2Time)}</strong></div>
                    <div className="agent-stats-moves">
                        Turns: {player2Times.length}
                    </div>
                </div>
            </div>
            
            <div className="agent-stats-overall">
                <h4>Overall Statistics</h4>
                <div>Avg Time per Turn: <strong>{formatTime(avgOverallTime)}</strong></div>
                <div>Avg Possible Moves per Turn: <strong>{formatNumber(avgPossibleMovesPerTurn)}</strong></div>
                <div>Avg Total Routes per Turn: <strong>{formatNumber(avgTotalRoutesPerTurn)}</strong></div>
                <div>Avg Evaluated Routes per Turn: <strong>{formatNumber(avgEvaluatedRoutesPerTurn)}</strong></div>
                <div>Pruning Efficiency: <strong>{pruningEfficiency.toFixed(1)}%</strong></div>
                <div className="agent-stats-moves">
                    Total Turns Played: {allTimes.length}
                </div>
            </div>
        </div>
    );
}