using System.Text.Json;
using STranslate.Core;
using STranslate.Sdk;

namespace STranslate.Avalonia.Services;

/// <summary>
/// 扩展设置持久化：{扩展Id → {设置项Id → 值}}，存于宿主独立目录（与 WPF 共存期隔离）。
/// </summary>
public sealed class ExtensionSettingsStore
{
    private static readonly string FilePath =
        Path.Combine(DataLocation.DataDirectory(), "Avalonia", "extension-settings.json");

    private Dictionary<string, Dictionary<string, JsonElement>> _data = [];

    public ExtensionSettingsStore()
    {
        try
        {
            if (File.Exists(FilePath))
                _data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(File.ReadAllText(FilePath)) ?? [];
        }
        catch (Exception e)
        {
            // ponytail: 配置损坏不应阻塞启动；M4 备份体系接管后记录日志
            _ = e;
            _data = [];
        }
    }

    /// <summary>取某扩展的设置视图（IPluginSettings 实现）。</summary>
    public IPluginSettings For(string extensionId) => new View(this, extensionId);

    /// <summary>读取某扩展的当前原始设置（设置页 UI 展示用）。</summary>
    public IReadOnlyDictionary<string, JsonElement> GetRaw(string extensionId)
    {
        if (_data.TryGetValue(extensionId, out var settings))
            return settings;
        return new Dictionary<string, JsonElement>();
    }

    /// <summary>写入并落盘（设置 UI 就绪后由宿主调用）。</summary>
    public void Update(string extensionId, string settingId, JsonElement value)
    {
        if (!_data.TryGetValue(extensionId, out var settings))
            _data[extensionId] = settings = [];
        settings[settingId] = value;
        Save();
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(_data));
    }

    private sealed class View(ExtensionSettingsStore store, string extensionId) : IPluginSettings
    {
        public IReadOnlyDictionary<string, JsonElement> Raw =>
            store._data.TryGetValue(extensionId, out var settings) ? settings : new Dictionary<string, JsonElement>();

        public T Get<T>(string id, T fallback)
        {
            if (!store._data.TryGetValue(extensionId, out var settings) ||
                !settings.TryGetValue(id, out var element))
                return fallback;
            try
            {
                return JsonSerializer.Deserialize<T>(element.GetRawText()) ?? fallback;
            }
            catch (JsonException)
            {
                return fallback;
            }
        }
    }
}
