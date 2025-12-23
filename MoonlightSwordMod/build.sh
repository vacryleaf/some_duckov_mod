#!/bin/bash

# 名刀月影 - 自动构建脚本
# 用于编译和部署Mod

set -e  # 遇到错误立即退出

echo "========================================="
echo "  名刀月影 Mod - 自动构建脚本"
echo "========================================="
echo ""

# 颜色定义
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# 项目路径
PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_FILE="$PROJECT_DIR/MoonlightSwordMod.csproj"

# 检查.NET SDK
echo -e "${YELLOW}[1/5]${NC} 检查.NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}错误: 未找到.NET SDK${NC}"
    echo "请从 https://dotnet.microsoft.com/download 下载安装"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✓${NC} 找到.NET SDK版本: $DOTNET_VERSION"
echo ""

# 清理旧文件
echo -e "${YELLOW}[2/5]${NC} 清理旧构建文件..."
dotnet clean "$PROJECT_FILE" --configuration Release > /dev/null 2>&1 || true
echo -e "${GREEN}✓${NC} 清理完成"
echo ""

# 编译项目
echo -e "${YELLOW}[3/5]${NC} 编译Mod..."
if dotnet build "$PROJECT_FILE" --configuration Release --no-incremental; then
    echo -e "${GREEN}✓${NC} 编译成功"
else
    echo -e "${RED}✗${NC} 编译失败"
    exit 1
fi
echo ""

# 检查输出文件
OUTPUT_DLL="$PROJECT_DIR/bin/Release/MoonlightSwordMod.dll"
if [ ! -f "$OUTPUT_DLL" ]; then
    echo -e "${RED}错误: 未找到输出文件${NC}"
    echo "路径: $OUTPUT_DLL"
    exit 1
fi

FILE_SIZE=$(du -h "$OUTPUT_DLL" | cut -f1)
echo -e "${GREEN}✓${NC} DLL大小: $FILE_SIZE"
echo ""

# 检查游戏安装
echo -e "${YELLOW}[4/5]${NC} 检查游戏安装..."

# 尝试多个可能的游戏路径
GAME_PATHS=(
    "/Users/$USER/Library/Application Support/Steam/steamapps/common/Duckov"
    "/Applications/Duckov.app/Contents"
    "$HOME/Games/Duckov"
)

GAME_PATH=""
for path in "${GAME_PATHS[@]}"; do
    if [ -d "$path" ]; then
        GAME_PATH="$path"
        break
    fi
done

if [ -z "$GAME_PATH" ]; then
    echo -e "${YELLOW}⚠${NC} 未找到游戏安装目录"
    echo "请手动复制以下文件到游戏Mods目录:"
    echo "  - $OUTPUT_DLL"
    echo "  - $PROJECT_DIR/info.ini"
    echo ""
    echo "目标目录: <游戏路径>/Duckov_Data/Mods/MoonlightSwordMod/"
    exit 0
fi

echo -e "${GREEN}✓${NC} 找到游戏: $GAME_PATH"
echo ""

# 部署Mod
echo -e "${YELLOW}[5/5]${NC} 部署Mod..."

# 确定Mod目录
if [ -d "$GAME_PATH/Duckov_Data" ]; then
    MOD_DIR="$GAME_PATH/Duckov_Data/Mods/MoonlightSwordMod"
else
    MOD_DIR="$GAME_PATH/Mods/MoonlightSwordMod"
fi

# 创建Mod目录
mkdir -p "$MOD_DIR"
mkdir -p "$MOD_DIR/Assets"

# 复制文件
echo "复制 MoonlightSwordMod.dll..."
cp "$OUTPUT_DLL" "$MOD_DIR/"

echo "复制 info.ini..."
cp "$PROJECT_DIR/info.ini" "$MOD_DIR/"

# 复制AssetBundle (如果存在)
ASSETBUNDLE="$PROJECT_DIR/Assets/moonlight_sword"
if [ -f "$ASSETBUNDLE" ]; then
    echo "复制 AssetBundle..."
    cp "$ASSETBUNDLE" "$MOD_DIR/Assets/"
    echo -e "${GREEN}✓${NC} AssetBundle已复制"
else
    echo -e "${YELLOW}⚠${NC} AssetBundle未找到: $ASSETBUNDLE"
    echo "   请先在Unity中生成AssetBundle"
fi

# 复制预览图 (如果存在)
if [ -f "$PROJECT_DIR/preview.png" ]; then
    echo "复制 preview.png..."
    cp "$PROJECT_DIR/preview.png" "$MOD_DIR/"
fi

echo ""
echo -e "${GREEN}✓${NC} Mod已部署到: $MOD_DIR"
echo ""

# 显示部署的文件
echo "已部署的文件:"
ls -lh "$MOD_DIR" | tail -n +2 | awk '{print "  " $9 " (" $5 ")"}'
echo ""

# 完成
echo "========================================="
echo -e "${GREEN}构建和部署完成！${NC}"
echo "========================================="
echo ""
echo "下一步:"
echo "1. 启动《逃离鸭科夫》游戏"
echo "2. 进入Mods菜单"
echo "3. 启用'名刀月影'Mod"
echo "4. 重启游戏并测试"
echo ""
echo "生成武器命令: Item.Instantiate(10001)"
echo ""
