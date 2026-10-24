using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace STranslate.Sdk;

/// <summary>
/// 扩展入口。实现类必须提供公共无参构造函数。
/// 不要直接实现本接口——请实现 <see cref="ITranslatePlugin"/>、<see cref="IDictionaryPlugin"/>、
/// <see cref="IOcrPlugin"/>、<see cref="ITtsPlugin"/> 或 <see cref="IVocabularyPlugin"/>。
/// </summary>
public interface IPlugin : IDisposable
{
    /// <summary>
    /// 初始化扩展。<paramref name="context"/> 携带宿主能力与已加载的设置值，
    /// 实现应保存引用供后续调用使用。
    /// </summary>
    void Init(IPluginContext context);

    /// <summary>
    /// 设置项声明。宿主据此生成设置 UI 并负责持久化。
    /// 必须是纯声明：不含 IO、不依赖 <see cref="Init"/> 先被调用。
    /// </summary>
    IReadOnlyList<SettingItem> SettingItems { get; }
}

/// <summary>
/// 宿主注入的能力集。全部为跨平台接口，不包含任何 UI 类型。
/// </summary>
public interface IPluginContext
{
    /// <summary>日志。</summary>
    ILogger Logger { get; }

    /// <summary>HTTP 能力（GET/POST/表单/下载/流式/代理测试）。</summary>
    IHttpService Http { get; }

    /// <summary>已持久化的设置值（宿主负责加载与保存）。</summary>
    IPluginSettings Settings { get; }

    /// <summary>界面文案国际化。</summary>
    II18n I18n { get; }

    /// <summary>系统通知。</summary>
    INotification Notification { get; }
}

/// <summary>
/// 只读访问宿主持久化的设置值。键对应 <see cref="SettingItem.Id"/>。
/// 建议在每次服务调用时经 <see cref="IPluginContext.Settings"/> 现读现用；
/// 用户修改设置后宿主保证该对象反映最新值，无需重新 <see cref="IPlugin.Init"/>。
/// </summary>
public interface IPluginSettings
{
    /// <summary>读取设置值；缺失或类型不符时返回 <paramref name="fallback"/>。</summary>
    T Get<T>(string id, T fallback);

    /// <summary>原始 JSON 兜底访问。</summary>
    IReadOnlyDictionary<string, JsonElement> Raw { get; }
}

/// <summary>界面文案国际化。</summary>
public interface II18n
{
    /// <summary>按当前界面语言取文案，缺失时返回键名。</summary>
    string GetTranslation(string key);
}

/// <summary>系统通知。</summary>
public interface INotification
{
    /// <summary>弹出系统通知。</summary>
    void Show(string title, string message);
}
