using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using STranslate.Avalonia.Services;
using STranslate.Extensions.Translate.ICiba;
using STranslate.Sdk;

namespace STranslate.Tests;

public class IcibaDictionaryTests
{
    private class StubContext : IPluginContext
    {
        public ILogger Logger => NullLogger.Instance;
        public IHttpService Http { get; } = new HttpService();
        public IPluginSettings Settings { get; } = new StubSettings();
        public II18n I18n { get; } = new StubI18n();
        public INotification Notification { get; } = new StubNotification();
    }

    private class StubSettings : IPluginSettings
    {
        public T Get<T>(string id, T fallback) => fallback;
        public IReadOnlyDictionary<string, JsonElement> Raw => new Dictionary<string, JsonElement>();
    }

    private class StubI18n : II18n
    {
        public string GetTranslation(string key) => key;
    }

    private class StubNotification : INotification
    {
        public void Show(string title, string message) { }
    }

    [Fact]
    public async Task Storm_ReturnsStructuredDictionary()
    {
        var main = new Main();
        main.Init(new StubContext());

        var result = await main.TranslateAsync("storm");

        Assert.Equal(DictionaryResultType.Success, result.ResultType);
        Assert.True(result.Symbols.Count > 0, "应有音标");
        Assert.True(result.DictMeans.Count > 0, "应有释义");
        Assert.Contains(result.Symbols, s => s.AudioUrl.Length > 0);
        Assert.True(result.Plurals.Count > 0 || result.PastTense.Count > 0 || result.ThirdPersonSingular.Count > 0,
            "storm 应有变位（复数/过去式/三单）");
    }
}
