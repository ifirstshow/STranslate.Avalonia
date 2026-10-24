using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace WeChatOcr;

[StructLayout(LayoutKind.Sequential)]
public class XPluginManager : IDisposable
{
	private readonly object _lifecycleLock = new object();

	private readonly IMmmojoApi _native;

	private readonly MmmojoRuntime _runtime;

	private bool _isMmmojoEnvInited;

	private bool _disposed;

	private nint _mmmojoEnvironment = IntPtr.Zero;

	private nint _userData;

	private string? _ocrExePath;

	private List<string> _commandLines = new List<string>();

	private readonly DefaultCallbacks _defaultCallbacks = new DefaultCallbacks();

	private readonly Dictionary<string, Delegate> _callbacks = new Dictionary<string, Delegate>();

	private readonly Dictionary<string, string> _switchNative = new Dictionary<string, string>();

	private readonly List<nint> _nativeArguments = new List<nint>();

	private IDisposable? _runtimeLease;

	protected bool IsDisposed => _disposed;

	internal int NativeArgumentCount => _nativeArguments.Count;

	public XPluginManager()
		: this(MmmojoApi.Instance, MmmojoRuntime.Shared)
	{
	}

	internal XPluginManager(IMmmojoApi native, MmmojoRuntime runtime)
	{
		_native = native;
		_runtime = runtime;
	}

	~XPluginManager()
	{
		try
		{
			Dispose(disposing: false);
		}
		catch
		{
		}
	}

	public void SetExePath(string exePath = "")
	{
		ThrowIfDisposed();
		if (string.IsNullOrEmpty(exePath))
		{
			exePath = DataLocation.WeChatOcrData;
		}
		if (!exePath.EndsWith("WeChatOCR.exe", StringComparison.OrdinalIgnoreCase) && Directory.Exists(exePath))
		{
			exePath = Path.Combine(exePath, "WeChatOCR.exe");
		}
		if (!File.Exists(exePath))
		{
			throw new Exception("指定的 WeChatOCR.exe 路径不存在!");
		}
		lock (_lifecycleLock)
		{
			if (_isMmmojoEnvInited)
			{
				throw new InvalidOperationException("MMMojo 环境启动后不能更改可执行文件路径。");
			}
			_ocrExePath = exePath;
		}
	}

	public void AppendSwitchNativeCmdLine(string arg, string value)
	{
		ThrowIfDisposed();
		lock (_lifecycleLock)
		{
			if (_isMmmojoEnvInited)
			{
				throw new InvalidOperationException("MMMojo 环境启动后不能更改启动参数。");
			}
			_switchNative[arg] = value;
		}
	}

	public void SetCommandLine(List<string> cmdline)
	{
		ThrowIfDisposed();
		_commandLines = cmdline;
	}

	public void SetOneCallback(string name, Delegate func)
	{
		ThrowIfDisposed();
		_callbacks[name] = func;
	}

	public void SetCallbacks(Dictionary<string, Delegate> callBacks)
	{
		ThrowIfDisposed();
		foreach (KeyValuePair<string, Delegate> callBack in callBacks)
		{
			_callbacks[callBack.Key] = callBack.Value;
		}
	}

	public void SetCallbackUsrData(nint cbUsrData)
	{
		ThrowIfDisposed();
		_userData = cbUsrData;
	}

	public void InitMmMojoEnv()
	{
		ThrowIfDisposed();
		lock (_lifecycleLock)
		{
			if (_isMmmojoEnvInited && _mmmojoEnvironment != IntPtr.Zero)
			{
				return;
			}
			string ocrExePath = _ocrExePath;
			if (string.IsNullOrEmpty(ocrExePath) || !File.Exists(ocrExePath))
			{
				throw new Exception("给定的 WeChatOcr.exe 路径错误 (m_exe_path): " + ocrExePath);
			}
			IDisposable disposable = null;
			nint num = IntPtr.Zero;
			bool flag = false;
			try
			{
				disposable = _runtime.Acquire();
				num = _native.CreateEnvironment();
				if (num == IntPtr.Zero)
				{
					throw new Exception("CreateMMMojoEnvironment 失败!");
				}
				_native.SetEnvironmentCallback(num, 0, _userData);
				SetDefaultCallbacks(num);
				_native.SetEnvironmentInitParam(num, 0, new IntPtr(1));
				nint parameter = AllocateNativeString(ocrExePath, unicode: true);
				_native.SetEnvironmentInitParam(num, 2, parameter);
				foreach (KeyValuePair<string, string> item in _switchNative)
				{
					nint switchName = AllocateNativeString(item.Key, unicode: false);
					nint value = AllocateNativeString(item.Value, unicode: true);
					_native.AppendSubProcessSwitch(num, switchName, value);
				}
				_native.StartEnvironment(num);
				flag = true;
				_mmmojoEnvironment = num;
				_runtimeLease = disposable;
				_isMmmojoEnvInited = true;
				num = IntPtr.Zero;
				disposable = null;
			}
			catch
			{
				if (num != IntPtr.Zero)
				{
					try
					{
						if (flag)
						{
							_native.StopEnvironment(num);
						}
					}
					finally
					{
						_native.RemoveEnvironment(num);
					}
				}
				ReleaseNativeArguments();
				disposable?.Dispose();
				throw;
			}
		}
	}

