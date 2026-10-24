using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace WeChatOcr;

public class Utilities
{
	private const string WeChatDefaultPath = "C:\\Program Files\\Tencent\\WeChat";

	private const string MmMojoDll = "mmmojo.dll";

	private const string MmMojo64Dll = "mmmojo_64.dll";

	public static string? GetWeChatDir(string? path = null)
	{
		if (path == null)
		{
			path = "C:\\Program Files\\Tencent\\WeChat";
		}
		if (File.Exists(Path.Combine(path, "mmmojo.dll")))
		{
			return path.TrimEnd('\\');
		}
		using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\WeChat");
		if (!(registryKey?.GetValue("DisplayVersion") is string text))
		{
			return null;
		}
		string text2 = Path.Combine(path, "[" + text + "]");
		return Directory.Exists(text2) ? text2 : null;
	}

	public static string? GetWeChatOcrExePath()
	{
		string text = Directory.EnumerateFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tencent\\WeChat\\XPlugin\\Plugins\\WeChatOCR"), "WeChatOCR.exe", SearchOption.AllDirectories).FirstOrDefault();
		if (text != null && File.Exists(text))
		{
			return text;
		}
		return null;
	}

	public static void WriteBytesToFile(string filePath, byte[] bytes)
	{
		using FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
		fileStream.Write(bytes, 0, bytes.Length);
	}

	public static void CopyMmmojoDll(string wechatFullDir)
	{
		string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DataLocation.WeChatOcrData);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string sourceFileName = Path.Combine(wechatFullDir, "mmmojo.dll");
		string sourceFileName2 = Path.Combine(wechatFullDir, "mmmojo_64.dll");
		string text2 = Path.Combine(text, "mmmojo.dll");
		string text3 = Path.Combine(text, "mmmojo_64.dll");
		if (!File.Exists(text2))
		{
			File.Copy(sourceFileName, text2);
		}
		if (!File.Exists(text3))
		{
			File.Copy(sourceFileName2, text3);
		}
	}

	public static bool RemoveMmmojoDll(out string error)
	{
		bool result = true;
		error = string.Empty;
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		string path = Path.Combine(baseDirectory, "mmmojo.dll");
		string path2 = Path.Combine(baseDirectory, "mmmojo_64.dll");
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			if (File.Exists(path2))
			{
				File.Delete(path2);
			}
		}
		catch (Exception ex)
		{
			error = ex.Message;
			result = false;
		}
		return result;
	}
}
