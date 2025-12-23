# 名刀月影 Mod - Unity 完整配置指南

> 适合完全不懂 Unity 的用户，从零开始

---

## 第一步：安装 Unity

### 1.1 下载 Unity Hub
1. 打开浏览器，访问 https://unity.com/download
2. 点击 "Download Unity Hub"
3. 下载完成后，双击安装 Unity Hub

### 1.2 安装 Unity Editor
1. 打开 Unity Hub
2. 点击左侧 "Installs"（安装）
3. 点击右上角 "Install Editor"（安装编辑器）
4. **重要**：选择版本 **2021.3.x LTS**（与游戏版本匹配）
   - 如果找不到，点击 "Archive"（归档）找旧版本
5. 勾选以下模块：
   - ✅ Windows Build Support (如果你用Windows)
   - ✅ Mac Build Support (如果你用Mac)
6. 点击 "Install" 开始安装（约需 5-10GB 空间）

---

## 第二步：创建 Unity 项目

### 2.1 新建项目
1. 打开 Unity Hub
2. 点击左侧 "Projects"（项目）
3. 点击右上角 "New project"（新建项目）
4. 选择模板：**3D (Built-in Render Pipeline)**
5. 设置：
   - Project name: `MoonlightSwordMod`
   - Location: 选择你喜欢的位置
6. 点击 "Create project"（创建项目）
7. 等待 Unity 编辑器打开（首次可能需要几分钟）

---

## 第三步：设置项目结构

### 3.1 创建文件夹
在 Unity 编辑器的 **Project** 窗口（通常在底部）：

1. 右键 Assets 文件夹
2. 选择 Create > Folder
3. 创建以下文件夹结构：
```
Assets/
├── Editor/           <- 放编辑器脚本
├── Prefabs/          <- 放生成的预制体
├── Materials/        <- 放材质文件
├── Plugins/          <- 放游戏DLL
└── AssetBundles/     <- 放导出的包
```

### 3.2 导入游戏 DLL
1. 找到游戏的 Managed 文件夹：
   - Mac: `/Users/huangjinhui/workspace/duckov/Managed/`
   - Windows: `<游戏目录>/Duckov_Data/Managed/`

2. 复制以下 DLL 文件到 Unity 项目的 `Assets/Plugins/` 文件夹：
   - `ItemStatsSystem.dll`
   - `TeamSoda.Duckov.Core.dll`
   - `TeamSoda.Duckov.Utilities.dll`

3. 回到 Unity，它会自动检测并导入

---

## 第四步：导入编辑器脚本

### 4.1 复制脚本文件
将以下文件复制到 Unity 项目的 `Assets/Editor/` 文件夹：

**源文件位置：**
```
/Users/huangjinhui/workspace/duckov/some_duckov_mod/MoonlightSwordMod/Unity/Editor/
```

**需要复制的文件：**
- `MoonlightSwordModelGenerator.cs`
- `SwordAuraEffectGenerator.cs`

### 4.2 等待编译
1. 回到 Unity 编辑器
2. Unity 会自动编译脚本
3. 如果底部状态栏显示编译进度，等待完成
4. 如果有红色错误，查看 Console 窗口（Window > General > Console）

---

## 第五步：生成刀模型

### 5.1 打开模型生成器
1. 在 Unity 顶部菜单栏点击：**Tools > 名刀月影 > 模型生成器**
2. 会弹出一个新窗口

### 5.2 配置参数（可保持默认）
```
模型尺寸：
- 刀身长度: 1.2
- 刀身宽度: 0.08
- 刀身厚度: 0.02
- 刀柄长度: 0.2

材质颜色：
- 刀身颜色: 银白色 (默认)
- 发光颜色: 淡蓝色 (默认)
```

### 5.3 生成模型
1. 点击 **"生成刀模型"** 按钮
2. 场景中会出现一把刀
3. 点击 **"保存为Prefab"** 按钮
4. 在弹出的对话框中：
   - 导航到 `Assets/Prefabs/`
   - 文件名输入 `MoonlightSword`
   - 点击保存

---

## 第六步：生成剑气特效

### 6.1 打开特效生成器
1. 在 Unity 顶部菜单栏点击：**Tools > 名刀月影 > 剑气特效生成器**

### 6.2 配置参数（可保持默认）
```
剑气尺寸：
- 宽度: 2
- 高度: 1

剑气外观：
- 颜色: 淡蓝色 (默认)
- 发光强度: 2
```

### 6.3 生成特效
1. 点击 **"生成剑气特效"** 按钮
2. 点击 **"保存为Prefab"** 按钮
3. 保存到 `Assets/Prefabs/SwordAuraPrefab.prefab`

---

## 第七步：配置 Item 组件（关键步骤）

### 7.1 选择武器 Prefab
1. 在 Project 窗口中，双击 `Assets/Prefabs/MoonlightSword.prefab`
2. 进入 Prefab 编辑模式

