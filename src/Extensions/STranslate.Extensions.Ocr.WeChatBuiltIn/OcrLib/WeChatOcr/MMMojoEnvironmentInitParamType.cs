namespace WeChatOcr;

public enum MMMojoEnvironmentInitParamType
{
	kMMHostProcess = 0,
	kMMLoopStartThread = 1,
	kMMExePath = 2,
	kMMLogPath = 3,
	kMMLogToStderr = 4,
	kMMAddNumMessagepipe = 5,
	kMMSetDisconnectHandlers = 6,
	kMMDisableDefaultPolicy = 1000,
	kMMElevated = 1001,
	kMMCompatible = 1002
}
