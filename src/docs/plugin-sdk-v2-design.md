# 插件 SDK v2 契约设计（已评审定稿）

> 状态：已评审（2026-10-13）。依据：ADR-0004（混合插件架构）、M1 AOT 原型结论。

## 1. 背景与目标

现有 `STranslate.Plugin` SDK 的插件 ABI 深度绑定 WPF：

- `IPlugin.GetSettingUI()` 返回 WPF `UserControl`，插件自带 View/ViewModel
- `IPluginContext` 暴露 `Window`/`ObservableCollection`/`ApplyTheme(Window)` 等 WPF 类型
- 结果模型是**共享可变 ObservableObject**（`TranslateResult`/`DictionaryResult` 由插件持有并逐步改写，宿主直接绑定观察），`DictionaryResult.Update` 甚至内部调 `Application.Current.Dispatcher`

这些使插件无法：运行于非 Windows 宿主、编译进 AOT 主程序、跨进程被调用。

v2 目标：

1. 契约零 UI 依赖、零反射依赖（AOT 安全）
2. 同一契约覆盖两种宿主形态：**内置直调**（官方插件编译进主程序）与 **worker 调用**（社区 `.spkg` 经 PluginHost 进程 + IPC）
3. 插件作者迁移成本低：服务核心代码基本不动，删设置 UI、声明设置元数据即可

## 2. 新旧关键对照

| 维度 | v1（现役） | v2（本设计） |
| --- | --- | --- |
| 设置 UI | 插件实现 `GetSettingUI()` 返回 WPF UserControl | 插件声明 `SettingItems`（描述符），宿主生成 UI |
| 设置存储 | `context.LoadSettingStorage<T>()/SaveSettingStorage<T>()`（插件自管 JSON 文件） | 宿主持有持久化，插件经 `context.Settings.Get<T>(id)` 只读 |
| 翻译结果 | 共享可变 `TransResult`/`TransBackResult` 属性，插件原地改写 | `TranslateAsync` **返回**新结果实例（可序列化 POCO） |
| LLM 流式 | 渐进改写 Text，宿主绑定观察 | `IProgress<string>` 增量回调（IPC 映射为通知） |
| 回译 | 宿主写入 `TransBackResult` | 宿主直接二次调用 `TranslateAsync` 取返回值 |
| MVVM 依赖 | SDK 依赖 CommunityToolkit.Mvvm | SDK 无 MVVM；宿主自行包装 ViewModel |
| Prompt 编辑 | `context.GetPromptEditWindow(...)` 弹 WPF 窗 | `PromptListSetting` 描述符，宿主内置编辑器 |
| LLM Prompt 状态 | `LlmTranslatePluginBase.Prompts/SelectedPrompt` | 作为设置项持久化，Init 时注入 |
| 词典结果 | 共享可变 `DictionaryResult`（内部用 WPF Dispatcher） | 返回 POCO |

## 3. 契约草案

```csharp
namespace STranslate.Sdk;

/// <summary>插件入口。不再有 GetSettingUI。</summary>
public interface IPlugin : IDisposable
{
    /// <summary>初始化。context 携带宿主能力与已加载的设置值。</summary>
    void Init(IPluginContext context);

    /// <summary>设置项声明。宿主据此生成设置 UI 并负责持久化。可在 Init 前访问。</summary>
    IReadOnlyList<SettingItem> SettingItems { get; }
}

/// <summary>宿主注入的能力集。全部为跨平台接口。</summary>
public interface IPluginContext
{
    ILogger Logger { get; }
    IHttpService Http { get; }
    IPluginSettings Settings { get; }      // 只读访问已持久化的设置值
    II18n I18n { get; }                    // 取代 GetTranslation
    INotification Notification { get; }    // 系统通知（worker 下经 IPC 回传宿主）
}

public interface IPluginSettings
{
    T Get<T>(string id, T fallback);       // id 对应 SettingItem.Id
    IReadOnlyDictionary<string, JsonElement> Raw { get; }  // 兜底
}

// ── 翻译 ──
public interface ITranslatePlugin : IPlugin
{
    string? GetSourceLanguage(LangEnum lang);
    string? GetTargetLanguage(LangEnum lang);
    Task<TranslateResult> TranslateAsync(
        TranslateRequest request,
        IProgress<string>? deltas,          // LLM 增量输出；null 则只返回终值
        CancellationToken cancellationToken = default);
}

public sealed record TranslateRequest(string Text, LangEnum SourceLang, LangEnum TargetLang);

public sealed class TranslateResult   // 纯 POCO，可 JSON 序列化（worker 需要）
{
    public bool IsSuccess { get; init; }
    public string Text { get; init; } = "";
    public TimeSpan Duration { get; init; }
    public string ErrorMessage { get; init; } = "";
}

// ── OCR / TTS / 词典 / 生词本 ──
// 同原则：请求进、结果出，全部 POCO。
// IOcrPlugin.RecognizeAsync(OcrRequest) → Task<OcrResult>
//   OcrRequest 携带宿主按全局 ImageQuality 编码好的图像（v1 的 Context.ImageQuality 属宿主设置，参数化进请求）
// ITtsPlugin.SpeakAsync(text, ct) → Task<AudioData>
//   语义变化：v1 插件经 Context.AudioPlayer 自播（PlayAudioAsync）；v2 插件只产出音频数据，播放统一归宿主
// IDictionaryPlugin: Task<DictionaryResult> TranslateAsync(string content, CancellationToken)
// IVocabularyPlugin: Task<VocabularyResult> SaveAsync(string word, string? note, CancellationToken)
```

