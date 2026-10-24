using System.Text.Json.Serialization;

namespace WeChatOcr;

public class OneResult
{
	[JsonPropertyName("onePos")]
	public OnePos? OnePos { get; set; }

	[JsonPropertyName("oneStrUtf8")]
	public string? OneStrUtf8 { get; set; }
}
