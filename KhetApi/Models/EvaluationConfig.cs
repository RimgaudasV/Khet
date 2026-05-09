namespace KhetApi.Models;

public class PieceValues
{
    public int Pyramid { get; set; } = 10;
    public int Anubis  { get; set; } = 15;
}

public class EvaluationConfig
{
    public Dictionary<string, double> Weights { get; set; } = new();
    public PieceValues PieceValues            { get; set; } = new();
}
