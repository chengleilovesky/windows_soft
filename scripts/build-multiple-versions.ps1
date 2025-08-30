# 装甲车辆评估软件 - 多种打包方式构建脚本
# 创建多个不同的发布版本以解决兼容性问题

param(
    [switch]$CleanFirst,
    [switch]$SkipTests,
    [string]$Version = "1.0.0"
)

# 设置变量
$SolutionPath = "../ArmorVehicleDamageAssessment.sln"
$ProjectPath = "../src/ArmorVehicleDamageAssessment.UI/ArmorVehicleDamageAssessment.UI.csproj"
$OutputPath = "../publish"
$TempPath = "../temp"

function Write-BuildHeader {
    param([string]$Title)
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Blue
    Write-Host "    $Title" -ForegroundColor Cyan
    Write-Host "================================================================" -ForegroundColor Blue
    Write-Host ""
}

function Test-BuildEnvironment {
    Write-Host "检查构建环境..." -ForegroundColor Yellow
    
    # 检查 .NET SDK
    try {
        $dotnetVersion = dotnet --version
        Write-Host "[✓] .NET SDK 版本: $dotnetVersion" -ForegroundColor Green
    }
    catch {
        Write-Host "[✗] 未找到 .NET SDK" -ForegroundColor Red
        exit 1
    }
    
    # 检查项目文件
    if (-not (Test-Path $ProjectPath)) {
        Write-Host "[✗] 找不到项目文件: $ProjectPath" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "[✓] 构建环境检查通过" -ForegroundColor Green
    Write-Host ""
}

function Clean-BuildDirectories {
    Write-Host "清理构建目录..." -ForegroundColor Yellow
    
    if (Test-Path $OutputPath) {
        Remove-Item $OutputPath -Recurse -Force
        Write-Host "[✓] 清理发布目录" -ForegroundColor Green
    }
    
    if (Test-Path $TempPath) {
        Remove-Item $TempPath -Recurse -Force
        Write-Host "[✓] 清理临时目录" -ForegroundColor Green
    }
    
    # 清理 bin 和 obj 目录
    Get-ChildItem -Path ".." -Recurse -Directory -Name "bin" | ForEach-Object {
        $binPath = Join-Path (Split-Path $_) $_
        if (Test-Path $binPath) {
            Remove-Item $binPath -Recurse -Force
        }
    }
    
    Get-ChildItem -Path ".." -Recurse -Directory -Name "obj" | ForEach-Object {
        $objPath = Join-Path (Split-Path $_) $_
        if (Test-Path $objPath) {
            Remove-Item $objPath -Recurse -Force
        }
    }
    
    Write-Host "[✓] 清理完成" -ForegroundColor Green
    Write-Host ""
}

function Build-Version {
    param(
        [string]$Name,
        [string]$OutputDir,
        [hashtable]$PublishArgs
    )
    
    Write-Host "构建版本: $Name" -ForegroundColor Cyan
    Write-Host "输出目录: $OutputDir" -ForegroundColor Blue
    
    $args = @(
        "publish", $ProjectPath,
        "--configuration", "Release",
        "--output", $OutputDir,
        "--verbosity", "minimal"
    )
    
    foreach ($key in $PublishArgs.Keys) {
        $args += $key
        if ($PublishArgs[$key] -ne $null) {
            $args += $PublishArgs[$key]
        }
    }
    
    Write-Host "执行命令: dotnet $($args -join ' ')" -ForegroundColor Gray
    
    try {
        & dotnet @args
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[✓] $Name 构建成功" -ForegroundColor Green
            
            # 创建版本信息文件
            $versionInfo = @"
版本: $Name
构建时间: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
.NET 版本: $(dotnet --version)
构建参数: $($args -join ' ')
"@
            $versionFile = Join-Path $OutputDir "version.txt"
            Set-Content -Path $versionFile -Value $versionInfo -Encoding UTF8
            
            return $true
        } else {
            Write-Host "[✗] $Name 构建失败，退出代码: $LASTEXITCODE" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "[✗] $Name 构建异常: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Create-ConfigurationVariants {
    param([string]$BaseOutputDir)
    
    Write-Host "创建配置文件变体..." -ForegroundColor Yellow
    
    # 创建带有不同数据库配置的版本
    $configVariants = @(
        @{
            Name = "LocalDB"
            ConnectionString = "Data Source=Data\ArmorVehicleAssessment.db;Cache=Shared;"
            Description = "本地数据库版本"
        },
        @{
            Name = "MemoryDB"
            ConnectionString = "Data Source=:memory:;Cache=Shared;"
            Description = "内存数据库版本(测试用)"
        },
        @{
            Name = "UserDataDB"
            ConnectionString = "Data Source=%LOCALAPPDATA%\ArmorVehicleDamageAssessment\ArmorVehicleAssessment.db;Cache=Shared;"
            Description = "用户数据目录版本"
        }
    )
    
    foreach ($variant in $configVariants) {
        $variantDir = Join-Path $BaseOutputDir $variant.Name
        if (-not (Test-Path $variantDir)) {
            New-Item -Path $variantDir -ItemType Directory -Force | Out-Null
        }
        
        # 复制所有文件
        Copy-Item "$BaseOutputDir\*" $variantDir -Recurse -Force -Exclude @($variant.Name)
        
        # 修改配置文件
        $appSettingsPath = Join-Path $variantDir "appsettings.json"
        if (Test-Path $appSettingsPath) {
            $config = Get-Content $appSettingsPath | ConvertFrom-Json
            $config.ConnectionStrings.DefaultConnection = $variant.ConnectionString
            $config | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
        }
        
        # 创建说明文件
        $readmePath = Join-Path $variantDir "README.txt"
        Set-Content -Path $readmePath -Value $variant.Description -Encoding UTF8
        
        Write-Host "[✓] 创建配置变体: $($variant.Name)" -ForegroundColor Green
    }
}

function Create-StartupScripts {
    param([string]$OutputDir)
    
    Write-Host "创建启动脚本..." -ForegroundColor Yellow
    
    # 创建批处理启动脚本
    $batScript = @"
@echo off
chcp 65001 >nul
title 装甲车辆抗毁伤数字化评估软件平台

echo 正在启动装甲车辆评估软件...
echo.

REM 检查文件是否存在
if not exist "ArmorVehicleDamageAssessment.UI.exe" (
    echo [错误] 找不到主程序文件！
    echo 请确保在正确的目录中运行此脚本。
    pause
    exit /b 1
)

REM 创建日志目录
if not exist "logs" mkdir logs

REM 启动程序
echo [信息] 启动主程序...
start "" "ArmorVehicleDamageAssessment.UI.exe"

REM 等待一下检查是否启动成功
timeout /t 3 /nobreak >nul

echo [完成] 程序启动完成
echo.
echo 如果程序没有显示：
echo 1. 检查杀毒软件是否拦截
echo 2. 以管理员身份运行此脚本
echo 3. 查看 logs 目录中的日志文件
echo 4. 运行 diagnose.bat 进行诊断
echo.
pause
"@
    
    $batPath = Join-Path $OutputDir "start.bat"
    Set-Content -Path $batPath -Value $batScript -Encoding Default
    
    # 创建 PowerShell 启动脚本
    $ps1Script = @"
# 装甲车辆评估软件启动脚本
Set-Location `$PSScriptRoot

Write-Host "正在启动装甲车辆抗毁伤数字化评估软件平台..." -ForegroundColor Green

if (-not (Test-Path "ArmorVehicleDamageAssessment.UI.exe")) {
    Write-Host "[错误] 找不到主程序文件！" -ForegroundColor Red
    exit 1
}

# 创建日志目录
if (-not (Test-Path "logs")) {
    New-Item -Path "logs" -ItemType Directory | Out-Null
}

try {
    Write-Host "[信息] 启动主程序..." -ForegroundColor Blue
    Start-Process -FilePath "ArmorVehicleDamageAssessment.UI.exe" -WorkingDirectory `$PWD
    
    Start-Sleep -Seconds 2
    Write-Host "[完成] 程序启动完成" -ForegroundColor Green
}
catch {
    Write-Host "[错误] 启动失败: `$(`$_.Exception.Message)" -ForegroundColor Red
    Write-Host "请运行 diagnose.ps1 进行详细诊断" -ForegroundColor Yellow
}
"@
    
    $ps1Path = Join-Path $OutputDir "start.ps1"
    Set-Content -Path $ps1Path -Value $ps1Script -Encoding UTF8
    
    # 创建诊断脚本
    $diagScript = Join-Path (Split-Path $MyInvocation.MyCommand.Path) "diagnose-startup.ps1"
    if (Test-Path $diagScript) {
        Copy-Item $diagScript (Join-Path $OutputDir "diagnose.ps1")
    }
    
    Write-Host "[✓] 启动脚本创建完成" -ForegroundColor Green
}

function Create-ArchivePackages {
    param([string]$OutputPath)
    
    Write-Host "创建压缩包..." -ForegroundColor Yellow
    
    $versions = Get-ChildItem $OutputPath -Directory
    foreach ($version in $versions) {
        $archivePath = Join-Path $OutputPath "$($version.Name)-v$Version.zip"
        Compress-Archive -Path "$($version.FullName)\*" -DestinationPath $archivePath -Force
        Write-Host "[✓] 创建压缩包: $($version.Name)-v$Version.zip" -ForegroundColor Green
    }
}

# 主构建流程
Write-BuildHeader "装甲车辆评估软件 - 多版本构建"

# 检查环境
Test-BuildEnvironment

# 清理（如果需要）
if ($CleanFirst) {
    Clean-BuildDirectories
}

# 还原依赖
Write-Host "还原 NuGet 包..." -ForegroundColor Yellow
dotnet restore $SolutionPath

# 构建解决方案
Write-Host "构建解决方案..." -ForegroundColor Yellow
dotnet build $SolutionPath --configuration Release --no-restore

# 运行测试（如果需要）
if (-not $SkipTests) {
    Write-Host "运行测试..." -ForegroundColor Yellow
    dotnet test $SolutionPath --configuration Release --no-build --verbosity minimal
}

# 创建输出目录
if (-not (Test-Path $OutputPath)) {
    New-Item -Path $OutputPath -ItemType Directory -Force | Out-Null
}

# 定义构建配置
$buildConfigs = @(
    @{
        Name = "SelfContained-Win10"
        OutputDir = "$OutputPath\SelfContained-Win10"
        Args = @{
            "--runtime" = "win10-x64"
            "--self-contained" = "true"
            "-p:PublishSingleFile" = "false"
            "-p:PublishTrimmed" = "false"
            "-p:DebugType" = "embedded"
        }
    },
    @{
        Name = "SelfContained-Win7"
        OutputDir = "$OutputPath\SelfContained-Win7"
        Args = @{
            "--runtime" = "win7-x64"
            "--self-contained" = "true"
            "-p:PublishSingleFile" = "false"
            "-p:PublishTrimmed" = "false"
            "-p:TargetFramework" = "net48"
        }
    },
    @{
        Name = "FrameworkDependent"
        OutputDir = "$OutputPath\FrameworkDependent"
        Args = @{
            "--runtime" = "win-x64"
            "--self-contained" = "false"
        }
    },
    @{
        Name = "SingleFile"
        OutputDir = "$OutputPath\SingleFile"
        Args = @{
            "--runtime" = "win10-x64"
            "--self-contained" = "true"
            "-p:PublishSingleFile" = "true"
            "-p:PublishTrimmed" = "false"
        }
    },
    @{
        Name = "Portable"
        OutputDir = "$OutputPath\Portable"
        Args = @{
            "--runtime" = "win-x64"
            "--self-contained" = "false"
            "-p:PublishSingleFile" = "false"
        }
    }
)

# 构建所有版本
$successCount = 0
foreach ($config in $buildConfigs) {
    Write-BuildHeader "构建: $($config.Name)"
    
    if (Build-Version $config.Name $config.OutputDir $config.Args) {
        Create-StartupScripts $config.OutputDir
        Create-ConfigurationVariants $config.OutputDir
        $successCount++
    }
}

# 创建压缩包
if ($successCount -gt 0) {
    Create-ArchivePackages $OutputPath
}

# 构建总结
Write-BuildHeader "构建总结"
Write-Host "成功构建版本数: $successCount / $($buildConfigs.Count)" -ForegroundColor $(if($successCount -eq $buildConfigs.Count){'Green'}else{'Yellow'})
Write-Host "输出目录: $OutputPath" -ForegroundColor Blue

if ($successCount -gt 0) {
    Write-Host ""
    Write-Host "版本说明:" -ForegroundColor Cyan
    Write-Host "• SelfContained-Win10: 适用于 Windows 10/11，包含运行时" -ForegroundColor Gray
    Write-Host "• SelfContained-Win7:  适用于 Windows 7，兼容性版本" -ForegroundColor Gray
    Write-Host "• FrameworkDependent:  需要预装 .NET 8.0，体积小" -ForegroundColor Gray
    Write-Host "• SingleFile:          单文件版本，启动较慢但便于分发" -ForegroundColor Gray
    Write-Host "• Portable:            便携版本，需要 .NET 运行时" -ForegroundColor Gray
    Write-Host ""
    Write-Host "推荐使用顺序:" -ForegroundColor Yellow
    Write-Host "1. SelfContained-Win10 (首选)" -ForegroundColor Green
    Write-Host "2. FrameworkDependent + .NET 8.0" -ForegroundColor Green
    Write-Host "3. SelfContained-Win7 (兼容性)" -ForegroundColor Green
}

Write-Host ""
Write-Host "构建完成！" -ForegroundColor Green
