# 11-AssetBundle打包指南

## AssetBundle概述

AssetBundle是Unity的资源打包格式，用于在运行时动态加载资源。

**Mod中的用途**：
- 打包物品Prefab
- 打包图标Sprite
- 打包3D模型
- 打包材质和贴图

---

## 设置AssetBundle名称

### 方法一：Inspector设置

1. 在 Project 窗口选择你的 Prefab
2. 在 Inspector 窗口底部找到 **AssetBundle** 下拉菜单
3. 点击 **New...** 创建新的AssetBundle名称
4. 输入名称（如 `mymod_items`）

### 方法二：代码批量设置

```csharp
// Assets/Editor/AssetBundleNamer.cs
using UnityEditor;
using UnityEngine;

public class AssetBundleNamer
{
    [MenuItem("Tools/Set AssetBundle Names")]
    public static void SetAssetBundleNames()
    {
        // 获取Prefabs目录下所有预制体
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetImporter importer = AssetImporter.GetAtPath(path);

            // 设置AssetBundle名称
            importer.assetBundleName = "mymod_items";

            Debug.Log($"设置 {path} 的 AssetBundle 为 mymod_items");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("AssetBundle名称设置完成！");
    }
}
```

---

## 执行构建

### 使用菜单

1. 点击 Unity 菜单 **Build → Build AssetBundles (Current Platform)**
2. 等待构建完成
3. 查看 `Assets/AssetBundles/` 目录

### 构建输出

构建完成后，会生成以下文件：

```
Assets/AssetBundles/
├── AssetBundles              ← 主清单文件
├── AssetBundles.manifest     ← 主清单描述
├── mymod_items               ← 你的AssetBundle（复制这个）
└── mymod_items.manifest      ← 描述文件（可选复制）
```

**重要**：只需要复制 `mymod_items` 文件到Mod目录，不需要后缀！

---

## 平台匹配

### 必须匹配游戏平台

| 游戏平台 | 打包目标 |
|----------|----------|
| Windows | `BuildTarget.StandaloneWindows64` |
| macOS | `BuildTarget.StandaloneOSX` |
| Linux | `BuildTarget.StandaloneLinux64` |

### 跨平台打包

如果需要同时支持多个平台：

```csharp
[MenuItem("Build/Build All Platforms")]
public static void BuildAllPlatforms()
{
    // Windows
    BuildAssetBundles(BuildTarget.StandaloneWindows64, "AssetBundles/Windows");

    // macOS
    BuildAssetBundles(BuildTarget.StandaloneOSX, "AssetBundles/macOS");
}

private static void BuildAssetBundles(BuildTarget target, string outputPath)
{
    if (!Directory.Exists(outputPath))
        Directory.CreateDirectory(outputPath);

    BuildPipeline.BuildAssetBundles(outputPath,
        BuildAssetBundleOptions.None, target);
}
```

---

## 在Mod中加载AssetBundle

### 同步加载

```csharp
private AssetBundle assetBundle;
private Item itemPrefab;
private Sprite itemIcon;

private void LoadAssetBundle()
{
    // 获取Mod目录路径
    string modPath = GetModFolderPath();
    string bundlePath = Path.Combine(modPath, "Assets", "mymod_items");

    // 检查文件是否存在
    if (!File.Exists(bundlePath))
    {
        Debug.LogWarning($"[Mod] AssetBundle不存在: {bundlePath}");
        return;
    }

    // 加载AssetBundle
    assetBundle = AssetBundle.LoadFromFile(bundlePath);
    if (assetBundle == null)
    {
        Debug.LogError("[Mod] AssetBundle加载失败");
        return;
    }

    // 加载Prefab
    itemPrefab = assetBundle.LoadAsset<Item>("MyItem");

    // 加载图标
    itemIcon = assetBundle.LoadAsset<Sprite>("MyItemIcon");

    // 或者加载Texture2D后转换
    Texture2D tex = assetBundle.LoadAsset<Texture2D>("MyItemIcon");
    if (tex != null)
    {
        itemIcon = Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f));
    }

    Debug.Log("[Mod] AssetBundle加载成功");
}
```

