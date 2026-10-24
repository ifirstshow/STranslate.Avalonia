using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using STranslate.Worker.Ipc;

namespace STranslate.Avalonia.Services;

/// <summary>worker describe 上报的一个社区插件描述。</summary>
public sealed class WorkerPluginDescription
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Kind { get; init; }
    public required JsonElement SettingItems { get; init; }
}

/// <summary>
/// 社区插件 worker 进程管理（ADR-0004）：按需启动 JIT 子进程，命名管道 ndjson 通信。
/// worker 崩溃即标记不可用（下次启动重建），不拖垮主程序。
/// </summary>
public sealed class WorkerProcessManager : IDisposable
{
    private Process? _process;
    private IpcConnection? _connection;
    private long _nextId = 1;
    private string _pipeName = "";
    private string _pluginsRoot = "";
    private readonly SemaphoreSlim _callLock = new(1, 1);

    public IReadOnlyList<WorkerPluginDescription> Plugins { get; private set; } = [];

    public bool IsRunning => _connection is not null && _process is { HasExited: false };

    /// <summary>启动 worker 并等待 describe。无已装插件返回空列表。</summary>
    public async Task<IReadOnlyList<WorkerPluginDescription>> StartAsync(string pluginsRoot)
    {
        if (IsRunning)
            return Plugins;
        if (!Directory.Exists(pluginsRoot) || Directory.EnumerateDirectories(pluginsRoot).Any() is false)
            return Plugins;

        var exe = ProbeWorkerExe();
        if (exe is null)
        {
            DebugTrace.Log("worker: exe not found, community plugins disabled");
            return Plugins;
        }

        _pluginsRoot = pluginsRoot;
        _pipeName = $"stranslate-worker-{Guid.NewGuid():N}";
        _process = Process.Start(new ProcessStartInfo
        {
            FileName = exe,
            Arguments = $"{_pipeName} \"{pluginsRoot}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true
        });
        _process!.ErrorDataReceived += (_, e) => { if (e.Data is not null) DebugTrace.Log($"worker: {e.Data}"); };
        _process.BeginErrorReadLine();

        // 管道流由 IpcConnection 持有并在其 Dispose 时关闭；不能 using（方法返回即断管）
        var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        var connectCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await pipe.ConnectAsync(connectCts.Token);
        DebugTrace.Log("worker: host connected");
        _connection = new IpcConnection(pipe);

        // 第一帧必须是 describe
        var frame = await _connection.ReadAsync(connectCts.Token)
                    ?? throw new IOException("worker closed before describe");
        DebugTrace.Log($"worker: describe received ({frame.Method})");
        if (frame.Method != "describe")
            throw new IOException($"expected describe, got {frame.Method}");

        using var doc = JsonDocument.Parse(frame.Params ?? "{}");
        Plugins = doc.RootElement.GetProperty("plugins").EnumerateArray()
            .Select(p => new WorkerPluginDescription
            {
                Id = p.GetProperty("id").GetString() ?? "",
                Name = p.GetProperty("name").GetString() ?? "",
                Kind = p.GetProperty("kind").GetString() ?? "",
                SettingItems = p.GetProperty("settingItems").Clone()
            })
            .ToList();
        DebugTrace.Log($"worker: started pid={_process.Id} plugins={Plugins.Count}");

        // 推送全部已保存配置
        await PushAllSettingsAsync();
        _ = Task.Run(WatchExit);
        return Plugins;
    }

    /// <summary>发起一次 RPC 调用，返回 result JSON；error 抛异常。</summary>
    public async Task<string> CallAsync(string method, string paramsJson, CancellationToken cancellationToken)
    {
        if (_connection is null)
            throw new InvalidOperationException("worker not running");
        await _callLock.WaitAsync(cancellationToken);
        try
        {
            var id = _nextId++;
            _inflightId = id;
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var responseTask = WaitForResponseAsync(id, cts.Token);
            await _connection.WriteAsync(new IpcFrame { Id = id, Method = method, Params = paramsJson }, cts.Token);

            var response = await responseTask;
            if (response.Error is not null)
                throw new InvalidOperationException(response.Error);
            return response.Result ?? "null";
        }
        finally
        {
            _inflightId = null;
            _callLock.Release();
        }
    }

    private long? _inflightId;

    /// <summary>取消进行中的调用（调用锁保证同时最多一个 inflight）。</summary>
    public async Task CancelAsync()
    {
        if (_connection is null || _inflightId is not { } rid)
            return;
        await _connection.WriteAsync(new IpcFrame { Method = "cancel", Params = $$"""{"rid":{{rid}}}""" });
    }

    /// <summary>注册等待响应（CallAsync 内部用）。简化实现：轮询读连接。</summary>
    private async Task<IpcFrame> WaitForResponseAsync(long id, CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var frame = await _connection!.ReadAsync(ct);
            if (frame is null)
                throw new IOException("worker connection closed");
            if (frame.Id == id)
                return frame;
            // 非期待帧（暂无 worker 主动推送的运行时消息），丢弃
        }
    }

    /// <summary>把所有社区插件的已保存配置推给 worker（按 pluginId 分组）。</summary>
    private async Task PushAllSettingsAsync()
    {
        var grouped = new Dictionary<string, Dictionary<string, JsonElement>>();
        var store = App.Services.GetRequiredService<ExtensionSettingsStore>();
        foreach (var plugin in Plugins)
            grouped[plugin.Id] = new Dictionary<string, JsonElement>(store.GetRaw(plugin.Id));
        await _connection!.WriteAsync(new IpcFrame
        {
            Method = "settings.update",
            Params = JsonSerializer.Serialize(grouped)
        });
    }

    /// <summary>worker 进程退出监控：记录日志（崩溃隔离——下次需要时宿主重启）。</summary>
    private void WatchExit()
    {
        var process = _process;
        if (process is null)
            return;
        process.WaitForExit();
        DebugTrace.Log($"worker: exited code={process.ExitCode}");
        _connection?.Dispose();
        _connection = null;
    }

    private static string? ProbeWorkerExe()
    {
        var name = "STranslate.PluginHost.exe";
        // 安装包：同目录；开发：.artifacts 布局（BaseDirectory=src/.artifacts/Avalonia/Debug）
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, name),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..",
                "bin", "STranslate.PluginHost", "Debug", name))
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    public void Dispose()
    {
        try
        {
            if (_process is { HasExited: false } p)
            {
                p.Kill(entireProcessTree: true);
                // 等待进程真正退出，否则插件 dll 句柄未释放，删除目录会撞锁
                p.WaitForExit(3000);
            }
        }
        catch
        {
            // 进程已退出
        }
        _connection?.Dispose();
        _process?.Dispose();
    }
}
