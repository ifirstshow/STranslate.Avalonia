using System;

namespace WeChatOcr;

internal sealed class MmmojoApi : IMmmojoApi
{
	internal static MmmojoApi Instance { get; } = new MmmojoApi();

	private MmmojoApi()
	{
	}

	public void Initialize()
	{
		MmmojoDll.InitializeMMMojo(0, IntPtr.Zero);
	}

	public void Shutdown()
	{
		MmmojoDll.ShutdownMMMojo();
	}

	public nint CreateEnvironment()
	{
		return MmmojoDll.CreateMMMojoEnvironment();
	}

	public void SetEnvironmentCallback(nint environment, int type, nint callback)
	{
		MmmojoDll.SetMMMojoEnvironmentCallbacks(environment, type, callback);
	}

	public void SetEnvironmentInitParam(nint environment, int type, nint parameter)
	{
		MmmojoDll.SetMMMojoEnvironmentInitParams(environment, type, parameter);
	}

	public void AppendSubProcessSwitch(nint environment, nint switchName, nint value)
	{
		MmmojoDll.AppendMMSubProcessSwitchNative(environment, switchName, value);
	}

	public void StartEnvironment(nint environment)
	{
		MmmojoDll.StartMMMojoEnvironment(environment);
	}

	public void StopEnvironment(nint environment)
	{
		MmmojoDll.StopMMMojoEnvironment(environment);
	}

	public void RemoveEnvironment(nint environment)
	{
		MmmojoDll.RemoveMMMojoEnvironment(environment);
	}

	public nint CreateWriteInfo(int method, int sync, uint requestId)
	{
		return MmmojoDll.CreateMMMojoWriteInfo(method, sync, requestId);
	}

	public nint GetWriteInfoRequest(nint writeInfo, uint requestDataSize)
	{
		return MmmojoDll.GetMMMojoWriteInfoRequest(writeInfo, requestDataSize);
	}

	public bool SendWriteInfo(nint environment, nint writeInfo)
	{
		return MmmojoDll.SendMMMojoWriteInfo(environment, writeInfo);
	}

	public void RemoveWriteInfo(nint writeInfo)
	{
		MmmojoDll.RemoveMMMojoWriteInfo(writeInfo);
	}

	public nint GetReadInfoRequest(nint readInfo, ref uint requestDataSize)
	{
		return MmmojoDll.GetMMMojoReadInfoRequest(readInfo, ref requestDataSize);
	}

	public nint GetReadInfoAttach(nint readInfo, ref uint attachDataSize)
	{
		return MmmojoDll.GetMMMojoReadInfoAttach(readInfo, ref attachDataSize);
	}

	public void RemoveReadInfo(nint readInfo)
	{
		MmmojoDll.RemoveMMMojoReadInfo(readInfo);
	}
}
