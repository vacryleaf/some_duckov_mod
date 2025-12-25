# 10-Unity项目配置指南

## 安装Unity

### 1. 下载Unity Hub

访问 https://unity.com/download 下载 Unity Hub

### 2. 安装Unity Editor

1. 打开 Unity Hub
2. 点击 **Installs → Install Editor**
3. **重要**: 选择版本 **2022.3.x LTS**（与游戏版本匹配）
4. 勾选平台支持：
   - ✅ Windows Build Support（如果你用Windows）
   - ✅ Mac Build Support（如果你用Mac）

---

## 创建Unity项目

### 1. 新建项目

1. 打开 Unity Hub
2. 点击 **Projects → New project**
3. 选择模板：**3D (Built-in Render Pipeline)**
4. Project name: `MyModUnity`
5. 点击 **Create project**

### 2. 创建文件夹结构

在 Unity 编辑器的 Project 窗口中，右键 Assets 创建以下文件夹：

```
Assets/
├── Editor/           ← 编辑器脚本（仅在Unity编辑器中运行）
├── Prefabs/          ← 物品预制体
├── Materials/        ← 材质文件
├── Textures/         ← 贴图文件
├── Icons/            ← 物品图标
├── Models/           ← 3D模型
├── Plugins/          ← 游戏DLL引用
└── AssetBundles/     ← 导出的AssetBundle
```

---

## 导入游戏DLL

### 1. 找到游戏Managed目录

| 平台 | 路径 |
|------|------|
| Windows | `<游戏目录>\Duckov_Data\Managed\` |
| macOS | `Duckov.app/Contents/Managed/` |

### 2. 复制必需的DLL到Unity项目

将以下DLL复制到 `Assets/Plugins/` 目录：

| DLL文件 | 用途 | 必需 |
|---------|------|------|
| ItemStatsSystem.dll | 物品系统核心 | ✅ |
| TeamSoda.Duckov.Core.dll | 游戏核心类 | ✅ |
| TeamSoda.Duckov.Utilities.dll | 工具类 | ✅ |
| Assembly-CSharp.dll | 游戏主程序集 | 可选 |

### 3. 等待Unity导入

Unity会自动检测并编译这些DLL，可能需要几秒钟。

---

## 创建AssetBundle打包脚本

在 `Assets/Editor/` 目录创建 `AssetBundleBuilder.cs`：

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleBuilder
{
    [MenuItem("Build/Build AssetBundles (Windows)")]
    public static void BuildAssetBundlesWindows()
    {
        BuildAssetBundles(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Build/Build AssetBundles (macOS)")]
    public static void BuildAssetBundlesMac()
    {
        BuildAssetBundles(BuildTarget.StandaloneOSX);
    }

    [MenuItem("Build/Build AssetBundles (Current Platform)")]
    public static void BuildAssetBundlesCurrent()
    {
        BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget);
    }

    private static void BuildAssetBundles(BuildTarget target)
    {
        // 创建输出目录
        string outputPath = "Assets/AssetBundles";
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        // 构建AssetBundle
        BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.None,
            target
        );

        AssetDatabase.Refresh();

        Debug.Log($"AssetBundle构建完成！平台: {target}, 输出: {outputPath}");
        EditorUtility.DisplayDialog("构建完成",
            $"AssetBundle已导出到:\n{outputPath}", "确定");
    }
}
```

---

## 项目设置

### 1. Player Settings

菜单 **Edit → Project Settings → Player**

```
Company Name: YourName
Product Name: MyMod
```

### 2. 脚本编译符号（可选）

如果需要条件编译：

**Edit → Project Settings → Player → Other Settings → Scripting Define Symbols**

添加：`MOD_DEBUG`

---

## Unity项目与C#项目的关系

```
MyMod/
├── MyMod.csproj              ← C#项目（编译DLL）
├── Scripts/
│   └── ModBehaviour.cs       ← Mod代码
├── Unity/                    ← Unity项目（制作Prefab和AssetBundle）
│   ├── Assets/
│   │   ├── Editor/
│   │   ├── Prefabs/
│   │   └── Plugins/
│   └── ProjectSettings/
└── Release/                  ← 最终发布目录
    └── MyMod/
        ├── MyMod.dll
        ├── info.ini
        └── Assets/
            └── mymod_assets
```

### 工作流程

1. **在C#项目中**：编写Mod逻辑代码，编译生成DLL
2. **在Unity项目中**：制作Prefab、图标、模型，打包AssetBundle
3. **合并到Release**：将DLL和AssetBundle放到同一目录

---

## 常见问题

### DLL导入后报错

**原因**：Unity版本与游戏不匹配

**解决**：确保使用 Unity 2022.3.x

### 找不到ItemStatsSystem命名空间

**原因**：DLL未正确导入

**解决**：
1. 检查DLL是否在 `Assets/Plugins/` 目录
2. 等待Unity编译完成
3. 重启Unity

### 脚本无法挂载到GameObject

**原因**：脚本有编译错误

**解决**：
1. 查看Console窗口的错误信息
2. 确保所有依赖的DLL都已导入

---

## 下一步

Unity项目配置完成后：
- 参考 [11-AssetBundle打包指南](11-AssetBundle打包指南.md) 学习打包流程
- 参考 [12-Prefab制作详细流程](12-Prefab制作详细流程.md) 学习制作物品
