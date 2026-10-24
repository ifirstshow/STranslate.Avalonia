<div align="center">

<h1 align="center">STranslate.Avalonia</h1>

**简体中文** | [**English**](./README.md)

</div>

[STranslate](https://github.com/ZGGSONG/STranslate)（Windows 翻译/OCR 工具，作者 [zggsong](https://github.com/zggsong)）的 **Avalonia 跨平台重构版**：以 Native AOT 发布 Windows 版本，并具备向 macOS/Linux 扩展的架构基础。

> **与原版 WPF 的关系**：本仓库 fork 自原版并共享其历史，但 **Avalonia 版与 WPF 版的插件、配置互不通用**（插件契约重构为 v2 SDK）。请勿混用两边的插件与配置备份。

## 下载

前往 [Releases](https://github.com/ifirstshow/STranslate.Avalonia/releases) 下载：

| 文件 | 说明 |
| --- | --- |
| `STranslate.Avalonia-avalonia-Setup.exe` | 安装版（含自动更新，`avalonia` 更新通道） |
| `STranslate.Avalonia-avalonia-Portable.zip` | 便携版（解压即用） |

## 功能

- 翻译：主窗手动/自动翻译、划词（双击/拖选）、Ctrl+C+C、剪贴板监听、增量翻译
- OCR：截图识别（微信内置/Google）、静默 OCR、独立 OCR 窗（标注/二维码/导入导出）
- 图片翻译：原位贴图覆盖、重新翻译/重新截图
- 朗读（Windows TTS）、生词本（欧路词典）、历史记录（收藏/备注/导出）
- 全局热键（冲突自动重试）、外部调用 API（HTTP 51080）
- 备份：WebDav 云同步 + 本地导出/导入
- 更新：Velopack 自动更新

## 内置扩展

| 类型 | 扩展 |
| --- | --- |
| 翻译 | MTranServer（离线）、OpenAI、ICiba |
| OCR | 微信内置、Google |
| TTS | Windows TTS |
| 生词本 | 欧路词典 |

## 插件开发（社区插件）

Avalonia 版使用 **v2 插件 SDK**（`STranslate.Sdk`）：设置元数据化、无 UI 依赖、服务调用返回纯数据。
社区插件以 `.spkg`（zip）分发，由独立 JIT worker 进程加载（崩溃隔离主程序），在 设置 → 服务 → 社区插件 中安装。

- SDK 契约：[src/STranslate.Sdk](./src/STranslate.Sdk)
- 扩展示例：[src/Extensions](./src/Extensions)
- 设计文档：[docs/adr/0004-hybrid-plugin-architecture.md](./docs/adr/0004-hybrid-plugin-architecture.md)

> 注意：WPF 版（原版）的插件与本版**不通用**，需要按 v2 SDK 重新编译。

## 开发

```powershell
# 日常运行（windows TFM，含微信 OCR）
dotnet run --project src/STranslate.Avalonia

# 跨平台编译校验（CI 同款）
dotnet build src/STranslate.Avalonia/STranslate.Avalonia.csproj -p:TargetFrameworks=net10.0

# 发布（AOT + vpk 打包 + 上传 Release）
.\publish-avalonia.ps1 -Version <版本号> -Upload
```

架构决策见 [docs/adr](./docs/adr)，对等清单见 [src/docs/m5-parity-checklist.md](./src/docs/m5-parity-checklist.md)。

## 致谢

- 原版 [STranslate](https://github.com/ZGGSONG/STranslate) 与作者 [zggsong](https://github.com/zggsong)——本项目基于其成果重构
- 原版文档站（[stranslate.zggsong.com](https://stranslate.zggsong.com)）描述的是 **WPF 版**使用方式，部分内容不适用于本版

## 许可证

遵循原版 [MIT LICENSE](./LICENSE)。Copyright © 2022 zggsong。
