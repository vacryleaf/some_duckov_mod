# 名刀月影 Mod - 项目总览与快速开始

## 🎯 项目概述

**名刀月影** 是为《逃离鸭科夫》游戏开发的自定��近战武器Mod。这把1.5米长的传说级刀可以释放强大的剑气攻击,击飞沿途的子弹并造成伤害。

### 核心功能

✅ **普通攻击**: 正手挥击 + 反手挥击连击系统
✅ **特殊攻击**: 瞄准后向前冲刺3米并释放剑气
✅ **剑气特效**: 飞行10米,可穿透3个敌人
✅ **子弹偏转**: 剑气可击飞沿途的子弹
✅ **完整特效**: 发光、粒子、音效、动画

---

## 📁 项目结构

```
MoonlightSwordMod/
├── weapon_msk.md                    # 主开发手册(必读!)
├── SwordAuraConfig.md               # 剑气系统详细配置
├── Unity/
│   ├── README_Unity.md              # Unity操作指南
│   ├── AnimationConfig.md           # 动画配置文档
│   ├── Editor/
│   │   ├── MoonlightSwordModelGenerator.cs    # 刀模型生成器
│   │   └── SwordAuraEffectGenerator.cs        # 剑气特效生成器
│   ├── Prefabs/                     # Unity Prefab保存位置
│   └── Materials/                   # 材质保存位置
└── Scripts/                         # (待创建) C# Mod代码
    ├── MoonlightSwordModBehaviour.cs
    ├── MoonlightSwordAttack.cs
    └── SwordAuraProjectile.cs
```

---

## 🚀 快速开始流程

### 第一步: 阅读文档 📖

**必读文档顺序**:

1. **weapon_msk.md** - 完整开发手册
   - 了解武器需求和功能
   - 查看技术方案和架构
   - 理解开发步骤

2. **Unity/README_Unity.md** - Unity操作指南
   - 学习如何使用模型生成器
   - 了解AssetBundle打包流程

3. **Unity/AnimationConfig.md** - 动画配置
   - 了解攻击动作设计
   - 学习Animator Controller配置

4. **SwordAuraConfig.md** - 剑气系统
   - 了解剑气投射物机制
   - 学习子弹偏转实现

---

### 第二步: 在Unity中生成模型 🎨

#### 2.1 打开Unity项目

```bash
# 创建新的Unity项目(推荐Unity 2020.3 LTS)
# 或使用现有的游戏Mod开发项目
```

#### 2.2 导入编辑器脚本

```
1. 将 Unity/Editor/ 文件夹复制到项目的 Assets/Editor/
2. Unity会自动编译脚本
3. 在菜单栏会出现 "Tools → 名刀月影" 菜单
```

#### 2.3 生成刀模型

```
1. 点击 Tools → 名刀月影 → 模型生成器
2. 调整参数(使用默认值即可):
   - 刀身长度: 1.2米
   - 刀柄长度: 0.2米
   - 材质颜色: 银白/深蓝/深紫
3. 点击 "生成刀模型"
4. 检查生成的模型
5. 点击 "保存为Prefab"
   - 保存位置: Assets/MoonlightSword/Prefabs/MoonlightSword.prefab
```

#### 2.4 生成剑气特效

```
1. 点击 Tools → 名刀月影 → 剑气特效生成器
2. 调整参数:
   - 宽度: 2米
   - 高度: 1米
   - 粒子数量: 100
3. 点击 "生成剑气特效"
4. 点击 "预览效果" 查看动画
5. 点击 "保存为Prefab"
   - 保存位置: Assets/MoonlightSword/Prefabs/SwordAuraPrefab.prefab
```

#### 2.5 打包AssetBundle

```
1. 选中两个Prefab和所有材质
2. 在Inspector底部设置AssetBundle名: moonlight_sword
3. 点击 Assets → Build AssetBundles (需要先创建构建脚本)
4. 生成的文件在: Assets/AssetBundles/moonlight_sword
```

详细步骤参考: **Unity/README_Unity.md**

---

### 第三步: 编写Mod代码 💻

#### 3.1 创建C#项目

```bash
cd MoonlightSwordMod
mkdir Scripts
cd Scripts

# 创建 .NET Standard 2.1 项目
# 参考 weapon_msk.md 中的项目配置
```

#### 3.2 实现核心脚本

需要创建以下脚本(代码已在weapon_msk.md中):

```
✅ MoonlightSwordModBehaviour.cs    - Mod主入口
✅ MoonlightSwordAttack.cs          - 攻击逻辑
✅ SwordAuraProjectile.cs           - 剑气投射物
✅ BulletDeflector.cs               - 子弹偏转(可选)
```

所有代码示例都在 **weapon_msk.md** 文档中,可以直接复制使用。

