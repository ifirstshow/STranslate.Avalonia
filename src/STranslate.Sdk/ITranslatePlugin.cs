namespace STranslate.Sdk;

/// <summary>
/// 翻译扩展接口。服务核心纯异步无 UI；
/// 结果以返回值交付，不共享可变状态。
/// </summary>
public interface ITranslatePlugin : IPlugin
{
    /// <summary>把宿主语言枚举映射为服务端源语言代码；不支持返回 null。</summary>
    string? GetSourceLanguage(LangEnum lang);

    /// <summary>把宿主语言枚举映射为服务端目标语言代码；不支持返回 null。</summary>
    string? GetTargetLanguage(LangEnum lang);

    /// <summary>
    /// 执行翻译。
    /// </summary>
    /// <param name="request">翻译请求。</param>
    /// <param name="deltas">
    /// LLM 增量输出回调，可为 null。约定：deltas 仅用于界面渐进展示，
    /// 最终文本以返回的 <see cref="TranslateResult"/> 为准。
    /// </param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task<TranslateResult> TranslateAsync(TranslateRequest request, IProgress<string>? deltas = null, CancellationToken cancellationToken = default);
}

/// <summary>翻译请求。</summary>
/// <param name="Text">待翻译文本。</param>
/// <param name="SourceLang">源语言。</param>
/// <param name="TargetLang">目标语言。</param>
public sealed record TranslateRequest(string Text, LangEnum SourceLang, LangEnum TargetLang);

/// <summary>翻译结果（纯数据，可 JSON 序列化）。</summary>
public sealed class TranslateResult
{
    /// <summary>是否成功。</summary>
    public bool IsSuccess { get; init; } = true;

    /// <summary>译文或错误文本。</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>耗时。</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>错误信息（失败时）。</summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>构造成功结果。</summary>
    public static TranslateResult Ok(string text, TimeSpan duration) => new() { Text = text, Duration = duration };

    /// <summary>构造失败结果。</summary>
    public static TranslateResult Fail(string message, TimeSpan duration) => new() { IsSuccess = false, Text = message, ErrorMessage = message, Duration = duration };
}
