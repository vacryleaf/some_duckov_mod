# Unity 开发依赖

此文件夹包含在 Unity 中配置 Item 组件所需的 DLL 文件。

## 使用方法

**直接将此文件夹中的所有文件复制到 Unity 项目的 `Assets/Plugins/` 文件夹即可。**

```bash
# 示例
cp -r UnityDependencies/* <你的Unity项目>/Assets/Plugins/
```

## 包含的文件

| 文件 | 说明 |
|------|------|
| ItemStatsSystem.dll | 物品系统核心 |
| TeamSoda.Duckov.Utilities.dll | 游戏工具库 |
| SodaLocalization.dll | 本地化系统 |
| TeamSoda.MiniLocalizor.dll | 本地化依赖 |
| Sirenix.OdinInspector.Attributes.dll | Odin Inspector |
| Sirenix.Utilities.dll | Odin 工具库 |
| UniTask.dll | 异步任务库 |
| *.dll.meta | Unity 配置文件（已配置好引用验证禁用） |

## 注意

- 这些 DLL **仅用于 Unity 开发**，不是 Mod 运行时依赖
- Mod 运行时只需要 `Release/MoonlightSwordMod/` 中的文件
- meta 文件已配置好 `validateReferences: 0`，避免依赖缺失错误
