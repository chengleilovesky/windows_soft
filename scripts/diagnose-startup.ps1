# 装甲车辆评估软件 - 启动问题诊断工具
# 用于排查程序双击无响应的问题

param(
    [string]$AppPath = "",
    [switch]$Verbose
)

# 设置控制台编码
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

function Write-DiagnosticHeader {
    Clear-Host
    Write-Host "================================================================" -ForegroundColor Blue
    Write-Host "         装甲车辆评估软件 - 启动问题诊断工具" -ForegroundColor Cyan
    Write-Host "================================================================" -ForegroundColor Blue
    Write-Host ""
}

function Test-FileExists {
    param([string]$Path, [string]$Description)
    
    if (Test-Path $Path) {
        Write-Host "[✓] $Description 存在: $Path" -ForegroundColor Green
        return $true
    } else {
        Write-Host "[✗] $Description 不存在: $Path" -ForegroundColor Red
        return $false
    }
}

function Test-DotNetFramework {
    Write-Host "检查 .NET 运行时环境..." -ForegroundColor Yellow
    
    try {
        $dotnetVersion = dotnet --version 2>$null
        if ($dotnetVersion) {
            Write-Host "[✓] .NET CLI 版本: $dotnetVersion" -ForegroundColor Green
            
            if ($dotnetVersion.StartsWith("8.")) {
                Write-Host "[✓] .NET 8.0 运行时可用" -ForegroundColor Green
                return $true
            } else {
                Write-Host "[!] 需要 .NET 8.0，当前版本: $dotnetVersion" -ForegroundColor Yellow
                return $false
            }
        }
    }
    catch {
        Write-Host "[✗] 未找到 .NET CLI" -ForegroundColor Red
    }
    
    # 检查通过注册表
    try {
        $netFrameworks = Get-ChildItem "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP" -Recurse |
            Get-ItemProperty -Name Version, Release -ErrorAction SilentlyContinue |
            Where-Object { $_.PSChildName -match '^(?!S)\p{L}' } |
            Select-Object PSChildName, Version, Release
        
        if ($netFrameworks) {
            Write-Host "[✓] 已安装的 .NET Framework 版本:" -ForegroundColor Green
            $netFrameworks | ForEach-Object {
                Write-Host "    $($_.PSChildName): $($_.Version)" -ForegroundColor Gray
            }
        }
    }
    catch {
        Write-Host "[!] 无法检查 .NET Framework 版本" -ForegroundColor Yellow
    }
    
    return $false
}

function Test-SystemEnvironment {
    Write-Host "检查系统环境..." -ForegroundColor Yellow
    
    # 操作系统版本
    $osInfo = Get-WmiObject Win32_OperatingSystem
    Write-Host "[i] 操作系统: $($osInfo.Caption) $($osInfo.Version)" -ForegroundColor Blue
    
    # 系统架构
    $arch = $env:PROCESSOR_ARCHITECTURE
    Write-Host "[i] 系统架构: $arch" -ForegroundColor Blue
    
    # 内存信息
    $totalMemory = [math]::Round($osInfo.TotalVisibleMemorySize / 1MB, 2)
    Write-Host "[i] 可用内存: $totalMemory GB" -ForegroundColor Blue
    
    # 磁盘空间
    $disk = Get-WmiObject Win32_LogicalDisk | Where-Object { $_.DriveType -eq 3 }
    foreach ($drive in $disk) {
        $freeSpace = [math]::Round($drive.FreeSpace / 1GB, 2)
        Write-Host "[i] 磁盘 $($drive.DeviceID) 可用空间: $freeSpace GB" -ForegroundColor Blue
    }
    
    Write-Host ""
}

function Test-ApplicationFiles {
    param([string]$BasePath)
    
    Write-Host "检查应用程序文件..." -ForegroundColor Yellow
    
    $requiredFiles = @(
        @{ Path = "ArmorVehicleDamageAssessment.UI.exe"; Desc = "主程序" },
        @{ Path = "appsettings.json"; Desc = "配置文件" },
        @{ Path = "NLog.config"; Desc = "日志配置" },
        @{ Path = "ArmorVehicleDamageAssessment.Core.dll"; Desc = "核心库" },
        @{ Path = "ArmorVehicleDamageAssessment.Data.dll"; Desc = "数据层" },
        @{ Path = "ArmorVehicleDamageAssessment.Common.dll"; Desc = "公共库" }
    )
    
    $allFilesExist = $true
    
    foreach ($file in $requiredFiles) {
        $fullPath = Join-Path $BasePath $file.Path
        $exists = Test-FileExists $fullPath $file.Desc
        if (-not $exists) { $allFilesExist = $false }
    }
    
    return $allFilesExist
}