	private void SetDefaultCallbacks(nint environment)
	{
		foreach (MMMojoCallbackType value3 in Enum.GetValues(typeof(MMMojoCallbackType)))
		{
			if (value3 == MMMojoCallbackType.kMMUserData)
			{
				continue;
			}
			try
			{
				string key = value3.ToString();
				Delegate d = _defaultCallbacks.callbacks[key];
				if (_callbacks.TryGetValue(key, out Delegate value))
				{
					d = value;
				}
				nint functionPointerForDelegate = Marshal.GetFunctionPointerForDelegate(d);
				_native.SetEnvironmentCallback(environment, (int)value3, functionPointerForDelegate);
			}
			catch (Exception value2)
			{
				Console.WriteLine(value2);
			}
		}
	}

	public void StopMmMojoEnv()
	{
		lock (_lifecycleLock)
		{
			nint mmmojoEnvironment = _mmmojoEnvironment;
			IDisposable runtimeLease = _runtimeLease;
			_mmmojoEnvironment = IntPtr.Zero;
			_runtimeLease = null;
			_isMmmojoEnvInited = false;
			try
			{
				if (mmmojoEnvironment != IntPtr.Zero)
				{
					try
					{
						_native.StopEnvironment(mmmojoEnvironment);
						return;
					}
					finally
					{
						_native.RemoveEnvironment(mmmojoEnvironment);
					}
				}
			}
			finally
			{
				ReleaseNativeArguments();
				runtimeLease?.Dispose();
			}
		}
	}

	public void SendPbSerializedData(byte[] pbData, int pbSize, int method, int sync, uint requestId)
	{
		ThrowIfDisposed();
		nint mmmojoEnvironment = _mmmojoEnvironment;
		if (!_isMmmojoEnvInited || mmmojoEnvironment == IntPtr.Zero)
		{
			throw new InvalidOperationException("MMMojo 环境尚未启动。");
		}
		nint num = _native.CreateWriteInfo(method, sync, requestId);
		if (num == IntPtr.Zero)
		{
			throw new InvalidOperationException("CreateMMMojoWriteInfo 失败。");
		}
		bool flag = false;
		try
		{
			nint writeInfoRequest = _native.GetWriteInfoRequest(num, (uint)pbSize);
			Marshal.Copy(pbData, 0, writeInfoRequest, pbSize);
			flag = _native.SendWriteInfo(mmmojoEnvironment, num);
			if (!flag)
			{
				throw new InvalidOperationException("SendMMMojoWriteInfo 失败。");
			}
		}
		finally
		{
			if (!flag)
			{
				_native.RemoveWriteInfo(num);
			}
		}
	}

	public nint GetPbSerializedData(nint requestInfo, ref uint dataSize)
	{
		return _native.GetReadInfoRequest(requestInfo, ref dataSize);
	}

	public nint GetReadInfoAttachData(nint requestInfo, ref uint dataSize)
	{
		return _native.GetReadInfoAttach(requestInfo, ref dataSize);
	}

	public void RemoveReadInfo(nint requestInfo)
	{
		if (requestInfo != IntPtr.Zero)
		{
			_native.RemoveReadInfo(requestInfo);
		}
	}

	public virtual void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}
		try
		{
			StopMmMojoEnv();
		}
		finally
		{
			_disposed = true;
			ReleaseNativeArguments();
			_callbacks.Clear();
			_switchNative.Clear();
			_commandLines.Clear();
			_userData = IntPtr.Zero;
			_ocrExePath = null;
		}
	}

	protected void ThrowIfDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
	}

	private nint AllocateNativeString(string value, bool unicode)
	{
		nint num = (unicode ? Marshal.StringToHGlobalUni(value) : Marshal.StringToHGlobalAnsi(value));
		_nativeArguments.Add(num);
		return num;
	}

	private void ReleaseNativeArguments()
	{
		foreach (nint nativeArgument in _nativeArguments)
		{
			if (nativeArgument != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(nativeArgument);
			}
		}
		_nativeArguments.Clear();
	}
}