#### 3.3 配置引用

```xml
<ItemGroup>
  <Reference Include="$(DuckovPath)\Duckov_Data\Managed\TeamSoda.Duckov.Core.dll" />
  <Reference Include="$(DuckovPath)\Duckov_Data\Managed\ItemStatsSystem.dll" />
  <Reference Include="$(DuckovPath)\Duckov_Data\Managed\UnityEngine.CoreModule.dll" />
  <!-- 其他必需的DLL -->
</ItemGroup>
```

#### 3.4 编译项目

```bash
# 使用 Visual Studio 编译
# 或使用命令行
dotnet build MoonlightSwordMod.csproj
```

---

### 第四步: 测试Mod 🧪

#### 4.1 部署Mod

```
1. 创建Mod文件夹:
   <游戏目录>/Duckov_Data/Mods/MoonlightSwordMod/

2. 复制文件:
   MoonlightSwordMod/
   ├── MoonlightSwordMod.dll        # 编译的DLL
   ├── info.ini                     # Mod配置
   ├── preview.png                  # 预览图(256x256)
   └── Assets/
       └── moonlight_sword          # AssetBundle文件
```

#### 4.2 配置info.ini

```ini
name=MoonlightSwordMod
displayName=名刀月影
description=添加传说级近战武器"名刀月影",可释放强大的剑气攻击
version=1.0.0
author=你的名字
tags=Weapon,MeleeWeapon,Equipment & Gear
```

#### 4.3 测试步骤

```
1. 启动游戏
2. 进入Mods菜单
3. 启用 "名刀月影" Mod
4. 进入游戏
5. 使用控制台或代码生成武器:
   Item.Instantiate(10001)
6. 测试所有功能(参考weapon_msk.md中的测试清单)
```

---

## 📚 文档说明

### 主文档 - weapon_msk.md

**最重要的文档!** 包含:

- ✅ 完整的武器需求规格
- ✅ 3D模型设计规格
- ✅ Unity Prefab配置指南
- ✅ 完整的代码实现示例
- ✅ 测试步骤和检查清单
- ✅ 开发阶段记录

**适合**: 从零开始了解整个项目

### Unity操作指南 - Unity/README_Unity.md

**Unity实战指南!** 包含:

- ✅ Unity版本和环境要求
- ✅ 使用模型生成器的详细步骤
- ✅ 材质和特效配置方法
- ✅ AssetBundle打包流程
- ✅ 常见问题解答

**适合**: 准备在Unity中创建模型时阅读

### 动画配置 - Unity/AnimationConfig.md

**动画系统完整配置!** 包含:

- ✅ Animator Controller结构
- ✅ 正手/反手挥击动画详解
- ✅ 特殊攻击三阶段动画
- ✅ 动画事件配置
- ✅ 音效和特效时间轴
- ✅ 实现代码示例

**适合**: 配置动画系统时参考

### 剑气系统 - SwordAuraConfig.md

**剑气投射物深度解析!** 包含:

- ✅ 碰撞检测系统
- ✅ 敌人击中和伤害计算
- ✅ 子弹偏转机制详解
- ✅ 视觉特效系统
- ✅ 音效系统
- ✅ 性能优化方案
- ✅ 配置文件格式

**适合**: 实现剑气投射物时参考

---

## 🎓 学习路径推荐

### 对于新手

```
1. 通读 weapon_msk.md → 了解整体
2. 阅读 readme.md → 了解游戏Mod开发能力
3. 查看 weapon.md → 了解武器添加流程
4. 按照 Unity/README_Unity.md → 在Unity中实践
5. 参考代码示例 → 实现功能
```

### 对于有经验的开发者

```
1. 快速浏览 weapon_msk.md → 了解需求
2. 直接使用Unity生成器 → 创建模型
3. 复制代码示例 → 快速实现
4. 参考配置文档 → 细节调优
5. 开始测试和调试
```

---

## ⚙️ 技术要点总结

### 关键技术

| 技术点 | 说明 | 参考文档 |
|--------|------|----------|
| Unity Prefab | 武器模型和特效 | Unity/README_Unity.md |
| Animator Controller | 攻击动画系统 | AnimationConfig.md |
| 投射物系统 | 剑气飞行和碰撞 | SwordAuraConfig.md |
| 动画事件 | 伤害判定和特效触发 | AnimationConfig.md |
| AssetBundle | 资源打包和加载 | Unity/README_Unity.md |
| 物品系统API | 注册和实例化武器 | weapon_msk.md |

### 核心代码文件

