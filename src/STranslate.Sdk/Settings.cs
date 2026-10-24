namespace STranslate.Sdk;

/// <summary>
/// 设置项描述符基类。宿主据此渲染设置 UI 并持久化取值；
/// 扩展经 <see cref="IPluginSettings.Get{T}"/> 按 <see cref="Id"/> 读取。
/// </summary>
public abstract record SettingItem(string Id, string Label, string? Tooltip = null);

/// <summary>单行文本。</summary>
public sealed record TextSetting(string Id, string Label, string? Placeholder = null, string? Default = null, string? Tooltip = null) : SettingItem(Id, Label, Tooltip);

/// <summary>密码文本（界面掩码显示）。</summary>
public sealed record PasswordSetting(string Id, string Label, string? Placeholder = null, string? Tooltip = null) : SettingItem(Id, Label, Tooltip);

/// <summary>多行文本。</summary>
public sealed record MultilineTextSetting(string Id, string Label, string? Placeholder = null, string? Default = null, string? Tooltip = null) : SettingItem(Id, Label, Tooltip);

/// <summary>数值。</summary>
public sealed record NumberSetting(string Id, string Label, double Min, double Max, double Step = 1, double? Default = null, string? Tooltip = null) : SettingItem(Id, Label, Tooltip);

/// <summary>开关。</summary>
public sealed record ToggleSetting(string Id, string Label, bool Default = false, string? Tooltip = null) : SettingItem(Id, Label, Tooltip);

/// <summary>下拉选择。</summary>
public sealed record SelectSetting(string Id, string Label, IReadOnlyList<SelectOption> Options, string? Default = null, string? Tooltip = null) : SettingItem(Id, Label, Tooltip);

/// <summary>下拉选项。</summary>
public sealed record SelectOption(string Value, string Label)
{
    public override string ToString() => Label;
}

/// <summary>
/// LLM Prompt 列表。宿主提供内置编辑器（增删改、启用切换）；
/// 扩展经 <c>Settings.Get&lt;PromptList&gt;(Id)</c> 读取当前列表与选中项。
/// </summary>
public sealed record PromptListSetting(string Id, string Label, IReadOnlyList<Prompt>? Default = null) : SettingItem(Id, Label);

/// <summary>Prompt 条目。</summary>
public sealed class Prompt
{
    /// <summary>名称（用于展示与选择）。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Prompt 内容。</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>是否启用（列表中仅一项启用）。</summary>
    public bool IsEnabled { get; set; }
}

/// <summary><see cref="PromptListSetting"/> 的持久化形态。</summary>
public sealed class PromptList
{
    /// <summary>Prompt 列表。</summary>
    public List<Prompt> Prompts { get; set; } = [];

    /// <summary>当前启用的 Prompt；无启用项时返回 null。</summary>
    public Prompt? Selected => Prompts.FirstOrDefault(p => p.IsEnabled);
}
