# 同仓库共存，Link 单源共享逻辑

Avalonia 版与 WPF 版同仓库、同解决方案共存：共享 `STranslate.Plugin` SDK 与全部插件项目，业务逻辑文件通过 csproj `<Compile Include Link>` 单源包含（仓库先例：`SolutionAssemblyInfo.cs`），不复制分叉、不做物理抽取、不用文件系统 symlink（Windows 特权与 CI 成本）。WPF 版在 Avalonia Windows 版对等切换前正常维护，之后转维护模式。

Considered: 独立新仓库 + NuGet 分发 SDK（被否——插件生态会分叉为两套互不兼容的 `.spkg`）；逻辑复制后各自演化（被否——业务逻辑双份维护，bug 修两遍）。
