namespace KhetApi.Models;

public class PieceValues
{
    public int Pyramid { get; set; } = 10;
    public int Anubis  { get; set; } = 15;
}

public class EvaluationConfig
{
    public bool UseMaterial         { get; set; } = true;
    public bool UsePharaohAlignment { get; set; } = true;
    public bool UseSphinxAxisPresence { get; set; } = true;
    public bool UseSphinxDistance     { get; set; } = true;
    public PieceValues PieceValues  { get; set; } = new();
}
