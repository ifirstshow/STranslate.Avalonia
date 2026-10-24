# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 语言规则

- 所有解释、推理和评论使用简体中文；代码标识符、接口名、文件名保持英文。
- Git 提交信息必须使用英文。

## 文档导航（重要）

`src/AGENTS.md` 是仓库文档的唯一导航入口。深入任何模块前，先查 `src/docs/` 下对应文档（扁平结构）：

| 任务场景 | 文档 |
| --- | --- |
| 启动/单实例/DI/窗口生命周期 | `src/docs/runtime-bootstrap.md` |
| 插件加载/服务实例化/PluginContext | `src/docs/runtime-plugin-service.md` |
| 主翻译/OCR/图片翻译/输入触发链路 | `src/docs/flow-main-translation.md`、`flow-ocr-image.md`、`flow-image-translation.md`、`flow-input-trigger.md` |
| 插件市场/SDK/社区插件/国际化 | `src/docs/plugin-market-management.md`、`plugin-sdk-development.md`、`community-plugin-development.md`、`i18n-internationalization.md` |
| 配置存储/网络运维 | `src/docs/config-storage-history.md`、`integration-network-ops.md` |

## 常用命令

所有 dotnet 命令在 `src/` 目录下执行：

```powershell
# 运行 WPF 版
dotnet run --project STranslate/STranslate.csproj

# 运行 Avalonia 版（日常 windows TFM，含微信 OCR）
dotnet run --project STranslate.Avalonia

# Debug 构建
dotnet build STranslate.slnx --configuration Debug

# Avalonia 跨平台编译校验（CI/Linux 用，只验证可编译不出产物交付）
dotnet build STranslate.Avalonia/STranslate.Avalonia.csproj -p:TargetFrameworks=net10.0

# 全部测试
dotnet test Tests/STranslate.Tests/STranslate.Tests.csproj

# 单个测试类 / 单个测试方法
dotnet test Tests/STranslate.Tests/STranslate.Tests.csproj --filter "FullyQualifiedName~OcrWordBuilderTests"
dotnet test Tests/STranslate.Tests/STranslate.Tests.csproj --filter "FullyQualifiedName~OcrWordBuilderTests.方法名"
```

发布构建在仓库根目录执行 `.\build.ps1 <version>`（要求 .NET 10 SDK，Windows）：写版本号到 `src/SolutionAssemblyInfo.cs`、切换 `FodyWeavers.Release.xml`、Release 构建、清理插件目录中的多余 DLL。

`STranslate.Host` 是 Rust 辅助进程（`z_stranslate_host.exe`，负责更新解包/提权启动/延迟启动），改动后用 `cargo build --release` 构建。

## 架构

**项目本质**：STranslate 2.0 —— Windows 下的翻译/OCR 工具，**WPF + MVVM**（CommunityToolkit.Mvvm + iNKORE.UI.WPF.Modern）。注意：本地目录名带 "Avalonia" 但项目实际是 WPF（`net10.0-windows`），不要引入 Avalonia 依赖。

**四层结构**（均在 `src/` 下，解决方案为 `STranslate.slnx`）：

1. `STranslate.Plugin/` —— 插件 SDK。定义 `IPlugin`、`ITranslatePlugin`（含 `TranslatePluginBase`/`LlmTranslatePluginBase`/`DictionaryPluginBase`）、`IOcrPlugin`、`ITtsPlugin`、`IVocabularyPlugin`、`IPluginContext`。宿主通过 `InternalsVisibleTo` 可见其内部。
2. `Plugins/` —— 约 21 个官方插件项目（Translate/Ocr/Tts/Vocabulary），每个包含 `plugin.json` + `Main.cs` + `Languages/*.xaml`，引用 SDK 项目（非 NuGet 包）。
3. `STranslate/` —— 主程序。`ViewModels/` + `Views/`（MainWindow、SettingsWindow、OcrWindow、ImageTranslateWindow 等）+ `Services/`（TranslateService/OcrService/TtsService/VocabularyService/MouseSelectionService 等）+ `Core/`（Settings、PluginManager、ServiceManager、SqlService、HttpService 等基础设施）+ `Helpers/`（Win32 互操作、窗口激活）。
4. `STranslate.Host/` —— Rust 宿主进程。

**启动链路**：`App.Main()` 单实例判定（Mutex + NamedPipe，第二实例通知首实例）→ `App()` 构造函数读取三类 JSON 配置（`Settings.json`/`HotkeySettings.json`/`ServiceSettings.json`）并注册 DI → `OnStartup()` 初始化插件/服务/SQLite，三个配置都不存在时先弹 `WelcomeSetupWindow` 向导 → 创建 `MainWindow` 并延迟初始化。

**插件运行时**：`PluginManager` 读取 `plugin.json` 加载程序集（`PluginAssemblyLoader`），`ServiceManager` 按 `ServiceSettings` 实例化服务（`PluginInstance`）。插件通过 `IPluginContext` 获得宿主能力：`HttpService`、`Logger`、`AudioPlayer`、`Snackbar`、`Notification`、配置存储。历史记录存 SQLite（Dapper.Contrib）。

**构建产物**：全部输出集中到 `src/.artifacts/{Debug|Release}/`（不用 bin/obj），插件输出到 `.artifacts/*/Plugins/<名称>/`。Release 用 Costura.Fody 把 DLL 嵌入主程序。NuGet 包版本集中在 `src/Directory.Packages.props`（CPM），通用编译属性在 `src/Directory.Build.props`（`LangVersion=preview`、Nullable 全开）。

**UI 强制规则**：MessageBox 一律走 `AppMessageBox.Show()`，禁止直接调用 iNKORE `MessageBox.Show()`；从具体窗口/控件触发的 iNKORE `ContentDialog` 必须显式传入 owner。

**窗口激活**：统一走 `Win32Helper.ActivateForegroundWindow()`，普通/强制模式由 `WindowActivationContext`（`AsyncLocal`）决定；HTTP 外部调用和 Ctrl+C+C 回调期间为强制模式。单例窗口用 `SingletonWindowOpener` 打开。

## Agent skills

### Issue tracker

Issue 跟踪在 GitHub Issues（`gh` CLI）。 See `docs/agents/issue-tracker.md`.

### Triage labels

使用五个默认规范标签（needs-triage / needs-info / ready-for-agent / ready-for-human / wontfix）。 See `docs/agents/triage-labels.md`.

### Domain docs

单上下文布局：根目录 `CONTEXT.md` + `docs/adr/`。 See `docs/agents/domain.md`.
