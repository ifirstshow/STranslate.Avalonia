using System.IO.Pipes;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using STranslate.Avalonia.Services;
using STranslate.Sdk;
using STranslate.Worker.Ipc;

namespace STranslate.PluginHost;

/// <summary>
/// 社区插件承载进程（ADR-0004）：JIT 加载 .spkg 解压目录中的插件程序集，
/// 经命名管道 ndjson JSON-RPC 供宿主调用。用法：PluginHost &lt;pipeName&gt; &lt;pluginsRoot&gt;
/// </summary>
internal static class Program
{
    private static readonly Dictionary<string, PluginEntry> Plugins = new();
    private static readonly Dictionary<long, CancellationTokenSource> Inflight = new();

    private static async Task<int> Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: PluginHost <pipeName> <pluginsRoot>");
            return 1;
        }

        AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
            e.Name.StartsWith("STranslate.Sdk", StringComparison.Ordinal)
                ? typeof(IPlugin).Assembly
                : null;

        LoadPlugins(args[1]);

        await using var server = new NamedPipeServerStream(args[0], PipeDirection.InOut, 1,
            PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        await server.WaitForConnectionAsync();
        var connection = new IpcConnection(server);

        try
        {
            // describe 走管道（stdout 是宿主继承的控制台，不是 IPC 通道）
            await WriteDescribeAsync(connection);

            while (await connection.ReadAsync() is { } frame)
            {
                // 无 id 帧 = 宿主单向推送（settings.update），处理完继续
                if (frame.Id is null)
                {
                    if (frame.Method == "settings.update")
                    {
                        using var doc = JsonDocument.Parse(frame.Params ?? "{}");
                        ApplySettings(doc.RootElement);
                    }
                    continue;
                }
                try
                {
                    var result = await DispatchAsync(frame);
                    await connection.WriteAsync(new IpcFrame { Id = frame.Id, Result = result });
                }
                catch (OperationCanceledException)
                {
                    await connection.WriteAsync(new IpcFrame { Id = frame.Id, Error = "cancelled" });
                }
                catch (Exception e)
                {
                    await connection.WriteAsync(new IpcFrame { Id = frame.Id, Error = $"{e.Message}\n{e.StackTrace}" });
                }
            }
        }
        catch (IOException)
        {
            // 宿主断开：安静退出
        }
        catch (ObjectDisposedException)
        {
            // 同上（连接关闭后管道操作抛出）
        }
        finally
        {
            // Dispose 时管道可能已被宿主关闭，避免 Flush 撞断管炸进程
            try { connection.Dispose(); } catch { /* ignore */ }
        }
        return 0;
    }

    // ── 插件加载 ──

    private static void LoadPlugins(string pluginsRoot)
    {
        if (!Directory.Exists(pluginsRoot))
            return;
        foreach (var dir in Directory.EnumerateDirectories(pluginsRoot))
        {
            foreach (var dll in Directory.EnumerateFiles(dir, "*.dll"))
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(dll);
                }
                catch
                {
                    continue; // 非 CLR dll / 依赖缺失
                }

                Type[] types;
                try
                {
                    types = assembly.GetExportedTypes();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute<ExtensionAttribute>();
                    if (attr is null || !typeof(IPlugin).IsAssignableFrom(type))
                        continue;

                    var entry = CreateEntry(type, attr);
                    if (entry is not null)
                        Plugins[attr.Id] = entry;
                }
            }
        }
    }

    private static PluginEntry? CreateEntry(Type type, ExtensionAttribute attr)
    {
        var plugin = (IPlugin)Activator.CreateInstance(type)!;

        // SettingItems 必须可在 Init 前访问（纯声明）；context 稍后在 settings.update 注入时构建
        return new PluginEntry
        {
            Id = attr.Id,
            Name = attr.Name,
            Plugin = plugin,
            Kind =
                type.IsAssignableTo(typeof(ITranslatePlugin)) ? "translate" :
                type.IsAssignableTo(typeof(IOcrPlugin)) ? "ocr" :
                type.IsAssignableTo(typeof(ITtsPlugin)) ? "tts" :
                type.IsAssignableTo(typeof(IVocabularyPlugin)) ? "vocabulary" :
                type.IsAssignableTo(typeof(IDictionaryPlugin)) ? "dictionary" : null
        };
    }

    private static async Task WriteDescribeAsync(IpcConnection connection)
    {
        var plugins = Plugins.Values
            .Where(p => p.Kind is not null)
            .Select(p => new
            {
                id = p.Id,
                name = p.Name,
                kind = p.Kind,
                settingItems = DescribeSettingItems(p.Plugin.SettingItems)
            })
            .ToArray();
        await connection.WriteAsync(new IpcFrame
        {
            Method = "describe",
            Params = JsonSerializer.Serialize(new { plugins })
        });
    }

    /// <summary>SettingItem 多态序列化摊平为 kind 标记结构，宿主据此渲染设置表单。</summary>
    private static List<Dictionary<string, object?>> DescribeSettingItems(IReadOnlyList<SettingItem> items) =>
        items.Select(item =>
        {
            var dict = new Dictionary<string, object?>
            {
                ["kind"] = item switch
                {
                    TextSetting => "text",
                    PasswordSetting => "password",
                    MultilineTextSetting => "multiline",
                    NumberSetting => "number",
                    ToggleSetting => "toggle",
                    SelectSetting => "select",
                    _ => "text"
                },
                ["id"] = item.Id,
                ["label"] = item.Label,
                ["tooltip"] = item.Tooltip
            };
            switch (item)
            {
                case TextSetting t:
                    dict["placeholder"] = t.Placeholder;
                    dict["default"] = t.Default;
                    break;
                case PasswordSetting p:
                    dict["placeholder"] = p.Placeholder;
                    break;
                case MultilineTextSetting m:
                    dict["placeholder"] = m.Placeholder;
                    dict["default"] = m.Default;
                    break;
                case NumberSetting n:
                    dict["min"] = n.Min;
                    dict["max"] = n.Max;
                    dict["step"] = n.Step;
                    dict["default"] = n.Default;
                    break;
                case ToggleSetting tg:
                    dict["default"] = tg.Default;
                    break;
                case SelectSetting s:
                    dict["default"] = s.Default;
                    dict["options"] = s.Options.Select(o => (object?)new { value = o.Value, label = o.Label }).ToList();
                    break;
            }
            return dict;
        }).ToList();

    // ── 请求分发 ──

    private static async Task<string> DispatchAsync(IpcFrame frame)
    {
        using var doc = JsonDocument.Parse(frame.Params ?? "{}");
        var root = doc.RootElement;

        switch (frame.Method)
        {
            case "cancel":
                if (root.TryGetProperty("rid", out var rid) && Inflight.Remove(rid.GetInt64(), out var cts))
                    cts.Cancel();
                return "ok";

            case "settings.update":
                ApplySettings(root);
                return "ok";

            case "translate":
                return await CallTranslateAsync(root, frame.Id ?? 0);

            case "ocr":
                return await CallOcrAsync(root, frame.Id ?? 0);

            case "tts":
                return await CallTtsAsync(root, frame.Id ?? 0);

            case "vocabulary":
                return await CallVocabularyAsync(root, frame.Id ?? 0);

            default:
                throw new InvalidOperationException($"unknown method: {frame.Method}");
        }
    }

    private static void ApplySettings(JsonElement root)
    {
        foreach (var pair in Plugins.Values)
        {
            if (root.TryGetProperty(pair.Id, out var settings))
                pair.ApplySettings(settings);
        }
    }

    private static PluginEntry Require(JsonElement root, string kind)
    {
        var id = root.GetProperty("pluginId").GetString()
                 ?? throw new InvalidOperationException("missing pluginId");
        if (!Plugins.TryGetValue(id, out var entry) || entry.Kind != kind)
            throw new InvalidOperationException($"plugin not found: {id} ({kind})");
        return entry;
    }

    private static async Task<string> CallTranslateAsync(JsonElement root, long rid)
    {
        var entry = Require(root, "translate");
        var plugin = (ITranslatePlugin)entry.Plugin;
        var request = root.GetProperty("request");
        var tr = new TranslateRequest(
            request.GetProperty("text").GetString() ?? "",
            Enum.Parse<LangEnum>(request.GetProperty("sourceLang").GetString() ?? "Auto"),
            Enum.Parse<LangEnum>(request.GetProperty("targetLang").GetString() ?? "ChineseSimplified"));
        using var cts = Track(rid);
        var result = await plugin.TranslateAsync(tr, null, cts.Token);
        return JsonSerializer.Serialize(new
        {
            text = result.Text,
            isSuccess = result.IsSuccess,
            errorMessage = result.ErrorMessage,
            duration = result.Duration
        });
    }

    private static async Task<string> CallOcrAsync(JsonElement root, long rid)
    {
        var entry = Require(root, "ocr");
        var plugin = (IOcrPlugin)entry.Plugin;
        var request = root.GetProperty("request");
        var bytes = Convert.FromBase64String(request.GetProperty("imageBase64").GetString() ?? "");
        var or = new OcrRequest(
            bytes,
            request.GetProperty("imageFormat").GetString() ?? "image/png",
            Enum.Parse<LangEnum>(request.GetProperty("sourceLang").GetString() ?? "Auto"),
            request.TryGetProperty("pixelWidth", out var w) ? w.GetInt32() : 0,
            request.TryGetProperty("pixelHeight", out var h) ? h.GetInt32() : 0);
        using var cts = Track(rid);
        var result = await plugin.RecognizeAsync(or, cts.Token);
        return JsonSerializer.Serialize(new
        {
            text = result.Text,
            isSuccess = result.IsSuccess,
            errorMessage = result.ErrorMessage,
            duration = result.Duration
        });
    }

    private static async Task<string> CallTtsAsync(JsonElement root, long rid)
    {
        var entry = Require(root, "tts");
        var plugin = (ITtsPlugin)entry.Plugin;
        using var cts = Track(rid);
        var audio = await plugin.SpeakAsync(root.GetProperty("text").GetString() ?? "", cts.Token);
        return JsonSerializer.Serialize(new
        {
            audioBase64 = Convert.ToBase64String(audio.Content),
            format = audio.Format.ToString()
        });
    }

    private static async Task<string> CallVocabularyAsync(JsonElement root, long rid)
    {
        var entry = Require(root, "vocabulary");
        var plugin = (IVocabularyPlugin)entry.Plugin;
        using var cts = Track(rid);
        var result = await plugin.SaveAsync(root.GetProperty("text").GetString() ?? "", cts.Token);
        return JsonSerializer.Serialize(new
        {
            isSuccess = result.IsSuccess,
            errorMessage = result.ErrorMessage,
            duration = result.Duration
        });
    }

    private static CancellationTokenSource Track(long rid)
    {
        var cts = new CancellationTokenSource();
        lock (Inflight)
        {
            Inflight[rid] = cts;
        }
        return cts;
    }
}

