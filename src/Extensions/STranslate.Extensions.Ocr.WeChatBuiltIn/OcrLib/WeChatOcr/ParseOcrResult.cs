using System;
using System.Text;
using System.Text.Json.Serialization;

namespace WeChatOcr;

public class ParseOcrResult
{
	public static WeChatOcrResult? ParseJson(string jsonResponseStr)
	{
		WeChatOcrResult weChatOcrResult = System.Text.Json.JsonSerializer.Deserialize<WeChatOcrResult>(jsonResponseStr);
		if (weChatOcrResult != null)
		{
			OcrResult ocrResult = weChatOcrResult.OcrResult;
			if (ocrResult != null && ocrResult.SingleResult != null)
			{
				{
					foreach (SingleResult item in weChatOcrResult.OcrResult.SingleResult)
					{
						if (item.SingleStrUtf8 != null)
						{
							item.SingleStrUtf8 = Encoding.UTF8.GetString(Convert.FromBase64String(item.SingleStrUtf8));
						}
						if (item.OneResult == null)
						{
							continue;
						}
						foreach (OneResult item2 in item.OneResult)
						{
							if (item2.OneStrUtf8 != null)
							{
								item2.OneStrUtf8 = Encoding.UTF8.GetString(Convert.FromBase64String(item2.OneStrUtf8));
							}
						}
					}
					return weChatOcrResult;
				}
			}
		}
		return weChatOcrResult;
	}
}
