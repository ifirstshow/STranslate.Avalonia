using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeChatOcr;

public class SingleResult
{
	[JsonPropertyName("singlePos")]
	public SinglePos? SinglePos { get; set; }

	[JsonPropertyName("singleStrUtf8")]
	public string? SingleStrUtf8 { get; set; }

	[JsonPropertyName("singleRate")]
	public float SingleRate { get; set; }

	[JsonPropertyName("left")]
	public float Left { get; set; }

	[JsonPropertyName("top")]
	public float Top { get; set; }

	[JsonPropertyName("right")]
	public float Right { get; set; }

	[JsonPropertyName("bottom")]
	public float Bottom { get; set; }

	[JsonPropertyName("oneResult")]
	public List<OneResult>? OneResult { get; set; }
}