/// <summary>单个已加载插件：实例 + 注入的设置状态。</summary>
internal sealed class PluginEntry
{
    private WorkerPluginContext? _context;

    public required string Id { get; init; }
    public required string Name { get; init; }
    public required IPlugin Plugin { get; init; }
    public string? Kind { get; init; }

    /// <summary>settings.update 时构建/刷新 context 并重注入（契约保证 Init 后 Settings 反映最新值）。</summary>
    public void ApplySettings(JsonElement settings)
    {
        if (_context is null)
        {
            _context = new WorkerPluginContext(settings.Clone());
            Plugin.Init(_context);
        }
        else
        {
            _context.ApplySettingsJson(settings.Clone());
        }
    }
}

/// <summary>worker 侧 IPluginContext：HTTP 直连（本进程 HttpService），设置来自宿主注入。</summary>
internal sealed class WorkerPluginContext : IPluginContext
{
    public WorkerPluginContext(JsonElement settings)
    {
        Store = new WorkerSettings(settings);
        Settings = Store;
    }

    internal WorkerSettings Store { get; }

    public ILogger Logger { get; } = new ConsoleLogger("PluginHost");
    public IHttpService Http { get; } = new HttpService();
    public IPluginSettings Settings { get; }

    public II18n I18n { get; } = new WorkerI18n();
    public INotification Notification { get; } = new WorkerNotification();

