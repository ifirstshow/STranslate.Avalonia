using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WeChatOcr;

public class DefaultCallbacks
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void MMReadPushDelegate(uint request_id, nint request_info, nint user_data);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void MMReadPullDelegate(uint request_id, nint request_info, nint user_data);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void MMReadSharedDelegate(uint request_id, nint request_info, nint user_data);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void MMRemoteConnectDelegate(bool is_connected, nint user_data);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void MMRemoteDisconnectDelegate(nint user_data);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void MMRemoteProcessLaunchedDelegate(nint user_data);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void MMRemoteProcessLaunchFailedDelegate(int error_code, nint user_data);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void MMRemoteMojoErrorDelegate(nint errorbuf, int errorsize, nint user_data);

	public Dictionary<string, Delegate> callbacks = new Dictionary<string, Delegate>();

	public DefaultCallbacks()
	{
		callbacks["kMMReadPush"] = new MMReadPushDelegate(DefaultReadPush);
		callbacks["kMMReadPull"] = new MMReadPullDelegate(DefaultReadPull);
		callbacks["kMMReadShared"] = new MMReadSharedDelegate(DefaultReadShared);
		callbacks["kMMRemoteConnect"] = new MMRemoteConnectDelegate(DefaultRemoteConnect);
		callbacks["kMMRemoteDisconnect"] = new MMRemoteDisconnectDelegate(DefaultRemoteDisconnect);
		callbacks["kMMRemoteProcessLaunched"] = new MMRemoteProcessLaunchedDelegate(DefaultRemoteProcessLaunched);
		callbacks["kMMRemoteProcessLaunchFailed"] = new MMRemoteProcessLaunchFailedDelegate(DefaultRemoteProcessLaunchFailed);
		callbacks["kMMRemoteMojoError"] = new MMRemoteMojoErrorDelegate(DefaultRemoteMojoError);
	}

	private void DefaultReadPush(uint request_id, nint request_info, nint user_data)
	{
	}

	private void DefaultReadPull(uint request_id, nint request_info, nint user_data)
	{
	}

	private void DefaultReadShared(uint request_id, nint request_info, nint user_data)
	{
	}

	private void DefaultRemoteConnect(bool is_connected, nint user_data)
	{
	}

	private void DefaultRemoteDisconnect(nint user_data)
	{
	}

	private void DefaultRemoteProcessLaunched(nint user_data)
	{
	}

	private void DefaultRemoteProcessLaunchFailed(int error_code, nint user_data)
	{
	}

	private void DefaultRemoteMojoError(nint errorbuf, int errorsize, nint user_data)
	{
	}
}
