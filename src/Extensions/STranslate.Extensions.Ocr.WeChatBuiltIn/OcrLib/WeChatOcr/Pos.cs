using System.Text.Json.Serialization;

namespace WeChatOcr;

public class Pos
{
	[JsonPropertyName("x")]
	public float X { get; set; }

	[JsonPropertyName("y")]
	public float Y { get; set; }
}
