using STranslate.Sdk;

namespace STranslate.Extensions.Tts.EdgeTts;

/// <summary>
/// Windows 自带 TTS（WinRT SpeechSynthesizer）：离线、免 Key。
/// 扩展只产出 WAV 数据，播放由宿主负责。
/// WinRT 实现仅 windows TFM 编译；非 Windows 为不支持桩（跨平台编译校验用，Registry 同样隔离）。
/// </summary>
#if WINDOWS_TFM
[Extension(
    "7a3ab25875294602b3afc4ae15fec627",
    "Windows TTS",
    Description = "Windows 系统语音合成（离线）",
    Author = "zggsong")]
public sealed class Main : ITtsPlugin
{
    private const string VoiceId = "voice";

    private IPluginContext Context { get; set; } = null!;

    public IReadOnlyList<SettingItem> SettingItems { get; } =
    [
        new TextSetting(VoiceId, "Voice", Default: "", Tooltip: "留空用系统默认；如 Microsoft Huihui")
    ];

    public void Init(IPluginContext context) => Context = context;

    public void Dispose() { }

    public async Task<AudioData> SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        using var synthesizer = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
        var voiceName = Context.Settings.Get(VoiceId, string.Empty);
        if (!string.IsNullOrEmpty(voiceName))
        {
            var voice = Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices
                .FirstOrDefault(v => v.DisplayName.Contains(voiceName, StringComparison.OrdinalIgnoreCase));
            if (voice is not null)
                synthesizer.Voice = voice;
        }

        using var stream = await synthesizer.SynthesizeTextToStreamAsync(text);
        using var memory = new MemoryStream();
        await stream.AsStreamForRead().CopyToAsync(memory, cancellationToken);
        return new AudioData(memory.ToArray(), AudioFormat.Wav);
    }
}
#else
[Extension(
    "7a3ab25875294602b3afc4ae15fec627",
    "Windows TTS",
    Description = "Windows 系统语音合成（离线）",
    Author = "zggsong")]
public sealed class Main : ITtsPlugin
{
    public IReadOnlyList<SettingItem> SettingItems { get; } = [];

    public void Init(IPluginContext context) { }

    public void Dispose() { }

    public Task<AudioData> SpeakAsync(string text, CancellationToken cancellationToken = default)
        => Task.FromResult(new AudioData([], AudioFormat.Wav));
}
#endif
