using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeChatOcr;

public class SinglePos
{
	[JsonPropertyName("pos")]
	public List<Pos>? Pos { get; set; }
}
