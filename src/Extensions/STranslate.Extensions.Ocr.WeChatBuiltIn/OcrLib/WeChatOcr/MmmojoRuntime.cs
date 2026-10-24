using System;
using System.Threading;

namespace WeChatOcr;

internal sealed class MmmojoRuntime : IDisposable
{
	private sealed class RuntimeLease(MmmojoRuntime owner) : IDisposable
	{
		private MmmojoRuntime? _owner = owner;

		public void Dispose()
		{
			Interlocked.Exchange(ref _owner, null)?.Release();
		}
	}

	private readonly object _syncRoot = new object();

	private readonly IMmmojoApi _api;

	private int _leaseCount;

	private bool _initialized;

	private bool _disposed;

	private readonly bool _registeredForProcessExit;

	internal static MmmojoRuntime Shared { get; } = new MmmojoRuntime(MmmojoApi.Instance, registerForProcessExit: true);

	internal int LeaseCount
	{
		get
		{
			lock (_syncRoot)
			{
				return _leaseCount;
			}
		}
	}

	internal MmmojoRuntime(IMmmojoApi api)
		: this(api, registerForProcessExit: false)
	{
	}

	private MmmojoRuntime(IMmmojoApi api, bool registerForProcessExit)
	{
		_api = api;
		_registeredForProcessExit = registerForProcessExit;
		if (registerForProcessExit)
		{
			AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
		}
	}

	internal IDisposable Acquire()
	{
		lock (_syncRoot)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("MmmojoRuntime");
			}
			if (!_initialized)
			{
				_api.Initialize();
				_initialized = true;
			}
			_leaseCount++;
			return new RuntimeLease(this);
		}
	}

	private void Release()
	{
		lock (_syncRoot)
		{
			if (_leaseCount != 0)
			{
				_leaseCount--;
			}
		}
	}

	public void Dispose()
	{
		lock (_syncRoot)
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			if (_initialized)
			{
				_api.Shutdown();
				_initialized = false;
			}
		}
		if (_registeredForProcessExit)
		{
			AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
		}
	}

	private void OnProcessExit(object? sender, EventArgs e)
	{
		Dispose();
	}
}
