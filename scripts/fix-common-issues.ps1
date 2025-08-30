# 装甲车辆评估软件 - 常见问题修复工具
# 自动修复导致程序无法启动的常见配置问题

param(
    [string]$AppPath = "",
    [switch]$Force
)

function Write-FixHeader {
    Clear-Host
    Write-Host "================================================================" -ForegroundColor Blue
    Write-Host "         装甲车辆评估软件 - 常见问题修复工具" -ForegroundColor Cyan
    Write-Host "================================================================" -ForegroundColor Blue
    Write-Host ""
}

function Fix-ConfigurationFiles {
    param([string]$BasePath)
    
    Write-Host "修复配置文件..." -ForegroundColor Yellow
    
    # 修复 appsettings.json
    $appSettingsPath = Join-Path $BasePath "appsettings.json"
    $fixedConfig = $false
    
    if (Test-Path $appSettingsPath) {
        try {
            $config = Get-Content $appSettingsPath | ConvertFrom-Json
            
            # 修复数据库连接字符串
            if (-not $config.ConnectionStrings -or [string]::IsNullOrEmpty($config.ConnectionStrings.DefaultConnection)) {
                if (-not $config.ConnectionStrings) {
                    $config | Add-Member -Type NoteProperty -Name ConnectionStrings -Value @{}
                }
                $config.ConnectionStrings | Add-Member -Type NoteProperty -Name DefaultConnection -Value "Data Source=Data\ArmorVehicleAssessment.db;Cache=Shared;" -Force
                $fixedConfig = $true
                Write-Host "[✓] 修复数据库连接字符串" -ForegroundColor Green
            }
            
            # 确保应用设置存在
            if (-not $config.AppSettings) {
                $config | Add-Member -Type NoteProperty -Name AppSettings -Value @{
                    ApplicationName = "装甲车辆抗毁伤数字化评估软件平台"
                    Version = "1.0.0"
                    Organization = "沈阳理工大学"
                    DefaultPageSize = 20
                    MaxPageSize = 100
                }
                $fixedConfig = $true
                Write-Host "[✓] 添加应用程序设置" -ForegroundColor Green
            }
            
            # 确保日志配置存在
            if (-not $config.Logging) {
                $config | Add-Member -Type NoteProperty -Name Logging -Value @{
                    LogLevel = @{
                        Default = "Information"
                        "ArmorVehicleDamageAssessment" = "Debug"
                        "Microsoft.EntityFrameworkCore" = "Warning"
                    }
                }
                $fixedConfig = $true
                Write-Host "[✓] 添加日志配置" -ForegroundColor Green
            }
            
            if ($fixedConfig) {
                # 备份原文件
                $backupPath = "$appSettingsPath.backup"
                Copy-Item $appSettingsPath $backupPath -Force
                
                # 保存修复后的配置
                $config | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
                Write-Host "[✓] appsettings.json 修复完成" -ForegroundColor Green
            } else {
                Write-Host "[✓] appsettings.json 无需修复" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "[✗] appsettings.json 修复失败: $($_.Exception.Message)" -ForegroundColor Red
            
            # 创建新的配置文件
            $newConfig = @{
                ConnectionStrings = @{
                    DefaultConnection = "Data Source=Data\ArmorVehicleAssessment.db;Cache=Shared;"
                }
                Logging = @{
                    LogLevel = @{
                        Default = "Information"
                        "ArmorVehicleDamageAssessment" = "Debug"
                        "Microsoft.EntityFrameworkCore" = "Warning"
                    }
                }
                AppSettings = @{
                    ApplicationName = "装甲车辆抗毁伤数字化评估软件平台"
                    Version = "1.0.0"
                    Organization = "沈阳理工大学"
                    DefaultPageSize = 20
                    MaxPageSize = 100
                }
            }
            
            $newConfig | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
            Write-Host "[✓] 重新创建 appsettings.json" -ForegroundColor Green
        }
    } else {
        Write-Host "[!] appsettings.json 不存在，创建新文件..." -ForegroundColor Yellow
        
        $newConfig = @{
            ConnectionStrings = @{
                DefaultConnection = "Data Source=Data\ArmorVehicleAssessment.db;Cache=Shared;"
            }
            Logging = @{
                LogLevel = @{
                    Default = "Information"
                    "ArmorVehicleDamageAssessment" = "Debug"
                    "Microsoft.EntityFrameworkCore" = "Warning"
                }
            }
            AppSettings = @{
                ApplicationName = "装甲车辆抗毁伤数字化评估软件平台"
                Version = "1.0.0"
                Organization = "沈阳理工大学"
                DefaultPageSize = 20
                MaxPageSize = 100
            }
        }
        
        $newConfig | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
        Write-Host "[✓] 创建 appsettings.json" -ForegroundColor Green
    }
    
    # 检查 NLog.config
    $nlogPath = Join-Path $BasePath "NLog.config"
    if (-not (Test-Path $nlogPath)) {
        Write-Host "[!] NLog.config 不存在，创建新文件..." -ForegroundColor Yellow
        
        $nlogConfig = @'
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <targets>
    <!-- 文件日志 -->
    <target xsi:type="File" name="logfile" 
            fileName="logs/${shortdate}.log"
            layout="${longdate} ${level:uppercase=true:padding=-5} ${logger:shortName=true} ${message} ${exception:format=tostring}"
            archiveFileName="logs/archived/log.{#}.txt"
            archiveEvery="Day"
            archiveNumbering="Rolling"
            maxArchiveFiles="30" />
    
    <!-- 控制台日志 -->
    <target xsi:type="Console" name="console"
            layout="${time} ${level:uppercase=true:padding=-5} ${logger:shortName=true} ${message} ${exception:format=tostring}" />
  </targets>

  <rules>
    <!-- 记录所有信息级别及以上的日志到文件 -->
    <logger name="*" minlevel="Info" writeTo="logfile" />
    
    <!-- Debug模式下输出到控制台 -->
    <logger name="*" minlevel="Debug" writeTo="console" />
    
    <!-- 过滤噪音日志 -->
    <logger name="Microsoft.EntityFrameworkCore.*" maxlevel="Warn" final="true" />
    <logger name="Microsoft.Extensions.*" maxlevel="Warn" final="true" />
    <logger name="System.Net.Http.*" maxlevel="Warn" final="true" />
  </rules>
</nlog>
'@
        
        Set-Content -Path $nlogPath -Value $nlogConfig -Encoding UTF8
        Write-Host "[✓] 创建 NLog.config" -ForegroundColor Green
    }
}

function Fix-DirectoryStructure {
    param([string]$BasePath)
    
    Write-Host "修复目录结构..." -ForegroundColor Yellow
    
    $directories = @("logs", "Data", "temp", "backup")
    
    foreach ($dir in $directories) {
        $dirPath = Join-Path $BasePath $dir
        if (-not (Test-Path $dirPath)) {
            New-Item -Path $dirPath -ItemType Directory -Force | Out-Null
            Write-Host "[✓] 创建目录: $dir" -ForegroundColor Green
        }
    }
}

function Fix-FilePermissions {
    param([string]$BasePath)
    
    Write-Host "修复文件权限..." -ForegroundColor Yellow
    
    try {
        # 获取当前用户
        $currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
        
        # 设置目录权限
        $acl = Get-Acl $BasePath
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($currentUser, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
        $acl.SetAccessRule($accessRule)
        Set-Acl -Path $BasePath -AclObject $acl
        
        Write-Host "[✓] 文件权限修复完成" -ForegroundColor Green
    }
    catch {
        Write-Host "[!] 权限修复失败: $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "    请尝试以管理员身份运行此脚本" -ForegroundColor Yellow
    }
}

function Fix-Dependencies {
    param([string]$BasePath)
    
    Write-Host "检查依赖文件..." -ForegroundColor Yellow
    
    $requiredDlls = @(
        "ArmorVehicleDamageAssessment.Core.dll",
        "ArmorVehicleDamageAssessment.Data.dll",
        "ArmorVehicleDamageAssessment.Common.dll",
        "Microsoft.EntityFrameworkCore.dll",
        "Microsoft.EntityFrameworkCore.Sqlite.dll",
        "Microsoft.Extensions.DependencyInjection.dll",
        "Microsoft.Extensions.Hosting.dll",
        "MaterialDesignThemes.Wpf.dll"
    )
    
    $missingDlls = @()
    foreach ($dll in $requiredDlls) {
        $dllPath = Join-Path $BasePath $dll
        if (-not (Test-Path $dllPath)) {
            $missingDlls += $dll
        }
    }
    
    if ($missingDlls.Count -gt 0) {
        Write-Host "[!] 缺少以下依赖文件:" -ForegroundColor Red
        $missingDlls | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
        Write-Host "建议重新构建发布版本" -ForegroundColor Yellow
        return $false
    } else {
        Write-Host "[✓] 所有依赖文件完整" -ForegroundColor Green
        return $true
    }
}

function Test-ApplicationStart {
    param([string]$ExePath)
    
    Write-Host "测试应用程序启动..." -ForegroundColor Yellow
    
    try {
        $process = Start-Process -FilePath $ExePath -PassThru -WindowStyle Hidden
        Start-Sleep -Seconds 3
        
        if ($process.HasExited) {
            Write-Host "[!] 程序启动后立即退出，退出代码: $($process.ExitCode)" -ForegroundColor Yellow
            
            # 检查日志文件
            $logsDir = Join-Path (Split-Path $ExePath) "logs"
            if (Test-Path $logsDir) {
                $latestLog = Get-ChildItem $logsDir -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
                if ($latestLog) {
                    Write-Host "最新日志内容:" -ForegroundColor Blue
                    Get-Content $latestLog.FullName -Tail 5 | ForEach-Object {
                        Write-Host "    $_" -ForegroundColor Gray
                    }
                }
            }
            
            return $false
        } else {
            Write-Host "[✓] 程序启动成功，PID: $($process.Id)" -ForegroundColor Green
            $process.Kill()
            return $true
        }
    }
    catch {
        Write-Host "[✗] 启动测试失败: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Create-TroubleshootingReport {
    param([string]$BasePath)
    
    Write-Host "生成故障排除报告..." -ForegroundColor Yellow
    
    $reportPath = Join-Path $BasePath "troubleshooting-report.txt"
    $report = @"
装甲车辆评估软件 - 故障排除报告
生成时间: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
================================================

系统信息:
- 操作系统: $((Get-WmiObject Win32_OperatingSystem).Caption)
- 系统版本: $((Get-WmiObject Win32_OperatingSystem).Version)
- 系统架构: $env:PROCESSOR_ARCHITECTURE
- .NET 版本: $(try { dotnet --version } catch { "未安装" })

文件检查:
- 主程序: $(if (Test-Path (Join-Path $BasePath "ArmorVehicleDamageAssessment.UI.exe")) { "存在" } else { "缺失" })
- 配置文件: $(if (Test-Path (Join-Path $BasePath "appsettings.json")) { "存在" } else { "缺失" })
- 日志配置: $(if (Test-Path (Join-Path $BasePath "NLog.config")) { "存在" } else { "缺失" })

常见解决方案:
1. 以管理员身份运行程序
2. 安装 .NET 8.0 运行时
3. 关闭杀毒软件或添加白名单
4. 检查 Windows 事件查看器
5. 使用自包含版本

如需更多帮助，请联系技术支持。
"@
    
    Set-Content -Path $reportPath -Value $report -Encoding UTF8
    Write-Host "[✓] 故障排除报告已生成: $reportPath" -ForegroundColor Green
}

# 主修复流程
Write-FixHeader

# 检测应用程序路径
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
    Write-Host "请指定应用程序路径: .\fix-common-issues.ps1 -AppPath 'C:\path\to\app'" -ForegroundColor Yellow
    exit 1
}

$exePath = Join-Path $AppPath "ArmorVehicleDamageAssessment.UI.exe"
Write-Host "[i] 修复目标: $exePath" -ForegroundColor Blue
Write-Host ""

# 执行修复步骤
Fix-ConfigurationFiles $AppPath
Fix-DirectoryStructure $AppPath
Fix-FilePermissions $AppPath

$dependenciesOk = Fix-Dependencies $AppPath
if (-not $dependenciesOk) {
    Write-Host "[!] 依赖文件不完整，建议重新构建" -ForegroundColor Yellow
}

# 测试启动
Write-Host ""
if (Test-ApplicationStart $exePath) {
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host "                    修复成功！" -ForegroundColor Green
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host "程序现在应该可以正常启动了。" -ForegroundColor Green
} else {
    Write-Host "================================================================" -ForegroundColor Yellow
    Write-Host "                需要进一步诊断" -ForegroundColor Yellow
    Write-Host "================================================================" -ForegroundColor Yellow
    Write-Host "程序仍无法正常启动，请运行诊断工具：" -ForegroundColor Yellow
    Write-Host ".\diagnose-startup.ps1 -AppPath '$AppPath'" -ForegroundColor Cyan
}

# 生成报告
Create-TroubleshootingReport $AppPath

Write-Host ""
Write-Host "修复完成。按任意键退出..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
