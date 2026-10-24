# STranslate

STranslate 是一个翻译/OCR 桌面工具，当前存在两个宿主（WPF 版与 Avalonia 版）共享同一插件生态。本词汇表收敛跨宿主协作与多平台交付时使用的领域语言。

## Language

### 宿主

**WPF 版**:
现有主程序（`src/STranslate`），基于 WPF + iNKORE，Windows 专属。对等切换前为功能基准，之后进入维护模式。
_Avoid_: 旧版、老版本

**Avalonia 版**:
跨平台重写的主程序（`src/STranslate.Avalonia`），与 WPF 版同仓库共存，加载同一套插件。
_Avoid_: 新版（"新"会过期）

### 插件

**内置插件**:
官方维护的插件，编译进宿主进程内直调。两个宿主共享同一份源码与配置元数据。
_Avoid_: 官方插件加载

**插件 worker（PluginHost）**:
承载社区插件的独立 JIT 子进程，主程序经 IPC 调用。仅在装有社区插件时启动；worker 崩溃不影响主程序。
_Avoid_: 插件进程（不区分方向）、后台进程

### 多平台交付

**tier-1 / tier-2 / tier-3 平台**:
平台交付优先级：tier-1 = Windows，tier-2 = macOS，tier-3 = Linux。架构上面向三平台，交付按此顺序。
_Avoid_: 主平台/副平台

**降级矩阵**:
功能在非 Windows 平台的可用性分层：核心功能三平台必达；系统集成功能（全局热键、划词、截图 OCR、自启）Windows/macOS 全量、Linux 按 X11/Wayland 分级；Win32 专属特性仅 Windows 生效。降级必须在 UI 上可见，不允许静默失效。
_Avoid_: 功能阉割、简化版

**对等切换**:
Avalonia 版（Windows）达到降级矩阵第一层全量覆盖的时点。此后 WPF 版转维护模式：只修缺陷，不加功能。
_Avoid_: 替代、下线
