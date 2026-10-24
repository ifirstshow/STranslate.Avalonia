using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeChatOcr;

public class OcrResult
{
	[JsonPropertyName("singleResult")]
	public List<SingleResult>? SingleResult { get; set; }
}