### 异步加载

```csharp
private async void LoadAssetBundleAsync()
{
    string bundlePath = Path.Combine(GetModFolderPath(), "Assets", "mymod_items");

    // 异步加载AssetBundle
    AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
    await bundleRequest;

    assetBundle = bundleRequest.assetBundle;
    if (assetBundle == null) return;

    // 异步加载资源
    AssetBundleRequest assetRequest = assetBundle.LoadAssetAsync<Item>("MyItem");
    await assetRequest;

    itemPrefab = assetRequest.asset as Item;
}
```

### 列出所有资源

```csharp
private void ListAllAssets()
{
    if (assetBundle == null) return;

    string[] allNames = assetBundle.GetAllAssetNames();
    Debug.Log($"[Mod] AssetBundle包含 {allNames.Length} 个资源:");

    foreach (string name in allNames)
    {
        Debug.Log($"  - {name}");
    }
}
```

---

## 卸载AssetBundle

**重要**：Mod卸载时必须释放AssetBundle！

```csharp
void OnDestroy()
{
    if (assetBundle != null)
    {
        // true = 同时卸载所有已加载的资源
        assetBundle.Unload(true);
        assetBundle = null;
    }
}
```

---

## 资源依赖处理

### 确保材质被打包

如果Prefab使用了自定义材质，确保材质也被打包：

1. 选择材质文件
2. 设置相同的AssetBundle名称
3. 或者将材质放在Prefab同一目录下

### 打包选项

```csharp
// 推荐选项
BuildAssetBundleOptions.None

// 可选选项
BuildAssetBundleOptions.ChunkBasedCompression  // 更好的加载性能
BuildAssetBundleOptions.UncompressedAssetBundle // 不压缩，加载更快但文件更大
```

---

## 常见问题

### 加载失败：返回null

| 原因 | 解决方案 |
|------|----------|
| 平台不匹配 | 确保打包平台与游戏平台一致 |
| 路径错误 | 检查文件路径，使用Debug.Log输出 |
| 资源名称错误 | 使用GetAllAssetNames()列出所有资源 |

### 材质丢失/粉红色

| 原因 | 解决方案 |
|------|----------|
| 材质未打包 | 将材质设置相同的AssetBundle名称 |
| Shader不兼容 | 使用Unity内置Shader（如Standard） |

### Prefab组件丢失

| 原因 | 解决方案 |
|------|----------|
| DLL版本不匹配 | 确保Unity项目和游戏使用相同版本的DLL |
| 脚本未找到 | 自定义脚本需要打包到DLL中 |

---

## 调试技巧

### 打印加载路径

```csharp
private void DebugLoadPath()
{
    string modPath = GetModFolderPath();
    string bundlePath = Path.Combine(modPath, "Assets", "mymod_items");

    Debug.Log($"[Mod] Mod目录: {modPath}");
    Debug.Log($"[Mod] Bundle路径: {bundlePath}");
    Debug.Log($"[Mod] 文件存在: {File.Exists(bundlePath)}");
}
```

### 验证资源加载

```csharp
private void ValidateAssets()
{
    if (itemPrefab == null)
        Debug.LogError("[Mod] itemPrefab 为 null");
    else
        Debug.Log($"[Mod] itemPrefab 加载成功: {itemPrefab.name}");

    if (itemIcon == null)
        Debug.LogError("[Mod] itemIcon 为 null");
    else
        Debug.Log($"[Mod] itemIcon 加载成功: {itemIcon.name}");
}
```

---

## 下一步

AssetBundle打包完成后：
- 参考 [12-Prefab制作详细流程](12-Prefab制作详细流程.md) 学习如何制作Prefab
- 参考 [13-端到端完整教程](13-端到端完整教程.md) 查看完整开发流程
