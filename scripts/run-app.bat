@echo off
chcp 65001 >nul
title 装甲车辆抗毁伤数字化评估软件平台 - 启动器

echo.
echo ================================================================
echo              装甲车辆抗毁伤数字化评估软件平台
echo                    沈阳理工大学 v1.0.0
echo ================================================================
echo.

REM 设置变量
set APP_NAME=ArmorVehicleDamageAssessment.UI.exe
set PUBLISH_DIR=%~dp0..\publish
set SELF_CONTAINED_DIR=%PUBLISH_DIR%\win-x64-self-contained
set FRAMEWORK_DEPENDENT_DIR=%PUBLISH_DIR%\win-x64-framework-dependent

echo [INFO] 正在检查发布文件...

REM 首先尝试自包含版本
if exist "%SELF_CONTAINED_DIR%\%APP_NAME%" (
    echo [✓] 找到自包含版本: %SELF_CONTAINED_DIR%
    echo [INFO] 启动程序...
    echo.
    
    cd /d "%SELF_CONTAINED_DIR%"
    start "" "%APP_NAME%"
    
    echo [✓] 程序启动完成！
    echo [INFO] 如果程序没有显示，请检查：
    echo         1. 杀毒软件是否拦截
    echo         2. Windows事件查看器中的错误信息
    echo         3. logs目录中的日志文件
    echo.
    pause
    exit /b 0
)

REM 尝试框架依赖版本
if exist "%FRAMEWORK_DEPENDENT_DIR%\%APP_NAME%" (
    echo [✓] 找到框架依赖版本: %FRAMEWORK_DEPENDENT_DIR%
    echo [WARNING] 此版本需要安装 .NET 8.0 运行时
    echo [INFO] 启动程序...
    echo.
    
    cd /d "%FRAMEWORK_DEPENDENT_DIR%"
    start "" "%APP_NAME%"
    
    echo [✓] 程序启动完成！
    echo [INFO] 如果程序无法启动，请：
    echo         1. 下载安装 .NET 8.0 运行时
    echo         2. 网址: https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 0
)

REM 都没找到，显示错误信息
echo [✗] 错误：找不到可执行文件！
echo.
echo [INFO] 请确认以下路径中存在程序文件：
echo         1. %SELF_CONTAINED_DIR%\%APP_NAME%
echo         2. %FRAMEWORK_DEPENDENT_DIR%\%APP_NAME%
echo.
echo [HELP] 解决方法：
echo         1. 运行构建脚本重新生成发布文件
echo         2. 从压缩包中解压程序文件
echo         3. 检查文件路径是否正确
echo.
pause
exit /b 1
