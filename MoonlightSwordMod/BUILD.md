# 名刀月影 - 构建和部署指南

## 🛠️ 构建项目

### 前置要求

1. **.NET SDK**
   - 需要 .NET 5.0 或更高版本
   - 下载: https://dotnet.microsoft.com/download

2. **游戏安装**
   - 已安装《逃离鸭科夫》游戏
   - 知道游戏安装路径

3. **Unity项目** (用于生成AssetBundle)
   - Unity 2020.3 LTS或更高版本
   - 已使用编辑器工具生成模型和特效

---

## 📦 构建步骤

### 第1步: 配置游戏路径

编辑 `MoonlightSwordMod.csproj` 文件，设置正确的游戏路径：

**Windows:**
```xml
<DuckovPath>C:\Program Files\Steam\steamapps\common\Duckov</DuckovPath>
```

**macOS:**
```xml
<DuckovPath>/Users/你的用户名/Library/Application Support/Steam/steamapps/common/Duckov</DuckovPath>
```

### 第2步: 编译项目

在项目根目录执行：

```bash
# 编译Release版本
dotnet build -c Release

# 或编译Debug版本
dotnet build -c Debug
```

编译成功后，DLL文件位于: `bin/Release/MoonlightSwordMod.dll`

### 第3步: 自动部署（可选）

如果游戏路径配置正确，编译后会自动复制文件到Mod目录：

```
<游戏目录>/Duckov_Data/Mods/MoonlightSwordMod/
├── MoonlightSwordMod.dll
└── info.ini
```

---

## 🎨 Unity AssetBundle构建

### 在Unity中生成模型

1. 打开Unity项目
2. 导入 `Unity/Editor/` 下的脚本
3. 使用 `Tools → 名刀月影 → 模型生成器` 生成刀模型
4. 使用 `Tools → 名刀月影 → 剑气特效生成器` 生成剑气
5. 保存Prefab到 `Assets/MoonlightSword/Prefabs/`

### 标记AssetBundle

选中所有资源，在Inspector底部设置：
- **AssetBundle名**: `moonlight_sword`

包括：
- MoonlightSword.prefab
- SwordAuraPrefab.prefab
- 所有材质文件

### 构建AssetBundle

在Unity编辑器中：
1. 创建 `Assets/Editor/BuildAssetBundles.cs` （代码见Unity/README_Unity.md）
2. 点击 `Assets → Build AssetBundles`
3. 生成的文件在 `Assets/AssetBundles/moonlight_sword`

### 复制AssetBundle

将 `moonlight_sword` 文件复制到：
```
<游戏目录>/Duckov_Data/Mods/MoonlightSwordMod/Assets/moonlight_sword
```

---

## 📂 最终Mod结构

部署完成后，Mod文件夹应该是这样：

```
Duckov_Data/Mods/MoonlightSwordMod/
├── MoonlightSwordMod.dll        # 编译的Mod代码
├── info.ini                     # Mod配置文件
├── preview.png                  # 预览图(256x256，可选)
└── Assets/
    └── moonlight_sword          # AssetBundle文件
```

---

## 🧪 测试Mod

### 1. 启动游戏

启动《逃离鸭科夫》游戏

### 2. 加载Mod

- 进入游戏主菜单
- 打开 **Mods** 菜单
- 找到 "名刀月影" Mod
- 启用Mod
- 重启游戏（如需要）

### 3. 生成武器

在游戏中，使用控制台或代码生成武器：

```csharp
// 通过TypeID实例化
Item sword = await ItemAssetsCollection.InstantiateAsync(10001);

// 添加到玩家背包
if (sword != null)
{
    ItemUtilities.SendToPlayer(sword);
}
```

### 4. 功能测试

按照 `weapon_msk.md` 中的测试清单逐项测试：

- [ ] 武器可以装备
- [ ] 正手挥击正常
- [ ] 反手挥击正常
- [ ] 连击系统工作
- [ ] 瞄准+攻击触发特殊攻击
- [ ] 剑气正确生成
- [ ] 剑气可以穿透敌人
- [ ] 剑气可以偏转子弹

