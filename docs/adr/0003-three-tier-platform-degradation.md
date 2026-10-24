# 三层平台降级矩阵

"功能与 WPF 版一致"在非 Windows 平台按三层矩阵解释：核心功能（翻译、词典、OCR 窗、TTS、生词本、设置、插件市场、剪贴板、托盘）三平台必达；系统集成功能（全局热键、划词、截图 OCR、自启）Windows/macOS 全量，Linux 按 X11（尽力全量）/ Wayland（portal 手动降级）分级，且降级必须在 UI 上可见；Win32 专属特性（管理员模式、前台激活 hack）仅 Windows 生效。

Consequences: Wayland 的 compositor 安全模型使全局输入钩子与静默截屏物理不可行——相关功能缺失是设计约束而非未完成，不要试图"补齐"它。
