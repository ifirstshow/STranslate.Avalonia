# M5 Windows 对等盘点清单

> 2026-10-15 基于对 WPF 版源码与文档（src/docs/）的全面对照。分四档：✅ 已对等 / P0 日常高频 / P1 完整性必需 / P2 生态与长尾。
> 完成一项划掉一项（改 ✅ 并注明日期）。

## 一、已对等（Windows 日常主链路可用）

- ✅ 主窗手动翻译（输入/Ctrl+Enter/复制/错误提示）
- ✅ 托盘常驻 + 单实例 + 第二实例唤醒
- ✅ 全局热键 6 键可配置（Alt+G/A/D/S/F/X）+ 冲突托盘提示
- ✅ 软件内热键 Ctrl+Enter
- ✅ 鼠标划词（双击/拖选）+ Ctrl+C+C
- ✅ 截图翻译（Alt+S）→ 微信/Google OCR → 主链路
- ✅ 静默 OCR（Alt+Shift+F → 剪贴板）
- ✅ 图片翻译基础版（Alt+Shift+X 原位贴图 + 译文覆盖）
- ✅ 朗读（Windows TTS）+ 生词本（欧路）
- ✅ 历史记录（搜索/删除/清空/回填/容量裁剪）
- ✅ 服务管理（OCR/翻译选择 + 扩展设置动态表单）
- ✅ 设置持久化（语言对/开关/热键）+ 设置窗口（服务/通用/数据/热键）
- ✅ 配置导出/导入（zip）+ 便携模式（PortableConfig）
- ✅ 开机自启（HKCU Run）+ 检查更新（Velopack，安装版生效）
- ✅ 全局异常落盘 + 热键委托保活治理

## 二、P0 —— 日常高频缺口（建议 M5 立即补）

| 项 | WPF 行为 | 说明 |
| --- | --- | --- |
| 主窗交互细节 | 语言交换按钮、置顶、自动翻译开关、清空按钮、双击结果复制 | ✅ 2026-10-22 核对已全部具备（语言交换/置顶/自动翻译/清空/双击复制此前已实现，条目过时） |
| 主题（亮/暗/跟随系统） | ToggleColorTheme 热键 + 设置 | FluentAvalonia 支持 RequestedThemeVariant，成本低 |
| 开机自启已完成但入口 | 托盘/设置 | ✅ 已有设置开关 |
| vpk 打包发布管线 | build.ps1 + GitHub Release | ✅ 2026-10-15 publish-avalonia.ps1（AOT 发布 + vpk pack 本地验证通过，产出 Setup.exe/Portable.zip/增量包；-Upload 走 gh 上传 GitHub Release） |
| 开机自启偏差修正 | WPF 用注册表 | ✅ 已对等 |
| Snackbar 级提示 | 划词失败/生词成功等轻提示 | ✅ 2026-10-22 ShowLightMessage：3 秒自动消失的浅灰提示条；取词失败/生词本成功接入 |

## 三、P1 —— 完整性必需（对等切换前补齐）

