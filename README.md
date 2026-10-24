<div align="center">

<h1 align="center">STranslate.Avalonia</h1>

[**简体中文**](./README_ZH.md) | **English** | [**English**](./README.md)

</div>

An **Avalonia cross-platform rewrite** of [STranslate](https://github.com/ZGGSONG/STranslate) (a Windows translation/OCR tool by [zggsong](https://github.com/zggsong)): shipped as Native AOT for Windows, with an architecture ready for macOS/Linux.

> **Relation to the WPF original**: this repo is forked from and shares history with the original, but **plugins and configs are NOT interchangeable** between the Avalonia and WPF editions (the plugin contract was redesigned as SDK v2). Do not mix plugins or config backups across editions.

## Download

Get it from [Releases](https://github.com/ifirstshow/STranslate.Avalonia/releases):

| File | Description |
| --- | --- |
| `STranslate.Avalonia-avalonia-Setup.exe` | Installer (with auto-update on the `avalonia` channel) |
| `STranslate.Avalonia-avalonia-Portable.zip` | Portable (extract and run) |

## Features

- Translation: manual/auto in main window, text selection (double-click/drag), Ctrl+C+C, clipboard monitor, incremental translate
- OCR: screenshot OCR (built-in WeChat/Google), silent OCR, standalone OCR window (annotation/QR code/import-export)
- Image translation: in-place overlay, re-translate / re-screenshot
- TTS (Windows voices), vocabulary (Eudic), history (favorites/notes/export)
- Global hotkeys (auto-retry on conflict), external call API (HTTP 51080)
- Backup: WebDav sync + local export/import
- Updates: Velopack auto-update

## Bundled Extensions

| Type | Extensions |
| --- | --- |
| Translate | MTranServer (offline), OpenAI, ICiba |
| OCR | Built-in WeChat, Google |
| TTS | Windows TTS |
| Vocabulary | Eudic |

## Plugin Development (Community Plugins)

The Avalonia edition uses **plugin SDK v2** (`STranslate.Sdk`): metadata-driven settings, no UI dependencies, plain-data service results.
Community plugins ship as `.spkg` (zip) archives, loaded by an isolated JIT worker process (crash-isolated from the host), installed via Settings → Services → Community Plugins.

- SDK 契约：[src/STranslate.Sdk](./src/STranslate.Sdk)
- 扩展示例：[src/Extensions](./src/Extensions)
- 设计文档：[docs/adr/0004-hybrid-plugin-architecture.md](./docs/adr/0004-hybrid-plugin-architecture.md)

> Note: WPF-edition (original) plugins are **not compatible**; they must be recompiled against SDK v2.

## Development

```powershell
# Daily run (windows TFM, includes WeChat OCR)
dotnet run --project src/STranslate.Avalonia

# Cross-compile check (same as CI)
dotnet build src/STranslate.Avalonia/STranslate.Avalonia.csproj -p:TargetFrameworks=net10.0

# Release (AOT + vpk pack + upload to Release)
.\publish-avalonia.ps1 -Version <版本号> -Upload
```

Architecture decisions: [docs/adr](./docs/adr). Parity checklist: [src/docs/m5-parity-checklist.md](./src/docs/m5-parity-checklist.md).

## Acknowledgements

- The original [STranslate](https://github.com/ZGGSONG/STranslate) by [zggsong](https://github.com/zggsong) — this project is a rebuild on top of their work
- The original docs site ([stranslate.zggsong.com](https://stranslate.zggsong.com)) documents the **WPF edition**; parts of it do not apply here

## License

MIT, per the original [LICENSE](./LICENSE). Copyright © 2022 zggsong.
