namespace KhetApi.Models.Move;

public class OrderedMove
{
    public required Move Move { get; init; }

    /// <summary>2 = primary killer, 1 = secondary killer, 0 = not a killer.</summary>
    public required int KillerScore { get; init; }

    /// <summary>Accumulated depth² bonuses from beta cutoffs across the search tree.</summary>
    public required int HistoryScore { get; init; }

    /// <summary>Static piece-type heuristic — Scarab swap, Pharaoh move penalty, etc.</summary>
    public required int TacticalScore { get; init; }

    /// <summary>
    /// Combined score used for sorting. Killers dominate history; history dominates tactical.
    /// Scaling assumes HistoryScore stays below ~1 000 000 in practice (depth² * node count).
    /// </summary>
    public long FinalScore =>
        (long)KillerScore  * 1_000_000_000L +
        (long)HistoryScore * 1_000L +
        TacticalScore;
}
