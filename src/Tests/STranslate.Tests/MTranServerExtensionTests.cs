using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using STranslate.Avalonia.Services;
using STranslate.Extensions.Translate.MTranServer;
using STranslate.Sdk;
using Xunit;

namespace STranslate.Tests;

/// <summary>
/// v2 扩展契约端到端检查：真实 HttpService + 进程内 mock MTranServer 服务器。
/// </summary>
public class MTranServerExtensionTests
{
    private sealed class StubSettings(string url) : IPluginSettings
    {
        public IReadOnlyDictionary<string, JsonElement> Raw { get; } =
            new Dictionary<string, JsonElement> { ["url"] = JsonSerializer.SerializeToElement(url) };

        public T Get<T>(string id, T fallback) =>
            id == "url" && fallback is string ? (T)(object)url : fallback;
    }

    private sealed class StubContext(IHttpService http, string url) : IPluginContext
    {
        public ILogger Logger => NullLogger.Instance;
        public IHttpService Http { get; } = http;
        public IPluginSettings Settings { get; } = new StubSettings(url);
        public II18n I18n { get; } = new StubI18n();
        public INotification Notification { get; } = new StubNotification();

        private sealed class StubI18n : II18n
        {
            public string GetTranslation(string key) => key;
        }

        private sealed class StubNotification : INotification
        {
            public void Show(string title, string message) { }
        }

        public void Dispose() { }
    }

    [Fact]
    public async Task TranslateAsync_RequestsMockServer_AndReturnsResult()
    {
        using var listener = new HttpListener();
        var port = TcpPortFinder.FindFree();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/translate/");
        listener.Start();

        var serverTask = Task.Run(async () =>
        {
            try
            {
                while (listener.IsListening)
                {
                    var context = await listener.GetContextAsync();
                    var body = await new StreamReader(context.Request.InputStream, Encoding.UTF8).ReadToEndAsync();
                    var request = JsonSerializer.Deserialize<JsonElement>(body);
                    var echo = $"echo:{request.GetProperty("from").GetString()}->{request.GetProperty("to").GetString()}:{request.GetProperty("text").GetString()}";
                    var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { result = echo }));
                    context.Response.ContentLength64 = buffer.Length;
                    await context.Response.OutputStream.WriteAsync(buffer);
                    context.Response.Close();
                }
            }
            catch (HttpListenerException) when (!listener.IsListening)
            {
                // Stop() 会中止挂起的 GetContextAsync
            }
        });

        try
        {
            var extension = new Main();
            extension.Init(new StubContext(new HttpService(), $"http://127.0.0.1:{port}/translate"));

            var result = await extension.TranslateAsync(
                new TranslateRequest("hello", LangEnum.English, LangEnum.ChineseSimplified));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal("echo:en->zh:hello", result.Text);

            // 不支持的语言走失败路径
            var fail = await extension.TranslateAsync(
                new TranslateRequest("你好", LangEnum.Auto, LangEnum.ChineseSimplified));
            Assert.False(fail.IsSuccess);
        }
        finally
        {
            listener.Stop();
            await serverTask;
        }
    }
}

internal static class TcpPortFinder
{
    public static int FindFree()
    {
        var l = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }
}
