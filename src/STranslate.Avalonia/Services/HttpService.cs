using System.Collections.Concurrent;
using System.Net;
using STranslate.Core;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using STranslate.Sdk;

namespace STranslate.Avalonia.Services;

/// <summary>
/// STranslate.Sdk.IHttpService 的宿主实现：HttpClient 直映射。
/// 命名客户端按需创建，隔离超时配置。
/// </summary>
public sealed class HttpService : IHttpService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _defaultClient;
    private readonly ConcurrentDictionary<string, HttpClient> _namedClients = new();

    public HttpService()
    {
        _defaultClient = CreateClient();
    }

    /// <summary>按当前代理设置构建 handler；系统代理交由 WinHTTP/环境默认，自定义则显式注入。</summary>
    private static SocketsHttpHandler CreateHandler()
    {
        var proxy = AppSettings.Current.Proxy;
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            ConnectTimeout = TimeSpan.FromSeconds(15)
        };
        if (proxy.IsEnabled && proxy.ProxyType == ProxyType.Http)
        {
            var address = $"http://{proxy.ProxyAddress}:{proxy.ProxyPort}";
            handler.Proxy = new WebProxy(address);
            if (!string.IsNullOrEmpty(proxy.ProxyUsername))
                handler.Credentials = new NetworkCredential(proxy.ProxyUsername, proxy.ProxyPassword);
            if (!proxy.UseProxyForLocalAddresses)
                handler.UseProxy = true; // BypassList 交给 SocketsHttpHandler 的 Proxy Bypass 需自实现，v1 仅全局开关
        }
        else if (proxy.IsEnabled && proxy.ProxyType == ProxyType.System)
        {
            handler.Proxy = HttpClient.DefaultProxy; // 系统（WinHTTP）代理
        }
        else
        {
            handler.UseProxy = false; // 直连
        }
        return handler;
    }

    private static HttpClient CreateClient() => new(CreateHandler()) { Timeout = Timeout.InfiniteTimeSpan };

    /// <summary>代理设置变更后调用：清空命名客户端缓存重建（default 为只读字段，保留旧实例直到重启）。</summary>
    public void ReloadProxy()
    {
        foreach (var kv in _namedClients)
            kv.Value.Dispose();
        _namedClients.Clear();
    }

    private HttpClient GetClient(Options? options) => _defaultClient;

    private HttpClient GetClient(string serviceName, Options? options) =>
        _namedClients.GetOrAdd(serviceName, _ => CreateClient());

    // ── 核心：构建并发送请求 ──

    private static (HttpRequestMessage message, CancellationTokenSource cts) Build(
        HttpClient client, HttpMethod method, string url, Options? options, CancellationToken ct)
    {
        if (options?.QueryParams is { Count: > 0 } query)
        {
            var builder = new UriBuilder(url);
            var existing = builder.Query;
            var appended = string.Join("&", query.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            builder.Query = string.IsNullOrEmpty(existing) ? appended : $"{existing.TrimStart('?')}&{appended}";
            url = builder.Uri.ToString();
        }

        var message = new HttpRequestMessage(method, url);
        if (options?.Headers is { Count: > 0 } headers)
        {
            foreach (var (key, value) in headers)
            {
                if (key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    continue; // 内容类型随 HttpContent 设置
                message.Headers.TryAddWithoutValidation(key, value);
            }
        }

        var timeout = options?.Timeout ?? TimeSpan.FromSeconds(30);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);
        return (message, cts);
    }

    private static async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage message, CancellationToken ct)
    {
        var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        return response;
    }

    private static StringContent JsonContent(object content, Options? options) =>
        new(JsonSerializer.Serialize(content), Encoding.UTF8, options?.ContentType ?? "application/json");

    private static async Task<string> ReadStringAsync(HttpResponseMessage response, CancellationToken ct)
    {
        // 服务端可能声明 charset 失真，按 UTF-8 强解
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        return Encoding.UTF8.GetString(bytes);
    }

    // ── GET ──

    public Task<string> GetAsync(string url, CancellationToken cancellationToken = default) =>
        GetAsync(url, null, cancellationToken);

    public async Task<string> GetAsync(string url, Options? options = null, CancellationToken cancellationToken = default)
    {
        var client = GetClient(options);
        var (message, cts) = Build(client, HttpMethod.Get, url, options, cancellationToken);
        using (cts)
        {
            using var response = await SendAsync(client, message, cts.Token);
            return await ReadStringAsync(response, cts.Token);
        }
    }

    public async Task<T?> GetAsync<T>(string url, Options? options = null, CancellationToken cancellationToken = default)
    {
        var text = await GetAsync(url, options, cancellationToken);
        return JsonSerializer.Deserialize<T>(text, JsonOpts);
    }

    public Task<byte[]> GetAsBytesAsync(string url, CancellationToken cancellationToken = default) =>
        GetAsBytesAsync(url, null, cancellationToken);

    public async Task<byte[]> GetAsBytesAsync(string url, Options? options = null, CancellationToken cancellationToken = default)
    {
        var client = GetClient(options);
        var (message, cts) = Build(client, HttpMethod.Get, url, options, cancellationToken);
        using (cts)
        {
            using var response = await SendAsync(client, message, cts.Token);
            return await response.Content.ReadAsByteArrayAsync(cts.Token);
        }
    }

    public Task<Stream> GetAsStreamAsync(string url, CancellationToken cancellationToken = default) =>
        GetAsStreamAsync(url, null, cancellationToken);

    public async Task<Stream> GetAsStreamAsync(string url, Options? options = null, CancellationToken cancellationToken = default)
    {
        var client = GetClient(options);
        var (message, cts) = Build(client, HttpMethod.Get, url, options, cancellationToken);
        // 流的生命周期长于 cts；调用方持有流时由 response 自行处置
        var response = await SendAsync(client, message, cts.Token);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    // ── POST ──

    public Task<string> PostAsync(string url, object content, Options? options = null, CancellationToken cancellationToken = default) =>
        PostAsyncCore(url, content, options, cancellationToken);

    public async Task<T?> PostAsync<T>(string url, object content, Options? options = null, CancellationToken cancellationToken = default)
    {
        var text = await PostAsyncCore(url, content, options, cancellationToken);
        return JsonSerializer.Deserialize<T>(text, JsonOpts);
    }

    public async Task<byte[]> PostAsBytesAsync(string url, object content, Options? options = null, CancellationToken cancellationToken = default)
    {
        var client = GetClient(options);
        var (message, cts) = Build(client, HttpMethod.Post, url, options, cancellationToken);
        using (cts)
        {
            message.Content = JsonContent(content, options);
            using var response = await SendAsync(client, message, cts.Token);
            return await response.Content.ReadAsByteArrayAsync(cts.Token);
        }
    }

    private async Task<string> PostAsyncCore(string url, object content, Options? options, CancellationToken cancellationToken)
    {
        var client = GetClient(options);
        var (message, cts) = Build(client, HttpMethod.Post, url, options, cancellationToken);
        using (cts)
        {
            message.Content = JsonContent(content, options);
            using var response = await SendAsync(client, message, cts.Token);
            return await ReadStringAsync(response, cts.Token);
        }
    }

    // ── Form ──

    public Task<string> PostFormAsync(string url, Dictionary<string, string> formData, Options? options = null, CancellationToken cancellationToken = default) =>
        PostFormAsyncCore(url, formData, options, cancellationToken);

    private async Task<string> PostFormAsyncCore(string url, Dictionary<string, string> formData, Options? options, CancellationToken cancellationToken)
    {
        var client = GetClient(options);
        var (message, cts) = Build(client, HttpMethod.Post, url, options, cancellationToken);
        using (cts)
        {
            message.Content = new FormUrlEncodedContent(formData);
            using var response = await SendAsync(client, message, cts.Token);
            return await ReadStringAsync(response, cts.Token);
        }
    }

    // ── 流式（SSE） ──

    public async Task StreamPostAsync(string url, object content, Action<string> onDataReceived, Options? options = null, CancellationToken cancellationToken = default)
    {
        await foreach (var line in StreamPostAsyncEnumerable(url, content, options, cancellationToken))
            onDataReceived(line);
    }

    public async IAsyncEnumerable<string> StreamPostAsyncEnumerable(
        string url, object content, Options? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = GetClient(options);
        var (message, cts) = Build(client, HttpMethod.Post, url, options, cancellationToken);
        using (cts)
        {
            message.Content = JsonContent(content, options);
            using var response = await SendAsync(client, message, cts.Token);
            var stream = await response.Content.ReadAsStreamAsync(cts.Token);
            using var reader = new StreamReader(stream);
            while (await reader.ReadLineAsync(cts.Token) is { } line)
                yield return line;
        }
    }

    // ── 下载 ──

    public Task<string> DownloadFileAsync(string url, string savePath, string fileName, Options? options = null, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default) =>
        DownloadFileAsync(url, (object?)null, savePath, fileName, options, progress, cancellationToken);

    public async Task<string> DownloadFileAsync(string url, object? content, string savePath, string fileName, Options? options = null, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        var client = GetClient(options);
        var (message, cts) = Build(client, content is null ? HttpMethod.Get : HttpMethod.Post, url, options, cancellationToken);
        using (cts)
        {
            if (content is not null)
                message.Content = JsonContent(content, options);

            using var response = await SendAsync(client, message, cts.Token);
            var total = response.Content.Headers.ContentLength ?? -1;
            var startedAt = DateTimeOffset.UtcNow;

            Directory.CreateDirectory(savePath);
            var fullPath = Path.Combine(savePath, fileName);
            await using var source = await response.Content.ReadAsStreamAsync(cts.Token);
            await using var target = File.Create(fullPath);

            var buffer = new byte[81920];
            long downloaded = 0;
            int read;
            while ((read = await source.ReadAsync(buffer, cts.Token)) > 0)
            {
                await target.WriteAsync(buffer.AsMemory(0, read), cts.Token);
                downloaded += read;
                if (progress is not null)
                {
                    var elapsed = DateTimeOffset.UtcNow - startedAt;
                    progress.Report(new DownloadProgress
                    {
                        TotalBytes = total,
                        DownloadedBytes = downloaded,
                        ElapsedTime = elapsed,
                        Speed = elapsed.TotalSeconds > 0 ? downloaded / elapsed.TotalSeconds : 0
                    });
                }
            }
            return fullPath;
        }
    }

    // ── 命名客户端重载：M1 直连默认客户端，命名仅作超时/连接池隔离占位 ──

    public Task<string> GetAsync(string serviceName, string url, Options? options = null, CancellationToken cancellationToken = default) =>
        GetAsyncNamed(serviceName, url, options, cancellationToken);

    private async Task<string> GetAsyncNamed(string serviceName, string url, Options? options, CancellationToken cancellationToken)
    {
        var client = GetClient(serviceName, options);
        var (message, cts) = Build(client, HttpMethod.Get, url, options, cancellationToken);
        using (cts)
        {
            using var response = await SendAsync(client, message, cts.Token);
            return await ReadStringAsync(response, cts.Token);
        }
    }

    public async Task<byte[]> GetAsBytesAsync(string serviceName, string url, Options? options = null, CancellationToken cancellationToken = default)
    {
        var client = GetClient(serviceName, options);
        var (message, cts) = Build(client, HttpMethod.Get, url, options, cancellationToken);
        using (cts)
        {
            using var response = await SendAsync(client, message, cts.Token);
            return await response.Content.ReadAsByteArrayAsync(cts.Token);
        }
    }

    public async Task<Stream> GetAsStreamAsync(string serviceName, string url, Options? options = null, CancellationToken cancellationToken = default)
    {
        var client = GetClient(serviceName, options);
        var (message, cts) = Build(client, HttpMethod.Get, url, options, cancellationToken);
        var response = await SendAsync(client, message, cts.Token);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public Task<string> PostAsync(string serviceName, string url, object content, Options? options = null, CancellationToken cancellationToken = default)
    {
        var client = GetClient(serviceName, options);
        return PostOnClient(client, url, content, options, cancellationToken);
    }

    private async Task<string> PostOnClient(HttpClient client, string url, object content, Options? options, CancellationToken cancellationToken)
    {
        var (message, cts) = Build(client, HttpMethod.Post, url, options, cancellationToken);
        using (cts)
        {
            message.Content = JsonContent(content, options);
            using var response = await SendAsync(client, message, cts.Token);
            return await ReadStringAsync(response, cts.Token);
        }
    }

    public async Task<byte[]> PostAsBytesAsync(string serviceName, string url, object content, Options? options = null, CancellationToken cancellationToken = default)
    {
        var client = GetClient(serviceName, options);
        var (message, cts) = Build(client, HttpMethod.Post, url, options, cancellationToken);
        using (cts)
        {
            message.Content = JsonContent(content, options);
            using var response = await SendAsync(client, message, cts.Token);
            return await response.Content.ReadAsByteArrayAsync(cts.Token);
        }
    }

    public async Task<string> PostFormAsync(string serviceName, string url, Dictionary<string, string> formData, Options? options = null, CancellationToken cancellationToken = default)
    {
        var client = GetClient(serviceName, options);
        var (message, cts) = Build(client, HttpMethod.Post, url, options, cancellationToken);
        using (cts)
        {
            message.Content = new FormUrlEncodedContent(formData);
            using var response = await SendAsync(client, message, cts.Token);
            return await ReadStringAsync(response, cts.Token);
        }
    }

    public async Task StreamPostAsync(string serviceName, string url, object content, Action<string> onDataReceived, Options? options = null, CancellationToken cancellationToken = default)
    {
        await foreach (var line in StreamPostAsyncEnumerable(serviceName, url, content, options, cancellationToken))
            onDataReceived(line);
    }

    public async IAsyncEnumerable<string> StreamPostAsyncEnumerable(
        string serviceName, string url, object content, Options? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = GetClient(serviceName, options);
        var (message, cts) = Build(client, HttpMethod.Post, url, options, cancellationToken);
        using (cts)
        {
            message.Content = JsonContent(content, options);
            using var response = await SendAsync(client, message, cts.Token);
            var stream = await response.Content.ReadAsStreamAsync(cts.Token);
            using var reader = new StreamReader(stream);
            while (await reader.ReadLineAsync(cts.Token) is { } line)
                yield return line;
        }
    }

    public Task<string> DownloadFileAsync(string serviceName, string url, string savePath, string fileName, Options? options = null, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default) =>
        DownloadFileAsync(url, (object?)null, savePath, fileName, options, progress, cancellationToken);

    public Task<string> DownloadFileAsync(string serviceName, string url, object content, string savePath, string fileName, Options? options = null, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default) =>
        DownloadFileAsync(url, content, savePath, fileName, options, progress, cancellationToken);

    // ── 运维 ──

    /// <summary>最近一次代理测试的出口 IP（失败为 null）。</summary>
    public string? LastTestIp { get; private set; }

    public async Task<bool> TestProxyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            var ip = await GetAsync("https://api.ipify.org", cancellationToken: cts.Token);
            LastTestIp = ip.Trim();
            return true;
        }
        catch
        {
            LastTestIp = null;
            return false;
        }
    }

    public async Task<string> GetCurrentIpAsync(CancellationToken cancellationToken = default)
    {
        var body = await GetAsync("https://api.ipify.org", cancellationToken: cancellationToken);
        return body.Trim();
    }
}
