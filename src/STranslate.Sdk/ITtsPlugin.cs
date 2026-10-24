namespace STranslate.Sdk;

/// <summary>
/// TTS 扩展接口。扩展只负责合成并返回音频数据，播放统一由宿主负责。
/// </summary>
public interface ITtsPlugin : IPlugin
{
    /// <summary>合成语音并返回音频数据。</summary>
    Task<AudioData> SpeakAsync(string text, CancellationToken cancellationToken = default);
}
