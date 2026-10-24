# STranslate.Avalonia 打包发布脚本
# 用法: .\publish-avalonia.ps1 [-Version 1.0.0] [-SkipPack] [-Upload]
# 前置: dotnet tool install -g vpk（首次）; Upload 需 gh CLI 登录
param(
    [string]$Version = "1.0.0",
    [switch]$SkipPack,
    [switch]$Upload
)

$ErrorActionPreference = "Stop"
# AOT 链接需要 vswhere（VS Installer 目录默认不在 PATH）
$env:PATH += ";" + "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer"
$RepoRoot = $PSScriptRoot
$ProjectPath = Join-Path $RepoRoot "src\STranslate.Avalonia\STranslate.Avalonia.csproj"
$IconPath = Join-Path $RepoRoot "src\STranslate\Resources\app.ico"
$PublishDir = Join-Path $RepoRoot "src\.artifacts\publish\avalonia"
$Repo = "ifirstshow/STranslate.Avalonia"

function Log([string]$msg, [string]$color = "Yellow") {
    Write-Host $msg -ForegroundColor $color
}

$CleanVersion = $Version -replace '^v', ''

# 0) 社区插件 worker（JIT exe，随安装包分发；ADR-0004）
Log "=== 0/3 发布 PluginHost worker ===" "Green"
dotnet publish (Join-Path $RepoRoot "src\STranslate.PluginHost\STranslate.PluginHost.csproj") -c Release -r win-x64 --no-self-contained -o (Join-Path $RepoRoot "src\.artifacts\publish\worker")
if ($LASTEXITCODE -ne 0) { Log "worker 发布失败" "Red"; exit 1 }
Copy-Item (Join-Path $RepoRoot "src\.artifacts\publish\worker\*") $PublishDir -Force

# 1) AOT 发布
Log "=== 1/3 AOT 发布 ($CleanVersion) ===" "Green"
dotnet publish $ProjectPath -c Release -r win-x64 -p:PublishAot=true -o $PublishDir
if ($LASTEXITCODE -ne 0) { Log "AOT 发布失败" "Red"; exit 1 }

if ($SkipPack) {
    Log "=== 跳过 vpk 打包（-SkipPack），产物在 $PublishDir ===" "Yellow"
    exit 0
}

# 2) vpk 打包（产出 setup 安装包 + 增量包到 Releases/）
Log "=== 2/3 vpk 打包 ===" "Green"
vpk pack `
    --packId STranslate.Avalonia `
    --packTitle "STranslate" `
    --packVersion $CleanVersion `
    --packDir $PublishDir `
    --mainExe STranslate.Avalonia.exe `
    --icon $IconPath
if ($LASTEXITCODE -ne 0) { Log "vpk 打包失败" "Red"; exit 1 }

$ReleasesDir = Join-Path $RepoRoot "Releases"
Log "=== 打包完成: $ReleasesDir ===" "Green"

# 3) GitHub Release 上传（需 gh 登录；不存在 release 则创建）
if ($Upload) {
    Log "=== 3/3 上传 GitHub Release ===" "Green"
    $tag = "v$CleanVersion"
    gh release view $tag --repo $Repo 2>$null
    if ($LASTEXITCODE -ne 0) {
        gh release create $tag --repo $Repo --title $tag --generate-notes
        if ($LASTEXITCODE -ne 0) { Log "创建 Release 失败" "Red"; exit 1 }
    }
    gh release upload $tag "$ReleasesDir\*" --repo $Repo --clobber
    if ($LASTEXITCODE -ne 0) { Log "上传产物失败" "Red"; exit 1 }
    Log "=== 上传完成 ===" "Green"
}
