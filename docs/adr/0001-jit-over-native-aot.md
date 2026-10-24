---
status: superseded by ADR-0004
---

# JIT 优先于 Native AOT

Avalonia 版最初同时要求跨平台与 Native AOT 发布，但 Native AOT 下 `Assembly.LoadFile` 等动态加载 API 不可用，而运行时加载插件（`.spkg`、插件市场）是 STranslate 2.0 的产品身份。我们决定放弃 Native AOT，采用 JIT 自包含发布——插件生态的完整性优先于启动速度和体积收益。若未来出现无插件加载的精简形态，可重新评估 AOT。
