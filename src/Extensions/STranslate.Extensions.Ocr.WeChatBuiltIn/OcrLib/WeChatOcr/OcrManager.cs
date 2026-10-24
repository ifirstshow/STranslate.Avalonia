using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Google.Protobuf;
using OcrProtobuf;

namespace WeChatOcr;

[StructLayout(LayoutKind.Sequential)]
public class OcrManager : XPluginManager, IDisposable
{
	public const int OcrMaxTaskId = 32;

	private Action<string, WeChatOcrResult?>? Callback;

	private bool isWeChatOcrRunning;

	private readonly Dictionary<int, string> dicImageID = new Dictionary<int, string>();

	private readonly Queue<int> queueIds = new Queue<int>();

	private readonly Stopwatch[] timers = new Stopwatch[32];

	private volatile bool isConnected;

	private readonly object taskLock = new object();

	internal int TrackedImageCount
	{
		get
		{
			lock (taskLock)
			{
				return dicImageID.Count;
			}
		}
	}

	public OcrManager(string? path = null)
		: this(path, MmmojoApi.Instance, MmmojoRuntime.Shared)
	{
	}

	internal OcrManager(string? path, IMmmojoApi native, MmmojoRuntime runtime)
		: base(native, runtime)
	{
		if (string.IsNullOrEmpty(path))
		{
			SetExePath();
			SetUsrLibDir();
		}
		else
		{
			string exePath = Utilities.GetWeChatOcrExePath() ?? throw new Exception("get wechat ocr exe path is null");
			string text = Utilities.GetWeChatDir(path) ?? throw new Exception("get wechat path failed: " + (path ?? "NULL"));
			Utilities.CopyMmmojoDll(text);
			SetExePath(exePath);
			SetUsrLibDir(text);
		}
		for (int i = 0; i < 32; i++)
		{
			queueIds.Enqueue(i);
			timers[i] = new Stopwatch();
		}
	}

	public void SetUsrLibDir(string usrLibDir = "")
	{
		if (string.IsNullOrEmpty(usrLibDir))
		{
			usrLibDir = DataLocation.WeChatOcrData;
		}
		AppendSwitchNativeCmdLine("user-lib-dir", usrLibDir);
	}

	public void SetOcrResultCallback(Action<string, WeChatOcrResult?> func)
	{
		ThrowIfDisposed();
		Callback = func;
	}

	public void StartWeChatOcr(nint ocrManager)
	{
		ThrowIfDisposed();
		SetCallbackUsrData(ocrManager);
		SetDefaultCallbacks();
		InitMmMojoEnv();
		isWeChatOcrRunning = true;
	}

	public void KillWeChatOcr()
	{
		if (!IsDisposed)
		{
			isConnected = false;
			isWeChatOcrRunning = false;
			StopMmMojoEnv();
		}
	}

	public void DoOcrTask(string imgPath)
	{
		ThrowIfDisposed();
		if (!isWeChatOcrRunning)
		{
			throw new Exception("请先调用StartWeChatOCR启动");
		}
		if (!File.Exists(imgPath))
		{
			throw new FileNotFoundException("给定图片路径picPath不存在: " + imgPath);
		}
		imgPath = Path.GetFullPath(imgPath);
		int num = 0;
		while (!isConnected)
		{
			Thread.Sleep(100);
			if (num++ > 3)
			{
				throw new TimeoutException("连接Ocr服务超时！");
			}
		}
		int idleTaskId = GetIdleTaskId();
		if (idleTaskId < 0)
		{
			throw new OverflowException("当前队列已满，请等待后重试！");
		}
		SendOcrTask(idleTaskId, imgPath);
	}

