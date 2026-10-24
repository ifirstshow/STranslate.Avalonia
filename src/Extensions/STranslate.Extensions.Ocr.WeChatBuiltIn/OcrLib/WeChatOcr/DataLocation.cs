using System.IO;

namespace WeChatOcr;

public class DataLocation
{
	private static string _baseDirectory = ".";

	public static string WeChatOcrData => Path.Combine(_baseDirectory, "wco_data");

	public static string MojoDllName => Path.Combine(WeChatOcrData, "mmmojo_64.dll");

	public static void SetBaseDirectory(string baseDirectory)
	{
		_baseDirectory = baseDirectory;
	}
}
