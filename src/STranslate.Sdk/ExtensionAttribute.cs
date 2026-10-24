namespace STranslate.Sdk;

/// <summary>
/// 扩展身份元数据。内置直调宿主经特性反射读取（AOT 安全）；
/// 打包为 .spkg 分发时，打包工具据此生成 plugin.json。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ExtensionAttribute(string id, string name) : Attribute
{
    /// <summary>扩展唯一标识（GUID 字符串，与 v1 插件 ID 兼容复用）。</summary>
    public string Id { get; } = id;

    /// <summary>展示名。</summary>
    public string Name { get; } = name;

    /// <summary>描述。</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>作者。</summary>
    public string Author { get; init; } = string.Empty;

    /// <summary>版本。</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>官方网站。</summary>
    public string Website { get; init; } = string.Empty;
}
