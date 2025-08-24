# 装甲车辆抗毁伤数字化评估软件平台 - PowerShell启动器
# 沈阳理工大学 v1.0.0

param(
    [switch]$SelfContained,
    [switch]$FrameworkDependent,
    [switch]$Help
)

# 设置控制台编码
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

function Show-Header {
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Blue
    Write-Host "            装甲车辆抗毁伤数字化评估软件平台" -ForegroundColor Cyan
    Write-Host "                  沈阳理工大学 v1.0.0" -ForegroundColor Green
    Write-Host "================================================================" -ForegroundColor Blue
    Write-Host ""
}

function Show-Help {
    Show-Header
    Write-Host "使用方法:" -ForegroundColor Yellow
    Write-Host "  .\run-app.ps1                    # 自动检测并运行最佳版本"
    Write-Host "  .\run-app.ps1 -SelfContained     # 强制运行自包含版本"
    Write-Host "  .\run-app.ps1 -FrameworkDependent # 强制运行框架依赖版本"
    Write-Host "  .\run-app.ps1 -Help              # 显示帮助信息"
    Write-Host ""
    Write-Host "版本说明:" -ForegroundColor Yellow
    Write-Host "  自包含版本    : 无需安装.NET运行时，文件大小较大"
    Write-Host "  框架依赖版本  : 需要安装.NET 8.0运行时，文件大小较小"
    Write-Host ""
}

function Test-DotNetRuntime {
    try {
        $dotnetVersion = dotnet --version 2>$null
        if ($dotnetVersion -and $dotnetVersion.StartsWith("8.")) {
            return $true
        }
    }
    catch {
        return $false
    }
    return $false
}

function Start-Application {
    param(
        [string]$AppPath
    )
    
    if (-not (Test-Path $AppPath)) {
        Write-Host "[✗] 错误：找不到程序文件 $AppPath" -ForegroundColor Red
        return $false
    }
    
    Write-Host "[INFO] 启动程序: $AppPath" -ForegroundColor Blue
    
    try {
        Start-Process -FilePath $AppPath -WorkingDirectory (Split-Path $AppPath)
        Write-Host "[✓] 程序启动成功！" -ForegroundColor Green
        Write-Host ""
        Write-Host "如果程序没有显示，请检查：" -ForegroundColor Yellow
        Write-Host "  1. 杀毒软件是否拦截程序运行" -ForegroundColor Gray
        Write-Host "  2. Windows事件查看器中的错误信息" -ForegroundColor Gray
        Write-Host "  3. 程序目录下logs文件夹中的日志文件" -ForegroundColor Gray
        return $true
    }
    catch {
        Write-Host "[✗] 启动失败：$($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# 主逻辑
Show-Header

if ($Help) {
    Show-Help
    return
}

# 设置路径
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$PublishDir = Join-Path $ScriptDir "..\publish"
$SelfContainedDir = Join-Path $PublishDir "win-x64-self-contained"
$FrameworkDependentDir = Join-Path $PublishDir "win-x64-framework-dependent"
$AppName = "ArmorVehicleDamageAssessment.UI.exe"

$SelfContainedPath = Join-Path $SelfContainedDir $AppName
$FrameworkDependentPath = Join-Path $FrameworkDependentDir $AppName

Write-Host "[INFO] 正在检查发布文件..." -ForegroundColor Blue

# 强制运行自包含版本
if ($SelfContained) {
    if (Start-Application $SelfContainedPath) {
        return
    } else {
        Write-Host "[INFO] 自包含版本启动失败，请检查文件是否存在" -ForegroundColor Yellow
        return
    }
}

# 强制运行框架依赖版本
if ($FrameworkDependent) {
    if (-not (Test-DotNetRuntime)) {
        Write-Host "[WARNING] 未检测到 .NET 8.0 运行时" -ForegroundColor Yellow
        Write-Host "[INFO] 请访问以下地址下载安装：" -ForegroundColor Blue
        Write-Host "        https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
        Write-Host ""
    }
    
    if (Start-Application $FrameworkDependentPath) {
        return
    } else {
        Write-Host "[INFO] 框架依赖版本启动失败，请检查.NET运行时和文件" -ForegroundColor Yellow
        return
    }
}

# 自动检测模式
$SelfContainedExists = Test-Path $SelfContainedPath
$FrameworkDependentExists = Test-Path $FrameworkDependentPath
$HasDotNetRuntime = Test-DotNetRuntime

Write-Host "[INFO] 文件检查结果：" -ForegroundColor Blue
Write-Host "  自包含版本     : $(if($SelfContainedExists){'✓ 存在'}else{'✗ 不存在'})" -ForegroundColor $(if($SelfContainedExists){'Green'}else{'Red'})
Write-Host "  框架依赖版本   : $(if($FrameworkDependentExists){'✓ 存在'}else{'✗ 不存在'})" -ForegroundColor $(if($FrameworkDependentExists){'Green'}else{'Red'})
Write-Host "  .NET 8.0运行时 : $(if($HasDotNetRuntime){'✓ 已安装'}else{'✗ 未安装'})" -ForegroundColor $(if($HasDotNetRuntime){'Green'}else{'Red'})
Write-Host ""

# 优先级：自包含版本 > 框架依赖版本（如果有.NET）
if ($SelfContainedExists) {
    Write-Host "[INFO] 使用自包含版本（推荐）" -ForegroundColor Green
    Start-Application $SelfContainedPath
}
elseif ($FrameworkDependentExists -and $HasDotNetRuntime) {
    Write-Host "[INFO] 使用框架依赖版本" -ForegroundColor Green
    Start-Application $FrameworkDependentPath
}
elseif ($FrameworkDependentExists -and -not $HasDotNetRuntime) {
    Write-Host "[WARNING] 框架依赖版本需要 .NET 8.0 运行时" -ForegroundColor Yellow
    Write-Host "[INFO] 正在尝试启动，如果失败请安装 .NET 运行时" -ForegroundColor Blue
    Write-Host "        下载地址: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
    Write-Host ""
    Start-Application $FrameworkDependentPath
}
else {
    Write-Host "[✗] 错误：找不到可运行的程序文件！" -ForegroundColor Red
    Write-Host ""
    Write-Host "[HELP] 解决方法：" -ForegroundColor Yellow
    Write-Host "  1. 运行构建脚本重新生成发布文件" -ForegroundColor Gray
    Write-Host "     .\scripts\build-and-publish.ps1" -ForegroundColor Gray
    Write-Host "  2. 从压缩包中解压程序文件" -ForegroundColor Gray
    Write-Host "  3. 检查发布目录路径是否正确" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "按任意键退出..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