### 3.1 设置项描述符

```csharp
public abstract record SettingItem(string Id, string Label, string? Tooltip = null);

public sealed record TextSetting(string Id, string Label, string? Placeholder = null, string? Default = null, string? Tooltip = null) : SettingItem(Id, Label, Tooltip);
public sealed record PasswordSetting(string Id, string Label, string? Tooltip = null) : SettingItem(Id, Label, Tooltip);
public sealed record NumberSetting(string Id, string Label, double Min, double Max, double Step = 1, double? Default = null, string? Tooltip = null) : SettingItem(Id, Label, Tooltip);
public sealed record ToggleSetting(string Id, string Label, bool Default = false, string? Tooltip = null) : SettingItem(Id, Label, Tooltip);
public sealed record SelectSetting(string Id, string Label, IReadOnlyList<SelectOption> Options, string? Default = null, string? Tooltip = null) : SettingItem(Id, Label, Tooltip);
public sealed record SelectOption(string Value, string Label);
public sealed record PromptListSetting(string Id, string Label, IReadOnlyList<Prompt> Default) : SettingItem(Id, Label);  // 宿主内置 Prompt 编辑器
```

约定：

- `SettingItems` 必须是纯声明（无 IO、无逻辑），宿主可在不实例化服务的情况下渲染设置页
- Prompt 模型（名称/角色/内容/启用）为 SDK 内置 POCO；LLM 插件用 `PromptListSetting` 声明，读取 `Settings.Get<PromptList>(id)`
- 条件可见性（如"自定义 BaseUrl 开关"）v2.0 不做，YAGNI——用固定项覆盖

## 4. 双宿主形态

```
                      ┌─ 内置插件（官方 21 个，编译进主程序）
宿主 ServiceManager ──┤
                      └─ WorkerProxy ──命名管道/JSON-RPC── PluginHost.exe（JIT）← 社区 .spkg
```

- **内置**：宿主直接 `new Main()` + 注入真实 context。设置页由宿主按 `SettingItems` 生成（FluentAvalonia 控件直映射）。
- **worker**：PluginHost 加载 `.spkg`，逐实例暴露 RPC：`init(serviceId, settingsJson)`、`translate(request) → result`、`translateDelta → 通知流`、`ocr/speak/lookup/saveVocab/dispose`。宿主侧 `WorkerProxy` 实现与内置插件相同的接口，上层无感。
- 序列化统一 System.Text.Json；AOT 宿主侧用 source generator；worker 侧常规反射序列化即可。

## 5. SDK 工程与迁移策略

- 新项目 `src/STranslate.Sdk/`（目标 net10.0，无 WPF）；项目/包/命名空间全新命名，统一为 **`STranslate.Sdk`**（与旧 `STranslate.Plugin` 完全区分，无版本号歧义）
- **官方插件全部在 `src/Extensions/` 下全新实现，项目名以 `STranslate.Extensions.` 开头**（如 `STranslate.Extensions.Translate.OpenAI`、`STranslate.Extensions.Ocr.Baidu`），命名空间同名；旧 `src/Plugins/` 整体冻结，仅供 WPF 宿主使用，不做就地迁移
- 旧 `STranslate.Plugin` 同样冻结；纯类型（LangEnum、TextHelper、AudioData、I18nPair 等）以 csproj Link 方式单源共享到 STranslate.Sdk
- 扩展实现 = 参照旧插件服务核心（HTTP、鉴权、响应解析）重写为 v2 契约：声明 `SettingItems`、`TranslateAsync` 返回结果
- WPF 宿主回归验证继续使用 `src/Plugins` 旧版（完全不受迁移影响）
- 社区 `.spkg`：宿主按 SDK 大版本拒绝加载 v1 包并提示；**不做** Windows legacy 兼容 worker（先观察需求）

## 6. 落地顺序

1. `STranslate.Sdk` 项目 + 契约类型 + 纯文件 Link
2. 首个扩展（`STranslate.Extensions.Translate.MTranServer` 或 GoogleBuiltIn）在 `src/Extensions/` 走通内置直调端到端
3. 主翻译窗接入（任务 #8）
4. PluginHost + IPC + 社区 .spkg 加载
5. 批量实现其余 20 个官方扩展

## 7. 评审结论（2026-10-13）

1. **命名**：项目/包/命名空间全部为 `STranslate.Sdk`（全新命名，不沿用旧包名）。
2. **流式**：采用 `IProgress<string>? deltas`。可行性已确认：内置模式下宿主在 UI 线程构造 `Progress<string>`，Report 自动封送更新可观察文本；worker 模式下 Report 映射为 JSON-RPC 通知（命名管道亚毫秒）。约定：展示以 deltas 累积，终值以 `TranslateResult` 为准。
3. **Context 砍伐**：按草案执行，实测零损失——AudioPlayer 全插件仅 TTS.MicrosoftEdge 1 处（v2 改为返回 AudioData）；ImageQuality 仅 OpenAI OCR 用（参数化进 OcrRequest）；Snackbar 插件零使用；GetPromptEditWindow/ApplyTheme 由 PromptListSetting 宿主内置编辑器覆盖。
4. **旧包兼容**：v1 `.spkg` 直接拒绝加载并提示，不做 legacy worker。