| 文件 | 作用 | 行数 |
|------|------|------|
| MoonlightSwordModBehaviour.cs | Mod主入口,加载资源 | ~250行 |
| MoonlightSwordAttack.cs | 攻击逻辑,连击系统 | ~200行 |
| SwordAuraProjectile.cs | 剑气投射物 | ~300行 |
| MoonlightSwordModelGenerator.cs | Unity编辑器工具 | ~400行 |
| SwordAuraEffectGenerator.cs | Unity编辑器工具 | ~250行 |

---

## 📊 开发进度

### ✅ 已完成

- [x] 需求分析和功能设计
- [x] 技术方案设计
- [x] 3D模型规格设计
- [x] Unity编辑器生成器工具
- [x] 动画系统完整配置
- [x] 剑气系统详细设计
- [x] 完整代码示例
- [x] 测试计划和检查清单
- [x] 所有技术文档

### ⏳ 待执行

- [ ] 在Unity中生成模型和特效
- [ ] 打包AssetBundle
- [ ] 创建C#项目并实现代码
- [ ] 编译Mod DLL
- [ ] 在游戏中测试
- [ ] 调优和Bug修复
- [ ] 最终发布

---

## 🎮 测试清单(简化版)

### 基础功能
- [ ] 武器可以正常生成
- [ ] 武器可以装备
- [ ] 正手挥击正常
- [ ] 反手挥击正常
- [ ] 连击系统工作

### 特殊功能
- [ ] 瞄准触发正常
- [ ] 冲刺距离正确(3米)
- [ ] 剑气正确生成
- [ ] 剑气飞行距离正确(10米)
- [ ] 剑气可以穿透敌人(最多3个)
- [ ] 剑气可以偏转子弹

### 视觉效果
- [ ] 刀身发光效果正常
- [ ] 挥击轨迹显示
- [ ] 剑气特效正常
- [ ] 粒子效果流畅

### 音效
- [ ] 挥刀音效正常
- [ ] 剑气飞行音效正常
- [ ] 击中音效正常

完整测试清单参考: **weapon_msk.md 第九章**

---

## 🔧 常见问题

### Q1: 我应该先看哪个文档?
**A**: 先看 **weapon_msk.md**,这是最完整的主文档。

### Q2: 我不会用Unity怎么办?
**A**: 阅读 **Unity/README_Unity.md**,里面有详细的步骤说明。编辑器生成器可以自动创建模型,不需要手动建模。

### Q3: 代码在哪里?
**A**: 所有代码示例都在 **weapon_msk.md** 的 "💻 Mod代码实现" 章节。

### Q4: 如何测试武器?
**A**: 参考 **weapon_msk.md** 的 "🧪 测试步骤" 章节。

### Q5: 子弹偏转如何实现?
**A**: 详细实现在 **SwordAuraConfig.md** 的 "🛡️ 子弹偏转系统" 章节。

---

## 💡 开发建议

### 迭代开发策略

**第一版(最小可用)**:
- 只实现基本刀模型
- 只实现普通攻击(无连击)
- 简化特效

**第二版(核心功能)**:
- 添加连击系统
- 实现特殊攻击
- 添加基础剑气

**第三版(完整版)**:
- 完善所有特效
- 实现子弹偏转
- 优化性能

### 调试技巧

1. **使用Debug.Log大量输出日志**
   ```csharp
   Debug.Log($"[名刀月影] 当前状态: {状态}");
   ```

2. **使用OnDrawGizmos可视化调试**
   ```csharp
   void OnDrawGizmos()
   {
       Gizmos.color = Color.cyan;
       Gizmos.DrawWireSphere(transform.position, attackRange);
   }
   ```

3. **先在Unity场景中测试,再集成到Mod**

---

## 📞 获取帮助

### 查找信息
1. 首先查看对应的技术文档
2. 搜索文档中的关键词
3. 查看代码注释

### 参考资源
- 游戏API文档: `duckovAPI/docs/`
- 示例项目: `duckov_modding/`
- 完整参考: `DuckovCustomModel/`

---

## 🎉 总结

你现在拥有:

✅ **完整的设计文档** - 知道要做什么
✅ **详细的技术方案** - 知道怎么做
✅ **实用的工具脚本** - 可以快速生成模型
✅ **完整的代码示例** - 可以直接使用
✅ **测试计划** - 确保质量

**下一步**: 打开Unity,使用生成器创建你的第一个刀模型! 🎨

---

## 📝 项目信息

- **项目名称**: 名刀月影 (Moonlight Shadow Blade)
- **游戏**: 逃离鸭科夫 (Duckov)
- **类型**: 武器Mod
- **开发阶段**: 设计完成,待实现
- **文档版本**: 1.0
- **创建日期**: 2025-12-22
- **文档作者**: Claude Sonnet 4.5

---

**祝开发顺利!** 🚀

如有疑问,随时查阅对应的技术文档。所有功能都已经详细设计并提供了代码示例,你只需要按步骤实施即可! 💪
