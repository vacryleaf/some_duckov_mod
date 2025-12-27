@echo off
chcp 65001 >nul

echo ========================================
echo  虫洞科技Mod 编译脚本
echo ========================================

set "PROJECT_DIR=%~dp0WormholeTechMod"
set "OUTPUT_DIR=%~dp0release"
set "STEAM_MODS_DIR=C:\Program Files (x86)\Steam\steamapps\common\Escape from Duckov\Duckov_Data\Mods\WormholeTechMod"

cd /d "%PROJECT_DIR%"

echo [1/3] 正在编译 WormholeTechMod...
dotnet build -c Release

if %ERRORLEVEL% NEQ 0 (
    echo 编译失败！
    exit /b 1
)

echo [2/3] 正在复制文件到 Steam mods 目录...

if not exist "%STEAM_MODS_DIR%" mkdir "%STEAM_MODS_DIR%"

copy /y "%OUTPUT_DIR%\WormholeTechMod.dll" "%STEAM_MODS_DIR%\WormholeTechMod.dll" >nul
if %ERRORLEVEL% NEQ 0 (
    echo 警告：无法复制到 Steam 目录，请以管理员权限运行此脚本
    echo 已编译的文件位置：%OUTPUT_DIR%\WormholeTechMod.dll
    exit /b 1
)

echo [3/3] 编译并部署完成！
echo 文件已复制到：%STEAM_MODS_DIR%

echo ========================================
echo  完成！可以启动游戏测试回溯虫洞功能。
echo ========================================

pause