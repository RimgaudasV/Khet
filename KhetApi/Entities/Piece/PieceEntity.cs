using KhetApi.Entities.Board;
using System.Text.Json.Serialization;

namespace KhetApi.Entities.Piece;
public class PieceEntity
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PieceType Type { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Rotation Rotation { get; set; }
    public bool IsMovable { get; set; }
    public Player Owner { get; set; }
}
