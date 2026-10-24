using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WeChatOcr;

public class MmmojoDll
{
	private const string LibraryName = "mmmojo";

	static MmmojoDll()
	{
		NativeLibrary.SetDllImportResolver(typeof(MmmojoDll).Assembly, DllImportResolver);
	}

	private static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		if (libraryName == "mmmojo" && NativeLibrary.TryLoad(DataLocation.MojoDllName, out var handle))
		{
			return handle;
		}
		return IntPtr.Zero;
	}

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern void InitializeMMMojo(int argc, nint argv);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern void ShutdownMMMojo();

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern nint CreateMMMojoEnvironment();

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetMMMojoEnvironmentCallbacks(nint mmmojo_env, int type, nint callback);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern void SetMMMojoEnvironmentInitParams(nint mmmojo_env, int type, nint param);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern void AppendMMSubProcessSwitchNative(nint mmmojo_env, nint switchStringPtr, nint valuePtr);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern void StartMMMojoEnvironment(nint mmmojo_env);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern void StopMMMojoEnvironment(nint mmmojo_env);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RemoveMMMojoEnvironment(nint mmmojo_env);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern nint GetMMMojoReadInfoRequest(nint mmmojo_readinfo, ref uint requestDataSize);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern nint GetMMMojoReadInfoAttach(nint mmmojo_readinfo, ref uint attachDataSize);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RemoveMMMojoReadInfo(nint mmmojo_readinfo);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern int GetMMMojoReadInfoMethod(nint mmmojo_readinfo);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool GetMMMojoReadInfoSync(nint mmmojo_readinfo);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern nint CreateMMMojoWriteInfo(int method, int sync, uint requestId);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern nint GetMMMojoWriteInfoRequest(nint mmmojo_writeinfo, uint requestDataSize);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RemoveMMMojoWriteInfo(nint mmmojo_writeinfo);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern nint GetMMMojoWriteInfoAttach(nint mmmojo_writeinfo, uint attachDataSize);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetMMMojoWriteInfoMessagePipe(nint mmmojo_writeinfo, int numOfMessagePipe);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetMMMojoWriteInfoResponseSync(nint mmmojo_writeinfo, ref nint mmmojo_readinfo);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool SendMMMojoWriteInfo(nint mmmojo_env, nint mmmojo_writeinfo);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool SwapMMMojoWriteInfoCallback(nint mmmojo_writeinfo, nint mmmojo_readinfo);

	[DllImport("mmmojo", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool SwapMMMojoWriteInfoMessage(nint mmmojo_writeinfo, nint mmmojo_readinfo);
}
