# 混合插件架构：AOT 主程序 + 官方插件内置 + 社区插件 worker 加载

插件 ABI 原绑定 WPF（`IPlugin.GetSettingUI()` 返回 `UserControl`），跨平台必须重构契约；同时 Native AOT 是发布目标。决定：主程序按 Native AOT 发布；官方 21 个插件重构为新契约（设置页元数据化、无 WPF 依赖）后**编译进主程序进程内直调**；社区 `.spkg` 由独立 JIT 子进程 PluginHost 加载，经命名管道 JSON-RPC 调用，仅在装有社区插件时启动。替代 ADR-0001（该决策放弃 AOT 是为保全进程内动态加载，进程外加载方案使两者不再互斥）。

Considered: 纯 JIT + 进程内加载（工程量最小，但无 AOT 且坏插件连坐主程序）；纯 AOT 全静态内置（彻底放弃社区扩展）。

Consequences: Dapper.Contrib 不支持 AOT，历史存储层需替换实现；FluentAvalonia 的 AOT+裁剪兼容性需在 M1 先行原型验证，验证通过前不铺开 worker 设计；社区 `.spkg` 作者需按新 SDK 重编译一次（1.x→2.0 有过同级先例）；IPC 面限定为四类服务（Translate/Ocr/Tts/Vocabulary）+ 取消传播 + 配置注入。WeChatBuiltIn 过渡期作为内置规则的例外走 worker 加载（Newtonsoft 无第一方使用，Jwt 8.x 已内置 System.Text.Json，均经 `dotnet nuget why` 核实）；WeChatOcr 实为 100% C# 薄封装（P/Invoke mmojo + 管道通信均 AOT 安全，仅内部 Newtonsoft 反序列化不友好），计划 M3 vendor 进仓库并以 System.Text.Json source generator 替换，届时 WeChatBuiltIn 升级为内置插件、worker 专用于社区插件。该上游仓库标注"仅供学习"且无正式 LICENSE，vendor 前需与作者 ZGGSONG 确认；vendor 后微信版本升级导致的适配责任由本仓库承担。
