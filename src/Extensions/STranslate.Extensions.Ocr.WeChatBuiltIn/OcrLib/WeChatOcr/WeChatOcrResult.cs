using System.Text.Json.Serialization;

namespace WeChatOcr;

public class WeChatOcrResult
{
	[JsonPropertyName("taskId")]
	public int TaskId { get; set; }

	[JsonPropertyName("ocrResult")]
	public OcrResult? OcrResult { get; set; }
}
