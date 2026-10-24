namespace WeChatOcr;

internal interface IMmmojoApi
{
	void Initialize();

	void Shutdown();

	nint CreateEnvironment();

	void SetEnvironmentCallback(nint environment, int type, nint callback);

	void SetEnvironmentInitParam(nint environment, int type, nint parameter);

	void AppendSubProcessSwitch(nint environment, nint switchName, nint value);

	void StartEnvironment(nint environment);

	void StopEnvironment(nint environment);

	void RemoveEnvironment(nint environment);

	nint CreateWriteInfo(int method, int sync, uint requestId);

	nint GetWriteInfoRequest(nint writeInfo, uint requestDataSize);

	bool SendWriteInfo(nint environment, nint writeInfo);

	void RemoveWriteInfo(nint writeInfo);

	nint GetReadInfoRequest(nint readInfo, ref uint requestDataSize);

	nint GetReadInfoAttach(nint readInfo, ref uint attachDataSize);

	void RemoveReadInfo(nint readInfo);
}