function Test-ConfigurationFiles {
    param([string]$BasePath)
    
    Write-Host "检查配置文件内容..." -ForegroundColor Yellow
    
    # 检查 appsettings.json
    $appSettingsPath = Join-Path $BasePath "appsettings.json"
    if (Test-Path $appSettingsPath) {
        try {
            $config = Get-Content $appSettingsPath | ConvertFrom-Json
            Write-Host "[✓] appsettings.json 格式正确" -ForegroundColor Green
            
            if ($config.ConnectionStrings -and $config.ConnectionStrings.DefaultConnection) {
                Write-Host "[✓] 数据库连接字符串已配置" -ForegroundColor Green
            } else {
                Write-Host "[!] 数据库连接字符串未配置或为空" -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "[✗] appsettings.json 格式错误: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    # 检查 NLog.config
    $nlogPath = Join-Path $BasePath "NLog.config"
    if (Test-Path $nlogPath) {
        try {
            [xml]$nlogConfig = Get-Content $nlogPath
            Write-Host "[✓] NLog.config 格式正确" -ForegroundColor Green
        }
        catch {
            Write-Host "[✗] NLog.config 格式错误: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    Write-Host ""
}

function Test-WindowsEventLogs {
    Write-Host "检查 Windows 事件日志..." -ForegroundColor Yellow
    
    $recentEvents = Get-WinEvent -FilterHashtable @{LogName='Application'; Level=2; StartTime=(Get-Date).AddHours(-1)} -MaxEvents 5 -ErrorAction SilentlyContinue
    
    if ($recentEvents) {
        Write-Host "[!] 发现最近的应用程序错误:" -ForegroundColor Yellow
        foreach ($event in $recentEvents) {
            if ($event.ProviderName -like "*ArmorVehicle*" -or $event.Message -like "*ArmorVehicle*") {
                Write-Host "    时间: $($event.TimeCreated)" -ForegroundColor Red
                Write-Host "    消息: $($event.Message.Substring(0, [Math]::Min(100, $event.Message.Length)))..." -ForegroundColor Red
                Write-Host ""
            }
        }
    } else {
        Write-Host "[✓] 未发现相关错误日志" -ForegroundColor Green
    }
    
    Write-Host ""
}

function Test-ProcessPermissions {
    param([string]$ExePath)
    
    Write-Host "检查文件权限..." -ForegroundColor Yellow
    
    try {
        $acl = Get-Acl $ExePath
        $currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
        Write-Host "[i] 当前用户: $currentUser" -ForegroundColor Blue
        Write-Host "[i] 文件所有者: $($acl.Owner)" -ForegroundColor Blue
        
        # 尝试运行程序
        Write-Host "尝试启动程序..." -ForegroundColor Yellow
        $process = Start-Process -FilePath $ExePath -PassThru -WindowStyle Hidden
        Start-Sleep -Seconds 2
        
        if ($process.HasExited) {
            Write-Host "[✗] 程序启动后立即退出，退出代码: $($process.ExitCode)" -ForegroundColor Red
        } else {
            Write-Host "[✓] 程序启动成功，PID: $($process.Id)" -ForegroundColor Green
            $process.Kill()
        }
    }
    catch {
        Write-Host "[✗] 权限检查失败: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Write-Host ""
}

function Test-ApplicationLogs {
    param([string]$BasePath)
    
    Write-Host "检查应用程序日志..." -ForegroundColor Yellow
    
    $logsDir = Join-Path $BasePath "logs"
    if (Test-Path $logsDir) {
        Write-Host "[✓] 日志目录存在: $logsDir" -ForegroundColor Green
        
        # 查找最新的日志文件
        $logFiles = Get-ChildItem $logsDir -Filter "*.log" | Sort-Object LastWriteTime -Descending
        if ($logFiles) {
            $latestLog = $logFiles[0]
            Write-Host "[✓] 最新日志文件: $($latestLog.Name)" -ForegroundColor Green
            
            if ($Verbose -and $latestLog.Length -gt 0) {
                Write-Host "最近的日志内容:" -ForegroundColor Blue
                Get-Content $latestLog.FullName -Tail 10 | ForEach-Object {
                    Write-Host "    $_" -ForegroundColor Gray
                }
            }
        } else {
            Write-Host "[!] 日志目录为空" -ForegroundColor Yellow
        }
    } else {
        Write-Host "[!] 日志目录不存在" -ForegroundColor Yellow
    }
    
    # 检查启动日志
    $startupLog = Join-Path $logsDir "startup.log"
    if (Test-Path $startupLog) {
        Write-Host "[✓] 启动日志存在" -ForegroundColor Green
        if ($Verbose) {
            Write-Host "启动日志内容:" -ForegroundColor Blue
            Get-Content $startupLog | ForEach-Object {
                Write-Host "    $_" -ForegroundColor Gray
            }
        }
    }
    
    # 检查错误日志
    $errorLog = Join-Path $logsDir "error.log"
    if (Test-Path $errorLog) {
        Write-Host "[!] 发现错误日志文件" -ForegroundColor Red
        Write-Host "错误日志内容:" -ForegroundColor Red
        Get-Content $errorLog -Tail 20 | ForEach-Object {
            Write-Host "    $_" -ForegroundColor Red
        }
    }
    
    Write-Host ""
}

function Show-Solutions {
    Write-Host "================================================================" -ForegroundColor Blue
    Write-Host "                    建议解决方案" -ForegroundColor Cyan
    Write-Host "================================================================" -ForegroundColor Blue
    Write-Host ""
    
    Write-Host "1. 安装 .NET 8.0 运行时:" -ForegroundColor Yellow
    Write-Host "   下载地址: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "2. 以管理员身份运行程序:" -ForegroundColor Yellow
    Write-Host "   右键点击 .exe 文件 → 以管理员身份运行" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "3. 检查杀毒软件:" -ForegroundColor Yellow
    Write-Host "   临时关闭杀毒软件或将程序添加到白名单" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "4. 使用自包含版本:" -ForegroundColor Yellow
    Write-Host "   运行构建脚本生成自包含版本，无需安装 .NET" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "5. 检查系统日志:" -ForegroundColor Yellow
    Write-Host "   运行 eventvwr.msc 查看 Windows 事件查看器" -ForegroundColor Cyan
    Write-Host ""
}

# 主诊断流程
Write-DiagnosticHeader

# 自动检测应用程序路径
if (-not $AppPath) {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $publishDirs = @(
        (Join-Path $scriptDir "..\publish\win-x64-self-contained"),
        (Join-Path $scriptDir "..\publish\win-x64-framework-dependent"),
        (Join-Path $scriptDir "..\publish\win-x64")
    )
    
    foreach ($dir in $publishDirs) {
        $exePath = Join-Path $dir "ArmorVehicleDamageAssessment.UI.exe"
        if (Test-Path $exePath) {
            $AppPath = $dir
            break
        }
    }
}

if (-not $AppPath -or -not (Test-Path $AppPath)) {
    Write-Host "[✗] 无法找到应用程序目录" -ForegroundColor Red
    Write-Host "请指定应用程序路径: .\diagnose-startup.ps1 -AppPath 'C:\path\to\app'" -ForegroundColor Yellow
    exit 1
}

$exePath = Join-Path $AppPath "ArmorVehicleDamageAssessment.UI.exe"
Write-Host "[i] 诊断目标: $exePath" -ForegroundColor Blue
Write-Host ""

# 执行诊断步骤
Test-SystemEnvironment
Test-DotNetFramework
Test-ApplicationFiles $AppPath
Test-ConfigurationFiles $AppPath
Test-ProcessPermissions $exePath
Test-ApplicationLogs $AppPath
Test-WindowsEventLogs

# 显示解决方案
Show-Solutions

Write-Host "诊断完成。按任意键退出..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
