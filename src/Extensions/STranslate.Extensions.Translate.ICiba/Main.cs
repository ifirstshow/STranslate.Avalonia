using System.Text.Json;
using STranslate.Sdk;

namespace STranslate.Extensions.Translate.ICiba;

/// <summary>爱词吧词典（IDictionaryPlugin）：词页结构化解析（音标/发音/释义/变位），对齐 WPF 原版。</summary>
[Extension(
    "c2f14ac80a1e4b8f9d3a77e6c5b04f21",
    "ICiba",
    Description = "爱词吧词典（单词查询）",
    Author = "zggsong")]
public sealed class Main : IDictionaryPlugin
{
    private const string WordPageUrl = "https://www.iciba.com/word";
    private const string NextDataStartTag = "<script id=\"__NEXT_DATA__\" type=\"application/json\">";
    private const string ScriptEndTag = "</script>";

    private IPluginContext Context { get; set; } = null!;

    public IReadOnlyList<SettingItem> SettingItems { get; } = [];

    public void Init(IPluginContext context) => Context = context;

    public void Dispose() { }

    public async Task<DictionaryResult> TranslateAsync(string content, CancellationToken cancellationToken = default)
    {
        var result = new DictionaryResult();
        content = content.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            result.ResultType = DictionaryResultType.NoResult;
            return result;
        }

        var option = new Options
        {
            QueryParams = new Dictionary<string, string>
            {
                { "w", content.ToLowerInvariant() }
            }
        };

        var response = await Context.Http.GetAsync(WordPageUrl, option, cancellationToken);
        var nextDataJson = ExtractNextDataJson(response);
        if (string.IsNullOrWhiteSpace(nextDataJson))
        {
            result.ResultType = DictionaryResultType.NoResult;
            return result;
        }

        using var jsonDoc = JsonDocument.Parse(nextDataJson);
        if (!TryGetWordBaseInfo(jsonDoc.RootElement, out var root) || !IsExpectedWord(root, content))
        {
            result.ResultType = DictionaryResultType.NoResult;
            return result;
        }

        result.Text = root.TryGetProperty("word_name", out var wn) ? wn.GetString() ?? content : content;
        result.ResultType = DictionaryResultType.Success;

        if (root.TryGetProperty("symbols", out var symbols) &&
            symbols.ValueKind == JsonValueKind.Array &&
            symbols.GetArrayLength() > 0)
        {
            ProcessSymbols(symbols[0], result);
        }