    internal void ApplySettingsJson(JsonElement settings) => Store.ReplaceJson(settings);
}

/// <summary>最小控制台日志（避免引入 logging 包）。</summary>
internal sealed class ConsoleLogger(string category) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        try
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {logLevel} {category}: {formatter(state, exception)}");
            if (exception is not null)
                Console.WriteLine(exception);
        }
        catch
        {
            // stdout 可能被关闭，日志失败不致命
        }
    }
}

/// <summary>内存设置容器：宿主 settings.update 全量替换。</summary>
internal sealed class WorkerSettings : IPluginSettings
{
    private Dictionary<string, JsonElement> _raw;

    public WorkerSettings(JsonElement settings)
    {
        _raw = Parse(settings);
    }

    public void Replace(Dictionary<string, JsonElement> raw) => _raw = raw;

    internal void ReplaceJson(JsonElement settings) => _raw = Parse(settings);

    internal Dictionary<string, JsonElement> Parse(JsonElement settings)
    {
        var dict = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        if (settings.ValueKind == JsonValueKind.Object)
        {
            foreach (var p in settings.EnumerateObject())
                dict[p.Name] = p.Value.Clone();
        }
        return dict;
    }

    public T Get<T>(string id, T fallback)
    {
        if (!_raw.TryGetValue(id, out var element))
            return fallback;
        try
        {
            var value = JsonSerializer.Deserialize<T>(element.GetRawText());
            return value is null ? fallback : value;
        }
        catch
        {
            return fallback;
        }
    }

    public IReadOnlyDictionary<string, JsonElement> Raw => _raw;
}

internal sealed class WorkerI18n : II18n
{
    public string GetTranslation(string key) => key;
}

internal sealed class WorkerNotification : INotification
{
    public void Show(string title, string message) { /* worker 无 UI；宿主通知 IPC 面未含此项 */ }
}