---

## 🐛 调试

### 查看日志

**Windows:**
```
C:\Users\<用户名>\AppData\LocalLow\TeamSoda\Duckov\Player.log
```

**macOS:**
```
~/Library/Logs/TeamSoda/Duckov/Player.log
```

### 常见问题

**Q: Mod加载失败**
- 检查DLL是否在正确位置
- 检查info.ini配置
- 查看游戏日志错误信息

**Q: 武器生成失败**
- 检查AssetBundle是否存在
- 检查TypeID是否冲突（10001）
- 查看日志中的错误信息

**Q: 动画不播放**
- 检查Animator组件是否正确配置
- 检查动画事件是否正确添加
- 确保MoonlightSwordAnimationHandler已挂载

**Q: 剑气不显示**
- 检查SwordAuraPrefab是否正确加载
- 检查剑气的材质和Shader
- 确保SwordAuraProjectile组件存在

---

## 🔧 手动部署

如果自动部署不工作，手动复制文件：

### 1. 创建Mod目录

```bash
# Windows
mkdir "C:\Program Files\Steam\steamapps\common\Duckov\Duckov_Data\Mods\MoonlightSwordMod"

# macOS
mkdir -p "/Applications/Duckov.app/Contents/Mods/MoonlightSwordMod"
```

### 2. 复制文件

复制以下文件到Mod目录：
- `bin/Release/MoonlightSwordMod.dll`
- `info.ini`
- `Assets/moonlight_sword` (AssetBundle)

### 3. 创建预览图（可选）

创建一个 256x256 的PNG图片，命名为 `preview.png`，放在Mod目录。

---

## 📊 性能优化建议

### 编译优化

Release模式编译时使用优化：
```bash
dotnet build -c Release --no-incremental
```

### AssetBundle优化

- 使用LZ4压缩
- 合并相似材质
- 减少纹理大小
- 使用适当的LOD

### 代码优化

- 使用对象池管理剑气
- 使用NonAlloc API避免GC
- 合理设置碰撞检测频率

---

## 🚀 发布Mod

### 打包Mod

创建一个ZIP文件，包含：
```
MoonlightSwordMod.zip
└── MoonlightSwordMod/
    ├── MoonlightSwordMod.dll
    ├── info.ini
    ├── preview.png
    ├── README.txt (使用说明)
    └── Assets/
        └── moonlight_sword
```

### 发布位置

- Steam Workshop
- Nexus Mods
- 游戏官方Mod社区
- GitHub Release

### 发布信息模板

**标题**: 名刀月影 - 传说级近战武器

**描述**:
```
添加一把1.5米长的传说级刀"名刀月影"到游戏中。

特性：
- 正手/反手连击系统
- 瞄准后释放剑气特殊攻击
- 剑气可穿透3个敌人
- 剑气可击飞子弹
- 完整的视觉和音效特效

安装：
1. 解压到游戏Mods目录
2. 启动游戏并启用Mod
3. 在游戏中生成武器（ID: 10001）

版本: 1.0.0
作者: [你的名字]
```

---

## 📝 更新日志

### v1.0.0 (2025-12-22)
- 初始版本发布
- 实现基础攻击系统
- 实现特殊攻击和剑气
- 实现子弹偏转机制

---

## 💡 开发技巧

### 快速重新构建

```bash
# 清理并重新构建
dotnet clean && dotnet build -c Release
```

### 调试模式

Debug模式编译会包含调试信息：
```bash
dotnet build -c Debug
```

### 监视文件变化

使用文件监视器自动重新编译：
```bash
dotnet watch build
```

---

## 🆘 获取帮助

如果遇到问题：

1. 查看项目文档（特别是weapon_msk.md）
2. 检查游戏日志文件
3. 查看示例Mod：`duckov_modding/DisplayItemValue/`
4. 参考API文档：`duckovAPI/docs/`

---

**构建成功！** 🎉

现在你可以在游戏中测试你的名刀月影了！