        ProcessWordExchange(root, result);
        return result;
    }

    // ── 音标与释义 ──

    private static void ProcessSymbols(JsonElement firstSymbol, DictionaryResult result)
    {
        var isChinese = firstSymbol.TryGetProperty("word_symbol", out _);

        if (isChinese)
        {
            // 中文：拼音 + symbol_mp3
            if (firstSymbol.TryGetProperty("word_symbol", out var phZh))
            {
                result.Symbols.Add(new Symbol
                {
                    Label = "zh",
                    Phonetic = phZh.GetString() ?? string.Empty,
                    AudioUrl = firstSymbol.TryGetProperty("symbol_mp3", out var mp3)
                        ? mp3.GetString() ?? string.Empty
                        : string.Empty
                });
            }
        }
        else
        {
            // 英式/美式音标 + 发音
            if (firstSymbol.TryGetProperty("ph_en", out var phEn) && !string.IsNullOrEmpty(phEn.GetString()))
            {
                result.Symbols.Add(new Symbol
                {
                    Label = "uk",
                    Phonetic = phEn.GetString() ?? string.Empty,
                    AudioUrl = GetFirstString(firstSymbol, "ph_en_mp3", "ph_tts_mp3")
                });
            }
            if (firstSymbol.TryGetProperty("ph_am", out var phAm) && !string.IsNullOrEmpty(phAm.GetString()))
            {
                result.Symbols.Add(new Symbol
                {
                    Label = "us",
                    Phonetic = phAm.GetString() ?? string.Empty,
                    AudioUrl = GetFirstString(firstSymbol, "ph_am_mp3", "ph_tts_mp3")
                });
            }
        }

        // 词性 + 释义
        if (firstSymbol.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in parts.EnumerateArray())
            {
                var partOfSpeech =
                    part.TryGetProperty("part", out var p) ? p.GetString() :
                    part.TryGetProperty("part_name", out var pn) ? pn.GetString() : null;

                if (!part.TryGetProperty("means", out var means))
                    continue;

                var meanValues = ExtractMeanStrings(means);
                if (meanValues.Count == 0)
                    continue;

                result.DictMeans.Add(new DictMean
                {
                    PartOfSpeech = partOfSpeech ?? "释义",
                    Means = meanValues
                });
            }
        }
    }

    private static List<string> ExtractMeanStrings(JsonElement element)
    {
        var values = new List<string>();
        AddMeanStrings(element, values);
        return values;
    }

    private static void AddMeanStrings(JsonElement element, ICollection<string> values)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                AddIfNotEmpty(element.GetString(), values);
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    AddMeanStrings(item, values);
                break;
            case JsonValueKind.Object:
                if (element.TryGetProperty("word_mean", out var wordMean))
                    AddMeanStrings(wordMean, values);
                else if (element.TryGetProperty("means", out var means))
                    AddMeanStrings(means, values);
                else if (element.TryGetProperty("mean", out var mean))
                    AddMeanStrings(mean, values);
                break;
        }
    }

    // ── 词形变换 ──

    private static void ProcessWordExchange(JsonElement root, DictionaryResult result)
    {
        if (!root.TryGetProperty("exchange", out var exchange))
            return;

        if (exchange.TryGetProperty("word_pl", out var pl)) AddWordForms(pl, result.Plurals);
        if (exchange.TryGetProperty("word_past", out var past)) AddWordForms(past, result.PastTense);
        if (exchange.TryGetProperty("word_done", out var done)) AddWordForms(done, result.PastParticiple);
        if (exchange.TryGetProperty("word_ing", out var ing)) AddWordForms(ing, result.PresentParticiple);
        if (exchange.TryGetProperty("word_third", out var third)) AddWordForms(third, result.ThirdPersonSingular);
        if (exchange.TryGetProperty("word_er", out var er)) AddWordForms(er, result.Comparative);
        if (exchange.TryGetProperty("word_est", out var est)) AddWordForms(est, result.Superlative);
    }

    private static void AddWordForms(JsonElement element, ICollection<string> collection)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                AddIfNotEmpty(element.GetString(), collection);
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    AddWordForms(item, collection);
                break;
        }
    }

    // ── 词页定位（__NEXT_DATA__ 深度查找，移植自 WPF 原版）──

    private static string? ExtractNextDataJson(string response)
    {
        var trimmed = response.TrimStart();
        if (trimmed.StartsWith('{'))
            return trimmed;

        var startIndex = response.IndexOf(NextDataStartTag, StringComparison.OrdinalIgnoreCase);
        if (startIndex < 0)
            return null;
        startIndex += NextDataStartTag.Length;
        var endIndex = response.IndexOf(ScriptEndTag, startIndex, StringComparison.OrdinalIgnoreCase);
        return endIndex <= startIndex ? null : response[startIndex..endIndex];
    }

    private static bool TryGetWordBaseInfo(JsonElement root, out JsonElement baseInfo)
    {
        baseInfo = default;
        if (TryGetWordInfo(root, out var wordInfo))
        {
            if (wordInfo.TryGetProperty("baesInfo", out baseInfo))
                return true;
            if (LooksLikeBaseInfo(wordInfo))
            {
                baseInfo = wordInfo;
                return true;
            }
        }
        if (TryFindWordInfo(root, out wordInfo))
        {
            if (wordInfo.TryGetProperty("baesInfo", out baseInfo))
                return true;
            if (LooksLikeBaseInfo(wordInfo))
            {
                baseInfo = wordInfo;
                return true;
            }
        }
        return false;
    }

    private static bool TryGetWordInfo(JsonElement root, out JsonElement wordInfo)
    {
        wordInfo = default;
        if (root.TryGetProperty("props", out var props) &&
            props.TryGetProperty("pageProps", out var pagePropsFromProps))
            return TryGetWordInfoFromPageProps(pagePropsFromProps, out wordInfo);
        if (root.TryGetProperty("pageProps", out var pageProps))
            return TryGetWordInfoFromPageProps(pageProps, out wordInfo);
        return false;
    }

    private static bool TryGetWordInfoFromPageProps(JsonElement pageProps, out JsonElement wordInfo)
    {
        wordInfo = default;
        return pageProps.TryGetProperty("initialReduxState", out var redux) &&
               redux.TryGetProperty("word", out var wordState) &&
               wordState.TryGetProperty("wordInfo", out wordInfo);
    }

    private static bool TryFindWordInfo(JsonElement element, out JsonElement wordInfo)
    {
        wordInfo = default;
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                if (element.TryGetProperty("wordInfo", out var direct) && LooksLikeWordInfo(direct))
                {
                    wordInfo = direct;
                    return true;
                }
                if (LooksLikeWordInfo(element))
                {
                    wordInfo = element;
                    return true;
                }
                foreach (var property in element.EnumerateObject())
                    if (TryFindWordInfo(property.Value, out wordInfo))
                        return true;
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    if (TryFindWordInfo(item, out wordInfo))
                        return true;
                break;
        }
        return false;
    }

    private static bool LooksLikeWordInfo(JsonElement element) =>
        element.ValueKind == JsonValueKind.Object &&
        (element.TryGetProperty("baesInfo", out _) || LooksLikeBaseInfo(element));

    private static bool LooksLikeBaseInfo(JsonElement element) =>
        element.ValueKind == JsonValueKind.Object &&
        (element.TryGetProperty("word_name", out _) || element.TryGetProperty("symbols", out _));

    /// <summary>校验返回的单词与输入一致（iciba 识别失败会重定向到默认页）。</summary>
    private static bool IsExpectedWord(JsonElement root, string lookupContent)
    {
        if (!root.TryGetProperty("word_name", out var wordName) ||
            wordName.GetString() is not string word ||
            string.IsNullOrWhiteSpace(word))
            return false;

        if (NormalizeForCompare(word) == NormalizeForCompare(lookupContent))
            return true;

        if (!root.TryGetProperty("exchange", out var exchange))
            return false;

        var forms = new List<string>();
        AddExchangeForms(exchange, forms);
        return forms.Select(NormalizeForCompare).Any(item => item == NormalizeForCompare(lookupContent));
    }

    private static void AddExchangeForms(JsonElement element, ICollection<string> values)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                AddIfNotEmpty(element.GetString(), values);
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    AddExchangeForms(item, values);
                break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                    AddExchangeForms(property.Value, values);
                break;
        }
    }

    private static string NormalizeLookupContent(string content)
    {
        var value = content.Trim().Replace('’', '\'');
        while (value.Length > 0 && IsLookupTrimChar(value[0]))
            value = value[1..].TrimStart();
        while (value.Length > 0 && IsLookupTrimChar(value[^1]))
            value = value[..^1].TrimEnd();
        return value;
    }

    private static bool IsLookupTrimChar(char ch) =>
        char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch);

    private static string NormalizeForCompare(string value) =>
        new string(NormalizeLookupContent(value)
            .ToLowerInvariant()
            .Where(ch => char.IsLetterOrDigit(ch) || ch is '\'' or '-' || (ch >= '一' && ch <= '鿿'))
            .ToArray());

    private static void AddIfNotEmpty(string? value, ICollection<string> values)
    {
        if (!string.IsNullOrWhiteSpace(value))
            values.Add(value.Trim());
    }

    private static string? GetFirstString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var value) &&
                value.GetString() is { } s && !string.IsNullOrWhiteSpace(s))
                return s;
        }
        return null;
    }
}
