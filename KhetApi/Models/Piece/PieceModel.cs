using System.Text.Json.Serialization;

namespace KhetApi.Models.Piece;
public class PieceModel
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PieceType Type { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Rotation Rotation { get; set; }
    public bool IsMovable { get; set; }
    public Player.Player Owner { get; set; }
}
