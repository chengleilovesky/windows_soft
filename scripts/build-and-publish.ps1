# 装甲车辆评估软件 - 构建和发布脚本
# 适用于 Windows 平台

Write-Host "🚀 开始构建装甲车辆抗毁伤数字化评估软件平台..." -ForegroundColor Green

# 设置变量
$SolutionPath = "../ArmorVehicleDamageAssessment.sln"
$ProjectPath = "../src/ArmorVehicleDamageAssessment.UI/ArmorVehicleDamageAssessment.UI.csproj"
$OutputPath = "../publish"
$Version = "1.0.0"

# 清理之前的构建
Write-Host "🧹 清理之前的构建文件..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}

# 还原 NuGet 包
Write-Host "📦 还原 NuGet 包..." -ForegroundColor Blue
dotnet restore $SolutionPath

# 构建解决方案
Write-Host "🔨 构建解决方案..." -ForegroundColor Blue
dotnet build $SolutionPath --configuration Release --no-restore

# 发布应用程序
Write-Host "📋 发布应用程序..." -ForegroundColor Blue

# 自包含发布 (包含 .NET 运行时)
dotnet publish $ProjectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output "$OutputPath/win-x64-self-contained" `
    --verbosity minimal `
    -p:PublishSingleFile=false `
    -p:PublishTrimmed=false

# 依赖框架发布 (需要预装 .NET 8)
dotnet publish $ProjectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained false `
    --output "$OutputPath/win-x64-framework-dependent" `
    --verbosity minimal

# 创建压缩包
Write-Host "📦 创建发布包..." -ForegroundColor Blue
$ZipPath = "$OutputPath/ArmorVehicleDamageAssessment-Windows-v$Version.zip"
Compress-Archive -Path "$OutputPath/win-x64-self-contained/*" -DestinationPath $ZipPath -Force

Write-Host "✅ 构建完成！" -ForegroundColor Green
Write-Host ""
Write-Host "📁 发布文件位置:" -ForegroundColor Cyan
Write-Host "   自包含版本: $OutputPath/win-x64-self-contained/" -ForegroundColor White
Write-Host "   框架依赖版本: $OutputPath/win-x64-framework-dependent/" -ForegroundColor White
Write-Host "   压缩包: $ZipPath" -ForegroundColor White
Write-Host ""
Write-Host "🎯 运行方式:" -ForegroundColor Cyan
Write-Host "   双击 ArmorVehicleDamageAssessment.UI.exe 即可运行图形界面" -ForegroundColor White
Write-Host ""
Write-Host "💡 提示:" -ForegroundColor Yellow
Write-Host "   - 自包含版本可以在没有安装.NET的机器上运行" -ForegroundColor Gray
Write-Host "   - 框架依赖版本体积更小，但需要预装.NET 8运行时" -ForegroundColor Gray
