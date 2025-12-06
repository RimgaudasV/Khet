using KhetApi.Entities.Board;
using System.Text.Json.Serialization;

namespace KhetApi.Entities.Piece;
public class PieceEntity
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PieceType Type { get; set; }
    public List<Rotation>? PossibleRotations { get; set; } = new List<Rotation>();
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Rotation Rotation { get; set; }
    public Player Owner { get; set; }
}