| 项 | WPF 位置 | 说明 |
| --- | --- | --- |
| OCR 独立窗口 | ✅ 2026-10-16 Alt+Shift+S 精简版：截图 → 图片预览 + 识别文字并排 + 复制/重新识别；✅ 2026-10-22 增补：识别块红框标注（可开关）+ 导入图片文件识别 + 保存图片到文件 + 二维码识别（ZXing.Net 0.16.11 + ImageSharp V3 绑定，AOT 验证通过；结果条支持复制/打开链接） |
| 图片翻译完整版 | ImageTranslateWindow/Compact/Pinned | ✅ 2026-10-20 基础完整：Alt+Shift+X 原位贴图 + 译文卡片 + 同行贪心合并 + 按钮条避让（下方→上方→叠加，对齐原版铁律）+ 重新翻译/重新截图（重走图片翻译）+ 失焦/Esc 关闭；标注图/钉住/紧凑布局待补 |
| 增量翻译 | ✅ 2026-10-15 增量翻译键（热键页可配，默认关）：按住键划选累积到输入框，松开翻译 |
| 悬浮图标 | ✅ 2026-10-16 MouseSelectionIconWindow：划词松开后在选区旁显示“译”悬浮钮，点击取词翻译；5 秒/失焦自动消失；主窗“悬浮图标”开关 + MouseSelectionIconEnabled 持久化（直接翻译优先） |
| 剪贴板监听常驻 | ✅ 2026-10-15 WM_CLIPBOARDUPDATE 监听器 + 通用页开关 + 自触发去重 |
| 静默 TTS | ✅ 2026-10-15 Alt+Shift+G 取选中文本/剪贴板直接朗读 |
| 热键冲突完整处理 | ✅ 2026-10-15 启动/保存后 0.5/1/2/4s 四次自动重试 + 冲突实时反馈（tooltip/设置页状态） |
| i18n 国际化 | ✅ 基础 2026-10-15 HostI18n 中/英表跟随系统语言 + key 回退；UI 全面本地化随 P2 |
| 通知系统 | ✅ 2026-10-15 Windows Toast（Toolkit），失败静默不碍主流程 |
| 开机自启（含 UAC 跳过版） | TaskName=STranslateSkipUAC 计划任务 | 普通注册表版已对等；跳过 UAC 版为增强 |
| 首次运行向导 | WelcomeSetupWindow | 三配置缺失时的引导 |
| 外部调用 API | ✅ 2026-10-15 端口 51080（50020 落在本机 WinNAT 保留段）：translate(_force/_input/_crossword/_ocr)/ocr_silence；未知 action 记日志 |
| 更多翻译扩展 | ✅ OpenAI 2026-10-15（v2 首版：Chat Completions 非流式 + PromptListSetting 单条 Prompt 模式；流式/Responses 模式按需补）；✅ 2026-10-23 ICiba 词典翻译（批量接口 + 词页 __NEXT_DATA__ 回退，双签名/MD5、整链路移植 WPF 版，零配置项）。DeepL/百度/有道/腾讯…待迁 |
| WebDav 云备份 | ✅ 2026-10-15 WebDavBackupService（WebDav.Client PutFile/GetRawFile）+ 数据页 WebDav 区（地址/账号/密码 + 上传/恢复，配置持久化）；✅ 2026-10-20 恢复/导入即时生效（LoadIntoCurrent + ApplyRestoredSettings + RefreshAllFromVm，修复 MouseSelectionIconEnabled 漏拷与导出覆盖已存在文件报错） |

## 四、P2 —— 生态与长尾

| 项 | 说明 |
| --- | --- |
| 插件市场 UI + 社区 .spkg 加载 | ✅ 2026-10-24 基础闭环：.spkg（zip）安装/卸载（设置→服务页"社区插件"区）+ worker 进程按需加载（STranslate.PluginHost JIT exe，命名管道 ndjson JSON-RPC，settings.update 配置注入，取消传播，崩溃隔离）+ 四类服务代理并入服务列表；端到端 HelloWorker 测试插件验证（安装/加载/翻译/设置生效/卸载）。远端市场索引与 .spkg 分发渠道待后续 |
| 词典插件 | ICiba 词典结果（DictionaryResult 渲染 UI） |
| 更多 TTS/OCR 扩展 | 腾讯/百度/有道 OCR、OpenAI OCR、更多 TTS |
| 历史收藏/备注/导出 | ✅ 2026-10-22 ★ 收藏（切换+只看收藏过滤）、✎ 备注（对话框编辑+列表显示+参与搜索，Note 列自动迁移）、导出 JSON（按当前搜索/过滤）；另修复 Eudict 扩展把成功响应误判为失败 |
| 更新通道/检查频率设置 | 手动检查已对等 |
| WPF 版退役切换 | 发布公告 + 文档引流 |

## 五、非功能项

- ✅ AOT 可行性（M1 验证）；发布时需重跑 AOT 剪剪
- ✅ CI：跨平台编译校验 job（ubuntu，✅ 实测通过）+ WPF build job（✅ 实测通过，v0.1.1 已发布）；⚠ Avalonia 打包 job 在 GitHub Windows runner 上存在 restore 静默失败（MSB4181，NuGet 7.9 环境问题，已报排查多轮），**v0.1.1 的 Avalonia 产物经本地 publish-avalonia.ps1 -Upload 发布**（独立 avalonia 更新通道，与 WPF win 通道隔离）；后续 runner 镜像更新后重试接入
- ⏳ 图标/品牌资产沿用 WPF 版 ✓（app.ico 已复用）
