# 微型虫洞 Mod - Unity AssetBundle 配置指南

> 本指南帮助你在 Unity 中创建物品图标和 AssetBundle

---

## 第一步：安装 Unity

### 1.1 下载 Unity Hub
1. 访问 https://unity.com/download
2. 下载并安装 Unity Hub

### 1.2 安装 Unity Editor
1. 打开 Unity Hub
2. 点击 "Installs" → "Install Editor"
3. **重要**：选择版本 **2021.3.x LTS**（与游戏版本匹配）
4. 勾选对应平台的 Build Support
5. 点击 "Install"

---

## 第二步：创建 Unity 项目

### 2.1 新建项目
1. Unity Hub → "Projects" → "New project"
2. 模板选择：**3D (Built-in Render Pipeline)**
3. 项目名称：`MicroWormholeMod`
4. 点击 "Create project"

---

## 第三步：设置项目结构

### 3.1 创建文件夹
在 Project 窗口中右键 Assets，创建以下目录：

```
Assets/
├── Editor/           ← 编辑器脚本
├── Prefabs/          ← 物品预制体
├── Icons/            ← 物品图标
├── Plugins/          ← 游戏 DLL
└── AssetBundles/     ← 导出的包
```

### 3.2 导入游戏 DLL
复制以下文件到 `Assets/Plugins/`：

**源路径**: `/Users/huangjinhui/workspace/duckov/Managed/`

- `ItemStatsSystem.dll`
- `TeamSoda.Duckov.Core.dll`
- `TeamSoda.Duckov.Utilities.dll`

---

## 第四步：导入编辑器脚本

### 4.1 复制脚本
将以下文件复制到 `Assets/Editor/`：

**源路径**:
```
/Users/huangjinhui/workspace/duckov/some_duckov_mod/MicroWormholeMod/Unity/Editor/
```

需要复制的文件：
- `MicroWormholeGenerator.cs`
- `AssetBundleBuilder.cs`

### 4.2 等待编译
Unity 会自动编译脚本，等待底部状态栏完成。

---

## 第五步：生成物品和图标

### 5.1 打开物品生成器
菜单栏：**Tools → 微型虫洞 → 物品生成器**

### 5.2 生成物品模型
1. 调整参数（可保持默认）：
   - 球体半径: 0.1
   - 主颜色: 紫色
   - 发光颜色: 亮紫色
   - 发光强度: 2

2. 点击 **"生成物品模型"**
3. 点击 **"保存为 Prefab"**
4. 保存到 `Assets/Prefabs/MicroWormhole.prefab`

### 5.3 生成图标
1. 点击 **"生成程序化图标"**
2. 保存到 `Assets/Icons/MicroWormholeIcon.png`

或者你可以导入自己的图标：
- 将 PNG 图片拖入 `Assets/Icons/`
- 选中图片，在 Inspector 中设置 Texture Type = Sprite

---

## 第六步：配置 Item 组件（关键！）

### 6.1 打开 Prefab
双击 `Assets/Prefabs/MicroWormhole.prefab`

### 6.2 添加 Item 组件
1. Inspector → Add Component → 搜索 "Item"
2. 添加 Item 组件

### 6.3 配置 Item 属性

| 属性 | 值 | 说明 |
|------|-----|------|
| Type ID | `990001` | 唯一ID，避免冲突 |
| Display Name | `MicroWormhole_Name` | 本地化键 |
| Description | `MicroWormhole_Desc` | 描述键 |
| Icon | 拖入图标 | 物品图标 |
| Max Stack Count | `5` | 可堆叠5个 |
| Stackable | ✅ | 可堆叠 |
| Value | `10000` | 售价 |
| Weight | `0.1` | 重量 |
| Quality | `4` | 品质等级 |
| Usable | ✅ | 可使用 |

### 6.4 保存 Prefab
按 `Cmd+S` 或点击顶部 "Save"

---

## 第七步：构建 AssetBundle

### 7.1 构建
菜单栏：**Tools → 微型虫洞 → 构建 AssetBundle**

### 7.2 找到文件
导出位置：
```
<Unity项目>/Assets/AssetBundles/micro_wormhole
```

---

## 第八步：部署到 Mod

### 8.1 复制 AssetBundle
将 `micro_wormhole` 文件复制到：
```
/Users/huangjinhui/workspace/duckov/some_duckov_mod/MicroWormholeMod/Release/MicroWormholeMod/Assets/
```

### 8.2 更新代码加载 AssetBundle
需要修改 ModBehaviour.cs 来加载 AssetBundle（参考 MoonlightSwordMod）

### 8.3 最终目录结构
```
MicroWormholeMod/
├── MicroWormholeMod.dll
├── info.ini
├── preview.png            ← 需要添加
└── Assets/
    └── micro_wormhole     ← AssetBundle
```

---

## 常见问题

### Q: 找不到 Item 组件？
A: 确保正确导入了 `ItemStatsSystem.dll`

### Q: AssetBundle 构建失败？
A: 检查 Console 窗口的错误信息

### Q: 图标不显示？
A: 确保图片的 Texture Type 设置为 Sprite

---

## 快速命令

```
1. 安装 Unity 2021.3.x LTS
2. 新建 3D 项目
3. 复制 DLL 到 Assets/Plugins/
4. 复制编辑器脚本到 Assets/Editor/
5. Tools → 微型虫洞 → 物品生成器 → 生成
6. 配置 Item 组件 (TypeID=990001)
7. Tools → 微型虫洞 → 构建 AssetBundle
8. 复制 micro_wormhole 到 Release/MicroWormholeMod/Assets/
```