### 7.2 添加 Item 组件
1. 在右侧 **Inspector** 窗口（属性面板）
2. 点击 **"Add Component"**（添加组件）
3. 搜索并添加 **"Item"**
   - 如果找不到，说明 DLL 导入有问题

### 7.3 配置 Item 属性
在 Item 组件中设置以下值：

| 属性 | 值 | 说明 |
|------|-----|------|
| Type ID | `10001` | **重要！唯一ID** |
| Display Name | `MoonlightSword` | 本地化键 |
| Max Stack Count | `1` | 不可堆叠 |
| Value | `50000` | 售价 |
| Quality | `5` | 品质等级 |
| Display Quality | `Legendary` | 选择传说 |
| Weight | `1.2` | 重量 |

### 7.4 保存 Prefab
1. 按 `Ctrl+S`（Windows）或 `Cmd+S`（Mac）
2. 或点击 Prefab 编辑器顶部的 "Save"

---

## 第八步：创建 AssetBundle 导出脚本

### 8.1 创建打包脚本
1. 在 `Assets/Editor/` 文件夹右键
2. Create > C# Script
3. 命名为 `AssetBundleBuilder`
4. 双击打开，替换全部内容为：

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleBuilder
{
    [MenuItem("Tools/名刀月影/构建 AssetBundle")]
    public static void BuildAssetBundles()
    {
        // 设置 Prefab 的 AssetBundle 名称
        string swordPath = "Assets/Prefabs/MoonlightSword.prefab";
        string auraPath = "Assets/Prefabs/SwordAuraPrefab.prefab";

        if (File.Exists(swordPath))
        {
            AssetImporter.GetAtPath(swordPath).assetBundleName = "moonlight_sword";
        }
        if (File.Exists(auraPath))
        {
            AssetImporter.GetAtPath(auraPath).assetBundleName = "moonlight_sword";
        }

        // 创建输出目录
        string outputPath = "Assets/AssetBundles";
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        // 构建 AssetBundle
        BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget
        );

        AssetDatabase.Refresh();

        Debug.Log("AssetBundle 构建完成！输出位置: " + outputPath);
        EditorUtility.DisplayDialog("成功", "AssetBundle 已导出到:\n" + outputPath, "确定");
    }
}
```

5. 保存文件（Ctrl+S）

---

## 第九步：导出 AssetBundle

### 9.1 构建 AssetBundle
1. 在 Unity 顶部菜单栏点击：**Tools > 名刀月影 > 构建 AssetBundle**
2. 等待构建完成
3. 会弹出成功对话框

### 9.2 找到导出文件
导出的文件在：
```
<Unity项目>/Assets/AssetBundles/moonlight_sword
```

（注意：没有文件扩展名）

---

## 第十步：部署到 Mod

### 10.1 复制 AssetBundle
将 `moonlight_sword` 文件复制到：
```
/Users/huangjinhui/workspace/duckov/some_duckov_mod/MoonlightSwordMod/Release/MoonlightSwordMod/Assets/
```

### 10.2 最终 Mod 文件夹结构
```
MoonlightSwordMod/
├── MoonlightSwordMod.dll    <- 已编译的mod代码
├── info.ini                  <- mod配置
├── preview.png               <- 预览图(可选)
└── Assets/
    └── moonlight_sword       <- Unity导出的AssetBundle
```

### 10.3 安装到游戏
将整个 `MoonlightSwordMod` 文件夹复制到：
- **Mac**: `Duckov.app/Contents/Mods/`
- **Windows**: `<游戏目录>/Duckov_Data/Mods/`

---

## 常见问题

### Q: Unity 报错找不到 Item 组件？
A: 确保正确导入了 `ItemStatsSystem.dll` 到 `Assets/Plugins/`

### Q: AssetBundle 构建失败？
A: 检查 Console 窗口的错误信息，通常是 Prefab 没保存

### Q: 游戏中看不到武器？
A: 检查 TypeID 是否设置为 10001，以及 info.ini 中的 name 是否正确

### Q: 武器没有属性？
A: Item 组件的属性必须在 Unity Inspector 中配置

---

## 快速命令总结

```
1. 安装 Unity 2021.3.x LTS
2. 新建 3D 项目
3. 复制 DLL 到 Assets/Plugins/
4. 复制编辑器脚本到 Assets/Editor/
5. Tools > 名刀月影 > 模型生成器 > 生成 > 保存
6. Tools > 名刀月影 > 剑气特效生成器 > 生成 > 保存
7. 配置 Item 组件 (TypeID=10001)
8. Tools > 名刀月影 > 构建 AssetBundle
9. 复制 moonlight_sword 到 mod 的 Assets 文件夹
10. 安装 mod 到游戏
```

---

如有问题，检查 Unity Console 窗口（Window > General > Console）的错误信息。
