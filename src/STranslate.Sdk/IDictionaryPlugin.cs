using System.Text.Json.Serialization;

namespace STranslate.Sdk;

/// <summary>
/// 词典扩展接口。查词结果以返回值交付。
/// </summary>
public interface IDictionaryPlugin : IPlugin
{
    /// <summary>查词。文本类词典返回 <see cref="DictionaryResult.Text"/>；结构化词典填充各集合。</summary>
    Task<DictionaryResult> TranslateAsync(string content, CancellationToken cancellationToken = default);
}

/// <summary>词典结果类型。</summary>
public enum DictionaryResultType
{
    /// <summary>无。</summary>
    None,

    /// <summary>成功。</summary>
    Success,

    /// <summary>错误。</summary>
    Error,

    /// <summary>无结果。</summary>
    NoResult,
}

/// <summary>词典查询结果（纯数据，可 JSON 序列化）。</summary>
public sealed class DictionaryResult
{
    /// <summary>结果类型。</summary>
    public DictionaryResultType ResultType { get; set; } = DictionaryResultType.None;

    /// <summary>纯文本内容（原文或错误信息）。</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>音标。</summary>
    public List<Symbol> Symbols { get; set; } = [];

    /// <summary>词典释义。</summary>
    public List<DictMean> DictMeans { get; set; } = [];

    /// <summary>复数形式。</summary>
    public List<string> Plurals { get; set; } = [];

    /// <summary>过去式。</summary>
    public List<string> PastTense { get; set; } = [];

    /// <summary>过去分词。</summary>
    public List<string> PastParticiple { get; set; } = [];

    /// <summary>现在分词/动名词。</summary>
    public List<string> PresentParticiple { get; set; } = [];

    /// <summary>第三人称单数。</summary>
    public List<string> ThirdPersonSingular { get; set; } = [];

    /// <summary>比较级。</summary>
    public List<string> Comparative { get; set; } = [];

    /// <summary>最高级。</summary>
    public List<string> Superlative { get; set; } = [];

    /// <summary>标签。</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>例句。</summary>
    public List<string> Sentences { get; set; } = [];

    /// <summary>耗时。</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>标记为失败。</summary>
    public DictionaryResult Fail(string message)
    {
        ResultType = DictionaryResultType.Error;
        Text = message;
        return this;
    }
}

/// <summary>音标。</summary>
public sealed class Symbol
{
    /// <summary>标签（如美/英）。</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>音标内容。</summary>
    public string Phonetic { get; set; } = string.Empty;

    /// <summary>发音音频地址（播放由宿主负责）。</summary>
    public string AudioUrl { get; set; } = string.Empty;
}

/// <summary>词典释义。</summary>
public sealed class DictMean
{
    /// <summary>词性。</summary>
    public string PartOfSpeech { get; set; } = string.Empty;

    /// <summary>释义列表。</summary>
    public List<string> Means { get; set; } = [];

    /// <summary>释义合并文本（UI 渲染用）。</summary>
    [JsonIgnore]
    public string MeansText => string.Join("；", Means);
}
