namespace KhetApi.Models;

public class PieceValues
{
    public int Pyramid { get; set; } = 10;
    public int Anubis  { get; set; } = 15;
    public int Pharaoh { get; set; } = 200;
}

public class EvaluationConfig
{
    public bool UseMaterial         { get; set; } = true;
    public bool UsePharaohAlignment { get; set; } = true;
    public bool UseSphinxSupport    { get; set; } = true;
    public PieceValues PieceValues  { get; set; } = new();
}
