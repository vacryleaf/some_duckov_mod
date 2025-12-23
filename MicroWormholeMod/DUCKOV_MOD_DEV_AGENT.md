# Duckov Mod Development Agent

专门用于开发《逃离鸭科夫》(Duckov) 游戏 Mod 的 AI Agent 指南。

---

## 目录

1. [环境准备](#环境准备)
2. [项目结构](#项目结构)
3. [核心 API](#核心-api)
4. [物品系统](#物品系统)
5. [Unity AssetBundle](#unity-assetbundle)
6. [本地化系统](#本地化系统)
7. [常见问题与解决方案](#常见问题与解决方案)
8. [代码模板](#代码模板)
9. [开发流程](#开发流程)

---

## 环境准备

### 必需工具

- **.NET SDK**：支持 .NET Standard 2.1
- **Unity**：2022.3.x（用于创建 AssetBundle）
- **IDE**：VS Code / Visual Studio / Rider

### 游戏 DLL 位置

```
macOS:  ~/workspace/duckov/Managed/
Windows: C:\Program Files\Steam\steamapps\common\Duckov\Duckov_Data\Managed\
```

### 必需引用的 DLL

```xml
<!-- Unity 核心库 -->
UnityEngine.CoreModule.dll
UnityEngine.PhysicsModule.dll
UnityEngine.AssetBundleModule.dll
UnityEngine.ParticleSystemModule.dll

<!-- 游戏核心库 -->
TeamSoda.Duckov.Core.dll      <!-- 包含 Duckov.Modding 命名空间 -->
TeamSoda.Duckov.Utilities.dll
ItemStatsSystem.dll           <!-- 物品系统 -->
Assembly-CSharp.dll           <!-- 游戏主程序集 -->
SodaLocalization.dll          <!-- 本地化系统 -->
UniTask.dll                   <!-- 异步任务库 -->
```

---

## 项目结构

### 标准 Mod 项目结构

```
ModName/
├── ModName.csproj              # 项目配置文件
├── info.ini                    # Mod 元信息
├── README.md                   # 说明文档
├── Scripts/
│   ├── ModBehaviour.cs         # Mod 主入口类
│   └── *.cs                    # 其他脚本
├── Unity/                      # Unity 项目（可选）
│   └── ModAssets/
│       ├── Assets/
│       │   ├── Editor/         # 编辑器脚本
│       │   ├── Prefabs/        # 预制体
│       │   └── Icons/          # 图标
│       └── ProjectSettings/
├── Release/                    # 发布目录
│   └── ModName/
│       ├── ModName.dll         # 编译后的 DLL
│       ├── info.ini            # Mod 配置
│       ├── preview.png         # 预览图（256x256）
│       └── Assets/             # AssetBundle 资源
│           └── bundle_name     # AssetBundle 文件
└── bin/                        # 编译输出
```

### info.ini 格式

```ini
name=ModName
displayName=显示名称
description=Mod描述
tags=Utility,Quality of Life
```

---

## 核心 API

### ModBehaviour 基类

```csharp
using Duckov.Modding;

namespace MyMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        void Start()
        {
            // Mod 启动入口
            Debug.Log("[MyMod] 开始加载...");
        }

        void OnDestroy()
        {
            // Mod 卸载清理
            Debug.Log("[MyMod] 卸载完成");
        }
    }
}
```

### 关键类和命名空间

```csharp
// 游戏核心
using Duckov.Modding;           // ModBehaviour 基类
using ItemStatsSystem;          // Item, ItemAssetsCollection
using SodaCraft.Localizations;  // LocalizationManager

// Unity
using UnityEngine;
using UnityEngine.SceneManagement;

// 游戏管理器（通过反射或直接引用）
LevelManager.Instance           // 关卡管理
CharacterMainControl.Main       // 玩家角色
```

### 常用游戏状态检查

```csharp
// 检查是否在家（基地）
if (LevelManager.Instance.IsBaseLevel) { }

// 检查是否在突袭任务中
if (LevelManager.Instance.IsRaidMap) { }

// 获取玩家角色
CharacterMainControl player = CharacterMainControl.Main;
if (player != null)
{
    Vector3 position = player.transform.position;
    Quaternion rotation = player.transform.rotation;
}
```

### 触发撤离（回家）

```csharp
// 创建撤离信息并触发撤离
EvacuationInfo evacuationInfo = new EvacuationInfo();
LevelManager.Instance.NotifyEvacuated(evacuationInfo);
```

---

## 物品系统

### 创建自定义物品

```csharp
private const int MY_ITEM_TYPE_ID = 990001;  // 使用较大数值避免冲突
private Item itemPrefab;

private void CreateItem()
{
    // 创建 GameObject
    GameObject itemObj = new GameObject("MyItem");
    DontDestroyOnLoad(itemObj);
    itemObj.SetActive(false);

    // 添加 Item 组件
    itemPrefab = itemObj.AddComponent<Item>();

    // 配置属性（使用反射设置私有字段）
    SetFieldValue(itemPrefab, "typeID", MY_ITEM_TYPE_ID);
    SetFieldValue(itemPrefab, "displayName", "Item_Name_Key");
    SetFieldValue(itemPrefab, "description", "Item_Desc_Key");
    SetFieldValue(itemPrefab, "stackable", true);
    SetFieldValue(itemPrefab, "maxStackCount", 5);
    SetFieldValue(itemPrefab, "usable", true);
    SetFieldValue(itemPrefab, "quality", 4);        // 品质等级
    SetFieldValue(itemPrefab, "value", 10000);      // 价值
    SetFieldValue(itemPrefab, "weight", 0.1f);      // 重量

    // 如果有图标
    if (icon != null)
    {
        SetFieldValue(itemPrefab, "icon", icon);
    }
}

// 反射设置字段值的辅助方法
private void SetFieldValue(object obj, string fieldName, object value)
{
    var type = obj.GetType();
    var field = type.GetField(fieldName,
        System.Reflection.BindingFlags.Instance |
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.NonPublic);
    if (field != null)
    {
        field.SetValue(obj, value);
    }
    else
    {
        var prop = type.GetProperty(fieldName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(obj, value);
        }
    }
}
```

### 注册物品

```csharp
private void RegisterItems()
{
    if (itemPrefab != null)
    {
        bool success = ItemAssetsCollection.AddDynamicEntry(itemPrefab);
        Debug.Log($"[MyMod] 物品注册: {(success ? "成功" : "失败")}");
    }
}
```

### 监听物品使用事件

```csharp
private void RegisterEvents()
{
    Item.onUseStatic += OnItemUsed;
}

private void OnItemUsed(Item item, object user)
{
    if (item == null) return;

    if (item.TypeID == MY_ITEM_TYPE_ID)
    {
        Debug.Log("[MyMod] 物品被使用！");
        HandleItemUse(item);
    }
}

// 卸载时取消注册
void OnDestroy()
{
    Item.onUseStatic -= OnItemUsed;

    if (itemPrefab != null)
    {
        ItemAssetsCollection.RemoveDynamicEntry(itemPrefab);
        Destroy(itemPrefab.gameObject);
    }
}
```

### 消耗物品

```csharp
private void ConsumeItem(Item item)
{
    if (item == null) return;

    if (item.Stackable && item.StackCount > 1)
    {
        // 减少堆叠数量（需要使用反射或物品API）
        Debug.Log("[MyMod] 减少堆叠数量");
    }
    else
    {
        item.Detach();
        Destroy(item.gameObject);
        Debug.Log("[MyMod] 物品已消耗");
    }
}
```

### 创建物品视觉效果

```csharp
private void CreateItemVisual(GameObject parent, Color color)
{
    GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    visual.name = "Visual";
    visual.transform.SetParent(parent.transform);
    visual.transform.localPosition = Vector3.zero;
    visual.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

    // 创建发光材质
    Material material = new Material(Shader.Find("Standard"));
    material.color = new Color(color.r, color.g, color.b, 0.9f);
    material.SetFloat("_Metallic", 0.8f);
    material.SetFloat("_Glossiness", 0.9f);
    material.EnableKeyword("_EMISSION");
    material.SetColor("_EmissionColor", color * 2f);

    visual.GetComponent<Renderer>().material = material;
    Object.Destroy(visual.GetComponent<Collider>());

    // 添加拾取碰撞体
    SphereCollider collider = parent.AddComponent<SphereCollider>();
    collider.radius = 0.1f;
}
```

---

## Unity AssetBundle

### 为什么需要 AssetBundle

- Unity 编辑器无法直接加载游戏 DLL（会导致依赖错误）
- 解决方案：在 Unity 中创建纯模型/图标，打包为 AssetBundle，运行时动态加载

### Unity 项目设置

1. **不要**在 Unity 中添加任何游戏 DLL
2. 创建纯模型、材质、图标
3. 使用 Editor 脚本打包

### AssetBundle 构建脚本

```csharp
// Assets/Editor/AssetBundleBuilder.cs
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleBuilder
{
    private static string outputPath = "Assets/AssetBundles";
    private static string bundleName = "my_bundle";

    [MenuItem("Tools/MyMod/构建 AssetBundle")]
    public static void BuildAssetBundles()
    {
        // 设置资源的 AssetBundle 名称
        SetAssetBundleNames();

        // 创建输出目录
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        // 构建
        BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget
        );

        AssetDatabase.Refresh();
        Debug.Log($"AssetBundle 构建完成: {Path.GetFullPath(outputPath)}");
    }

    private static void SetAssetBundleNames()
    {
        SetBundleName("Assets/Prefabs/MyPrefab.prefab");
        SetBundleName("Assets/Icons/MyIcon.png");
        AssetDatabase.SaveAssets();
    }

    private static void SetBundleName(string assetPath)
    {
        if (File.Exists(assetPath))
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
            {
                importer.assetBundleName = bundleName;
            }
        }
    }
}
```

### 加载 AssetBundle

```csharp
private AssetBundle assetBundle;

private void LoadAssetBundle()
{
    // 获取 Mod DLL 所在目录
    string modPath = Path.GetDirectoryName(GetType().Assembly.Location);
    string bundlePath = Path.Combine(modPath, "Assets", "my_bundle");

    if (!File.Exists(bundlePath))
    {
        Debug.LogWarning($"AssetBundle 不存在: {bundlePath}");
        return;
    }

    assetBundle = AssetBundle.LoadFromFile(bundlePath);

    if (assetBundle == null)
    {
        Debug.LogWarning("AssetBundle 加载失败");
        return;
    }

    // 加载资源
    GameObject prefab = assetBundle.LoadAsset<GameObject>("MyPrefab");
    Sprite icon = assetBundle.LoadAsset<Sprite>("MyIcon");

    // 如果图标是 Texture2D，需要转换为 Sprite
    if (icon == null)
    {
        Texture2D tex = assetBundle.LoadAsset<Texture2D>("MyIcon");
        if (tex != null)
        {
            icon = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
        }
    }
}

void OnDestroy()
{
    if (assetBundle != null)
    {
        assetBundle.Unload(true);
        assetBundle = null;
    }
}
```

### 程序化生成图标

```csharp
// Assets/Editor/IconGenerator.cs
private void GenerateIcon(string path, Color coreColor, Color outerColor)
{
    int size = 256;
    Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

    Color[] pixels = new Color[size * size];
    Vector2 center = new Vector2(size / 2f, size / 2f);
    float outerRadius = size * 0.4f;
    float innerRadius = size * 0.2f;

    for (int y = 0; y < size; y++)
    {
        for (int x = 0; x < size; x++)
        {
            Vector2 pos = new Vector2(x, y);
            float dist = Vector2.Distance(pos, center);

            if (dist < outerRadius)
            {
                float t = 1f - (dist / outerRadius);
                float alpha = Mathf.Clamp01(t * 2f);

                Color col = dist < innerRadius ? coreColor :
                    Color.Lerp(coreColor, outerColor,
                        (dist - innerRadius) / (outerRadius - innerRadius));

                col.a = alpha;
                pixels[y * size + x] = col;
            }
            else
            {
                pixels[y * size + x] = Color.clear;
            }
        }
    }

    tex.SetPixels(pixels);
    tex.Apply();

    byte[] pngData = tex.EncodeToPNG();
    File.WriteAllBytes(path, pngData);
    DestroyImmediate(tex);

    // 设置为 Sprite 类型
    AssetDatabase.Refresh();
    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
    if (importer != null)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.SaveAndReimport();
    }
}
```

---

## 本地化系统

### 设置本地化文本

```csharp
using SodaCraft.Localizations;

private void SetupLocalization()
{
    // 中文
    LocalizationManager.SetOverrideText("Item_Name", "物品名称");
    LocalizationManager.SetOverrideText("Item_Desc", "物品描述\n\n<color=#FFD700>金色提示文字</color>");

    // 监听语言切换
    LocalizationManager.OnSetLanguage += OnLanguageChanged;
}

private void OnLanguageChanged(SystemLanguage language)
{
    switch (language)
    {
        case SystemLanguage.English:
            LocalizationManager.SetOverrideText("Item_Name", "Item Name");
            LocalizationManager.SetOverrideText("Item_Desc", "Item description\n\n<color=#FFD700>Golden tip text</color>");
            break;
        default: // 中文
            LocalizationManager.SetOverrideText("Item_Name", "物品名称");
            LocalizationManager.SetOverrideText("Item_Desc", "物品描述");
            break;
    }
}

void OnDestroy()
{
    LocalizationManager.OnSetLanguage -= OnLanguageChanged;
}
```

---

## 常见问题与解决方案

### 问题1：Unity 加载游戏 DLL 报错

**错误**：`Unloading broken assembly Assets/Plugins/XXX.dll`

**解决方案**：
- **不要**在 Unity 项目中添加任何游戏 DLL
- Unity 项目只用于创建纯模型和图标
- 在运行时通过反射或游戏 API 配置物品属性

### 问题2：Item 组件找不到

**错误**：在 Unity 中搜索不到 `Item` 组件

**解决方案**：
- Item 组件来自游戏 DLL，但我们不在 Unity 中添加
- 在代码中使用 `itemObj.AddComponent<Item>()` 动态添加

### 问题3：csproj 包含 Unity 项目文件

**错误**：编译时包含了 Unity 目录下的 .cs 文件

**解决方案**：
```xml
<PropertyGroup>
  <DefaultItemExcludes>$(DefaultItemExcludes);Unity/**</DefaultItemExcludes>
</PropertyGroup>
```

### 问题4：跨场景数据丢失

**问题**：场景切换后变量被重置

**解决方案**：
```csharp
// 使用 static 变量保持跨场景数据
private static MyData savedData = new MyData();
private static bool pendingAction = false;
```

### 问题5：场景加载后需要延迟操作

**问题**：场景加载后立即操作会失败

**解决方案**：
```csharp
void Start()
{
    if (pendingAction)
    {
        StartCoroutine(DelayedAction());
    }
}

private IEnumerator DelayedAction()
{
    yield return new WaitForSeconds(1f);
    // 执行操作
    pendingAction = false;
}
```

---

## 代码模板

### 完整的 Mod 入口模板

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using Duckov.Modding;
using ItemStatsSystem;
using SodaCraft.Localizations;
using System.IO;

namespace MyMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const int ITEM_TYPE_ID = 990001;
        private Item itemPrefab;
        private AssetBundle assetBundle;
        private Sprite itemIcon;

        void Start()
        {
            Debug.Log("[MyMod] 开始加载...");

            try
            {
                LoadAssetBundle();
                SetupLocalization();
                CreateItem();
                RegisterItems();
                RegisterEvents();

                Debug.Log("[MyMod] 加载完成!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MyMod] 加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        private void LoadAssetBundle()
        {
            string modPath = Path.GetDirectoryName(GetType().Assembly.Location);
            string bundlePath = Path.Combine(modPath, "Assets", "my_bundle");

            if (File.Exists(bundlePath))
            {
                assetBundle = AssetBundle.LoadFromFile(bundlePath);
                if (assetBundle != null)
                {
                    itemIcon = LoadIconFromBundle("ItemIcon");
                }
            }
        }

        private Sprite LoadIconFromBundle(string iconName)
        {
            if (assetBundle == null) return null;

            Sprite icon = assetBundle.LoadAsset<Sprite>(iconName);
            if (icon == null)
            {
                Texture2D tex = assetBundle.LoadAsset<Texture2D>(iconName);
                if (tex != null)
                {
                    icon = Sprite.Create(tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f));
                }
            }
            return icon;
        }

        private void SetupLocalization()
        {
            LocalizationManager.SetOverrideText("Item_Name", "物品名称");
            LocalizationManager.SetOverrideText("Item_Desc", "物品描述");
            LocalizationManager.OnSetLanguage += OnLanguageChanged;
        }

        private void OnLanguageChanged(SystemLanguage language)
        {
            // 更新本地化文本
        }

        private void CreateItem()
        {
            GameObject itemObj = new GameObject("MyItem");
            DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            // 添加视觉效果（如果没有AssetBundle）
            if (assetBundle == null)
            {
                CreateItemVisual(itemObj, Color.blue);
            }

            itemPrefab = itemObj.AddComponent<Item>();
            ConfigureItem(itemPrefab);
        }

        private void CreateItemVisual(GameObject parent, Color color)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(parent.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2f);

            visual.GetComponent<Renderer>().material = material;
            Object.Destroy(visual.GetComponent<Collider>());

            SphereCollider collider = parent.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
        }

        private void ConfigureItem(Item item)
        {
            SetFieldValue(item, "typeID", ITEM_TYPE_ID);
            SetFieldValue(item, "displayName", "Item_Name");
            SetFieldValue(item, "description", "Item_Desc");
            SetFieldValue(item, "stackable", true);
            SetFieldValue(item, "maxStackCount", 5);
            SetFieldValue(item, "usable", true);
            SetFieldValue(item, "quality", 4);
            SetFieldValue(item, "value", 10000);
            SetFieldValue(item, "weight", 0.1f);

            if (itemIcon != null)
            {
                SetFieldValue(item, "icon", itemIcon);
            }
        }

        private void SetFieldValue(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                var prop = type.GetProperty(fieldName,
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(obj, value);
                }
            }
        }

        private void RegisterItems()
        {
            if (itemPrefab != null)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(itemPrefab);
                Debug.Log($"[MyMod] 物品注册: {(success ? "成功" : "失败")}");
            }
        }

        private void RegisterEvents()
        {
            Item.onUseStatic += OnItemUsed;
        }

        private void OnItemUsed(Item item, object user)
        {
            if (item == null || item.TypeID != ITEM_TYPE_ID) return;

            Debug.Log("[MyMod] 物品被使用!");
            // 处理物品使用逻辑
        }

        private void ShowMessage(string message)
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter != null)
            {
                Duckov.UI.DialogueBubbles.DialogueBubblesManager.Show(
                    message,
                    mainCharacter.transform,
                    -1f, false, false, -1f, 2f
                );
            }
        }

        void OnDestroy()
        {
            Debug.Log("[MyMod] 开始卸载");

            Item.onUseStatic -= OnItemUsed;
            LocalizationManager.OnSetLanguage -= OnLanguageChanged;

            if (itemPrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(itemPrefab);
                Destroy(itemPrefab.gameObject);
            }

            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                assetBundle = null;
            }

            Debug.Log("[MyMod] 卸载完成");
        }
    }
}
```

### csproj 模板

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <OutputType>Library</OutputType>
    <AssemblyName>MyMod</AssemblyName>
    <RootNamespace>MyMod</RootNamespace>
    <Version>1.0.0</Version>
    <Authors>YourName</Authors>
    <Description>Mod描述</Description>
    <LangVersion>latest</LangVersion>
    <Nullable>disable</Nullable>
    <OutputPath>bin\$(Configuration)\</OutputPath>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <DefaultItemExcludes>$(DefaultItemExcludes);Unity/**</DefaultItemExcludes>
  </PropertyGroup>

  <!-- 游戏路径配置 -->
  <PropertyGroup Condition="'$(OS)' == 'Windows_NT'">
    <DuckovPath Condition="'$(DuckovPath)' == ''">C:\Program Files\Steam\steamapps\common\Duckov</DuckovPath>
  </PropertyGroup>

  <PropertyGroup Condition="'$(OS)' != 'Windows_NT'">
    <DuckovPath Condition="'$(DuckovPath)' == ''">/Users/huangjinhui/workspace/duckov</DuckovPath>
  </PropertyGroup>

  <!-- 游戏DLL引用 -->
  <ItemGroup>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>$(DuckovPath)/Managed/UnityEngine.CoreModule.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="UnityEngine.PhysicsModule">
      <HintPath>$(DuckovPath)/Managed/UnityEngine.PhysicsModule.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="UnityEngine.AssetBundleModule">
      <HintPath>$(DuckovPath)/Managed/UnityEngine.AssetBundleModule.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="UnityEngine.ParticleSystemModule">
      <HintPath>$(DuckovPath)/Managed/UnityEngine.ParticleSystemModule.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="TeamSoda.Duckov.Core">
      <HintPath>$(DuckovPath)/Managed/TeamSoda.Duckov.Core.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="TeamSoda.Duckov.Utilities">
      <HintPath>$(DuckovPath)/Managed/TeamSoda.Duckov.Utilities.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="ItemStatsSystem">
      <HintPath>$(DuckovPath)/Managed/ItemStatsSystem.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="Assembly-CSharp">
      <HintPath>$(DuckovPath)/Managed/Assembly-CSharp.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="UniTask">
      <HintPath>$(DuckovPath)/Managed/UniTask.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="SodaLocalization">
      <HintPath>$(DuckovPath)/Managed/SodaLocalization.dll</HintPath>
      <Private>False</Private>
    </Reference>
  </ItemGroup>

  <!-- 编译后复制到Release目录 -->
  <Target Name="CopyToReleaseFolder" AfterTargets="Build">
    <PropertyGroup>
      <ReleaseFolder>$(ProjectDir)Release/MyMod</ReleaseFolder>
    </PropertyGroup>
    <MakeDir Directories="$(ReleaseFolder)" Condition="!Exists('$(ReleaseFolder)')" />
    <ItemGroup>
      <OutputFiles Include="$(OutputPath)MyMod.dll" />
    </ItemGroup>
    <Copy SourceFiles="@(OutputFiles)" DestinationFolder="$(ReleaseFolder)" SkipUnchangedFiles="true" />
    <Message Text="Mod已复制到: $(ReleaseFolder)" Importance="high" />
  </Target>

</Project>
```

---

## 开发流程

### 1. 创建新 Mod 项目

```bash
# 创建目录结构
mkdir -p MyMod/Scripts
mkdir -p MyMod/Release/MyMod/Assets
mkdir -p MyMod/Unity/ModAssets/Assets/{Editor,Prefabs,Icons}

# 创建必要文件
touch MyMod/MyMod.csproj
touch MyMod/Scripts/ModBehaviour.cs
touch MyMod/info.ini
touch MyMod/README.md
```

### 2. 配置项目

- 复制 csproj 模板并修改名称
- 创建 info.ini（name, displayName, description, tags）
- 编写 ModBehaviour.cs

### 3. 创建 Unity 资源（可选）

```bash
# 打开 Unity 创建项目
# 添加 Editor 脚本用于生成图标和构建 AssetBundle
# 构建 AssetBundle 后复制到 Release/ModName/Assets/
```

### 4. 编译和测试

```bash
# 编译
dotnet build MyMod.csproj

# 复制到游戏 Mods 目录
# Windows: Duckov_Data/Mods/
# macOS: Duckov.app/Contents/Resources/Data/Mods/

# 启动游戏测试
```

### 5. 发布

确保 Release 目录包含：
- `ModName.dll` - 编译后的 DLL
- `info.ini` - Mod 配置
- `preview.png` - 预览图（256x256）
- `Assets/` - AssetBundle 资源（如果有）

---

## 参考项目

- **MicroWormholeMod**：微型虫洞传送物品示例
  - 双物品系统（微型虫洞 + 回溯虫洞）
  - AssetBundle 加载
  - 本地化支持
  - 跨场景数据保存

---

*最后更新：2025-12*
