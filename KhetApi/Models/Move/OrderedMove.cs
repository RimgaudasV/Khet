namespace KhetApi.Models.Move;

public class OrderedMove
{
    public required Move Move { get; init; }

    public required int KillerScore { get; init; }

    public required int HistoryScore { get; init; }

    public required int TacticalScore { get; init; }

    public long FinalScore =>
        (long)KillerScore  * 1_000_000_000L +
        (long)HistoryScore * 1_000L +
        TacticalScore;
}
