using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using STranslate.Sdk;

namespace STranslate.Avalonia.Services;

/// <summary>运行中的扩展服务实例。</summary>
public sealed class ExtensionService
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required IPlugin Plugin { get; init; }

    public ITranslatePlugin? AsTranslate => Plugin as ITranslatePlugin;
    public IOcrPlugin? AsOcr => Plugin as IOcrPlugin;
    public ITtsPlugin? AsTts => Plugin as ITtsPlugin;
    public IVocabularyPlugin? AsVocabulary => Plugin as IVocabularyPlugin;

    public override string ToString() => Name;
}

/// <summary>
/// 内置扩展装载器。注册表显式登记（AOT 安全，零反射扫描）；
/// 社区 .spkg 走 worker，不经此路径。
/// </summary>
public sealed class ExtensionHost
{
    // 新增官方扩展时在此登记；OCR 解析按顺序取第一个，免 Key 的微信 OCR 优先
    private static readonly Type[] Registry =
    [
        typeof(global::STranslate.Extensions.Translate.MTranServer.Main),
#if WINDOWS_TFM
        // Windows 专属扩展仅在 windows TFM 构建中存在（降级矩阵第三层）
        typeof(global::STranslate.Extensions.Ocr.WeChatBuiltIn.Main),
#endif
        typeof(global::STranslate.Extensions.Ocr.Google.Main),
        typeof(global::STranslate.Extensions.Tts.EdgeTts.Main),
        typeof(global::STranslate.Extensions.Vocabulary.Eudict.Main),
        typeof(global::STranslate.Extensions.Translate.OpenAI.Main),
        typeof(global::STranslate.Extensions.Translate.ICiba.Main),
    ];

    private readonly ExtensionSettingsStore _settings;
    private readonly IHttpService _http;
    private readonly II18n _i18n;
    private readonly INotification _notification;

    public ExtensionHost(ExtensionSettingsStore settings, IHttpService http, II18n i18n, INotification notification)
    {
        _settings = settings;
        _http = http;
        _i18n = i18n;
        _notification = notification;
    }

    public IReadOnlyList<ExtensionService> LoadAll()
    {
        var services = new List<ExtensionService>();
        foreach (var type in Registry)
        {
            var attr = (ExtensionAttribute)Attribute.GetCustomAttribute(type, typeof(ExtensionAttribute))!;
            var plugin = (IPlugin)Activator.CreateInstance(type)!;
            plugin.Init(new HostPluginContext(_http, _settings.For(attr.Id), _i18n, _notification));
            services.Add(new ExtensionService { Id = attr.Id, Name = attr.Name, Plugin = plugin });
            DebugTrace.Log($"extension loaded: {attr.Name} ({type.FullName}) ocr={plugin is IOcrPlugin}");
        }
        services.AddRange(LoadWorkerPlugins());
        return services;
    }

    /// <summary>社区 .spkg 插件：有已装包时同步启动 worker（亚秒级，5s 超时兜底），describe 后转代理并入列表。</summary>
    private IEnumerable<ExtensionService> LoadWorkerPlugins()
    {
        var manager = new WorkerProcessManager();
        try
        {
            // 必须经 Task.Run：StartAsync 的 await 续体要回调用线程上下文，
            // UI 线程同步 Wait 会死锁到超时（进程活着但 manager 被丢弃 → dll 锁无法释放）
            var startTask = Task.Run(() => manager.StartAsync(PluginPackageManager.PluginsRoot));
            if (!startTask.Wait(TimeSpan.FromSeconds(5)))
            {
                DebugTrace.Log("worker: start timed out");
                manager.Dispose();
                return [];
            }
            if (startTask.Exception is not null)
            {
                DebugTrace.Log($"worker: start failed ({startTask.Exception.InnerException?.Message})");
                manager.Dispose();
                return [];
            }
            _worker = manager;
            return startTask.Result.Select(desc =>
            {
                ExtensionService service = desc.Kind switch
                {
                    "translate" => new ExtensionService { Id = desc.Id, Name = desc.Name, Plugin = new WorkerTranslateProxy(manager, desc) },
                    "ocr" => new ExtensionService { Id = desc.Id, Name = desc.Name, Plugin = new WorkerOcrProxy(manager, desc) },
                    "tts" => new ExtensionService { Id = desc.Id, Name = desc.Name, Plugin = new WorkerTtsProxy(manager, desc) },
                    "vocabulary" => new ExtensionService { Id = desc.Id, Name = desc.Name, Plugin = new WorkerVocabularyProxy(manager, desc) },
                    _ => throw new InvalidOperationException($"unsupported worker plugin kind: {desc.Kind}")
                };
                DebugTrace.Log($"worker plugin loaded: {desc.Name} ({desc.Kind})");
                return service;
            }).ToList();
        }
        catch (Exception e)
        {
            DebugTrace.Log($"worker: load failed: {e.Message}");
            manager.Dispose();
            return [];
        }
    }

    private WorkerProcessManager? _worker;

    /// <summary>停止 worker 进程（释放社区插件 dll 文件锁），卸载/重装前调用。</summary>
    public void StopWorker()
    {
        if (_worker is null)
            return;
        DebugTrace.Log("worker: stopping for uninstall");
        _worker.Dispose();
        _worker = null;
    }

    private sealed class HostPluginContext(
        IHttpService http,
        IPluginSettings settings,
        II18n i18n,
        INotification notification) : IPluginContext
    {
        public ILogger Logger => NullLogger.Instance;
        public IHttpService Http { get; } = http;
        public IPluginSettings Settings { get; } = settings;
        public II18n I18n { get; } = i18n;
        public INotification Notification { get; } = notification;

        public void Dispose() { }
    }
}
