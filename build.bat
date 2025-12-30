@echo off
:: Fix encoding to UTF-8 to display Chinese characters correctly
chcp 65001 >nul
cls
echo ==========================================
echo       MusicBee 插件编译脚本
echo ==========================================
echo.

:: 1. Try to find csc.exe (Standard .NET Framework Compiler)
:: This is usually present on all Windows machines
if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" (
    set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
    goto CompileCSC
)

if exist "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" (
    set CSC="C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
    goto CompileCSC
)

echo [错误] 未找到 csc.exe 编译器。
echo 请确认您的系统已安装 .NET Framework 4.0 或更高版本。
pause
exit /b 1

:CompileCSC
echo [信息] 使用编译器: %CSC%
echo [信息] 正在编译...
echo.

:: Compile command
%csc% /target:library /out:mb_MusicBeeQuickTag.dll /debug:full /optimize- /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.dll /r:System.Core.dll Plugin.cs MusicBeeInterface.cs TagBrowserForm.cs SettingsForm.cs Localization.cs PresetManager.cs

if %errorlevel% neq 0 (
    echo.
    echo [失败] 编译过程中出现错误。
    pause
    exit /b %errorlevel%
)

echo.
echo ==========================================
echo [成功] 编译完成！
echo 插件文件已生成: %CD%\mb_MusicBeeQuickTag.dll
echo ==========================================
echo.
echo 请将 MusicBeeQuickTag.dll 复制到 MusicBee 的 Plugins 文件夹中。
pause
