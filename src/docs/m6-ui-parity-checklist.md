# M6 原版靠拢清单（UI / 功能对等）

> 2026-10-24 对照 WPF 原版（STranslate/Views + Controls 60 控件 + Pages 12 页）盘点。
> 目标：界面形态与功能覆盖向原版靠拢。完成一项划掉一项（改 ✅ 并注明日期）。

## 一、主窗（原版 MainWindow.xaml 386 行：InputControl + OutputControl + HeaderControl）

| 项 | 原版形态 | 状态 |
| --- | --- | --- |
| 输入区工具行 | 生词/朗读/复制/去换行/去空格 | ✅ 2026-10-25 卡片化+工具行（识别语种检测随语言检测服务后续） |
| 输出区操作行 | 复制/朗读/生词 | ✅ 2026-10-25 卡片化（Prompt 切换随 Prompt 编辑器后续） |
| 语言交换与选择 | 图标按钮 + 输出目标多选（OutputControl 多目标输出） | ⏳ 现为两下拉 |
| 词典视图 | 查单词时内嵌词典结果（音标/发音/释义/变位/例句） | ✅ 2026-10-26 ICiba 转为 IDictionaryPlugin（结构化音标+发音播放+释义+变位；句子查询时该卡提示未查询到） |
| 服务快捷切换 | ServiceSwitcherPopup 弹层切换翻译服务 | ✅ 2026-10-25 标题行按钮+Flyout |
| 置顶/主题/紧凑模式 | 位置宽度记忆 | ✅ 2026-10-28 位置/宽度记忆（防抖落盘） |
| Snackbar | 底部自动消失提示（SnackbarContainer） | ⏳ 已有简版轻提示 |

## 二、设置窗（原版 NavigationView + 12 页）

| 页 | 内容 | 状态 |
| --- | --- | --- |
| 导航形态 | ui:NavigationView 左侧导航 + 页面切换 | ❌ 现为 TabControl |
| 通用 GeneralPage | 启动/主题/划词/剪贴板/自动翻译/悬浮图标等 | ⏳ 内容大部分有，形态简陋 |
| 热键 HotkeyPage | HotkeyControl 点击录制 + 冲突显示 | ⏳ 现为文本框 |
| 服务 TranslatePage/OcrPage/TtsPage/VocabularyPage | ServicePanel 卡片：启停开关/拖拽排序/右键重命名删除/ServiceContentDialog 新增 | ⏳ 现为下拉选择+平铺表单 |
| 插件 PluginPage | 插件市场（远端索引+在线安装） | ✅ 2026-10-28 在线市场（docs/plugin-market.json 索引 + 拉取/安装/卸载 UI；等社区包填充） |
| 网络 NetworkPage | 代理设置（启用/系统/HTTP+端口+测试连通显示出口 IP） | ✅ 2026-10-25 网络页 + HttpService 消费代理（ReloadProxy 即时生效）；✅ 2026-10-28 SOCKS5 支持 |
| 历史 HistoryPage | 原版设置内嵌历史管理 | ⏳ 独立窗口已有 |
| 关于 AboutPage | 版本/检查更新/致谢/开源许可 | ✅ 2026-10-25 关于页（检查更新/致谢/打开原版仓库） |
| 独立窗口 StandalonePage | 独立取词窗口配置 | ❌ 缺 |

## 三、图片翻译（原版三形态）

| 项 | 说明 | 状态 |
| --- | --- | --- |
| ImageTranslateWindow 完整版 | 左图右文（识别全文→一键送翻译链路） | ✅ 2026-10-25 OCR 窗"翻译此文本"打通完整路径（ImageZoom 缩放待后续） |
| CompactWindow 精简贴图 | 贴图+悬浮胶囊工具栏 | ✅ 形态接近 |
| PinnedWindow 钉住 | 贴图钉在桌面（隐藏译文、失焦不关） | ✅ 2026-10-25 贴图窗钉住模式 |
| 分段精调 | 词块合并模式 | ✅ 2026-10-28 两档合并模式（智能行合并/逐块直翻，设置→通用）；完整布局分析待后续 |

## 四、流程与杂项

| 项 | 说明 | 状态 |
| --- | --- | --- |
| 首次向导 WelcomeSetupWindow | 首启引导（语言对/主题/自启） | ✅ 2026-10-26 简化版向导（settings.json 缺失时首启弹出） |
| UpdateChangelogDialog | 更新完成后展示新变更 | ✅ 2026-10-26 简化版（版本变化首启弹窗+Release 入口） |
| LLM Prompt 编辑器 | PromptListSetting 的编辑 UI（PromptEditWindow） | ❌ 缺（SDK 已有契约） |
| 词典扩展 | ICiba 词典结构化渲染 | ✅ 2026-10-26 同上（例句提取待后续） |
| 更多扩展迁移 | DeepL/百度/有道/腾讯… | ❌ 按需 |
| WebDav 对话框 | WebDavContentDialog 表单化 | ⏳ 现为平铺输入框 |

## 建议批次

- **M6a 主窗对等**：输入/输出区还原、多目标输出、服务切换弹层、词典视图
- **M6b 设置窗重构**：NavigationView + 页面拆分 + ServicePanel 卡片（含插件市场 UI）
- **M6c 图片翻译完整版**：完整窗口 + 钉住 + 分段精调
- **M6d 收尾**：向导、更新弹窗、Prompt 编辑器、网络页、关于页