	public void SendOcrTask(int taskId, string picPath)
	{
		lock (taskLock)
		{
			dicImageID[taskId] = picPath;
		}
		OcrRequest ocrRequest = new OcrRequest
		{
			Unknow = 0,
			TaskId = taskId
		};
		OcrRequest.Types.PicPaths picPaths = new OcrRequest.Types.PicPaths();
		picPaths.PicPath.Add(picPath);
		ocrRequest.PicPath = picPaths;
		byte[] array = MessageExtensions.ToByteArray((IMessage)(object)ocrRequest);
		SendPbSerializedData(array, array.Length, 1, 0, 1u);
	}

	public WeChatOcrResult? ParseJsonResponse(string jsonResponseStr)
	{
		return ParseOcrResult.ParseJson(jsonResponseStr);
	}

	public void SetConnectState(bool connect)
	{
		isConnected = connect;
	}

	public int GetIdleTaskId()
	{
		lock (taskLock)
		{
			int num = -1;
			try
			{
				num = queueIds.Dequeue();
				timers[num].Restart();
			}
			catch (InvalidOperationException)
			{
			}
			return num;
		}
	}

	public void SetTaskIdIdle(int taskId)
	{
		lock (taskLock)
		{
			if (dicImageID.Remove(taskId))
			{
				timers[taskId].Stop();
				queueIds.Enqueue(taskId);
			}
		}
	}

	public void SetDefaultCallbacks()
	{
		SetOneCallback("kMMRemoteConnect", new DefaultCallbacks.MMRemoteConnectDelegate(OCRRemoteOnConnect));
		SetOneCallback("kMMRemoteDisconnect", new DefaultCallbacks.MMRemoteDisconnectDelegate(OCRRemoteOnDisconnect));
		SetOneCallback("kMMReadPush", new DefaultCallbacks.MMReadPushDelegate(OCRReadOnPush));
	}

	public void OCRRemoteOnConnect(bool isConnected, nint userData)
	{
		if (userData != IntPtr.Zero && GCHandle.FromIntPtr(userData).Target is OcrManager ocrManager)
		{
			ocrManager.SetConnectState(connect: true);
		}
	}

	public void OCRRemoteOnDisconnect(nint userData)
	{
		if (userData != IntPtr.Zero)
		{
			SetConnectState(connect: false);
		}
	}

	public void OCRReadOnPush(uint requestId, nint requestInfo, nint userData)
	{
		if (userData == IntPtr.Zero || requestInfo == IntPtr.Zero)
		{
			return;
		}
		try
		{
			uint dataSize = 0u;
			nint pbSerializedData = GetPbSerializedData(requestInfo, ref dataSize);
			if (dataSize > 20 && pbSerializedData != IntPtr.Zero)
			{
				CallUserCallback(requestId, pbSerializedData, (int)dataSize);
			}
		}
		finally
		{
			RemoveReadInfo(requestInfo);
		}
	}

	public void CallUserCallback(uint requestId, nint serializedData, int dataSize)
	{
		byte[] array = new byte[dataSize];
		Marshal.Copy(serializedData, array, 0, dataSize);
		OcrResponse ocrResponse = OcrResponse.Parser.ParseFrom(array);
		string jsonResponseStr = ocrResponse.ToString();
		int taskId = ocrResponse.TaskId;
		string value;
		lock (taskLock)
		{
			dicImageID.TryGetValue(taskId, out value);
		}
		if (value == null)
		{
			return;
		}
		try
		{
			Callback?.Invoke(value, ParseJsonResponse(jsonResponseStr));
		}
		catch (Exception)
		{
		}
		finally
		{
			SetTaskIdIdle(taskId);
		}
	}

	public override void Dispose()
	{
		base.Dispose();
	}

	protected override void Dispose(bool disposing)
	{
		if (IsDisposed)
		{
			return;
		}
		try
		{
			if (isWeChatOcrRunning)
			{
				KillWeChatOcr();
			}
		}
		finally
		{
			Callback = null;
			lock (taskLock)
			{
				dicImageID.Clear();
				queueIds.Clear();
				Stopwatch[] array = timers;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Stop();
				}
			}
			base.Dispose(disposing);
		}
	}
}
