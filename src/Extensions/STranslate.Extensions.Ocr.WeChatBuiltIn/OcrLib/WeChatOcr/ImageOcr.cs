using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace WeChatOcr;

public class ImageOcr : IDisposable
{
	private const uint MaxRetryTimes = 99u;

	private OcrManager? _ocrManager;

	private GCHandle _ocrManagerHandle;

	private int _disposeState;

	internal bool IsManagerHandleAllocated => _ocrManagerHandle.IsAllocated;

	public ImageOcr(string? path = null)
		: this(string.IsNullOrEmpty(path) ? new OcrManager() : new OcrManager(path))
	{
	}

	internal ImageOcr(OcrManager ocrManager)
	{
		_ocrManager = ocrManager;
		try
		{
			_ocrManagerHandle = GCHandle.Alloc(ocrManager);
			ocrManager.StartWeChatOcr(GCHandle.ToIntPtr(_ocrManagerHandle));
		}
		catch
		{
			try
			{
				ocrManager.Dispose();
			}
			finally
			{
				if (_ocrManagerHandle.IsAllocated)
				{
					_ocrManagerHandle.Free();
				}
				_ocrManager = null;
			}
			throw;
		}
	}

	~ImageOcr()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void Run(string imagePath, Action<string, WeChatOcrResult?>? callback)
	{
		OcrManager ocrManager = GetOcrManager();
		if (callback != null)
		{
			ocrManager.SetOcrResultCallback(callback);
		}
		int num = 0;
		while ((long)num <= 99L)
		{
			try
			{
				ocrManager.DoOcrTask(imagePath);
				break;
			}
			catch (OverflowException)
			{
				Thread.Sleep(10);
				num++;
			}
			catch (Exception)
			{
				break;
			}
		}
	}

	public void Run(byte[] bytes, Action<string, WeChatOcrResult?>? callback, ImageType imgType = ImageType.Png)
	{
		ThrowIfDisposed();
		string text = Path.Combine(Path.GetTempPath(), "generate4wechat." + imgType.ToString().ToLower());
		Utilities.WriteBytesToFile(text, bytes);
		Run(text, callback);
	}

	private OcrManager GetOcrManager()
	{
		ThrowIfDisposed();
		return _ocrManager ?? throw new ObjectDisposedException("ImageOcr");
	}

	private void ThrowIfDisposed()
	{
		if (Volatile.Read(in _disposeState) != 0)
		{
			throw new ObjectDisposedException("ImageOcr");
		}
	}

	private void Dispose(bool disposing)
	{
		if (Interlocked.Exchange(ref _disposeState, 1) != 0)
		{
			return;
		}
		try
		{
			_ocrManager?.Dispose();
		}
		catch when (!disposing)
		{
		}
		finally
		{
			if (_ocrManagerHandle.IsAllocated)
			{
				_ocrManagerHandle.Free();
			}
			_ocrManager = null;
		}
	}
}
