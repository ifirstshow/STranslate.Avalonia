namespace STranslate.Sdk;

/// <summary>
/// 生词本扩展接口。
/// </summary>
public interface IVocabularyPlugin : IPlugin
{
    /// <summary>保存生词。</summary>
    Task<VocabularyResult> SaveAsync(string word, CancellationToken cancellationToken = default);

    /// <summary>保存生词并附笔记；默认实现忽略笔记。需要笔记的扩展请重写。</summary>
    Task<VocabularyResult> SaveWithNoteAsync(string word, string note, CancellationToken cancellationToken = default)
        => SaveAsync(word, cancellationToken);
}

/// <summary>生词本保存结果。</summary>
public sealed class VocabularyResult
{
    /// <summary>耗时。</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>是否成功。</summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>错误信息（失败时）。</summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>标记为失败。</summary>
    public VocabularyResult Fail(string message)
    {
        IsSuccess = false;
        ErrorMessage = message;
        return this;
    }
}
