@echo off
echo ====================================
echo YuGi 桌面宠物 - 编译并运行
echo ====================================
echo.

echo [1/2] 正在编译项目...
dotnet build
if %errorlevel% neq 0 (
    echo.
    echo [错误] 编译失败！
    echo 请检查是否已安装 .NET 8.0 SDK
    echo 下载地址: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo.
echo [2/2] 正在启动 YuGi 桌面宠物...
echo 提示: 程序将最小化到系统托盘，请查看任务栏右下角
echo.
dotnet run

pause
