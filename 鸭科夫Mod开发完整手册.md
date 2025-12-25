# 鸭科夫 (Escape from Duckov) Mod 开发完整手册

> 本手册面向初学者，详细介绍物品系统、效果系统、事件注册等核心内容

---

## 目录

### 第一部分：基础与物品系统
1. [环境配置](#一环境配置)
2. [物品生命周期](#二物品生命周期概览)
3. [物品初始化流程](#三核心初始化流程)
4. [物品可用性配置](#四物品可用性配置-canbeused)
5. [效果系统](#五效果系统-effect-system)
6. [物品注册到游戏](#六物品注册到游戏)
7. [完整物品示例](#七完整示例创建可用的治疗物品)
8. [其他重要注意事项](#八其他重要注意事项)
9. [完整物品开发检查清单](#九完整的物品开发检查清单)
10. [非物品系统](#十非物品系统---注册与事件监听)
11. [完整Mod事件注册模板](#十一完整的-mod-事件注册模板)
12. [注册清单总结](#十二注册清单总结)

### 第二部分：游戏核心系统
13. [战斗系统](#十三战斗系统)
14. [经济系统](#十四经济系统)
15. [存档系统](#十五存档系统)
16. [制作系统](#十六制作系统)
17. [任务系统](#十七任务系统)
18. [音频系统](#十八音频系统)
19. [成就系统](#十九成就系统)
20. [难度系统](#二十难度系统)
21. [完整事件系统参考](#二十一完整事件系统参考)

### 附录
22. [系统间依赖关系图](#二十二附录系统间依赖关系图)
23. [API快速查找表](#二十三api快速查找表)

### 第三部分：实战开发教程
24. [C#项目配置完整指南](#二十四c项目配置完整指南)
25. [Unity项目配置完整指南](#二十五unity项目配置完整指南)
26. [Prefab制作详细流程](#二十六prefab制作详细流程)
27. [AssetBundle打包指南](#二十七assetbundle打包指南)
28. [Mod目录结构与info.ini](#二十八mod目录结构与infoini)
29. [调试方法与日志查看](#二十九调试方法与日志查看)
30. [完整实战示例：消耗品物品](#三十完整实战示例消耗品物品)

### 第四部分：游戏内部实现参考
31. [游戏内部实现参考](#三十一游戏内部实现参考)
    - [31.1 生命值与伤害系统 (Health.cs)](#311-生命值与伤害系统-healthcs)
    - [31.2 药物使用行为 (Drug.cs)](#312-药物使用行为-drugcs)
    - [31.3 Buff系统核心 (Buff.cs)](#313-buff系统核心-buffcs)
    - [31.4 添加Buff使用行为 (AddBuff.cs)](#314-添加buff使用行为-addbuffcs)
    - [31.5 商店系统 (StockShop.cs)](#315-商店系统-stockshopcs)
    - [31.6 制作系统 (CraftingManager.cs)](#316-制作系统-craftingmanagercs)
    - [31.7 枪械武器系统 (ItemAgent_Gun.cs)](#317-枪械武器系统-itemagent_guncs)
    - [31.8 近战武器系统 (ItemAgent_MeleeWeapon.cs)](#318-近战武器系统-itemagent_meleeweaponcs)
    - [31.9 AI药物使用行为 (UseDrug.cs)](#319-ai药物使用行为-usedrugcs)

### 第五部分：高级开发教程
32. [高级开发：技能系统](#三十二高级开发技能系统)
    - [32.1 SkillBase 技能基类](#321-skillbase-技能基类)
    - [32.2 自定义技能实现示例](#322-自定义技能实现示例)
    - [32.3 将技能绑定到物品](#323-将技能绑定到物品)
33. [高级开发：自定义Effect组件](#三十三高级开发自定义effect组件)
    - [33.1 自定义 EffectTrigger](#331-自定义-effecttrigger)
    - [33.2 自定义 EffectAction](#332-自定义-effectaction)
    - [33.3 在物品上配置Effect系统](#333-在物品上配置effect系统)
34. [高级开发：近战武器完整示例](#三十四高级开发近战武器完整示例)
    - [34.1 近战武器核心属性](#341-近战武器核心属性)
    - [34.2 近战攻击逻辑实现](#342-近战攻击逻辑实现)
    - [34.3 近战武器物品配置](#343-近战武器物品配置)
35. [总结](#三十五总结)
36. [高级开发：投掷物系统完整实现](#三十六高级开发投掷物系统完整实现)
    - [36.1 投掷物基础属性](#361-投掷物基础属性)
    - [36.2 投掷物发射逻辑](#362-投掷物发射逻辑)
    - [36.3 碰撞检测与音效](#363-碰撞检测与音效)
    - [36.4 爆炸与伤害逻辑](#364-爆炸与伤害逻辑)
    - [36.5 爆炸特效生成](#365-爆炸特效生成)
    - [36.6 投掷物速度计算](#366-投掷物速度计算)
    - [36.7 完整投掷物使用示例](#367-完整投掷物使用示例)

### 第六部分：进阶实战技巧
52. [技能系统深入：SkillContext详解](#五十二技能系统深入skillcontext详解)
    - [52.1 SkillContext 配置参数](#521-skillcontext-配置参数)
    - [52.2 SkillReleaseContext 运行时数据](#522-skillreleasecontext-运行时数据)
    - [52.3 抛物线投掷速度计算](#523-抛物线投掷速度计算)
53. [物品实例化与CreateInstance](#五十三物品实例化与createinstance)
    - [53.1 CreateInstance 方法](#531-createinstance-方法)
    - [53.2 实例化后初始化](#532-实例化后初始化)
54. [Mod信息与目录获取](#五十四mod信息与目录获取)
    - [54.1 info对象](#541-info对象)
    - [54.2 获取Mod目录路径](#542-获取mod目录路径)
55. [Stats属性Hash缓存优化](#五十五stats属性hash缓存优化)
56. [连击与冲刺系统实现](#五十六连击与冲刺系统实现)
    - [56.1 连击索引系统](#561-连击索引系统)
    - [56.2 协程控制攻击时序](#562-协程控制攻击时序)
    - [56.3 冲刺移动实现](#563-冲刺移动实现)
57. [SceneLoader反射调用](#五十七sceneloader反射调用)
58. [调试快捷键系统](#五十八调试快捷键系统)
59. [完整ModBehaviour模板](#五十九完整modbehaviour模板)
60. [开发检查清单汇总（完整版）](#六十开发检查清单汇总完整版)

---

## 一、环境配置

### 1.1 开发环境要求

- **Unity版本**: 2022.3.x（与游戏版本匹配）
- **.NET框架**: .NET Standard 2.1
- **IDE**: Visual Studio 2022 / Rider

### 1.2 项目配置

```xml
<PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
</PropertyGroup>

<ItemGroup>
    <Reference Include="$(DuckovPath)\Duckov_Data\Managed\TeamSoda.Duckov.Core.dll" />
    <Reference Include="$(DuckovPath)\Duckov_Data\Managed\ItemStatsSystem.dll" />
    <Reference Include="$(DuckovPath)\Duckov_Data\Managed\UnityEngine.CoreModule.dll" />
    <!-- 其他需要的Unity模块 -->
</ItemGroup>
```

### 1.3 必需的DLL引用

| DLL文件 | 用途 |
|---------|------|
| TeamSoda.Duckov.Core.dll | 游戏核心逻辑 |
| ItemStatsSystem.dll | 物品统计系统 |
| UnityEngine.CoreModule.dll | Unity核心 |
| UnityEngine.UI.dll | UI系统 |

---

## 二、物品生命周期概览

```
┌─────────────────────────────────────────────────────────────┐
│                    物品完整生命周期                           │
├─────────────────────────────────────────────────────────────┤
│  1. Prefab加载 → 2. AddDynamicEntry注册 → 3. Instantiate    │
│       ↓                    ↓                    ↓           │
│  4. Item.Initialize() → 5. ItemSettingBase.Awake()          │
│       ↓                    ↓                                 │
│  6. Effect系统激活 → 7. Trigger注册事件监听                   │
│       ↓                                                      │
│  8. 物品进入玩家背包/手持 → 9. 触发使用/效果                   │
└─────────────────────────────────────────────────────────────┘
```

---

## 三、核心初始化流程

### 3.1 Item.Initialize() - 物品核心初始化

```csharp
// 源码位置: ItemStatsSystem/Item.cs
public void Initialize()
{
    if (this.initialized) return;
    this.initialized = true;

    // 1. 初始化代理系统（用于手持/穿戴显示）
    this.agentUtilities.Initialize(this);

    // 2. 初始化统计属性（伤害、射速等）
    StatCollection statCollection = this.Stats;
    if (statCollection != null) statCollection.Initialize();

    // 3. 初始化插槽系统（配件、弹药等）
    SlotCollection slotCollection = this.Slots;
    if (slotCollection != null) slotCollection.Initialize();

    // 4. 初始化修改器系统（属性加成）
    ModifierDescriptionCollection modifiers = this.Modifiers;
    if (modifiers != null) modifiers.Initialize();

    // 5. 重新应用所有修改器
    if (this.modifiers != null) this.modifiers.ReapplyModifiers();

    // 6. 激活效果系统
    this.HandleEffectsActive();
}
```

**关键点**：
- 物品必须调用 `Initialize()` 才能正常工作
- 游戏会在实例化物品后自动调用此方法
- 自定义物品的所有组件必须在此之前正确配置

### 3.2 ItemSettingBase - 物品类型标记初始化

```csharp
// 源码位置: TeamSoda.Duckov.Core/ItemSettingBase.cs
public abstract class ItemSettingBase : MonoBehaviour
{
    public Item Item => this.item;

    public void Awake()
    {
        if (this.Item)
        {
            this.SetMarkerParam(this.Item);  // 设置物品类型标记
            this.OnInit();                    // 子类自定义初始化
        }
    }

    // 子类必须实现：设置物品类型参数
    public abstract void SetMarkerParam(Item selfItem);

    // 子类可选实现：额外初始化逻辑
    protected virtual void OnInit() { }
}
```

**物品类型标记的作用**：
- 告诉游戏这是什么类型的物品（枪械、近战、药品等）
- 决定物品可以放入哪些插槽
- 影响AI行为和游戏逻辑

---

## 四、物品可用性配置 (CanBeUsed)

### 4.1 UsageUtilities - 可用性检查核心

```csharp
// 源码位置: ItemStatsSystem/UsageUtilities.cs
public class UsageUtilities
{
    public List<UsageBehavior> behaviors;  // 使用行为列表
    public bool useDurability;              // 是否消耗耐久
    public int durabilityUsage;             // 每次消耗的耐久值

    // 检查物品是否可用
    public bool IsUsable(Item item, object user)
    {
        if (!item) return false;

        // 检查耐久度是否足够
        if (this.useDurability && item.Durability < (float)this.durabilityUsage)
            return false;

        // 遍历所有使用行为，只要有一个可用即返回true
        foreach (UsageBehavior behavior in this.behaviors)
        {
            if (behavior != null && behavior.CanBeUsed(item, user))
                return true;
        }
        return false;
    }

    // 执行使用行为
    public void Use(Item item, object user)
    {
        foreach (UsageBehavior behavior in this.behaviors)
        {
            if (behavior != null && behavior.CanBeUsed(item, user))
                behavior.Use(item, user);
        }

        // 消耗耐久度
        if (this.useDurability && item.UseDurability)
            item.Durability -= (float)this.durabilityUsage;

        // 触发使用事件
        item.NotifyOnUse(user);
    }
}
```

### 4.2 UsageBehavior - 使用行为基类

```csharp
// 源码位置: ItemStatsSystem/UsageBehavior.cs
public abstract class UsageBehavior : MonoBehaviour
{
    // 子类必须实现：判断是否可以使用
    public abstract bool CanBeUsed(Item item, object user);

    // 子类必须实现：执行使用逻辑
    protected abstract void OnUse(Item item, object user);

    public void Use(Item item, object user)
    {
        this.OnUse(item, user);
    }
}
```

### 4.3 各类物品的 CanBeUsed 实现

#### 药品 (Drug)
```csharp
// 源码位置: Duckov/ItemUsage/Drug.cs
public class Drug : UsageBehavior
{
    public int healValue;          // 治疗量
    public bool useDurability;     // 是否消耗耐久
    public float durabilityUsage;  // 消耗量
    public bool canUsePart;        // 是否可部分使用

    public override bool CanBeUsed(Item item, object user)
    {
        CharacterMainControl character = user as CharacterMainControl;
        // 只有当用户是角色，且角色血量未满时可用
        return character && this.CheckCanHeal(character);
    }

    private bool CheckCanHeal(CharacterMainControl character)
    {
        // 治疗量<=0 或 当前血量<最大血量 时可用
        return this.healValue <= 0 ||
               character.Health.CurrentHealth < character.Health.MaxHealth;
    }

    protected override void OnUse(Item item, object user)
    {
        CharacterMainControl character = user as CharacterMainControl;
        if (!character) return;

        float healAmount = (float)this.healValue;

        // 处理部分使用逻辑
        if (this.useDurability && item.UseDurability && this.canUsePart)
        {
            // 计算实际治疗量和耐久消耗
            float needed = character.Health.MaxHealth - character.Health.CurrentHealth;
            healAmount = Mathf.Min(needed, (float)this.healValue);
            // ... 根据实际治疗量计算耐久消耗
        }

        this.Heal(character, item, healAmount);
    }
}
```

#### 食物饮料 (FoodDrink)
```csharp
// 源码位置: Duckov/ItemUsage/FoodDrink.cs
public class FoodDrink : UsageBehavior
{
    public float energyValue;   // 能量恢复
    public float waterValue;    // 水分恢复
    public float UseDurability; // 耐久消耗

    public override bool CanBeUsed(Item item, object user)
    {
        // 只要用户是角色就可以使用（不检查饥饿/口渴状态）
        return user as CharacterMainControl;
    }

    protected override void OnUse(Item item, object user)
    {
        CharacterMainControl character = user as CharacterMainControl;
        if (!character) return;

        // 恢复能量和水分
        if (this.energyValue != 0f) character.AddEnergy(this.energyValue);
        if (this.waterValue != 0f) character.AddWater(this.waterValue);

        // 消耗耐久
        if (this.UseDurability > 0f && item.UseDurability)
            item.Durability -= this.UseDurability;
    }
}
```

#### Buff药剂 (AddBuff)
```csharp
// 源码位置: Duckov/ItemUsage/AddBuff.cs
public class AddBuff : UsageBehavior
{
    public Buff buffPrefab;           // Buff预制体
    [Range(0.01f, 1f)]
    public float chance = 1f;         // 生效概率

    public override bool CanBeUsed(Item item, object user)
    {
        return true;  // Buff药剂总是可用的
    }

    protected override void OnUse(Item item, object user)
    {
        CharacterMainControl character = user as CharacterMainControl;
        if (character == null) return;

        // 概率检查
        if (UnityEngine.Random.Range(0f, 1f) > this.chance) return;

        // 添加Buff
        character.AddBuff(this.buffPrefab, character, 0);
    }
}
```

---

## 五、效果系统 (Effect System)

### 5.1 Effect 组件结构

```csharp
// 源码位置: ItemStatsSystem/Effect.cs
public class Effect : MonoBehaviour
{
    [SerializeField] private Item item;                    // 所属物品
    [SerializeField] private bool display;                 // 是否显示效果描述
    [SerializeField] private string description;           // 效果描述

    internal List<EffectTrigger> triggers;  // 触发器列表
    internal List<EffectFilter> filters;    // 过滤器列表
    internal List<EffectAction> actions;    // 动作列表

    // 触发效果（由Trigger调用）
    internal void Trigger(EffectTriggerEventContext context)
    {
        if (!this.enabled) return;
        if (!this.gameObject.activeInHierarchy) return;

        // 通过所有过滤器检查
        if (!this.EvaluateFilters(context)) return;

        // 执行所有动作
        foreach (EffectAction action in this.actions)
        {
            action.NotifyTriggered(context);
        }
    }
}
```

### 5.2 EffectTrigger - 触发器基类

```csharp
// 源码位置: ItemStatsSystem/EffectTrigger.cs
public class EffectTrigger : EffectComponent
{
    // 触发效果（positive=true为正向触发，false为负向/取消）
    protected void Trigger(bool positive = true)
    {
        base.Master.Trigger(new EffectTriggerEventContext(this, positive));
    }

    // 当Effect设置目标Item时调用
    protected virtual void OnMasterSetTargetItem(Effect effect, Item item) { }

    // 禁用时自动触发负向效果
    protected virtual void OnDisable()
    {
        this.Trigger(false);
    }
}
```

### 5.3 常用触发器实现

#### ItemUsedTrigger - 物品使用触发器
```csharp
// 源码位置: ItemStatsSystem/ItemUsedTrigger.cs
public class ItemUsedTrigger : EffectTrigger
{
    public override string DisplayName => "当物品被使用";

    private void OnEnable()
    {
        // 注册物品使用事件
        if (base.Master?.Item != null)
        {
            base.Master.Item.onUse += this.OnItemUsed;
        }
    }

    private void OnDisable()
    {
        // 取消注册
        base.Master?.Item?.onUse -= this.OnItemUsed;
    }

    private void OnItemUsed(Item item, object user)
    {
        base.Trigger(true);  // 触发效果
    }
}
```

#### OnShootAttackTrigger - 射击/攻击触发器
```csharp
// 源码位置: TeamSoda.Duckov.Core/ItemStatsSystem/OnShootAttackTrigger.cs
[MenuPath("General/On Shoot&Attack")]
public class OnShootAttackTrigger : EffectTrigger
{
    [SerializeField] private bool onShoot = true;   // 射击时触发
    [SerializeField] private bool onAttack = true;  // 近战攻击时触发
    private CharacterMainControl target;

    private void OnEnable() { this.RegisterEvents(); }
    protected override void OnDisable() { base.OnDisable(); this.UnregisterEvents(); }

    private void RegisterEvents()
    {
        this.UnregisterEvents();

        Item item = base.Master?.Item;
        if (item == null) return;

        this.target = item.GetCharacterMainControl();
        if (this.target == null) return;

        // 注册角色射击/攻击事件
        if (this.onShoot) this.target.OnShootEvent += this.OnShootAttack;
        if (this.onAttack) this.target.OnAttackEvent += this.OnShootAttack;
    }

    private void OnShootAttack(DuckovItemAgent agent)
    {
        base.Trigger(true);
    }
}
```

#### OnTakeDamageTrigger - 受伤触发器
```csharp
// 源码位置: TeamSoda.Duckov.Core/ItemStatsSystem/OnTakeDamageTrigger.cs
[MenuPath("General/On Take Damage")]
public class OnTakeDamageTrigger : EffectTrigger
{
    [SerializeField] public int threshold;  // 伤害阈值
    private Health target;

    private void RegisterEvents()
    {
        Item item = base.Master?.Item;
        CharacterMainControl character = item?.GetCharacterMainControl();
        if (character == null) return;

        this.target = character.Health;
        // 注册受伤事件
        this.target.OnHurtEvent.AddListener(this.OnTookDamage);
    }

    private void OnTookDamage(DamageInfo info)
    {
        // 只有伤害超过阈值才触发
        if (info.damageValue < (float)this.threshold) return;
        base.Trigger(true);
    }
}
```

#### TickTrigger - 周期触发器
```csharp
// 源码位置: ItemStatsSystem/TickTrigger.cs
public class TickTrigger : EffectTrigger, IUpdatable
{
    [SerializeField] private float period = 1f;           // 触发周期（秒）
    [SerializeField] private bool allowMultipleTrigger;   // 是否允许单帧多次触发
    private float buffer;

    public override string DisplayName => $"每{this.period}秒";

    private void OnEnable() { UpdatableInvoker.Register(this); }
    private void OnDisable() { UpdatableInvoker.Unregister(this); }

    public void OnUpdate()
    {
        this.buffer += Time.deltaTime / this.period;
        while (this.buffer > 1f)
        {
            this.buffer -= 1f;
            base.Trigger(true);
            if (!this.allowMultipleTrigger) break;
        }
    }
}
```

### 5.4 EffectAction - 动作基类与实现

#### 基类
```csharp
// 源码位置: ItemStatsSystem/EffectAction.cs
public class EffectAction : EffectComponent
{
    // 被触发时调用
    internal void NotifyTriggered(EffectTriggerEventContext context)
    {
        if (!this.enabled) return;

        this.OnTriggered(context.positive);

        if (context.positive)
            this.OnTriggeredPositive();
        else
            this.OnTriggeredNegative();
    }

    protected virtual void OnTriggered(bool positive) { }
    protected virtual void OnTriggeredPositive() { }
    protected virtual void OnTriggeredNegative() { }
}
```

#### ModifierAction - 属性修改动作
```csharp
// 源码位置: TeamSoda.Duckov.Core/ModifierAction.cs
public class ModifierAction : EffectAction
{
    public string targetStatKey;          // 目标属性名
    public ModifierType ModifierType;     // 修改类型（加法/乘法等）
    public float modifierValue;           // 修改值
    private Modifier modifier;
    private Stat targetStat;

    protected override void Awake()
    {
        base.Awake();
        // 创建修改器
        this.modifier = new Modifier(
            this.ModifierType,
            this.modifierValue,
            this.overrideOrder,
            this.overrideOrderValue,
            base.Master
        );
    }

    protected override void OnTriggered(bool positive)
    {
        Item characterItem = base.Master.Item?.GetCharacterItem();
        if (characterItem == null) return;

        if (positive)
        {
            // 正向触发：添加属性修改
            this.targetStat?.RemoveModifier(this.modifier);
            this.targetStat = characterItem.GetStat(this.targetStatKey.GetHashCode());
            this.targetStat.AddModifier(this.modifier);
        }
        else
        {
            // 负向触发：移除属性修改
            this.targetStat?.RemoveModifier(this.modifier);
            this.targetStat = null;
        }
    }
}
```

#### AddBuffAction - 添加Buff动作
```csharp
// 源码位置: TeamSoda.Duckov.Core/AddBuffAction.cs
public class AddBuffAction : EffectAction
{
    public Buff buffPfb;

    private CharacterMainControl MainControl =>
        base.Master?.Item?.GetCharacterMainControl();

    protected override void OnTriggered(bool positive)
    {
        if (!this.MainControl) return;
        this.MainControl.AddBuff(this.buffPfb, this.MainControl, 0);
    }
}
```

#### HealAction - 治疗动作
```csharp
// 源码位置: TeamSoda.Duckov.Core/HealAction.cs
public class HealAction : EffectAction
{
    public int healValue = 10;

    protected override void OnTriggered(bool positive)
    {
        CharacterMainControl mainControl = base.Master?.Item?.GetCharacterMainControl();
        if (!mainControl) return;
        mainControl.Health.AddHealth((float)this.healValue);
    }
}
```

---

## 六、物品注册到游戏

### 6.1 ItemAssetsCollection - 物品资产注册

```csharp
// 源码位置: ItemStatsSystem/ItemAssetsCollection.cs
public class ItemAssetsCollection : ScriptableObject
{
    private static Dictionary<int, DynamicEntry> dynamicDic = new Dictionary<int, DynamicEntry>();

    // 添加动态物品（Mod使用）
    public static bool AddDynamicEntry(Item prefab)
    {
        if (prefab == null) return false;
        if (Instance == null) return false;

        int typeID = prefab.TypeID;

        // 检查ID冲突
        if (Instance.entries.Any(e => e?.typeID == typeID))
        {
            Debug.LogWarning($"Warning: TypeID {typeID} collides with main game.");
        }

        // 添加到动态字典
        dynamicDic[typeID] = new DynamicEntry { typeID = typeID, prefab = prefab };
        return true;
    }

    // 移除动态物品
    public static bool RemoveDynamicEntry(Item prefab)
    {
        if (prefab == null) return false;
        return dynamicDic.Remove(prefab.TypeID);
    }

    // 同步实例化物品
    public static Item InstantiateSync(int typeID)
    {
        // 优先从动态字典获取
        if (TryGetDynamicEntry(typeID, out DynamicEntry entry))
        {
            return Object.Instantiate(entry.prefab);
        }

        // 从游戏本体获取
        Entry gameEntry = Instance.GetEntry(typeID);
        if (gameEntry?.prefab == null) return null;
        return Object.Instantiate(gameEntry.prefab);
    }

    // 异步实例化物品
    public static async UniTask<Item> InstantiateAsync(int typeID) { ... }
}
```

### 6.2 完整的Mod注册流程

```csharp
// 完整的ModBehaviour实现
using UnityEngine;
using Duckov.Modding;
using ItemStatsSystem;
using SodaCraft.Localizations;
using Cysharp.Threading.Tasks;

public class MyItemMod : ModBehaviour
{
    private Item myItemPrefab;
    private bool registered = false;

    async void Start()
    {
        Debug.Log("[MyItemMod] 开始加载...");

        // 1. 加载AssetBundle
        string bundlePath = System.IO.Path.Combine(info.path, "Assets", "myitems.bundle");
        AssetBundle bundle = await AssetBundle.LoadFromFileAsync(bundlePath);

        // 2. 加载物品Prefab
        myItemPrefab = bundle.LoadAsset<Item>("MyCustomItem");

        if (myItemPrefab == null)
        {
            Debug.LogError("[MyItemMod] 物品Prefab加载失败!");
            return;
        }

        // 3. 设置TypeID（必须唯一，建议10000+）
        // 注意：TypeID通常在Unity编辑器中设置，这里只是示例

        // 4. 注册到游戏
        registered = ItemAssetsCollection.AddDynamicEntry(myItemPrefab);

        if (registered)
        {
            Debug.Log($"[MyItemMod] 物品注册成功! TypeID: {myItemPrefab.TypeID}");

            // 5. 注册本地化
            RegisterLocalization();
        }
        else
        {
            Debug.LogError("[MyItemMod] 物品注册失败!");
        }
    }

    void RegisterLocalization()
    {
        // 注册物品名称
        LocalizationManager.SetOverrideText($"Item_{myItemPrefab.TypeID}_Name", "自定义武器");
        LocalizationManager.SetOverrideText($"Item_{myItemPrefab.TypeID}_Desc", "这是一把自定义武器");
    }

    protected override void OnBeforeDeactivate()
    {
        // Mod卸载时清理
        if (registered && myItemPrefab != null)
        {
            ItemAssetsCollection.RemoveDynamicEntry(myItemPrefab);
            Debug.Log("[MyItemMod] 物品已从游戏中移除");
        }
    }
}
```

---

## 七、完整示例：创建可用的治疗物品

### 7.1 Prefab结构

```
MyHealingItem (GameObject)
├── Item (Component)
│   ├── TypeID: 10001
│   ├── DisplayName: "自定义治疗包"
│   ├── MaxStackCount: 5
│   ├── Stackable: true
│   ├── MaxDurability: 100
│   └── UsageUtilities
│       ├── useDurability: true
│       ├── durabilityUsage: 25
│       └── behaviors: [Drug组件引用]
│
├── Drug (Component) - UsageBehavior实现
│   ├── healValue: 50
│   ├── useDurability: true
│   ├── durabilityUsage: 25
│   └── canUsePart: true
│
└── Effects (Child GameObject)
    └── Effect (Component)
        ├── description: "使用后恢复50点生命"
        ├── triggers: [ItemUsedTrigger]
        ├── filters: []
        └── actions: [AddBuffAction]
            └── buffPfb: 再生Buff
```

### 7.2 代码实现

```csharp
// 在Unity中通过代码创建完整可用的物品
public Item CreateHealingItem()
{
    // 1. 创建根物体
    GameObject itemObj = new GameObject("MyHealingItem");

    // 2. 添加Item组件并配置
    Item item = itemObj.AddComponent<Item>();
    item.TypeID = 10001;  // 需要通过反射或序列化设置
    // ... 其他Item属性

    // 3. 添加UsageBehavior
    Drug drugBehavior = itemObj.AddComponent<Drug>();
    drugBehavior.healValue = 50;
    drugBehavior.useDurability = true;
    drugBehavior.durabilityUsage = 25f;
    drugBehavior.canUsePart = true;

    // 4. 配置UsageUtilities（通过反射访问）
    // item.usageUtilities.behaviors.Add(drugBehavior);
    // item.usageUtilities.useDurability = true;
    // item.usageUtilities.durabilityUsage = 25;

    // 5. 创建Effect系统（可选，用于额外效果）
    GameObject effectObj = new GameObject("Effect_HealBuff");
    effectObj.transform.SetParent(itemObj.transform);

    Effect effect = effectObj.AddComponent<Effect>();

    // 添加触发器
    ItemUsedTrigger trigger = effectObj.AddComponent<ItemUsedTrigger>();
    effect.triggers.Add(trigger);

    // 添加动作
    AddBuffAction buffAction = effectObj.AddComponent<AddBuffAction>();
    // buffAction.buffPfb = 再生Buff预制体;
    effect.actions.Add(buffAction);

    return item;
}
```

---

## 八、其他重要注意事项

### 8.1 ItemAgent 代理系统（手持/穿戴显示）

```csharp
// 如果物品需要在角色手中显示（武器、工具等），必须配置ItemAgentUtilities
// 源码位置: ItemStatsSystem/ItemAgentUtilities.cs

// AgentTypes 枚举
public enum AgentTypes
{
    normal,     // 普通显示
    pickUp,     // 掉落物显示
    handheld,   // 手持显示（武器必须）
    equipment   // 装备显示（护甲等）
}

// Prefab中需要配置agents列表：
// ItemAgentUtilities.agents = [
//   { key: "handheld", agentPrefab: 手持模型预制体 },
//   { key: "pickup", agentPrefab: 掉落物预制体 }
// ]
```

**注意**：没有配置 `handheld` Agent 的武器，玩家拿在手里会是空的！

### 8.2 插槽系统（配件/弹药）

```csharp
// 武器配件插槽配置
// Item.Slots 需要配置允许安装的配件类型

// 插槽配置示例：
SlotCollection slots;
// 瞄准镜插槽
slots.Add(new Slot {
    Key = "Sight",
    DisplayName = "瞄准镜",
    AllowedTypes = new ItemFilter { tags = new[] { "Attachment_Sight" } }
});
// 弹匣插槽
slots.Add(new Slot {
    Key = "Magazine",
    DisplayName = "弹匣",
    AllowedTypes = new ItemFilter { tags = new[] { "Magazine" }, caliber = "5.56mm" }
});
```

### 8.3 存档兼容性问题

```
⚠️ 重要警告：
- 动态注册的物品（AddDynamicEntry）存档后，如果Mod未加载，物品会消失
- TypeID 一旦确定，不要更改，否则玩家存档中的物品会丢失
- 建议在Mod描述中声明使用的TypeID范围，避免与其他Mod冲突

推荐TypeID分配：
- 10000-19999: 武器类
- 20000-29999: 护甲/装备类
- 30000-39999: 消耗品类
- 40000-49999: 材料类
- 50000+: 其他
```

### 8.4 Unity AssetBundle 打包注意

```csharp
// 1. Unity版本必须匹配游戏使用的版本
// 鸭科夫使用: Unity 2022.3.x

// 2. 打包脚本示例
[MenuItem("Build/Build AssetBundles")]
static void BuildAssetBundles()
{
    BuildPipeline.BuildAssetBundles(
        "Assets/AssetBundles",
        BuildAssetBundleOptions.None,
        BuildTarget.StandaloneWindows64  // 或 StandaloneOSX
    );
}

// 3. 加载时注意路径
string bundlePath = Path.Combine(info.path, "Assets", "mymod.bundle");
AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

// 4. 卸载时必须释放
void OnDestroy()
{
    if (bundle != null)
    {
        bundle.Unload(true);  // true = 同时卸载已加载的资源
    }
}
```

### 8.5 ItemGraphicInfo（UI显示配置）

```csharp
// 物品在UI中的显示配置，影响背包、商店等界面
[Serializable]
public class ItemGraphicInfo
{
    public GameObject DisplayPrefab;      // UI显示用预制体
    public Vector3 PreviewCameraOffset;   // 预览相机偏移
    public Vector3 PreviewRotation;       // 预览旋转
    public float PreviewScale = 1f;       // 预览缩放
}

// 如果不配置，物品在UI中可能显示异常或不显示
```

### 8.6 Modifier 修改器顺序

```csharp
// 修改器按顺序应用，顺序很重要！
public enum ModifierType
{
    FlatAdd,        // 1. 先加法 (+10)
    PercentAdd,     // 2. 再百分比加法 (+10%)
    PercentMult,    // 3. 最后百分比乘法 (*1.1)
    Override        // 特殊：直接覆盖
}

// 计算公式：
// FinalValue = (BaseValue + FlatAdd) * (1 + PercentAdd) * PercentMult

// 示例：基础伤害100，+20平加，+10%百分比加，*1.5乘法
// = (100 + 20) * (1 + 0.1) * 1.5 = 198
```

### 8.7 调试技巧

```csharp
// 1. 日志位置
// Windows: %USERPROFILE%\AppData\LocalLow\TeamSoda\Duckov\Player.log
// macOS: ~/Library/Logs/TeamSoda/Duckov/Player.log

// 2. 在代码中添加详细日志
Debug.Log($"[MyMod] Item registered: {item.TypeID}, Name: {item.DisplayName}");
Debug.Log($"[MyMod] UsageBehaviors count: {item.UsageUtilities?.behaviors?.Count}");
Debug.Log($"[MyMod] Effects count: {item.Effects?.Count}");

// 3. 检查物品是否正确初始化
void ValidateItem(Item item)
{
    Debug.Log($"Initialized: {item.Initialized}");
    Debug.Log($"TypeID: {item.TypeID}");
    Debug.Log($"UsageUtilities: {item.UsageUtilities != null}");
    Debug.Log($"Behaviors: {item.UsageUtilities?.behaviors?.Count ?? 0}");
    Debug.Log($"AgentUtilities: {item.AgentUtilities != null}");
    Debug.Log($"Stats: {item.Stats?.Count ?? 0}");
    Debug.Log($"Effects: {item.Effects?.Count ?? 0}");
}

// 4. 测试物品生成（需要在游戏中）
async void TestSpawnItem()
{
    Item item = await ItemAssetsCollection.InstantiateAsync(10001);
    if (item != null)
    {
        ItemUtilities.SendToPlayer(item);  // 发送到玩家背包
        Debug.Log("物品已生成并发送给玩家");
    }
}
```

### 8.8 常见错误清单

| 错误现象 | 可能原因 | 解决方案 |
|---------|---------|---------|
| 物品无法使用 | UsageBehavior未添加到behaviors列表 | 检查UsageUtilities配置 |
| 效果不触发 | Trigger的OnEnable未被调用 | 检查GameObject是否active |
| 手持物品不显示 | 未配置handheld Agent | 添加ItemAgentUtilities配置 |
| UI图标不显示 | Icon未设置或Sprite无效 | 检查Item.Icon配置 |
| 游戏崩溃 | TypeID冲突或空引用 | 使用唯一ID，添加null检查 |
| 存档后物品消失 | Mod未加载时TypeID无效 | 这是正常行为，需告知玩家 |
| 配件无法安装 | 插槽Filter不匹配 | 检查ItemFilter的tags和caliber |
| 属性修改无效 | Stat key不正确 | 使用正确的属性键名 |

### 8.9 性能优化建议

```csharp
// 1. 避免在Update中频繁查找
// ❌ 错误
void Update()
{
    var character = item.GetCharacterMainControl();  // 每帧查找
}

// ✅ 正确
private CharacterMainControl cachedCharacter;
void OnEnable()
{
    cachedCharacter = item.GetCharacterMainControl();  // 缓存
}

// 2. 使用对象池管理频繁创建的物体（如子弹、特效）

// 3. AssetBundle加载后缓存引用，不要重复加载

// 4. Effect系统的Filter可以减少不必要的Action执行
```

### 8.10 版本兼容性

```
⚠️ 游戏更新可能导致API变化：

1. 每次游戏更新后，检查：
   - DLL引用是否需要更新
   - API签名是否变化
   - 新增/删除的方法

2. 建议做法：
   - 使用try-catch包裹关键代码
   - 在Mod描述中标注兼容的游戏版本
   - 订阅游戏更新公告

3. 常用版本检查：
   // 获取游戏版本
   string gameVersion = Application.version;
   Debug.Log($"Game Version: {gameVersion}");
```

---

## 九、完整的物品开发检查清单

```
□ 基础配置
  □ TypeID 唯一且在推荐范围内
  □ DisplayName 已设置
  □ Icon Sprite 已配置
  □ 品质(Quality)已设置
  □ 重量(UnitSelfWeight)已设置

□ 使用系统（如果可使用）
  □ UsageUtilities 已配置
  □ UsageBehavior 组件已添加
  □ UsageBehavior 已加入 behaviors 列表
  □ CanBeUsed() 逻辑正确
  □ OnUse() 逻辑正确

□ 显示系统
  □ ItemAgentUtilities 已配置（如果需要显示）
  □ handheld Agent 预制体已设置（武器）
  □ pickup Agent 预制体已设置（掉落物）
  □ ItemGraphicInfo 已配置（UI显示）

□ 效果系统（如果有效果）
  □ Effect 组件已添加
  □ 至少一个 Trigger
  □ 至少一个 Action
  □ Trigger 正确注册事件

□ 其他
  □ 本地化文本已注册
  □ 已在测试环境验证功能
  □ 已处理Mod卸载时的清理
  □ 日志输出便于调试
```

---

## 十、非物品系统 - 注册与事件监听

### 10.1 全局事件系统

#### LevelManager 事件（关卡生命周期）
```csharp
// 关卡初始化事件 - 适合初始化Mod数据
public static event Action OnLevelBeginInitializing;  // 关卡开始初始化
public static event Action OnLevelInitialized;        // 关卡初始化完成
public static event Action OnAfterLevelInitialized;   // 关卡初始化后

// 使用示例
public class MyMod : ModBehaviour
{
    void Start()
    {
        LevelManager.OnLevelInitialized += OnLevelReady;
    }

    void OnDestroy()
    {
        LevelManager.OnLevelInitialized -= OnLevelReady;  // 必须取消注册！
    }

    void OnLevelReady()
    {
        Debug.Log("关卡已加载，可以访问主角了");
        var player = LevelManager.Instance.MainCharacter;
    }
}
```

### 10.2 角色事件系统 (CharacterMainControl)

```csharp
// 可用的角色事件
public event Action<Teams> OnTeamChanged;                    // 阵营改变
public event Action<CharacterMainControl, Vector3> OnSetPositionEvent;  // 位置设置
public event Action<DamageInfo> BeforeCharacterSpawnLootOnDead;  // 死亡掉落前
public event Action<CharacterActionBase> OnActionStartEvent;  // 动作开始
public event Action<CharacterActionBase> OnActionProgressFinishEvent;  // 动作完成
public event Action<DuckovItemAgent> OnHoldAgentChanged;      // 手持物品改变
public event Action<DuckovItemAgent> OnShootEvent;            // 射击事件
public event Action<DuckovItemAgent> OnAttackEvent;           // 近战攻击事件
public event Action OnSkillStartReleaseEvent;                 // 技能释放事件
public static event Action<Item> OnMainCharacterStartUseItem; // 主角使用物品（静态）

// 使用示例：监听玩家射击
void RegisterPlayerEvents()
{
    var player = LevelManager.Instance.MainCharacter;
    if (player != null)
    {
        player.OnShootEvent += OnPlayerShoot;
        player.OnAttackEvent += OnPlayerAttack;
    }
}

void OnPlayerShoot(DuckovItemAgent agent)
{
    Debug.Log($"玩家使用 {agent.Item.DisplayName} 射击了");
}
```

### 10.3 生命值系统 (Health)

```csharp
// Health 组件的事件
public UnityEvent<Health> OnHealthChange;      // 血量变化
public UnityEvent<Health> OnMaxHealthChange;   // 最大血量变化
public UnityEvent<DamageInfo> OnHurtEvent;     // 受伤事件
public UnityEvent<DamageInfo> OnDeadEvent;     // 死亡事件

// 使用示例：监听玩家受伤
void Start()
{
    LevelManager.OnLevelInitialized += () => {
        var player = LevelManager.Instance.MainCharacter;
        if (player?.Health != null)
        {
            player.Health.OnHurtEvent.AddListener(OnPlayerHurt);
            player.Health.OnDeadEvent.AddListener(OnPlayerDead);
        }
    };
}

void OnPlayerHurt(DamageInfo info)
{
    Debug.Log($"玩家受到 {info.damageValue} 点伤害");
}

void OnPlayerDead(DamageInfo info)
{
    Debug.Log("玩家死亡了");
}
```

### 10.4 Buff 系统

```csharp
// Buff 不需要预注册，但需要正确创建预制体
// Buff 的生命周期由 CharacterBuffManager 管理

// Buff 类重要属性和方法
public class Buff : MonoBehaviour
{
    public int ID;                      // Buff唯一ID
    public int maxLayers;               // 最大层数
    public BuffExclusiveTags ExclusiveTag;  // 互斥标签
    public bool limitedLifeTime;        // 是否限时
    public float totalLifeTime;         // 总持续时间
    public List<Effect> effects;        // 附带的效果

    // 子类可重写的方法
    protected virtual void OnSetup() { }           // Buff添加时
    protected virtual void OnUpdate() { }          // 每帧更新
    protected virtual void OnNotifiedOutOfTime() { }  // 时间到期时

    public event Action OnLayerChangedEvent;       // 层数变化事件
}

// CharacterBuffManager 事件
public event Action<CharacterBuffManager, Buff> onAddBuff;     // 添加Buff
public event Action<CharacterBuffManager, Buff> onRemoveBuff;  // 移除Buff

// 使用示例：监听Buff变化
void MonitorBuffs()
{
    var player = LevelManager.Instance.MainCharacter;
    var buffManager = player.GetComponent<CharacterBuffManager>();

    buffManager.onAddBuff += (manager, buff) => {
        Debug.Log($"获得Buff: {buff.DisplayName}");
    };

    buffManager.onRemoveBuff += (manager, buff) => {
        Debug.Log($"失去Buff: {buff.DisplayName}");
    };
}

// 添加自定义Buff
void AddCustomBuff(CharacterMainControl character, Buff buffPrefab)
{
    character.AddBuff(buffPrefab, character, 0);
}
```

### 10.5 更新系统 (IUpdatable)

```csharp
// 如果你的组件需要每帧更新但不想用MonoBehaviour.Update
// 可以实现IUpdatable接口并注册到UpdatableInvoker

public interface IUpdatable
{
    void OnUpdate();
}

// 使用示例
public class MyCustomUpdatable : IUpdatable
{
    public void Register()
    {
        UpdatableInvoker.Register(this);
    }

    public void Unregister()
    {
        UpdatableInvoker.Unregister(this);
    }

    public void OnUpdate()
    {
        // 每帧执行的逻辑
    }
}

// 注意：必须在不需要时调用 Unregister，否则会内存泄漏！
```

### 10.6 物品全局事件

```csharp
// Item 类的静态事件
public static event Action<Item, object> onUseStatic;  // 任何物品被使用时

// 使用示例：监听所有物品使用
void Start()
{
    Item.onUseStatic += OnAnyItemUsed;
}

void OnDestroy()
{
    Item.onUseStatic -= OnAnyItemUsed;
}

void OnAnyItemUsed(Item item, object user)
{
    Debug.Log($"物品 {item.DisplayName} 被使用了");
}
```

### 10.7 游戏管理器访问

```csharp
// 通过 GameManager 访问各种系统
GameManager.AudioManager          // 音频管理器
GameManager.ModManager            // Mod管理器
GameManager.AchievementManager    // 成就管理器
GameManager.DifficultyManager     // 难度管理器
GameManager.SceneLoader           // 场景加载器
GameManager.PauseMenu             // 暂停菜单
GameManager.Paused                // 是否暂停

// 通过 LevelManager 访问关卡内容
LevelManager.Instance.MainCharacter     // 主角
LevelManager.Instance.PetCharacter      // 宠物
LevelManager.Instance.GameCamera        // 游戏相机
LevelManager.Instance.TimeOfDayController  // 时间控制器
LevelManager.Instance.ExplosionManager  // 爆炸管理器
```

---

## 十一、完整的 Mod 事件注册模板

```csharp
using UnityEngine;
using Duckov.Modding;
using Duckov.Buffs;
using ItemStatsSystem;

public class CompleteMod : ModBehaviour
{
    private CharacterMainControl player;
    private bool eventsRegistered = false;

    void Start()
    {
        Debug.Log("[CompleteMod] 加载中...");

        // 注册关卡事件
        LevelManager.OnLevelInitialized += OnLevelReady;

        // 注册全局物品事件
        Item.onUseStatic += OnAnyItemUsed;
    }

    void OnLevelReady()
    {
        Debug.Log("[CompleteMod] 关卡已加载");

        player = LevelManager.Instance?.MainCharacter;
        if (player != null)
        {
            RegisterPlayerEvents();
        }
    }

    void RegisterPlayerEvents()
    {
        if (eventsRegistered) return;
        eventsRegistered = true;

        // 角色事件
        player.OnShootEvent += OnPlayerShoot;
        player.OnAttackEvent += OnPlayerAttack;
        player.OnHoldAgentChanged += OnWeaponChanged;

        // 生命值事件
        if (player.Health != null)
        {
            player.Health.OnHurtEvent.AddListener(OnPlayerHurt);
            player.Health.OnDeadEvent.AddListener(OnPlayerDead);
        }

        // Buff事件
        var buffManager = player.GetComponent<CharacterBuffManager>();
        if (buffManager != null)
        {
            buffManager.onAddBuff += OnBuffAdded;
            buffManager.onRemoveBuff += OnBuffRemoved;
        }

        Debug.Log("[CompleteMod] 事件注册完成");
    }

    void UnregisterPlayerEvents()
    {
        if (!eventsRegistered || player == null) return;

        player.OnShootEvent -= OnPlayerShoot;
        player.OnAttackEvent -= OnPlayerAttack;
        player.OnHoldAgentChanged -= OnWeaponChanged;

        if (player.Health != null)
        {
            player.Health.OnHurtEvent.RemoveListener(OnPlayerHurt);
            player.Health.OnDeadEvent.RemoveListener(OnPlayerDead);
        }

        var buffManager = player.GetComponent<CharacterBuffManager>();
        if (buffManager != null)
        {
            buffManager.onAddBuff -= OnBuffAdded;
            buffManager.onRemoveBuff -= OnBuffRemoved;
        }

        eventsRegistered = false;
    }

    // 事件处理方法
    void OnPlayerShoot(DuckovItemAgent agent) { }
    void OnPlayerAttack(DuckovItemAgent agent) { }
    void OnWeaponChanged(DuckovItemAgent agent) { }
    void OnPlayerHurt(DamageInfo info) { }
    void OnPlayerDead(DamageInfo info) { }
    void OnBuffAdded(CharacterBuffManager mgr, Buff buff) { }
    void OnBuffRemoved(CharacterBuffManager mgr, Buff buff) { }
    void OnAnyItemUsed(Item item, object user) { }

    protected override void OnBeforeDeactivate()
    {
        // ⚠️ 关键：Mod卸载时必须取消所有注册！
        LevelManager.OnLevelInitialized -= OnLevelReady;
        Item.onUseStatic -= OnAnyItemUsed;
        UnregisterPlayerEvents();

        Debug.Log("[CompleteMod] 已卸载，事件已清理");
    }
}
```

---

## 十二、注册清单总结

| 系统 | 需要注册 | 注册方法 | 卸载时需清理 |
|------|---------|---------|-------------|
| 自定义物品 | ✅ | `ItemAssetsCollection.AddDynamicEntry()` | `RemoveDynamicEntry()` |
| 关卡事件 | ✅ | `LevelManager.OnXXX += handler` | `-= handler` |
| 角色事件 | ✅ | `character.OnXXX += handler` | `-= handler` |
| 生命值事件 | ✅ | `health.OnXXX.AddListener()` | `RemoveListener()` |
| Buff事件 | ✅ | `buffManager.onXXX += handler` | `-= handler` |
| 全局物品事件 | ✅ | `Item.onUseStatic += handler` | `-= handler` |
| IUpdatable | ✅ | `UpdatableInvoker.Register()` | `Unregister()` |
| 自定义Buff | ❌ | 直接通过预制体添加 | 自动管理 |
| 本地化文本 | ✅ | `LocalizationManager.SetOverrideText()` | 不需要 |

---

## 关键要点总结

### 让物品可以使用

| 组件 | 作用 | 必需性 |
|------|------|--------|
| `Item` | 物品核心组件 | **必需** |
| `UsageUtilities` | 使用系统配置 | **必需**（在Item内） |
| `UsageBehavior` | 具体使用行为 | **必需** |
| `ItemSettingBase` | 物品类型标记 | 推荐 |
| `Effect` | 效果系统 | 可选 |

### 让效果生效

```
Effect生效条件：
1. Effect组件enabled = true
2. GameObject.activeInHierarchy = true
3. 至少一个Trigger
4. 至少一个Action
5. 所有Filter通过检查（如果有的话）
6. Trigger正确注册到对应事件
```

### 事件注册时机

| 触发器 | 注册时机 | 注册的事件 |
|--------|----------|------------|
| ItemUsedTrigger | OnEnable | Item.onUse |
| OnShootAttackTrigger | OnEnable | CharacterMainControl.OnShootEvent/OnAttackEvent |
| OnTakeDamageTrigger | OnEnable | Health.OnHurtEvent |
| TickTrigger | OnEnable | UpdatableInvoker（每帧更新） |

### 常见问题排查

```
物品无法使用？
├── 检查 UsageUtilities.behaviors 是否包含 UsageBehavior
├── 检查 UsageBehavior.CanBeUsed() 返回值
├── 检查 Item.Durability 是否足够
└── 检查 UsageUtilities.useDurability 和 durabilityUsage 配置

效果不触发？
├── 检查 Effect.enabled 是否为 true
├── 检查 triggers 列表是否为空
├── 检查 Trigger 是否正确注册事件（OnEnable被调用）
├── 检查 actions 列表是否为空
└── 检查 Filter 是否阻止了触发

物品无法在游戏中获取？
├── 检查 ItemAssetsCollection.AddDynamicEntry() 返回值
├── 检查 TypeID 是否与其他物品冲突
└── 检查 Prefab 是否正确加载
```

---

**⚠️ 最重要的原则：所有 `+=` 注册的事件，必须在 `OnBeforeDeactivate()` 或 `OnDestroy()` 中用 `-=` 取消注册，否则会导致：**
1. 内存泄漏
2. Mod卸载后代码仍然执行
3. 空引用异常导致游戏崩溃

---

## 参考资源

- API文档：`duckovAPI/` 目录
- 示例项目：`duckov_modding/DisplayItemValue/`
- 完整项目参考：`DuckovCustomModel/`
- 物品系统文档：`duckovAPI/docs/systems/items.md`
- 反编译源码：`duckovAPI/Decompilation/`

---

## 十三、战斗系统

### 13.1 Health 生命值组件

```csharp
// Health 重要属性
public float MaxHealth;              // 最大生命值
public float CurrentHealth;          // 当前生命值
public bool IsDead;                  // 是否死亡
public bool Invincible;              // 是否无敌
public float BodyArmor;              // 身体护甲
public float HeadArmor;              // 头部护甲

// Health 重要事件
public UnityEvent<DamageInfo> OnHurtEvent;     // 受伤事件
public UnityEvent<DamageInfo> OnDeadEvent;     // 死亡事件
public UnityEvent<Health> OnHealthChange;      // 血量变化事件
public UnityEvent<Health> OnMaxHealthChange;   // 最大血量变化事件

// 常用方法
health.AddHealth(50f);                         // 恢复生命值
health.SetHealth(100f);                        // 设置生命值
health.SetInvincible(true);                    // 设置无敌
health.Hurt(damageInfo);                       // 造成伤害
```

### 13.2 DamageInfo 伤害信息

```csharp
// 创建伤害信息
DamageInfo damageInfo = new DamageInfo();
damageInfo.damageValue = 100f;                  // 伤害值
damageInfo.damageType = DamageTypes.physics;    // 伤害类型
damageInfo.fromCharacter = attacker;            // 伤害来源
damageInfo.hitPoint = hitPosition;              // 命中点
damageInfo.isCritical = false;                  // 是否暴击
damageInfo.elementType = ElementTypes.physics;  // 元素类型

// 元素类型枚举
public enum ElementTypes
{
    physics,      // 物理伤害
    fire,         // 火焰伤害
    poison,       // 毒素伤害
    electricity,  // 电击伤害
    space         // 空间伤害
}
```

### 13.3 战斗事件监听

```csharp
// 监听玩家受伤
void Start()
{
    LevelManager.OnLevelInitialized += () => {
        var player = LevelManager.Instance.MainCharacter;
        player.Health.OnHurtEvent.AddListener(OnPlayerHurt);
        player.Health.OnDeadEvent.AddListener(OnPlayerDead);
    };
}

void OnPlayerHurt(DamageInfo info)
{
    Debug.Log($"受到 {info.damageValue} 点 {info.elementType} 伤害");
    if (info.isCritical) Debug.Log("暴击！");
}

void OnPlayerDead(DamageInfo info)
{
    Debug.Log("玩家死亡");
}
```

---

## 十四、经济系统

### 14.1 EconomyManager 经济管理器

```csharp
// 重要属性
EconomyManager.Instance                        // 获取实例

// 重要事件
public static event Action OnEconomyManagerLoaded;    // 经济管理器加载
public static event Action<string> OnItemUnlocked;    // 物品解锁
public static event Action<int> OnCurrencyChanged;    // 货币改变

// 重要方法
EconomyManager.AddCurrency(1000);               // 添加货币
EconomyManager.RemoveCurrency(500);             // 移除货币
int money = EconomyManager.GetCurrency();       // 获取当前货币

EconomyManager.UnlockItem("Weapon_AK47");       // 解锁物品
bool unlocked = EconomyManager.IsItemUnlocked("Weapon_AK47"); // 检查解锁状态
List<string> items = EconomyManager.GetUnlockedItems(); // 获取已解锁物品
```

### 14.2 商店系统

```csharp
StockShop shop = GetStockShop();

shop.OpenShop();                               // 开放商店
shop.CloseShop();                              // 关闭商店
bool isOpen = shop.IsOpen;                     // 检查状态

bool success = shop.BuyItem("Weapon_AK47", 1); // 购买物品
bool sold = shop.SellItem("Weapon_AK47", 1);   // 出售物品
int price = shop.GetItemPrice("Weapon_AK47");  // 获取物品价格
```

---

## 十五、存档系统

### 15.1 SavesSystem 存档系统

```csharp
// 重要属性
SavesSystem.CurrentSlot                        // 当前存档槽
SavesSystem.CurrentFilePath                    // 当前存档文件路径
SavesSystem.SavesFolder                        // 存档文件夹路径

// 重要事件
public static event Action OnCollectSaveData;  // 收集存档数据事件
public static event Action OnLoadSaveData;     // 加载存档数据事件

// 保存和加载数据
SavesSystem.Save("PlayerName", "玩家1");
SavesSystem.Save("PlayerLevel", 5);
SavesSystem.Save("PlayerPosition", new Vector3(10, 0, 10));

string name = SavesSystem.Load("PlayerName", "默认名");
int level = SavesSystem.Load("PlayerLevel", 1);
Vector3 pos = SavesSystem.Load("PlayerPosition", Vector3.zero);

// 全局数据（不随存档槽改变）
SavesSystem.SaveGlobal("GameVersion", "1.0.0");
string version = SavesSystem.LoadGlobal("GameVersion", "0.0.0");

// 键管理
bool exists = SavesSystem.KeyExists("PlayerName");
SavesSystem.DeleteKey("OldData");

// 文件操作
SavesSystem.SaveToFile();                      // 保存到文件
SavesSystem.LoadFromFile();                    // 从文件加载
```

### 15.2 Mod数据存储示例

```csharp
public class MyModSaveSystem
{
    void OnEnable()
    {
        SavesSystem.OnCollectSaveData += SaveModData;
        SavesSystem.OnLoadSaveData += LoadModData;
    }

    void OnDisable()
    {
        SavesSystem.OnCollectSaveData -= SaveModData;
        SavesSystem.OnLoadSaveData -= LoadModData;
    }

    void SaveModData()
    {
        SavesSystem.Save("MyMod_CustomData", myCustomData);
        Debug.Log("Mod数据已保存");
    }

    void LoadModData()
    {
        myCustomData = SavesSystem.Load("MyMod_CustomData", new CustomData());
        Debug.Log("Mod数据已加载");
    }
}
```

---

## 十六、制作系统

### 16.1 CraftingManager 制作管理器

```csharp
// 重要属性
CraftingManager.Instance                       // 获取实例
CraftingManager.UnlockedFormulaIDs             // 已解锁配方ID集合

// 重要事件
public static event Action<Item> OnItemCrafted;       // 物品制作完成
public static event Action<string> OnFormulaUnlocked; // 配方解锁

// 配方管理
CraftingManager.UnlockFormula("Weapon_AK47");  // 解锁配方
bool unlocked = CraftingManager.IsFormulaUnlocked("Weapon_AK47"); // 检查状态
CraftingFormula formula = CraftingManager.GetFormula("Weapon_AK47"); // 获取配方

// 制作物品（异步）
List<Item> items = await CraftingManager.Instance.Craft("Weapon_AK47");
if (items != null && items.Count > 0)
{
    Debug.Log($"成功制作 {items.Count} 个物品");
}
```

### 16.2 CraftingFormula 配方结构

```csharp
public struct CraftingFormula
{
    public string id;                  // 配方ID
    public string result;              // 制作结果
    public string[] tags;              // 标签
    public CraftingCost[] cost;        // 制作成本
    public bool unlockByDefault;       // 是否默认解锁
    public bool lockInDemo;            // 演示版是否锁定
    public string requirePerk;         // 需要的技能点
    public bool hideInIndex;           // 是否在索引中隐藏
}
```

---

## 十七、任务系统

### 17.1 QuestManager 任务管理器

```csharp
// 重要属性
QuestManager.Instance                          // 获取实例
QuestManager.AnyQuestNeedsInspection           // 是否有任务需要检查

// 重要事件
public static event Action<string> OnQuestActivated;  // 任务激活
public static event Action<string> OnQuestCompleted;  // 任务完成
public static event Action<string> OnTaskCompleted;   // 子任务完成

// 任务管理
QuestManager.ActivateQuest("MainQuest_01");    // 激活任务
QuestManager.CompleteQuest("MainQuest_01");    // 完成任务

bool active = QuestManager.IsQuestActive("MainQuest_01");     // 检查是否激活
bool completed = QuestManager.IsQuestCompleted("MainQuest_01"); // 检查是否完成

List<string> activeQuests = QuestManager.GetActiveQuests();   // 获取活动任务
List<string> completedQuests = QuestManager.GetCompletedQuests(); // 获取已完成任务
```

### 17.2 任务事件监听

```csharp
void OnEnable()
{
    QuestManager.OnQuestActivated += OnQuestActivated;
    QuestManager.OnQuestCompleted += OnQuestCompleted;
}

void OnDisable()
{
    QuestManager.OnQuestActivated -= OnQuestActivated;
    QuestManager.OnQuestCompleted -= OnQuestCompleted;
}

void OnQuestActivated(string questID)
{
    Debug.Log($"任务激活: {questID}");
}

void OnQuestCompleted(string questID)
{
    Debug.Log($"任务完成: {questID}");
    // 可以在这里发放奖励
    EconomyManager.AddCurrency(1000);
}
```

---

## 十八、音频系统

### 18.1 AudioManager 音频管理器

```csharp
// 重要属性
AudioManager.Instance                          // 获取实例
AudioManager.IsStingerPlaying                  // 是否正在播放Stinger音效

// 重要方法
AudioManager.PlaySound("GunShot");             // 播放声音
AudioManager.PlaySound("Explosion", position); // 在指定位置播放3D声音
AudioManager.StopSound("GunShot");             // 停止声音

// 设置角色声音类型
AudioManager.SetVoiceType(character.gameObject, AudioManager.VoiceType.Male);

// 设置脚步声材质类型
AudioManager.SetFootStepMaterialType(character.gameObject,
    AudioManager.FootStepMaterialType.Concrete);
```

### 18.2 脚步声材质类型

```csharp
public enum FootStepMaterialType
{
    Concrete,    // 混凝土
    Metal,       // 金属
    Wood,        // 木头
    Grass,       // 草地
    Water,       // 水
    Sand         // 沙地
}
```

---

## 十九、成就系统

### 19.1 AchievementManager 成就管理器

```csharp
// 重要属性
AchievementManager.Instance                    // 获取实例
AchievementManager.CanUnlockAchievement        // 是否可以解锁成就
AchievementManager.UnlockedAchievements        // 已解锁成就列表

// 重要事件
public static event Action<string> OnAchievementUnlocked;  // 成就解锁
public static event Action OnAchievementDataLoaded;        // 成就数据加载

// 成就管理
AchievementManager.UnlockAchievement("Kill_100_Enemies");  // 解锁成就
bool unlocked = AchievementManager.IsAchievementUnlocked("Kill_100_Enemies");
float progress = AchievementManager.GetAchievementProgress("Kill_100_Enemies");
```

### 19.2 StatisticsManager 统计管理器

```csharp
// 统计管理
StatisticsManager.IncrementStat("EnemiesKilled", 1);  // 增加统计值
StatisticsManager.SetStat("PlayerLevel", 5);          // 设置统计值
int kills = StatisticsManager.GetStat("EnemiesKilled"); // 获取统计值
StatisticsManager.ResetAllStats();                    // 重置所有统计
```

---

## 二十、难度系统

### 20.1 GameRulesManager 游戏规则管理器

```csharp
// 重要属性
GameRulesManager.Instance                      // 获取实例
GameRulesManager.Current                       // 当前规则集
GameRulesManager.SelectedRuleIndex             // 选中的规则索引

// 重要事件
public static event Action OnRuleChanged;      // 规则改变事件

// 难度索引枚举
public enum RuleIndex
{
    Standard,           // 标准难度
    Custom,             // 自定义难度
    Easy,               // 简单难度
    ExtraEasy,          // 超简单难度
    Hard,               // 困难难度
    ExtraHard,          // 超困难难度
    Rage,               // 愤怒难度
    StandardChallenge   // 标准挑战难度
}
```

### 20.2 Ruleset 规则集属性

```csharp
Ruleset rules = GameRulesManager.Current;

// 重要属性
rules.DisplayName                    // 显示名称
rules.Description                    // 描述
rules.SpawnDeadBody                  // 是否生成尸体
rules.FogOfWar                       // 是否启用战争迷雾
rules.AdvancedDebuffMode             // 是否启用高级减益模式
rules.RecoilMultiplier               // 后坐力倍数
rules.DamageFactor_ToPlayer          // 对玩家伤害倍数
rules.EnemyHealthFactor              // 敌人生命值倍数
rules.EnemyReactionTimeFactor        // 敌人反应时间倍数
rules.EnemyAttackTimeFactor          // 敌人攻击时间倍数
```

---

## 二十一、完整事件系统参考

### 21.1 所有可用事件列表

| 系统 | 事件名 | 说明 |
|------|--------|------|
| **角色系统** | CharacterMainControl.OnMainCharacterStartUseItem | 主角使用物品 |
| | CharacterMainControl.OnMainCharacterInventoryChangedEvent | 背包改变 |
| | CharacterMainControl.OnMainCharacterSlotContentChangedEvent | 插槽内容改变 |
| | CharacterMainControl.OnMainCharacterChangeHoldItemAgentEvent | 手持物品改变 |
| | character.OnShootEvent | 射击事件 |
| | character.OnAttackEvent | 近战攻击事件 |
| | character.OnTeamChanged | 阵营改变 |
| **生命值** | health.OnHurtEvent | 受伤事件 |
| | health.OnDeadEvent | 死亡事件 |
| | health.OnHealthChange | 血量变化 |
| | health.OnMaxHealthChange | 最大血量变化 |
| **经验值** | EXPManager.OnLevelChanged | 等级改变 |
| | EXPManager.OnExpChanged | 经验值改变 |
| **关卡** | LevelManager.OnLevelBeginInitializing | 关卡开始初始化 |
| | LevelManager.OnLevelInitialized | 关卡初始化完成 |
| | LevelManager.OnAfterLevelInitialized | 关卡初始化后 |
| | LevelManager.OnEvacuated | 撤离 |
| | LevelManager.OnMainCharacterDead | 主角死亡 |
| **制作** | CraftingManager.OnItemCrafted | 物品制作完成 |
| | CraftingManager.OnFormulaUnlocked | 配方解锁 |
| **成就** | AchievementManager.OnAchievementUnlocked | 成就解锁 |
| **任务** | QuestManager.OnQuestActivated | 任务激活 |
| | QuestManager.OnQuestCompleted | 任务完成 |
| **经济** | EconomyManager.OnItemUnlocked | 物品解锁 |
| | EconomyManager.OnCurrencyChanged | 货币改变 |
| **难度** | GameRulesManager.OnRuleChanged | 规则改变 |
| **存档** | SavesSystem.OnCollectSaveData | 收集存档数据 |
| | SavesSystem.OnLoadSaveData | 加载存档数据 |
| **Buff** | buffManager.onAddBuff | 添加Buff |
| | buffManager.onRemoveBuff | 移除Buff |

### 21.2 事件订阅最佳实践

```csharp
public class CompleteMod : ModBehaviour
{
    private bool eventsRegistered = false;

    void Start()
    {
        // 1. 注册全局事件（不需要等待关卡加载）
        LevelManager.OnLevelInitialized += OnLevelReady;
        Item.onUseStatic += OnAnyItemUsed;
        CraftingManager.OnItemCrafted += OnItemCrafted;
        EconomyManager.OnCurrencyChanged += OnCurrencyChanged;

        Debug.Log("[MyMod] 全局事件已注册");
    }

    void OnLevelReady()
    {
        // 2. 关卡加载后注册角色相关事件
        if (!eventsRegistered)
        {
            var player = LevelManager.Instance.MainCharacter;
            if (player != null)
            {
                player.OnShootEvent += OnPlayerShoot;
                player.Health.OnHurtEvent.AddListener(OnPlayerHurt);

                var buffMgr = player.GetComponent<CharacterBuffManager>();
                if (buffMgr != null)
                {
                    buffMgr.onAddBuff += OnBuffAdded;
                }
            }
            eventsRegistered = true;
            Debug.Log("[MyMod] 角色事件已注册");
        }
    }

    protected override void OnBeforeDeactivate()
    {
        // 3. ⚠️ 关键：必须取消所有事件订阅！

        // 全局事件
        LevelManager.OnLevelInitialized -= OnLevelReady;
        Item.onUseStatic -= OnAnyItemUsed;
        CraftingManager.OnItemCrafted -= OnItemCrafted;
        EconomyManager.OnCurrencyChanged -= OnCurrencyChanged;

        // 角色事件
        if (eventsRegistered)
        {
            var player = LevelManager.Instance?.MainCharacter;
            if (player != null)
            {
                player.OnShootEvent -= OnPlayerShoot;
                player.Health?.OnHurtEvent.RemoveListener(OnPlayerHurt);

                var buffMgr = player.GetComponent<CharacterBuffManager>();
                if (buffMgr != null)
                {
                    buffMgr.onAddBuff -= OnBuffAdded;
                }
            }
        }

        Debug.Log("[MyMod] 所有事件已取消订阅");
    }

    // 事件处理方法
    void OnPlayerShoot(DuckovItemAgent agent) { }
    void OnPlayerHurt(DamageInfo info) { }
    void OnAnyItemUsed(Item item, object user) { }
    void OnItemCrafted(Item item) { }
    void OnCurrencyChanged(int amount) { }
    void OnBuffAdded(CharacterBuffManager mgr, Buff buff) { }
}
```

---

## 二十二、附录：系统间依赖关系图

```
┌─────────────────────────────────────────────────────────────────────┐
│                        鸭科夫游戏系统架构                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   ┌──────────┐    ┌──────────┐    ┌──────────┐                     │
│   │ 物品系统 │◄──►│ 经济系统 │◄──►│ 制作系统 │                     │
│   │  Item    │    │ Economy  │    │ Crafting │                     │
│   └────┬─────┘    └────┬─────┘    └────┬─────┘                     │
│        │               │               │                            │
│        └───────────────┼───────────────┘                            │
│                        │                                            │
│   ┌──────────┐    ┌────▼─────┐    ┌──────────┐                     │
│   │ 角色系统 │◄──►│ 存档系统 │◄──►│ 成就系统 │                     │
│   │Character │    │  Save    │    │Achievement│                     │
│   └────┬─────┘    └──────────┘    └────┬─────┘                     │
│        │                               │                            │
│   ┌────▼─────┐    ┌──────────┐    ┌────▼─────┐                     │
│   │ 战斗系统 │◄──►│ 难度系统 │◄──►│ 任务系统 │                     │
│   │ Combat   │    │Difficulty│    │  Quest   │                     │
│   └────┬─────┘    └──────────┘    └──────────┘                     │
│        │                                                            │
│   ┌────▼─────┐    ┌──────────┐    ┌──────────┐                     │
│   │ Buff系统 │◄──►│ 效果系统 │◄──►│ 音频系统 │                     │
│   │  Buff    │    │  Effect  │    │  Audio   │                     │
│   └──────────┘    └──────────┘    └──────────┘                     │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 二十三、API快速查找表

### 管理器实例获取

| 管理器 | 获取方式 |
|--------|----------|
| LevelManager | `LevelManager.Instance` |
| EconomyManager | `EconomyManager.Instance` |
| CraftingManager | `CraftingManager.Instance` |
| QuestManager | `QuestManager.Instance` |
| AchievementManager | `AchievementManager.Instance` |
| GameRulesManager | `GameRulesManager.Instance` |
| AudioManager | `AudioManager.Instance` |
| GameManager | `GameManager.Instance` |

### 常用角色访问

| 对象 | 获取方式 |
|------|----------|
| 主角 | `LevelManager.Instance.MainCharacter` |
| 主角生命值 | `LevelManager.Instance.MainCharacter.Health` |
| 主角Buff管理器 | `character.GetComponent<CharacterBuffManager>()` |
| 宠物 | `LevelManager.Instance.PetCharacter` |
| 游戏相机 | `LevelManager.Instance.GameCamera` |

### 物品系统快速操作

| 操作 | 代码 |
|------|------|
| 注册物品 | `ItemAssetsCollection.AddDynamicEntry(prefab)` |
| 移除物品 | `ItemAssetsCollection.RemoveDynamicEntry(prefab)` |
| 实例化物品 | `await ItemAssetsCollection.InstantiateAsync(typeID)` |
| 发送给玩家 | `ItemUtilities.SendToPlayer(item)` |

---

## 二十四、C#项目配置完整指南

### 24.1 创建C#项目

#### 方式一：使用Visual Studio

1. 打开 Visual Studio 2022
2. 文件 → 新建 → 项目
3. 选择 **类库 (.NET Standard)**
4. 项目名称：`MyMod`
5. 框架选择：**.NET Standard 2.1**

#### 方式二：使用命令行

```bash
# 创建项目目录
mkdir MyMod
cd MyMod

# 创建项目
dotnet new classlib -f netstandard2.1 -n MyMod
```

### 24.2 完整的.csproj配置文件

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- 目标框架 - 必须是 netstandard2.1 -->
    <TargetFramework>netstandard2.1</TargetFramework>

    <!-- 输出类型 -->
    <OutputType>Library</OutputType>

    <!-- 项目信息 -->
    <AssemblyName>MyMod</AssemblyName>
    <RootNamespace>MyMod</RootNamespace>
    <Version>1.0.0</Version>
    <Authors>你的名字</Authors>
    <Description>我的第一个鸭科夫Mod</Description>

    <!-- 构建配置 -->
    <LangVersion>latest</LangVersion>
    <Nullable>disable</Nullable>
    <AllowUnsafeBlocks>false</AllowUnsafeBlocks>

    <!-- 输出路径配置 -->
    <OutputPath>bin\$(Configuration)\</OutputPath>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>

    <!-- 排除 Unity 项目目录（如果有） -->
    <DefaultItemExcludes>$(DefaultItemExcludes);Unity/**</DefaultItemExcludes>
  </PropertyGroup>

  <!-- 游戏路径配置 - Windows -->
  <PropertyGroup Condition="'$(OS)' == 'Windows_NT'">
    <DuckovPath Condition="'$(DuckovPath)' == ''">C:\Program Files\Steam\steamapps\common\Duckov</DuckovPath>
  </PropertyGroup>

  <!-- 游戏路径配置 - macOS -->
  <PropertyGroup Condition="'$(OS)' != 'Windows_NT'">
    <DuckovPath Condition="'$(DuckovPath)' == ''">/Users/你的用户名/workspace/duckov</DuckovPath>
  </PropertyGroup>

  <!-- 游戏DLL引用 -->
  <ItemGroup>
    <!-- Unity核心库 -->
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

    <Reference Include="UnityEngine.InputLegacyModule">
      <HintPath>$(DuckovPath)/Managed/UnityEngine.InputLegacyModule.dll</HintPath>
      <Private>False</Private>
    </Reference>

    <!-- 游戏核心库 -->
    <Reference Include="TeamSoda.Duckov.Core">
      <HintPath>$(DuckovPath)/Managed/TeamSoda.Duckov.Core.dll</HintPath>
      <Private>False</Private>
    </Reference>

    <Reference Include="TeamSoda.Duckov.Utilities">
      <HintPath>$(DuckovPath)/Managed/TeamSoda.Duckov.Utilities.dll</HintPath>
      <Private>False</Private>
    </Reference>

    <!-- 物品系统库 -->
    <Reference Include="ItemStatsSystem">
      <HintPath>$(DuckovPath)/Managed/ItemStatsSystem.dll</HintPath>
      <Private>False</Private>
    </Reference>

    <!-- 游戏主程序集 -->
    <Reference Include="Assembly-CSharp">
      <HintPath>$(DuckovPath)/Managed/Assembly-CSharp.dll</HintPath>
      <Private>False</Private>
    </Reference>

    <!-- UniTask异步库 -->
    <Reference Include="UniTask">
      <HintPath>$(DuckovPath)/Managed/UniTask.dll</HintPath>
      <Private>False</Private>
    </Reference>

    <!-- 本地化系统 -->
    <Reference Include="SodaLocalization">
      <HintPath>$(DuckovPath)/Managed/SodaLocalization.dll</HintPath>
      <Private>False</Private>
    </Reference>
  </ItemGroup>

  <!-- 编译后自动复制到Release目录 -->
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

### 24.3 常用DLL引用表

| DLL名称 | 用途 | 必需 |
|---------|------|------|
| UnityEngine.CoreModule.dll | Unity核心（GameObject、Transform等） | ✅ |
| TeamSoda.Duckov.Core.dll | 游戏核心（ModBehaviour、LevelManager等） | ✅ |
| ItemStatsSystem.dll | 物品系统（Item、UsageBehavior等） | ✅ |
| Assembly-CSharp.dll | 游戏主程序集 | ✅ |
| UniTask.dll | 异步任务支持 | 推荐 |
| SodaLocalization.dll | 本地化系统 | 推荐 |
| UnityEngine.PhysicsModule.dll | 物理系统 | 视需求 |
| UnityEngine.ParticleSystemModule.dll | 粒子系统 | 视需求 |
| UnityEngine.AudioModule.dll | 音频系统 | 视需求 |
| UnityEngine.AnimationModule.dll | 动画系统 | 视需求 |
| FMODUnity.dll | FMOD音频引擎 | 视需求 |

### 24.4 ModBehaviour基础模板

```csharp
using UnityEngine;
using Duckov.Modding;
using ItemStatsSystem;
using SodaCraft.Localizations;
using System;
using System.IO;

namespace MyMod
{
    /// <summary>
    /// Mod主入口类
    /// 必须继承自 Duckov.Modding.ModBehaviour
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // 物品TypeID（使用较大数值避免冲突，建议10000+）
        private const int MY_ITEM_TYPE_ID = 990001;

        // 物品Prefab
        private Item myItemPrefab;

        // AssetBundle
        private AssetBundle assetBundle;

        /// <summary>
        /// Mod启动入口 - Unity生命周期
        /// </summary>
        void Start()
        {
            Debug.Log("[MyMod] 开始加载...");

            try
            {
                // 1. 加载AssetBundle（如果有）
                LoadAssetBundle();

                // 2. 设置本地化文本
                SetupLocalization();

                // 3. 创建物品Prefab
                CreateItems();

                // 4. 注册物品到游戏系统
                RegisterItems();

                // 5. 注册事件监听
                RegisterEvents();

                Debug.Log("[MyMod] 加载完成!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MyMod] 加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 加载AssetBundle
        /// </summary>
        private void LoadAssetBundle()
        {
            // 获取Mod所在目录
            string modPath = Path.GetDirectoryName(GetType().Assembly.Location);
            string bundlePath = Path.Combine(modPath, "Assets", "mymod_assets");

            if (!File.Exists(bundlePath))
            {
                Debug.LogWarning($"[MyMod] AssetBundle不存在: {bundlePath}");
                return;
            }

            assetBundle = AssetBundle.LoadFromFile(bundlePath);
            Debug.Log("[MyMod] AssetBundle加载成功");
        }

        /// <summary>
        /// 设置本地化文本
        /// </summary>
        private void SetupLocalization()
        {
            // 设置物品名称（多语言）
            LocalizationManager.SetOverrideText("MyItem_Name", "我的物品");
            LocalizationManager.SetOverrideText("MyItem_Desc", "这是我的第一个Mod物品");
        }

        /// <summary>
        /// 创建物品Prefab
        /// </summary>
        private void CreateItems()
        {
            // 从AssetBundle加载或通过代码创建
            // 详见后续章节
        }

        /// <summary>
        /// 注册物品到游戏系统
        /// </summary>
        private void RegisterItems()
        {
            if (myItemPrefab != null)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(myItemPrefab);
                if (success)
                {
                    Debug.Log($"[MyMod] 物品注册成功: TypeID={myItemPrefab.TypeID}");
                }
            }
        }

        /// <summary>
        /// 注册事件监听
        /// </summary>
        private void RegisterEvents()
        {
            LevelManager.OnLevelInitialized += OnLevelLoaded;
        }

        /// <summary>
        /// 关卡加载完成回调
        /// </summary>
        private void OnLevelLoaded()
        {
            Debug.Log("[MyMod] 关卡加载完成");
        }

        /// <summary>
        /// Mod卸载时清理 - 必须实现！
        /// </summary>
        void OnDestroy()
        {
            // 取消事件注册
            LevelManager.OnLevelInitialized -= OnLevelLoaded;

            // 移除动态物品
            if (myItemPrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(myItemPrefab);
            }

            // 卸载AssetBundle
            if (assetBundle != null)
            {
                assetBundle.Unload(true);
            }

            Debug.Log("[MyMod] 已卸载");
        }
    }
}
```

### 24.5 编译和构建

```bash
# 方式一：使用dotnet CLI
dotnet build -c Release

# 方式二：使用Visual Studio
# 选择 Release 配置，然后 生成 → 生成解决方案
```

编译成功后，DLL文件位于：`bin/Release/MyMod.dll`

---

## 二十五、Unity项目配置完整指南

### 25.1 安装Unity

1. **下载 Unity Hub**: https://unity.com/download
2. **安装 Unity Editor**:
   - 打开 Unity Hub
   - 点击 Installs → Install Editor
   - **重要**: 选择版本 **2021.3.x LTS** 或 **2022.3.x**（与游戏版本匹配）
   - 勾选平台支持：
     - ✅ Windows Build Support（如果你用Windows）
     - ✅ Mac Build Support（如果你用Mac）

### 25.2 创建Unity项目

1. **新建项目**:
   - 打开 Unity Hub
   - 点击 Projects → New project
   - 选择模板：**3D (Built-in Render Pipeline)**
   - Project name: `MyModUnity`
   - 点击 Create project

2. **创建文件夹结构**:
   在 Unity 编辑器的 Project 窗口中，右键 Assets 创建以下文件夹：
   ```
   Assets/
   ├── Editor/           ← 编辑器脚本（仅在Unity编辑器中运行）
   ├── Prefabs/          ← 物品预制体
   ├── Materials/        ← 材质文件
   ├── Textures/         ← 贴图文件
   ├── Plugins/          ← 游戏DLL引用
   └── AssetBundles/     ← 导出的AssetBundle
   ```

### 25.3 导入游戏DLL

1. **找到游戏Managed目录**:
   - Windows: `<游戏目录>\Duckov_Data\Managed\`
   - macOS: `Duckov.app/Contents/Managed/`

2. **复制必需的DLL到Unity项目**:
   将以下DLL复制到 `Assets/Plugins/` 目录：
   - `ItemStatsSystem.dll`
   - `TeamSoda.Duckov.Core.dll`
   - `TeamSoda.Duckov.Utilities.dll`

3. **等待Unity导入**: Unity会自动检测并编译这些DLL

### 25.4 创建AssetBundle打包脚本

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

### 25.5 设置Prefab的AssetBundle名称

1. 在 Project 窗口选择你的 Prefab
2. 在 Inspector 窗口底部找到 **AssetBundle** 下拉菜单
3. 点击 **New...** 创建新的AssetBundle名称（如 `mymod_items`）
4. 或选择已存在的AssetBundle名称

### 25.6 构建AssetBundle

1. 点击 Unity 菜单 **Build → Build AssetBundles (Current Platform)**
2. 等待构建完成
3. 导出的文件在 `Assets/AssetBundles/` 目录

---

## 二十六、Prefab制作详细流程

### 26.1 创建基础物品Prefab

#### 步骤1：创建GameObject
1. 在 Hierarchy 窗口右键 → Create Empty
2. 重命名为 `MyItem`

#### 步骤2：添加Item组件
1. 选中 MyItem
2. Inspector 窗口点击 **Add Component**
3. 搜索并添加 **Item**（来自 ItemStatsSystem.dll）

#### 步骤3：配置Item属性

在 Inspector 中设置以下字段：

| 属性 | 值 | 说明 |
|------|-----|------|
| Type ID | `990001` | 唯一ID，建议990000+ |
| Display Name | `MyItem_Name` | 本地化键名 |
| Description | `MyItem_Desc` | 描述本地化键名 |
| Max Stack Count | `1` | 最大堆叠数 |
| Stackable | `false` | 是否可堆叠 |
| Value | `1000` | 物品价值 |
| Quality | `2` | 品质等级(0-5) |
| Unit Self Weight | `0.5` | 物品重量 |
| Max Durability | `100` | 最大耐久度 |
| Durability | `100` | 当前耐久度 |

### 26.2 创建可使用物品（UsageBehavior）

#### 步骤1：创建自定义UsageBehavior脚本

```csharp
using UnityEngine;
using ItemStatsSystem;

namespace MyMod
{
    /// <summary>
    /// 自定义物品使用行为
    /// </summary>
    public class MyItemUse : UsageBehavior
    {
        // 在Inspector中可配置的参数
        public int healValue = 50;
        public float useTime = 1.5f;

        // 显示在物品信息中
        public override DisplaySettingsData DisplaySettings
        {
            get
            {
                return new DisplaySettingsData
                {
                    display = true,
                    description = $"恢复 {healValue} 点生命值"
                };
            }
        }

        /// <summary>
        /// 判断物品是否可以使用
        /// </summary>
        public override bool CanBeUsed(Item item, object user)
        {
            // 确保使用者是角色
            CharacterMainControl character = user as CharacterMainControl;
            if (character == null) return false;

            // 治疗物品：只有受伤时才能使用
            if (character.Health.CurrentHealth >= character.Health.MaxHealth)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 执行使用逻辑
        /// </summary>
        protected override void OnUse(Item item, object user)
        {
            CharacterMainControl character = user as CharacterMainControl;
            if (character == null) return;

            // 恢复生命值
            character.Health.AddHealth(healValue);

            Debug.Log($"[MyMod] 恢复了 {healValue} 点生命值");

            // 显示提示文本
            character.PopText($"+{healValue} HP");
        }
    }
}
```

#### 步骤2：配置Prefab

1. 在 MyItem 物品上添加 `MyItemUse` 组件
2. 配置 `healValue` 和 `useTime`
3. 确保 Item 组件的 Usage Utilities 已正确关联

### 26.3 添加视觉模型

#### 方式一：使用简单几何体
```
MyItem (GameObject)
├── Item (Component)
├── MyItemUse (Component)
└── Model (Child GameObject)
    ├── Mesh Filter (Cube/Sphere等)
    ├── Mesh Renderer
    └── Material
```

#### 方式二：使用3D模型
1. 将模型文件(.fbx, .obj等)导入Unity
2. 拖入Prefab作为子物体
3. 调整位置、旋转、缩放

### 26.4 保存Prefab

1. 将场景中的物品拖到 `Assets/Prefabs/` 目录
2. 或右键物品 → Prefab → Create Prefab
3. 设置 AssetBundle 名称

---

## 二十七、AssetBundle打包指南

### 27.1 打包流程

1. **设置AssetBundle名称**
   - 选择Prefab → Inspector底部 → AssetBundle → 选择或创建名称

2. **执行构建**
   - 菜单 Build → Build AssetBundles

3. **验证输出**
   - 检查 `Assets/AssetBundles/` 目录
   - 应该有两个文件：`mymod_items` 和 `mymod_items.manifest`

### 27.2 在Mod中加载AssetBundle

```csharp
/// <summary>
/// 加载AssetBundle并获取Prefab
/// </summary>
private void LoadAssetBundle()
{
    // 获取Mod目录路径
    string modPath = Path.GetDirectoryName(GetType().Assembly.Location);
    string bundlePath = Path.Combine(modPath, "Assets", "mymod_items");

    // 检查文件是否存在
    if (!File.Exists(bundlePath))
    {
        Debug.LogError($"[MyMod] AssetBundle不存在: {bundlePath}");
        return;
    }

    // 加载AssetBundle
    assetBundle = AssetBundle.LoadFromFile(bundlePath);
    if (assetBundle == null)
    {
        Debug.LogError("[MyMod] AssetBundle加载失败");
        return;
    }

    // 加载Prefab
    myItemPrefab = assetBundle.LoadAsset<Item>("MyItem");
    if (myItemPrefab == null)
    {
        Debug.LogError("[MyMod] Prefab加载失败");
        return;
    }

    Debug.Log($"[MyMod] 成功加载物品: {myItemPrefab.DisplayName}");
}
```

### 27.3 常见问题

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 加载失败 | 平台不匹配 | 确保打包平台与游戏平台一致 |
| Prefab为空 | 资源名称错误 | 检查LoadAsset中的名称 |
| 组件丢失 | DLL未正确引用 | 确保Unity项目引用了相同的DLL |
| 材质丢失 | 材质未打包 | 将材质设置相同的AssetBundle |

---

## 二十八、Mod目录结构与info.ini

### 28.1 标准Mod目录结构

```
MyMod/                          ← Mod根目录
├── MyMod.dll                   ← 编译的Mod代码（必需）
├── info.ini                    ← Mod配置文件（必需）
├── preview.png                 ← 预览图（256x256，可选）
└── Assets/                     ← 资源目录（可选）
    ├── mymod_items             ← AssetBundle文件
    ├── mymod_items.manifest    ← AssetBundle清单（可删除）
    ├── icons/                  ← 图标目录
    │   └── item_icon.png
    └── sounds/                 ← 音效目录
        └── use_sound.wav
```

### 28.2 info.ini配置文件

```ini
[Mod Information]
# Mod唯一标识（必需，与DLL名称一致）
name=MyMod

# 显示名称（必需，游戏中显示）
displayName=我的第一个Mod

# 描述（必需，支持多行）
description=这是一个示例Mod，添加了一个可以恢复生命值的物品。\n使用方法：在背包中右键使用。

# 版本号（推荐）
version=1.0.0

# 作者（推荐）
author=你的名字

# 标签（用于分类，可选）
# 常用标签: Weapon, Armor, Consumable, Utility, Quality of Life, Equipment & Gear
tags=Consumable,Utility

[Requirements]
# 最低游戏版本（可选）
gameVersion=1.0.0

# 依赖的其他Mod（可选，逗号分隔）
dependencies=

[Technical]
# 自定义技术信息（可选）
# 声明使用的TypeID范围，避免与其他Mod冲突
typeIDRange=990001-990010
```

### 28.3 Mod安装位置

| 平台 | 安装路径 |
|------|----------|
| Windows | `<游戏目录>\Duckov_Data\Mods\MyMod\` |
| macOS | `Duckov.app/Contents/Mods/MyMod/` |

### 28.4 快速部署脚本

创建 `deploy.sh`（macOS）或 `deploy.bat`（Windows）：

**macOS (deploy.sh)**:
```bash
#!/bin/bash
MOD_NAME="MyMod"
GAME_MODS_DIR="/Applications/Duckov.app/Contents/Mods"
RELEASE_DIR="./Release/$MOD_NAME"

# 创建Mod目录
mkdir -p "$GAME_MODS_DIR/$MOD_NAME"

# 复制文件
cp -r "$RELEASE_DIR/"* "$GAME_MODS_DIR/$MOD_NAME/"

echo "Mod已部署到: $GAME_MODS_DIR/$MOD_NAME"
```

**Windows (deploy.bat)**:
```batch
@echo off
set MOD_NAME=MyMod
set GAME_MODS_DIR=C:\Program Files\Steam\steamapps\common\Duckov\Duckov_Data\Mods
set RELEASE_DIR=.\Release\%MOD_NAME%

xcopy /E /I /Y "%RELEASE_DIR%" "%GAME_MODS_DIR%\%MOD_NAME%"

echo Mod已部署到: %GAME_MODS_DIR%\%MOD_NAME%
```

---

## 二十九、调试方法与日志查看

### 29.1 日志文件位置

| 平台 | 日志路径 |
|------|----------|
| Windows | `C:\Users\<用户名>\AppData\LocalLow\TeamSoda\Duckov\Player.log` |
| macOS | `~/Library/Logs/TeamSoda/Duckov/Player.log` |

### 29.2 添加调试日志

```csharp
// 普通日志
Debug.Log("[MyMod] 这是一条普通日志");

// 警告日志
Debug.LogWarning("[MyMod] 这是一条警告");

// 错误日志
Debug.LogError("[MyMod] 这是一条错误");

// 带堆栈的异常日志
try
{
    // 可能出错的代码
}
catch (Exception e)
{
    Debug.LogError($"[MyMod] 发生异常: {e.Message}\n{e.StackTrace}");
}
```

### 29.3 实用调试代码

```csharp
/// <summary>
/// 调试：打印物品详细信息
/// </summary>
private void DebugPrintItem(Item item)
{
    if (item == null)
    {
        Debug.Log("[DEBUG] Item is null");
        return;
    }

    Debug.Log($"[DEBUG] ===== 物品信息 =====");
    Debug.Log($"  TypeID: {item.TypeID}");
    Debug.Log($"  DisplayName: {item.DisplayName}");
    Debug.Log($"  Initialized: {item.Initialized}");
    Debug.Log($"  Durability: {item.Durability}/{item.MaxDurability}");
    Debug.Log($"  UsageUtilities: {(item.UsageUtilities != null ? "存在" : "不存在")}");
    Debug.Log($"  AgentUtilities: {(item.AgentUtilities != null ? "存在" : "不存在")}");
    Debug.Log($"  HasHandHeldAgent: {item.HasHandHeldAgent}");
}

/// <summary>
/// 调试：监听游戏事件
/// </summary>
private void SetupDebugEvents()
{
    LevelManager.OnLevelInitialized += () => Debug.Log("[DEBUG] 关卡初始化完成");
    Item.onUseStatic += (item, user) => Debug.Log($"[DEBUG] 物品使用: {item.DisplayName}");
}
```

### 29.4 测试快捷键

```csharp
void Update()
{
    // 按 F9 添加测试物品
    if (Input.GetKeyDown(KeyCode.F9))
    {
        AddTestItem();
    }

    // 按 F10 打印调试信息
    if (Input.GetKeyDown(KeyCode.F10))
    {
        PrintDebugInfo();
    }
}

private void AddTestItem()
{
    try
    {
        var character = CharacterMainControl.Main;
        if (character?.CharacterItem?.Inventory == null)
        {
            Debug.Log("[MyMod] 无法找到玩家背包");
            return;
        }

        // 实例化物品并添加到背包
        Item item = Instantiate(myItemPrefab);
        item.Initialize();
        character.CharacterItem.Inventory.AddItem(item);

        Debug.Log("[MyMod] 测试物品已添加到背包");
    }
    catch (Exception e)
    {
        Debug.LogError($"[MyMod] 添加测试物品失败: {e.Message}");
    }
}
```

### 29.5 常见错误排查

| 错误信息 | 可能原因 | 解决方案 |
|----------|----------|----------|
| NullReferenceException | 访问了null对象 | 添加null检查 |
| MissingReferenceException | 对象已被销毁 | 在访问前检查对象状态 |
| TypeLoadException | DLL引用问题 | 确保所有DLL版本匹配 |
| FileNotFoundException | 资源文件缺失 | 检查AssetBundle路径 |

---

## 三十、完整实战示例：消耗品物品

### 30.1 项目结构

```
MyHealingItemMod/
├── MyHealingItemMod.csproj
├── Scripts/
│   ├── ModBehaviour.cs         ← Mod主入口
│   └── HealingItemUse.cs       ← 使用行为
├── Unity/
│   ├── Editor/
│   │   └── AssetBundleBuilder.cs
│   └── Prefabs/
│       └── HealingItem.prefab
├── Release/
│   └── MyHealingItemMod/
│       ├── MyHealingItemMod.dll
│       ├── info.ini
│       └── Assets/
│           └── healing_items
└── README.md
```

### 30.2 完整代码

#### Scripts/HealingItemUse.cs
```csharp
using UnityEngine;
using ItemStatsSystem;

namespace MyHealingItemMod
{
    /// <summary>
    /// 治疗物品使用行为
    /// </summary>
    public class HealingItemUse : UsageBehavior
    {
        [Header("治疗设置")]
        public int healValue = 50;           // 治疗量
        public bool consumeOnUse = true;     // 使用后消耗
        public float useTime = 2f;           // 使用时间

        private Item item;

        public override DisplaySettingsData DisplaySettings => new DisplaySettingsData
        {
            display = true,
            description = $"恢复 {healValue} 点生命值"
        };

        void Awake()
        {
            item = GetComponent<Item>();
        }

        public override bool CanBeUsed(Item item, object user)
        {
            CharacterMainControl character = user as CharacterMainControl;
            if (character == null) return false;

            // 满血时不能使用
            if (character.Health.CurrentHealth >= character.Health.MaxHealth)
            {
                return false;
            }

            return true;
        }

        protected override void OnUse(Item item, object user)
        {
            CharacterMainControl character = user as CharacterMainControl;
            if (character == null) return;

            // 计算实际治疗量（不超过最大生命值）
            float needed = character.Health.MaxHealth - character.Health.CurrentHealth;
            float actualHeal = Mathf.Min(healValue, needed);

            // 恢复生命值
            character.Health.AddHealth(actualHeal);

            // 显示提示
            character.PopText($"+{Mathf.RoundToInt(actualHeal)} HP");

            Debug.Log($"[MyHealingItemMod] 恢复了 {actualHeal} 点生命值");

            // 消耗物品（如果配置为消耗）
            if (consumeOnUse)
            {
                // 物品会自动从背包移除
            }
        }
    }
}
```

#### Scripts/ModBehaviour.cs
```csharp
using UnityEngine;
using Duckov.Modding;
using ItemStatsSystem;
using SodaCraft.Localizations;
using System;
using System.IO;
using System.Reflection;

namespace MyHealingItemMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // 物品TypeID
        private const int HEALING_ITEM_TYPE_ID = 990101;

        // 物品Prefab
        private Item healingItemPrefab;

        // AssetBundle
        private AssetBundle assetBundle;

        void Start()
        {
            Debug.Log("[MyHealingItemMod] 开始加载...");

            try
            {
                // 设置本地化
                SetupLocalization();

                // 加载AssetBundle
                LoadAssetBundle();

                // 创建物品（如果AssetBundle加载失败则通过代码创建）
                CreateHealingItem();

                // 注册物品
                RegisterItems();

                // 注册事件
                RegisterEvents();

                Debug.Log("[MyHealingItemMod] 加载完成!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MyHealingItemMod] 加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        private void SetupLocalization()
        {
            LocalizationManager.SetOverrideText("HealingItem_Name", "神奇药水");
            LocalizationManager.SetOverrideText("HealingItem_Desc", "喝下后可以恢复50点生命值");
        }

        private void LoadAssetBundle()
        {
            string modPath = Path.GetDirectoryName(GetType().Assembly.Location);
            string bundlePath = Path.Combine(modPath, "Assets", "healing_items");

            if (!File.Exists(bundlePath))
            {
                Debug.LogWarning($"[MyHealingItemMod] AssetBundle不存在，将通过代码创建物品");
                return;
            }

            assetBundle = AssetBundle.LoadFromFile(bundlePath);
            if (assetBundle != null)
            {
                healingItemPrefab = assetBundle.LoadAsset<Item>("HealingItem");
            }
        }

        private void CreateHealingItem()
        {
            // 如果已从AssetBundle加载，直接返回
            if (healingItemPrefab != null) return;

            Debug.Log("[MyHealingItemMod] 通过代码创建物品Prefab...");

            // 创建物品GameObject
            GameObject itemObj = new GameObject("HealingItem");
            itemObj.SetActive(false); // 先禁用，配置完成后再启用
            DontDestroyOnLoad(itemObj);

            // 添加Item组件
            Item item = itemObj.AddComponent<Item>();

            // 通过反射设置TypeID（因为TypeID是私有字段）
            SetFieldValue(item, "typeID", HEALING_ITEM_TYPE_ID);
            SetFieldValue(item, "displayName", "HealingItem_Name");
            SetFieldValue(item, "description", "HealingItem_Desc");
            SetFieldValue(item, "maxStackCount", 10);
            SetFieldValue(item, "stackable", true);
            SetFieldValue(item, "value", 500);
            SetFieldValue(item, "quality", 2);
            SetFieldValue(item, "unitSelfWeight", 0.2f);
            SetFieldValue(item, "maxDurability", 1f);
            SetFieldValue(item, "durability", 1f);
            SetFieldValue(item, "useDurability", false);

            // 添加使用行为
            HealingItemUse useBehavior = itemObj.AddComponent<HealingItemUse>();
            useBehavior.healValue = 50;
            useBehavior.consumeOnUse = true;
            useBehavior.useTime = 2f;

            // 配置UsageUtilities
            UsageUtilities usageUtils = itemObj.AddComponent<UsageUtilities>();
            SetFieldValue(usageUtils, "useTime", 2f);
            SetFieldValue(usageUtils, "useDurability", false);

            // 将useBehavior添加到behaviors列表
            var behaviorsField = typeof(UsageUtilities).GetField("behaviors",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var behaviors = behaviorsField?.GetValue(usageUtils) as System.Collections.Generic.List<UsageBehavior>;
            if (behaviors == null)
            {
                behaviors = new System.Collections.Generic.List<UsageBehavior>();
                behaviorsField?.SetValue(usageUtils, behaviors);
            }
            behaviors.Add(useBehavior);

            // 设置item的usageUtilities
            SetFieldValue(item, "usageUtilities", usageUtils);

            // 配置AgentUtilities（用于手持显示）
            ConfigureAgentUtilities(itemObj, item);

            itemObj.SetActive(true);
            healingItemPrefab = item;

            Debug.Log($"[MyHealingItemMod] 物品创建完成: TypeID={HEALING_ITEM_TYPE_ID}");
        }

        private void ConfigureAgentUtilities(GameObject itemObj, Item item)
        {
            // 获取游戏内置的手持代理预制体
            var handheldPrefab = GameplayDataSettings.Prefabs.HandheldAgentPrefab;
            if (handheldPrefab == null)
            {
                Debug.LogWarning("[MyHealingItemMod] 无法获取HandheldAgentPrefab");
                return;
            }

            // 添加ItemAgentUtilities组件
            ItemAgentUtilities agentUtils = itemObj.AddComponent<ItemAgentUtilities>();
            SetFieldValue(agentUtils, "master", item);

            // 设置item的agentUtilities
            SetFieldValue(item, "agentUtilities", agentUtils);
        }

        private void RegisterItems()
        {
            if (healingItemPrefab == null)
            {
                Debug.LogError("[MyHealingItemMod] 物品Prefab为空，无法注册");
                return;
            }

            bool success = ItemAssetsCollection.AddDynamicEntry(healingItemPrefab);
            if (success)
            {
                Debug.Log($"[MyHealingItemMod] 物品注册成功: TypeID={healingItemPrefab.TypeID}");
            }
            else
            {
                Debug.LogError("[MyHealingItemMod] 物品注册失败");
            }
        }

        private void RegisterEvents()
        {
            LevelManager.OnLevelInitialized += OnLevelLoaded;
        }

        private void OnLevelLoaded()
        {
            Debug.Log("[MyHealingItemMod] 关卡加载完成");
        }

        void Update()
        {
            // F9: 添加测试物品到背包
            if (Input.GetKeyDown(KeyCode.F9))
            {
                AddTestItem();
            }
        }

        private void AddTestItem()
        {
            try
            {
                var character = CharacterMainControl.Main;
                if (character?.CharacterItem?.Inventory == null)
                {
                    Debug.Log("[MyHealingItemMod] 无法找到玩家背包");
                    return;
                }

                // 实例化物品
                Item newItem = Instantiate(healingItemPrefab);
                newItem.Initialize();

                // 添加到背包
                character.CharacterItem.Inventory.AddItem(newItem);

                Debug.Log("[MyHealingItemMod] 测试物品已添加到背包");
                character.PopText("获得: 神奇药水");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MyHealingItemMod] 添加测试物品失败: {e.Message}");
            }
        }

        void OnDestroy()
        {
            // 取消事件注册
            LevelManager.OnLevelInitialized -= OnLevelLoaded;

            // 移除物品注册
            if (healingItemPrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(healingItemPrefab);
            }

            // 卸载AssetBundle
            if (assetBundle != null)
            {
                assetBundle.Unload(true);
            }

            Debug.Log("[MyHealingItemMod] 已卸载");
        }

        /// <summary>
        /// 通过反射设置私有字段
        /// </summary>
        private void SetFieldValue(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"[MyHealingItemMod] 找不到字段: {fieldName}");
            }
        }
    }
}
```

#### info.ini
```ini
[Mod Information]
name=MyHealingItemMod
displayName=神奇药水Mod
description=添加一个可以恢复50点生命值的消耗品。\n按F9可以快速获取测试物品。
version=1.0.0
author=你的名字
tags=Consumable,Utility

[Requirements]
gameVersion=1.0.0
dependencies=

[Technical]
typeIDRange=990101-990110
```

### 30.3 开发检查清单

```
□ C#项目配置
  □ 目标框架为 netstandard2.1
  □ 所有必需DLL已正确引用
  □ 项目可以成功编译

□ Unity项目（如果需要AssetBundle）
  □ Unity版本与游戏匹配
  □ DLL已导入到Plugins目录
  □ Prefab已正确配置Item组件
  □ AssetBundle已构建

□ 代码检查
  □ ModBehaviour继承自Duckov.Modding.ModBehaviour
  □ 所有事件订阅在OnDestroy中取消
  □ 物品在OnDestroy中从游戏移除
  □ 添加了调试日志

□ 部署检查
  □ info.ini配置正确
  □ DLL文件已复制到Mod目录
  □ Assets目录包含AssetBundle（如果需要）
  □ Mod目录放置在正确的游戏目录下

□ 测试检查
  □ 游戏能够加载Mod（查看日志）
  □ 物品能够正常获取
  □ 物品能够正常使用
  □ 卸载Mod后游戏正常
```

---

## 三十一、游戏内部实现参考

> 以下是游戏反编译代码中的真实实现，供Mod开发者参考

### 31.1 生命值与伤害系统 (Health.cs)

游戏中的生命值系统核心实现：

```csharp
// 源码位置: TeamSoda.Duckov.Core/Health.cs
public class Health : MonoBehaviour
{
    public Teams team;
    private Item item;
    private float _currentHealth;

    // 最大生命值通过物品Stat获取
    public float MaxHealth
    {
        get
        {
            if (this.item)
                return this.item.GetStatValue("MaxHealth".GetHashCode());
            return (float)this.defaultMaxHealth;
        }
    }

    // 护甲值
    public float BodyArmor => item ? item.GetStatValue("BodyArmor".GetHashCode()) : 0f;
    public float HeadArmor => item ? item.GetStatValue("HeadArmor".GetHashCode()) : 0f;

    // 元素抗性计算（天气会影响元素效果）
    public float ElementFactor(ElementTypes type)
    {
        float num = 1f;
        Weather currentWeather = TimeOfDayController.Instance.CurrentWeather;
        bool isBaseLevel = LevelManager.Instance.IsBaseLevel;

        switch (type)
        {
            case ElementTypes.fire:
                num = item.GetStat("ElementFactor_Fire".GetHashCode()).Value;
                // 下雨天火焰抗性+15%
                if (!isBaseLevel && currentWeather == Weather.Rainy)
                    num -= 0.15f;
                break;
            case ElementTypes.electricity:
                num = item.GetStat("ElementFactor_Electricity".GetHashCode()).Value;
                // 下雨天电击伤害+20%
                if (!isBaseLevel && currentWeather == Weather.Rainy)
                    num += 0.2f;
                break;
            // ... 其他元素
        }
        return num;
    }

    // 核心伤害处理逻辑
    public bool Hurt(DamageInfo damageInfo)
    {
        if (invincible || isDead) return false;

        // 处理Buff附加（流血、毒等）
        if (damageInfo.buff != null && Random.Range(0f, 1f) < damageInfo.buffChance)
            AddBuff(damageInfo.buff, damageInfo.fromCharacter, damageInfo.fromWeaponItemID);

        // 暴击计算
        bool isCrit = Random.Range(0f, 1f) < damageInfo.critRate;
        float damage = damageInfo.damageValue * (isCrit ? damageInfo.critDamageFactor : 1f);

        // 护甲减伤计算
        if (damageInfo.damageType != DamageTypes.realDamage && !damageInfo.ignoreArmor)
        {
            float armor = isCrit ? HeadArmor : BodyArmor;
            float reduction = 2f / (Mathf.Clamp(armor - damageInfo.armorPiercing, 0f, 999f) + 2f);
            damage *= reduction;
        }

        // 应用伤害
        CurrentHealth -= damage;

        // 死亡处理
        if (CurrentHealth <= 0f)
        {
            isDead = true;
            // 击杀敌人获得经验
            if (team != Teams.player && damageInfo.fromCharacter?.IsMainCharacter == true)
                EXPManager.AddExp(item.GetInt("Exp", 0));

            OnDeadEvent?.Invoke(damageInfo);
            OnDead?.Invoke(this, damageInfo);
        }
        return true;
    }

    // 恢复生命值
    public void AddHealth(float healthValue)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + healthValue);
    }
}
```

### 31.2 药物使用行为 (Drug.cs)

游戏中药物/治疗物品的实现：

```csharp
// 源码位置: Duckov.ItemUsage/Drug.cs
[MenuPath("医疗/药")]
public class Drug : UsageBehavior
{
    public int healValue;            // 治疗量
    public bool useDurability;       // 是否消耗耐久度
    public float durabilityUsage;    // 每次使用消耗的耐久度
    public bool canUsePart;          // 是否支持部分使用（只治疗需要的量）

    // 物品描述信息
    public override DisplaySettingsData DisplaySettings
    {
        get
        {
            DisplaySettingsData result = default;
            result.display = true;
            result.description = $"治疗量: {healValue}";
            if (useDurability)
                result.description += $" (耐久消耗: {durabilityUsage})";
            return result;
        }
    }

    // 是否可以使用（满血时不可用）
    public override bool CanBeUsed(Item item, object user)
    {
        CharacterMainControl character = user as CharacterMainControl;
        return character && (healValue <= 0 ||
               character.Health.CurrentHealth < character.Health.MaxHealth);
    }

    // 使用逻辑
    protected override void OnUse(Item item, object user)
    {
        CharacterMainControl character = user as CharacterMainControl;
        if (!character) return;

        float actualHeal = (float)healValue;

        // 支持部分使用时，只治疗需要的量
        if (useDurability && item.UseDurability && canUsePart)
        {
            float needed = character.Health.MaxHealth - character.Health.CurrentHealth;
            if (needed > healValue) needed = healValue;

            float durabilityCost = needed / healValue * durabilityUsage;
            if (durabilityCost > item.Durability)
            {
                durabilityCost = item.Durability;
                actualHeal = healValue * item.Durability / durabilityUsage;
            }
            item.Durability -= durabilityCost;
        }

        Heal(character, item, actualHeal);
    }

    private void Heal(CharacterMainControl character, Item selfItem, float _healValue)
    {
        if (_healValue > 0f)
            character.AddHealth(Mathf.CeilToInt(_healValue));
        else if (_healValue < 0f)
        {
            // 负数表示造成伤害（如辐射物品）
            DamageInfo damageInfo = new DamageInfo(null);
            damageInfo.damageValue = -_healValue;
            character.Health.Hurt(damageInfo);
        }
    }
}
```

### 31.3 Buff系统核心 (Buff.cs)

游戏中Buff的实现：

```csharp
// 源码位置: Duckov.Buffs/Buff.cs
public class Buff : MonoBehaviour
{
    [SerializeField] private int id;                    // Buff唯一ID
    [SerializeField] private int maxLayers = 1;         // 最大叠加层数
    [SerializeField] private BuffExclusiveTags exclusiveTag;  // 互斥标签
    [SerializeField] private int exclusiveTagPriority;  // 互斥优先级
    [SerializeField] private string displayName;        // 显示名称(本地化Key)
    [SerializeField] private string description;        // 描述(本地化Key)
    [SerializeField] private Sprite icon;               // 图标
    [SerializeField] private bool limitedLifeTime;      // 是否有时间限制
    [SerializeField] private float totalLifeTime;       // 持续时间
    [SerializeField] private List<Effect> effects;      // 附带的效果列表
    [SerializeField] private GameObject buffFxPfb;      // 视觉特效预制体

    public CharacterMainControl Character => master?.Master;
    public int CurrentLayers { get; set; } = 1;
    public float RemainingTime => limitedLifeTime ? totalLifeTime - CurrentLifeTime : float.PositiveInfinity;

    // Buff被添加到角色时调用
    internal void Setup(CharacterBuffManager manager)
    {
        master = manager;
        timeWhenStarted = Time.time;
        transform.SetParent(CharacterItem.transform, false);

        // 生成视觉特效
        if (buffFxPfb && manager.Master?.characterModel)
        {
            buffFxInstance = Instantiate(buffFxPfb);
            Transform socket = manager.Master.characterModel.ArmorSocket ?? manager.Master.transform;
            buffFxInstance.transform.SetParent(socket);
        }

        // 将Effects设置到角色物品
        foreach (Effect effect in effects)
            effect.SetItem(CharacterItem);

        OnSetup();
        OnSetupEvent?.Invoke();
    }

    // 同ID Buff再次添加时（叠加层数、刷新时间）
    internal virtual void NotifyIncomingBuffWithSameID(Buff incomingPrefab)
    {
        timeWhenStarted = Time.time;  // 刷新时间
        if (CurrentLayers < maxLayers)
            CurrentLayers += incomingPrefab.CurrentLayers;
    }

    // Buff互斥标签枚举
    public enum BuffExclusiveTags
    {
        NotExclusive,   // 不互斥
        Bleeding,       // 流血
        Starve,         // 饥饿
        Thirsty,        // 口渴
        Weight,         // 负重
        Poison,         // 中毒
        Pain,           // 疼痛
        Electric,       // 触电
        Burning,        // 燃烧
        Space,          // 空间
        StormProtection,// 风暴保护
        Nauseous,       // 恶心
        Stun            // 眩晕
    }
}
```

### 31.4 添加Buff使用行为 (AddBuff.cs)

物品使用时添加Buff的实现：

```csharp
// 源码位置: Duckov.ItemUsage/AddBuff.cs
public class AddBuff : UsageBehavior
{
    public Buff buffPrefab;         // Buff预制体
    [Range(0.01f, 1f)]
    public float chance = 1f;       // 触发概率

    public override DisplaySettingsData DisplaySettings
    {
        get
        {
            DisplaySettingsData result = default;
            result.display = true;
            result.description = buffPrefab.DisplayName ?? "";

            if (buffPrefab.LimitedLifeTime)
                result.description += $" : {buffPrefab.TotalLifeTime}s";

            if (chance < 1f)
                result.description += $" (概率: {Mathf.RoundToInt(chance * 100)}%)";

            return result;
        }
    }

    public override bool CanBeUsed(Item item, object user) => true;

    protected override void OnUse(Item item, object user)
    {
        CharacterMainControl character = user as CharacterMainControl;
        if (character == null) return;

        // 概率判定
        if (Random.Range(0f, 1f) > chance) return;

        character.AddBuff(buffPrefab, character, 0);
    }
}
```

### 31.5 商店系统 (StockShop.cs)

游戏中商店的完整实现：

```csharp
// 源码位置: Duckov.Economy/StockShop.cs
public class StockShop : MonoBehaviour, IMerchant, ISaveDataProvider
{
    [SerializeField] private string merchantID = "Albert";  // 商人ID
    [SerializeField] private long refreshAfterTimeSpan;     // 刷新间隔
    public float sellFactor = 0.5f;  // 卖出价格系数（50%）
    public List<Entry> entries = new List<Entry>();  // 商品列表

    // 初始化：从数据库加载商品
    private void InitializeEntries()
    {
        var profile = StockShopDatabase.Instance.GetMerchantProfile(merchantID);
        foreach (var cur in profile.entries)
            entries.Add(new Entry(cur));
    }

    // 刷新库存
    private void DoRefreshStock()
    {
        bool advancedMode = LevelManager.Rule.AdvancedDebuffMode;
        foreach (Entry entry in entries)
        {
            // 概率刷新
            if (entry.Possibility > 0f && entry.Possibility < 1f &&
                Random.Range(0f, 1f) > entry.Possibility)
            {
                entry.Show = false;
                entry.CurrentStock = 0;
            }
            else
            {
                // 高级难度专属物品
                ItemMetaData metaData = ItemAssetsCollection.GetMetaData(entry.ItemTypeID);
                if (!advancedMode && metaData.tags.Contains(GameplayDataSettings.Tags.AdvancedDebuffMode))
                {
                    entry.Show = false;
                    entry.CurrentStock = 0;
                }
                else
                {
                    entry.Show = true;
                    entry.CurrentStock = entry.MaxStock;
                }
            }
        }
    }

    // 计算物品价格
    public int ConvertPrice(Item item, bool selling = false)
    {
        int price = item.GetTotalRawValue();

        if (!selling)
        {
            // 购买时，应用商店价格系数
            Entry entry = entries.Find(e => e.ItemTypeID == item.TypeID);
            if (entry != null)
                price = Mathf.FloorToInt(price * entry.PriceFactor);
        }
        else
        {
            // 卖出时，应用卖出系数
            return Mathf.FloorToInt(price * sellFactor);
        }
        return price;
    }

    // 购买物品（异步）
    public async UniTask<bool> Buy(int itemTypeID, int amount = 1)
    {
        return await BuyTask(itemTypeID, amount);
    }

    // 存档/读档
    public object GenerateSaveData()
    {
        SaveData saveData = new SaveData();
        saveData.lastTimeRefreshedStock = lastTimeRefreshedStock;
        foreach (Entry entry in entries)
        {
            saveData.stockCounts.Add(new SaveData.StockCountEntry {
                itemTypeID = entry.ItemTypeID,
                stock = entry.CurrentStock
            });
        }
        return saveData;
    }
}
```

### 31.6 制作系统 (CraftingManager.cs)

游戏中制作系统的实现：

```csharp
// 源码位置: TeamSoda.Duckov.Core/CraftingManager.cs
public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }
    public static Action<CraftingFormula, Item> OnItemCrafted;  // 制作完成事件
    public static Action<string> OnFormulaUnlocked;             // 配方解锁事件

    private List<string> unlockedFormulaIDs = new List<string>();

    private void Awake()
    {
        Instance = this;
        Load();
        SavesSystem.OnCollectSaveData += Save;
    }

    // 加载已解锁配方
    private void Load()
    {
        unlockedFormulaIDs = SavesSystem.Load<List<string>>("Crafting/UnlockedFormulaIDs")
                            ?? new List<string>();

        // 添加默认解锁的配方
        foreach (CraftingFormula formula in FormulaCollection.Entries)
        {
            if (formula.unlockByDefault && !unlockedFormulaIDs.Contains(formula.id))
                unlockedFormulaIDs.Add(formula.id);
        }
        unlockedFormulaIDs.Sort();
    }

    // 解锁配方
    public static void UnlockFormula(string formulaID)
    {
        if (Instance == null || string.IsNullOrEmpty(formulaID)) return;

        CraftingFormula formula = FormulaCollection.Entries
            .FirstOrDefault(e => e.id == formulaID);

        if (!formula.IDValid)
        {
            Debug.LogError($"Invalid formula ID: {formulaID}");
            return;
        }
        if (formula.unlockByDefault)
        {
            Debug.LogError($"Formula is unlocked by default: {formulaID}");
            return;
        }

        if (!Instance.unlockedFormulaIDs.Contains(formulaID))
        {
            Instance.unlockedFormulaIDs.Add(formulaID);
            OnFormulaUnlocked?.Invoke(formulaID);
        }
    }

    // 检查配方是否已解锁
    internal static bool IsFormulaUnlocked(string value)
    {
        return Instance != null && !string.IsNullOrEmpty(value) &&
               Instance.unlockedFormulaIDs.Contains(value);
    }

    // 制作物品
    public async UniTask<List<Item>> Craft(string id)
    {
        CraftingFormula formula = GetFormula(id);
        if (!formula.IDValid) return null;
        return await Craft(formula);
    }
}

// 制作配方结构
[Serializable]
public struct CraftingFormula
{
    public string id;                    // 配方ID
    public ItemEntry result;             // 产出物品
    public string[] tags;                // 标签
    public Cost cost;                    // 制作成本
    public bool unlockByDefault;         // 默认解锁
    public bool lockInDemo;              // Demo版锁定
    public string requirePerk;           // 需要的技能
    public bool hideInIndex;             // 在索引中隐藏

    [Serializable]
    public struct ItemEntry
    {
        [ItemTypeID] public int id;      // 物品TypeID
        public int amount;               // 数量
    }
}
```

### 31.7 枪械武器系统 (ItemAgent_Gun.cs)

游戏中枪械的完整实现（关键部分）：

```csharp
// 源码位置: TeamSoda.Duckov.Core/ItemAgent_Gun.cs
public class ItemAgent_Gun : DuckovItemAgent
{
    // 武器属性（通过Stat系统获取）
    public float ShootSpeed => Item.GetStatValue("ShootSpeed".GetHashCode());
    public float ReloadTime => Item.GetStatValue("ReloadTime".GetHashCode()) / (1f + CharacterReloadSpeedGain);
    public int Capacity => Mathf.RoundToInt(Item.GetStatValue("Capacity".GetHashCode()));
    public float Damage => Item.GetStatValue("Damage".GetHashCode());
    public float CritRate => Item.GetStatValue("CritRate".GetHashCode());
    public float CritDamageFactor => Item.GetStatValue("CritDamageFactor".GetHashCode());
    public float ArmorPiercing => Item.GetStatValue("ArmorPiercing".GetHashCode());
    public float SoundRange => Item.GetStatValue("SoundRange".GetHashCode());

    // 弹匣中的子弹
    public Item BulletItem { get; }
    public int BulletCount => GunItemSetting.BulletCount;

    // 枪械状态机
    public enum GunStates { shootCooling, ready, fire, burstEachShotCooling, empty, reloading }
    private GunStates gunState = GunStates.ready;

    // 开火
    private void TransToFire(bool isFirstShot)
    {
        if (BulletCount <= 0 || Item.Durability <= 0f) return;

        gunState = GunStates.fire;
        Vector3 shootDir = muzzle.forward;

        // 发射多颗子弹（霰弹枪）
        for (int i = 0; i < ShotCount; i++)
        {
            Vector3 dir = shootDir;
            // 应用散射角度
            if (ShotCount > 1)
                dir = Quaternion.Euler(0f, shotAngleOffset, 0f) * shootDir;

            ShootOneBullet(muzzle.position, dir, ...);

            // 发出声音（敌人可以听到）
            if (Holder != null)
            {
                AIMainBrain.MakeSound(new AISound {
                    fromCharacter = Holder,
                    pos = muzzle.position,
                    soundType = SoundTypes.combatSound,
                    radius = SoundRange
                });
            }
        }

        // 后坐力、散射增加
        scatterBeforeControl = Mathf.Clamp(scatterBeforeControl + ScatterGrow, DefaultScatter, MaxScatter);
        AimRecoil(shootDir);

        // 消耗子弹和耐久度
        GunItemSetting.UseABullet();
        if (Holder?.IsMainCharacter == true && LevelManager.Instance.IsRaidMap)
            Item.Durability -= bulletDurabilityCost;

        burstCounter++;
    }

    // 发射单颗子弹
    private void ShootOneBullet(Vector3 muzzlePoint, Vector3 shootDirection, ...)
    {
        // 计算散射
        float scatterAngle = CurrentScatter * Random.Range(-0.5f, 0.5f);
        shootDirection = Quaternion.Euler(0f, scatterAngle, 0f) * shootDirection;

        // 从子弹池获取子弹实例
        Projectile proj = LevelManager.Instance.BulletPool.GetABullet(bulletPfb);

        // 设置子弹属性
        ProjectileContext ctx = new ProjectileContext {
            direction = shootDirection.normalized,
            speed = BulletSpeed * Holder.GunBulletSpeedMultiplier,
            distance = BulletDistance,
            damage = Damage * BulletDamageMultiplier * CharacterDamageMultiplier / ShotCount,
            critRate = CritRate * (1f + CharacterGunCritRateGain),
            critDamageFactor = CritDamageFactor * (1f + CharacterGunCritDamageGain),
            armorPiercing = ArmorPiercing + BulletArmorPiercingGain,
            fromCharacter = Holder,
            fromWeaponItemID = Item.TypeID,
            buff = gunItemSetting.buff,
            buffChance = BulletBuffChanceMultiplier * BuffChance,
            bleedChance = BulletBleedChance
        };

        proj.Init(ctx);
    }

    // 换弹
    public bool BeginReload()
    {
        if (gunState != GunStates.ready && gunState != GunStates.empty) return false;
        if (BulletCount >= Capacity) return false;

        gunState = GunStates.reloading;
        stateTimer = 0f;
        PostStartReloadSound();
        return true;
    }
}
```

### 31.8 近战武器系统 (ItemAgent_MeleeWeapon.cs)

游戏中近战武器的实现：

```csharp
// 源码位置: TeamSoda.Duckov.Core/ItemAgent_MeleeWeapon.cs
public class ItemAgent_MeleeWeapon : DuckovItemAgent
{
    // 武器属性
    public float Damage => Item.GetStatValue("Damage".GetHashCode());
    public float CritRate => Item.GetStatValue("CritRate".GetHashCode());
    public float CritDamageFactor => Item.GetStatValue("CritDamageFactor".GetHashCode());
    public float ArmorPiercing => Item.GetStatValue("ArmorPiercing".GetHashCode());
    public float AttackSpeed => Mathf.Max(0.1f, Item.GetStatValue("AttackSpeed".GetHashCode()));
    public float AttackRange => Item.GetStatValue("AttackRange".GetHashCode());
    public float StaminaCost => Item.GetStatValue("StaminaCost".GetHashCode());
    public float BleedChance => Item.GetStatValue("BleedChance".GetHashCode());

    // 检测并造成伤害
    public void CheckAndDealDamage()
    {
        CheckCollidersInRange(dealDamage: true);
    }

    private int CheckCollidersInRange(bool dealDamage)
    {
        int hitCount = UpdateColliders();  // Physics.OverlapSphereNonAlloc
        int targetsHit = 0;

        for (int i = 0; i < hitCount; i++)
        {
            Collider collider = colliders[i];
            DamageReceiver receiver = collider.GetComponent<DamageReceiver>();

            if (receiver == null || !Team.IsEnemy(receiver.Team, Holder.Team))
                continue;

            // 检查是否在攻击角度内（正面90度）
            Vector3 toTarget = collider.transform.position - Holder.transform.position;
            toTarget.y = 0f;
            if (Vector3.Angle(toTarget.normalized, Holder.CurrentAimDirection) >= 90f)
                continue;

            targetsHit++;

            if (dealDamage)
            {
                DamageInfo damage = new DamageInfo(Holder) {
                    damageValue = Damage * CharacterDamageMultiplier,
                    armorPiercing = ArmorPiercing,
                    critDamageFactor = CritDamageFactor * (1f + CharacterCritDamageGain),
                    critRate = CritRate * (1f + CharacterCritRateGain),
                    fromWeaponItemID = Item.TypeID,
                    bleedChance = BleedChance
                };

                receiver.Hurt(damage);
                receiver.AddBuff(GameplayDataSettings.Buffs.Pain, Holder);

                // 击中反馈
                if (hitFx)
                    Instantiate(hitFx, damage.damagePoint, ...);

                if (Holder == CharacterMainControl.Main)
                    CameraShaker.Shake(...);
            }
        }
        return targetsHit;
    }
}
```

### 31.9 AI药物使用行为 (UseDrug.cs)

AI角色使用药物的NodeCanvas行为：

```csharp
// 源码位置: NodeCanvas.Tasks.Actions/UseDrug.cs
public class UseDrug : ActionTask<AICharacterController>
{
    public bool stopMove;  // 是否停止移动

    protected override string info => stopMove ? "原地打药" : "打药";

    protected override void OnExecute()
    {
        // 获取AI背包中的药物
        Item drugItem = agent.GetDrugItem();
        if (drugItem == null)
        {
            EndAction(false);  // 没有药物，行为失败
            return;
        }

        // 使用药物
        agent.CharacterMainControl.UseItem(drugItem);
    }

    protected override void OnUpdate()
    {
        if (stopMove && agent.IsMoving())
            agent.StopMove();

        // 检查使用是否完成
        if (!agent.CharacterMainControl.useItemAction.Running)
            EndAction(true);
    }

    protected override void OnStop()
    {
        // 使用完成后切换回武器
        agent.CharacterMainControl.SwitchToFirstAvailableWeapon();
    }
}
```

---

## 三十二、高级开发：技能系统

### 32.1 SkillBase 技能基类

技能系统用于创建可投掷物品（手雷、技能物品等）：

```csharp
// 源码位置: TeamSoda.Duckov.Core/SkillBase.cs
public abstract class SkillBase : MonoBehaviour
{
    // 技能配置
    public float staminaCost;          // 体力消耗
    public float coolDownTime;         // 冷却时间
    public float damageRange;          // 伤害范围
    public float delayTime;            // 延迟时间（如手雷引信）

    // 投掷配置
    public float throwForce;           // 投掷力度
    public float throwAngle;           // 投掷角度
    public bool canControlCastDistance; // 可控制投掷距离
    public float grenadeVerticleSpeed; // 垂直速度
    public bool canHurtSelf;           // 是否伤害自己

    // 运行时引用
    protected CharacterMainControl fromCharacter; // 施放者
    protected SkillContext skillContext;           // 技能上下文
    protected SkillReleaseContext skillReleaseContext; // 释放上下文

    // 子类必须实现：技能释放逻辑
    public abstract void OnRelease();

    // 可选重写：技能准备、取消等
    protected virtual void OnPrepare() { }
    protected virtual void OnCancel() { }
}

// 技能上下文（配置参数）
public struct SkillContext
{
    public float castRange;            // 最大施放距离
    public float effectRange;          // 效果范围
    public bool isGrenade;             // 是否为手雷类型
    public float grenageVerticleSpeed; // 垂直速度
    public bool movableWhileAim;       // 瞄准时可移动
    public float skillReadyTime;       // 准备时间
    public bool checkObsticle;         // 检查障碍物
    public bool releaseOnStartAim;     // 开始瞄准时释放
}
```

### 32.2 自定义技能实现示例

```csharp
using UnityEngine;
using Duckov;
using ItemStatsSystem;

/// <summary>
/// 虫洞手雷技能
/// 继承 SkillBase，通过技能系统释放
/// </summary>
public class WormholeGrenadeSkill : SkillBase
{
    // 自定义参数
    public float teleportRange = 8f;

    /// <summary>
    /// 释放技能（投掷手雷）
    /// </summary>
    public override void OnRelease()
    {
        if (fromCharacter == null)
        {
            Debug.LogWarning("没有投掷者");
            return;
        }

        // 获取投掷位置和方向
        Vector3 position = fromCharacter.CurrentUsingAimSocket.position;
        Vector3 releasePoint = skillReleaseContext.releasePoint;

        // 计算投掷方向
        Vector3 point = releasePoint - fromCharacter.transform.position;
        point.y = 0;
        float castDistance = point.magnitude;

        if (!canControlCastDistance)
        {
            castDistance = skillContext.castRange;
        }

        point.Normalize();

        // 投掷手雷
        ThrowGrenade(position, point, castDistance, releasePoint.y);
    }

    private void ThrowGrenade(Vector3 position, Vector3 direction,
        float distance, float height)
    {
        // 计算目标位置
        Vector3 target = position + direction * distance;
        target.y = height;

        // 创建投掷物
        GameObject grenadeObj = new GameObject("CustomGrenade");
        grenadeObj.transform.position = position;

        // 添加投掷物组件
        var projectile = grenadeObj.AddComponent<GrenadeProjectile>();
        projectile.delayTime = delayTime;
        projectile.damageRange = damageRange;

        // 设置伤害信息
        DamageInfo dmgInfo = new DamageInfo(fromCharacter);
        dmgInfo.damageValue = 50f;
        dmgInfo.damageType = DamageTypes.normal;
        projectile.SetDamageInfo(dmgInfo);

        // 计算抛物线速度
        Vector3 velocity = CalculateVelocity(position, target, grenadeVerticleSpeed);

        // 投掷
        projectile.Launch(position, velocity, fromCharacter, canHurtSelf);
    }

    /// <summary>
    /// 计算抛物线投掷速度
    /// </summary>
    private Vector3 CalculateVelocity(Vector3 start, Vector3 target,
        float verticleSpeed)
    {
        float g = Physics.gravity.magnitude;
        float y = target.y - start.y;
        Vector3 vector = target - start;
        vector.y = 0f;
        float num = vector.magnitude;
        float num2 = Mathf.Sqrt(y * y + num * num);
        float num3 = num2 + y;
        float num4 = num2 - y;
        float t = 2f * num3 / (g * num4);
        float num5 = num / Mathf.Sqrt(t);
        float num6 = g * t / 2f;
        Vector3 result = vector.normalized * num5;
        result.y = num6;
        return result;
    }
}
```

### 32.3 将技能绑定到物品

```csharp
/// <summary>
/// 配置物品使用技能系统
/// </summary>
private void ConfigureSkillItem(Item item, int typeId)
{
    // 设置基础属性
    SetFieldValue(item, "typeID", typeId);
    SetFieldValue(item, "stackable", true);
    SetFieldValue(item, "maxStackCount", 3);

    // 创建技能子对象
    GameObject skillObj = new GameObject("GrenadeSkill");
    skillObj.transform.SetParent(item.gameObject.transform);

    // 添加技能组件
    var skill = skillObj.AddComponent<WormholeGrenadeSkill>();
    skill.staminaCost = 0f;
    skill.coolDownTime = 0.5f;
    skill.damageRange = 5f;
    skill.delayTime = 3f;
    skill.throwForce = 15f;
    skill.throwAngle = 30f;
    skill.canControlCastDistance = true;
    skill.grenadeVerticleSpeed = 10f;
    skill.canHurtSelf = true;

    // 设置 SkillContext
    var skillContext = new SkillContext
    {
        castRange = 15f,
        effectRange = 5f,
        isGrenade = true,
        grenageVerticleSpeed = 10f,
        movableWhileAim = true,
        skillReadyTime = 0f,
        checkObsticle = true,
        releaseOnStartAim = false
    };

    var contextField = typeof(SkillBase).GetField("skillContext",
        BindingFlags.NonPublic | BindingFlags.Instance);
    contextField?.SetValue(skill, skillContext);

    // 添加 ItemSetting_Skill 组件
    var skillSetting = item.gameObject.AddComponent<ItemSetting_Skill>();
    SetFieldValue(skillSetting, "Skill", skill);
    SetFieldValue(skillSetting, "onRelease",
        ItemSetting_Skill.OnReleaseAction.reduceCount);

    // 标记为技能物品
    item.SetBool("IsSkill", true, true);
}
```

---

## 三十三、高级开发：自定义Effect组件

### 33.1 自定义 EffectTrigger

EffectTrigger 用于监听游戏事件并触发效果：

```csharp
using UnityEngine;
using ItemStatsSystem;

/// <summary>
/// 自定义闪避触发器
/// 当玩家受伤时有概率触发闪避效果
/// </summary>
public class DodgeTrigger : EffectTrigger
{
    // 闪避概率
    private const float DODGE_CHANCE = 0.1f; // 10%

    // 目标Health组件
    private Health targetHealth;

    // 冷却控制
    private float lastTriggerTime = 0f;
    private const float TRIGGER_COOLDOWN = 0.3f;

    /// <summary>
    /// 当Effect设置目标Item时调用
    /// </summary>
    protected override void OnMasterSetTargetItem(Effect effect, Item item)
    {
        RegisterEvents();
    }

    private void OnEnable()
    {
        RegisterEvents();
    }

    protected override void OnDisable()
    {
        UnregisterEvents();
        base.OnDisable();
    }

    /// <summary>
    /// 注册玩家受伤事件
    /// </summary>
    private void RegisterEvents()
    {
        UnregisterEvents();

        if (base.Master?.Item == null) return;

        // 获取物品绑定的角色
        CharacterMainControl character = base.Master.Item.GetCharacterMainControl();
        if (character == null) return;

        targetHealth = character.Health;
        if (targetHealth != null)
        {
            targetHealth.OnHurtEvent.AddListener(OnTookDamage);
            Debug.Log("[闪避触发器] 已注册玩家受伤事件");
        }
    }

    /// <summary>
    /// 取消注册事件
    /// </summary>
    private void UnregisterEvents()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHurtEvent.RemoveListener(OnTookDamage);
            targetHealth = null;
        }
    }

    /// <summary>
    /// 玩家受伤回调
    /// </summary>
    private void OnTookDamage(DamageInfo damageInfo)
    {
        // 检查冷却
        if (Time.time - lastTriggerTime < TRIGGER_COOLDOWN) return;

        // 获取伤害值
        float damage = damageInfo.finalDamage;
        if (damage <= 0) return;

        // 计算闪避
        if (UnityEngine.Random.value < DODGE_CHANCE)
        {
            lastTriggerTime = Time.time;

            // 触发闪避效果
            base.Trigger(true); // positive = true 表示成功闪避

            Debug.Log($"[闪避触发器] 触发闪避！伤害: {damage:F1}");
        }
    }
}
```

### 33.2 自定义 EffectAction

EffectAction 定义当Trigger触发时执行的具体效果：

```csharp
using UnityEngine;
using ItemStatsSystem;

/// <summary>
/// 闪避动作
/// 当闪避成功时恢复生命值并显示特效
/// </summary>
public class DodgeAction : EffectAction
{
    // 恢复的生命值
    public float healAmount = 5f;

    /// <summary>
    /// 触发时执行（正向触发）
    /// </summary>
    protected override void OnTriggeredPositive()
    {
        // 获取角色
        CharacterMainControl character = base.Master?.Item?.GetCharacterMainControl();
        if (character == null) return;

        // 恢复生命值
        character.Health.AddHealth(healAmount);

        // 显示文字提示
        character.PopText("闪避！");

        // 播放特效
        PlayDodgeEffect(character.transform.position);

        Debug.Log($"[闪避动作] 闪避成功，恢复 {healAmount} 生命");
    }

    /// <summary>
    /// 负向触发（可选）
    /// </summary>
    protected override void OnTriggeredNegative()
    {
        // 效果取消时的逻辑（如果需要）
    }

    /// <summary>
    /// 播放闪避特效
    /// </summary>
    private void PlayDodgeEffect(Vector3 position)
    {
        // 创建粒子特效
        GameObject effectObj = new GameObject("DodgeEffect");
        effectObj.transform.position = position;

        ParticleSystem particles = effectObj.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = new Color(0.2f, 0.8f, 1f, 0.8f); // 青色
        main.startSize = 0.3f;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.duration = 0.3f;
        main.loop = false;

        var emission = particles.emission;
        emission.rateOverTime = 30f;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        particles.Play();
        Object.Destroy(effectObj, 1f);
    }
}
```

### 33.3 在物品上配置Effect系统

```csharp
/// <summary>
/// 为物品添加被动Effect系统
/// </summary>
private void CreateItemEffect(GameObject itemObj, Item item)
{
    // 创建 Effect 容器
    GameObject effectObj = new GameObject("DodgeEffect");
    effectObj.transform.SetParent(itemObj.transform);
    effectObj.transform.localPosition = Vector3.zero;

    // 添加 Effect 组件
    Effect effect = effectObj.AddComponent<Effect>();
    SetFieldValue(effect, "display", true);
    SetFieldValue(effect, "description", "被动：受伤时有10%概率闪避");

    // 添加触发器
    DodgeTrigger trigger = effectObj.AddComponent<DodgeTrigger>();

    // 添加动作
    DodgeAction action = effectObj.AddComponent<DodgeAction>();
    action.healAmount = 5f;

    // 将 Effect 添加到物品的 Effects 列表
    item.Effects.Add(effect);

    Debug.Log("已为物品添加 Effect 系统");
}
```

---

## 三十四、高级开发：近战武器完整示例

### 34.1 近战武器核心属性

近战武器使用 Item.Stats 系统存储属性：

```csharp
// 近战武器标准Stat属性（在Unity Prefab中配置）
public class MeleeWeaponStats
{
    // 属性名 -> Hash值（用于快速查找）
    public static readonly int Damage = "Damage".GetHashCode();
    public static readonly int CritRate = "CritRate".GetHashCode();
    public static readonly int CritDamageFactor = "CritDamageFactor".GetHashCode();
    public static readonly int ArmorPiercing = "ArmorPiercing".GetHashCode();
    public static readonly int AttackSpeed = "AttackSpeed".GetHashCode();
    public static readonly int AttackRange = "AttackRange".GetHashCode();
    public static readonly int StaminaCost = "StaminaCost".GetHashCode();
    public static readonly int BleedChance = "BleedChance".GetHashCode();
}
```

### 34.2 近战攻击逻辑实现

```csharp
using UnityEngine;
using ItemStatsSystem;
using System.Collections;

/// <summary>
/// 自定义近战武器攻击逻辑
/// </summary>
public class CustomMeleeAttack : MonoBehaviour
{
    [Header("特殊攻击配置")]
    public GameObject specialEffectPrefab;
    public float specialDamage = 100f;
    public float specialRange = 10f;
    public float specialCooldown = 8f;

    [Header("状态")]
    private bool isAttacking = false;
    private int comboIndex = 0;
    private float lastAttackTime;
    private float lastSpecialTime;
    private float comboResetTime = 1.5f;

    [Header("引用")]
    private Item weaponItem;
    private CharacterMainControl character;
    private Animator playerAnimator;

    // Stat Hash 缓存
    private static readonly int DamageHash = "Damage".GetHashCode();
    private static readonly int CritRateHash = "CritRate".GetHashCode();
    private static readonly int AttackRangeHash = "AttackRange".GetHashCode();
    private static readonly int StaminaCostHash = "StaminaCost".GetHashCode();

    // 从 Item.Stats 读取属性
    public float Damage => weaponItem?.GetStatValue(DamageHash) ?? 50f;
    public float CritRate => weaponItem?.GetStatValue(CritRateHash) ?? 0.05f;
    public float AttackRange => weaponItem?.GetStatValue(AttackRangeHash) ?? 2f;
    public float StaminaCost => weaponItem?.GetStatValue(StaminaCostHash) ?? 5f;

    void Start()
    {
        weaponItem = GetComponent<Item>();
        character = GetComponentInParent<CharacterMainControl>()
            ?? CharacterMainControl.Main;

        if (character != null)
        {
            playerAnimator = character.GetComponent<Animator>();
        }
    }

    void Update()
    {
        CheckInput();

        // 自动重置连击
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboIndex = 0;
        }
    }

    private void CheckInput()
    {
        if (character == null) return;

        bool attackPressed = Input.GetMouseButtonDown(0);
        bool aimPressed = Input.GetMouseButton(1);

        if (attackPressed && !isAttacking)
        {
            if (aimPressed)
            {
                TrySpecialAttack();
            }
            else
            {
                PerformNormalAttack();
            }
        }
    }

    /// <summary>
    /// 执行普通攻击
    /// </summary>
    private void PerformNormalAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // 播放连击动画
        if (playerAnimator != null)
        {
            string trigger = comboIndex == 0 ? "ForwardSlash" : "BackhandSlash";
            playerAnimator.SetTrigger(trigger);
        }

        StartCoroutine(NormalAttackCoroutine());
        comboIndex = (comboIndex + 1) % 2;
    }

    private IEnumerator NormalAttackCoroutine()
    {
        yield return new WaitForSeconds(0.3f);
        PerformMeleeDamage();
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

    /// <summary>
    /// 近战伤害判定（扇形范围）
    /// </summary>
    public void PerformMeleeDamage()
    {
        if (character == null) return;

        // 消耗体力
        if (StaminaCost > 0)
        {
            if (character.CurrentStamina < StaminaCost)
            {
                Debug.Log("体力不足");
                return;
            }
            character.UseStamina(StaminaCost);
        }

        // 扇形范围检测
        Vector3 origin = character.transform.position + character.transform.forward;
        Vector3 forward = character.transform.forward;
        Collider[] hits = Physics.OverlapSphere(origin, AttackRange);

        foreach (Collider hit in hits)
        {
            DamageReceiver receiver = hit.GetComponent<DamageReceiver>();
            if (receiver == null) continue;

            // 检查是否是敌人
            if (!Team.IsEnemy(receiver.Team, character.Team)) continue;

            // 检查是否在90度扇形范围内
            Vector3 toTarget = (hit.transform.position - character.transform.position);
            toTarget.y = 0;
            if (Vector3.Angle(forward, toTarget.normalized) <= 90f)
            {
                // 使用标准 DamageInfo
                DamageInfo damageInfo = new DamageInfo(character);
                damageInfo.damageValue = Damage;
                damageInfo.critRate = CritRate;
                damageInfo.fromWeaponItemID = weaponItem?.TypeID ?? 0;

                receiver.Hurt(damageInfo);
                Debug.Log($"命中 {hit.name}，伤害: {Damage}");
            }
        }
    }

    /// <summary>
    /// 尝试特殊攻击
    /// </summary>
    private void TrySpecialAttack()
    {
        if (Time.time - lastSpecialTime < specialCooldown)
        {
            Debug.Log($"特殊攻击冷却中");
            return;
        }

        isAttacking = true;
        lastSpecialTime = Time.time;
        lastAttackTime = Time.time;

        StartCoroutine(SpecialAttackCoroutine());
    }

    private IEnumerator SpecialAttackCoroutine()
    {
        // 蓄力
        yield return new WaitForSeconds(0.3f);

        // 冲刺
        yield return StartCoroutine(DashMovement(3f));

        // 释放特效
        LaunchSpecialEffect();

        yield return new WaitForSeconds(0.6f);
        isAttacking = false;
    }

    private IEnumerator DashMovement(float distance)
    {
        Vector3 dashDir = character.transform.forward;
        Vector3 startPos = character.transform.position;
        Vector3 endPos = startPos + dashDir * distance;

        // 检查障碍物
        if (Physics.Raycast(startPos, dashDir, out RaycastHit hit, distance))
        {
            endPos = hit.point - dashDir * 0.5f;
        }

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            character.transform.position = Vector3.Lerp(startPos, endPos,
                elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        character.transform.position = endPos;
    }

    private void LaunchSpecialEffect()
    {
        if (specialEffectPrefab == null) return;

        Vector3 spawnPos = character.transform.position + Vector3.up
            + character.transform.forward * 1.5f;
        Quaternion spawnRot = Quaternion.LookRotation(character.transform.forward);

        GameObject effect = Instantiate(specialEffectPrefab, spawnPos, spawnRot);

        // 配置投射物（如果有）
        var projectile = effect.GetComponent<Projectile>();
        if (projectile != null)
        {
            // 设置投射物属性
        }
    }
}
```

### 34.3 近战武器物品配置

```csharp
/// <summary>
/// 创建完整的近战武器物品
/// </summary>
private void CreateMeleeWeapon()
{
    GameObject itemObj = new GameObject("CustomSword");
    DontDestroyOnLoad(itemObj);

    // 添加视觉模型
    CreateSwordVisual(itemObj);

    // 添加 Item 组件
    Item item = itemObj.AddComponent<Item>();
    SetFieldValue(item, "typeID", 990100);
    SetFieldValue(item, "displayName", "CustomSword_Name");
    SetFieldValue(item, "description", "CustomSword_Desc");
    SetFieldValue(item, "stackable", false);
    SetFieldValue(item, "quality", 4);
    SetFieldValue(item, "value", 50000);

    // 配置武器属性（Stats）
    ConfigureWeaponStats(item);

    // 添加攻击逻辑
    var attack = itemObj.AddComponent<CustomMeleeAttack>();
    attack.specialDamage = 100f;
    attack.specialRange = 10f;
    attack.specialCooldown = 8f;

    // 添加武器设置组件
    var weaponSetting = itemObj.AddComponent<ItemSetting_MeleeWeapon>();
    SetFieldValue(weaponSetting, "item", item);

    // 注册物品
    item.Initialize();
    ItemAssetsCollection.AddDynamicEntry(item);
}

private void ConfigureWeaponStats(Item item)
{
    // 通过反射访问 Stats 集合
    var stats = item.Stats;
    if (stats == null)
    {
        Debug.LogWarning("Stats 集合为空");
        return;
    }

    // 设置各项属性
    // 注意：具体实现取决于游戏的 StatCollection API
    item.SetFloat("Damage", 60f, true);
    item.SetFloat("CritRate", 0.1f, true);
    item.SetFloat("CritDamageFactor", 2f, true);
    item.SetFloat("ArmorPiercing", 5f, true);
    item.SetFloat("AttackSpeed", 1.2f, true);
    item.SetFloat("AttackRange", 2.5f, true);
    item.SetFloat("StaminaCost", 8f, true);
    item.SetFloat("BleedChance", 0.15f, true);
}
```

---

## 三十五、总结

本手册涵盖了《逃离鸭科夫》Mod开发的所有核心内容：

### 第一部分：基础与物品系统
1. **环境配置** - .NET Standard 2.1、Unity版本匹配、DLL引用
2. **物品生命周期** - Prefab加载、注册、初始化、效果激活
3. **使用系统** - UsageBehavior、CanBeUsed、OnUse
4. **效果系统** - Trigger、Filter、Action组合

### 第二部分：游戏核心系统
5. **战斗系统** - 伤害计算、护甲、暴击、元素效果
6. **经济系统** - 货币、商店、交易
7. **制作系统** - 配方、解锁、制作流程
8. **Buff系统** - 状态效果、叠加、互斥
9. **事件系统** - 所有可用事件的订阅与处理

### 第三部分：实战开发教程
10. **C#项目配置** - 完整.csproj配置、DLL引用表
11. **Unity项目配置** - Unity安装、文件夹结构、DLL导入
12. **Prefab制作** - Item组件配置、UsageBehavior添加
13. **AssetBundle打包** - 构建脚本、加载代码
14. **Mod目录结构** - info.ini配置、部署脚本
15. **调试方法** - 日志查看、测试快捷键、错误排查
16. **完整示例** - 消耗品物品从零到上线

### 第四部分：游戏内部实现参考
17. **游戏源码** - Health、Drug、Buff、商店、制作、枪械、近战等系统的真实实现

### 第五部分：高级开发教程
18. **技能系统** - SkillBase继承、SkillContext配置、手雷投掷实现
19. **自定义Effect组件** - EffectTrigger事件监听、EffectAction效果执行
20. **近战武器开发** - Stats属性系统、连击系统、DamageInfo伤害处理

### 快速开始检查清单

```
从零开始开发一个Mod：

1. □ 创建C#项目（netstandard2.1）
2. □ 配置DLL引用（复制.csproj模板）
3. □ 创建ModBehaviour类（继承Duckov.Modding.ModBehaviour）
4. □ 创建UsageBehavior类（如果物品可使用）
5. □ 设置本地化文本（LocalizationManager.SetOverrideText）
6. □ 创建物品Prefab（代码创建或Unity打包）
7. □ 注册物品（ItemAssetsCollection.AddDynamicEntry）
8. □ 编译DLL（dotnet build -c Release）
9. □ 创建info.ini配置文件
10. □ 部署到游戏Mods目录
11. □ 测试功能（查看日志、使用F9快捷键）
12. □ 实现OnDestroy清理（取消事件、移除物品）
```

### 高级开发检查清单

```
开发技能/投掷物品：
1. □ 创建继承自 SkillBase 的技能类
2. □ 实现 OnRelease() 方法处理技能释放
3. □ 配置 SkillContext（投掷距离、效果范围等）
4. □ 创建投掷物 Projectile 组件
5. □ 使用 ItemSetting_Skill 绑定到物品

开发被动效果装备：
1. □ 创建继承自 EffectTrigger 的触发器
2. □ 在 OnMasterSetTargetItem 或 OnEnable 中注册事件
3. □ 在 OnDisable 中取消注册事件
4. □ 创建 EffectAction 处理触发后的效果
5. □ 配置 Effect 组件关联 Trigger 和 Action

开发近战武器：
1. □ 使用 Hash 缓存属性名（如 "Damage".GetHashCode()）
2. □ 通过 Item.GetStatValue() 读取武器属性
3. □ 使用 Physics.OverlapSphere 检测攻击范围
4. □ 创建 DamageInfo 并调用 DamageReceiver.Hurt()
5. □ 配置 ItemSetting_MeleeWeapon 组件
```

开发者可以参考游戏内部实现代码来理解各系统的工作原理，并据此创建自己的Mod。

---

## 三十六、高级开发：投掷物系统完整实现

### 36.1 投掷物基础属性

```csharp
/// <summary>
/// 投掷物组件（如手雷）
/// 参考游戏原版 Grenade 实现
/// </summary>
public class CustomProjectile : MonoBehaviour
{
    // ========== 核心属性 ==========

    [Header("伤害")]
    public float damage = 50f;              // 伤害值
    public float damageRange = 5f;          // 爆炸范围半径

    [Header("地雷模式")]
    public bool isLandmine = false;         // 是否是地雷模式
    public float landmineTriggerRange = 0.5f; // 地雷触发范围

    [Header("延迟")]
    public float delayTime = 3f;            // 引爆延迟时间（秒）
    public bool delayFromCollide = false;   // 是否碰撞后才开始计时

    [Header("多投掷物")]
    public int blastCount = 1;              // 爆炸投掷物数量
    public float blastDelayTimeSpace = 0.2f; // 多投掷物间隔
    public float blastAngle = 0f;           // 爆炸角度

    [Header("特效")]
    public bool createExplosion = true;     // 是否创建爆炸特效
    [Range(0f, 1f)]
    public float explosionShakeStrength = 1f; // 爆炸震动强度
    public ExplosionFxTypes fxType = ExplosionFxTypes.custom; // 特效类型
    public GameObject fx;                   // 自定义特效预制体
    public GameObject createOnExplode;      // 爆炸时生成的物体
    public float destroyDelay = 0.5f;       // 销毁延迟

    [Header("音效")]
    public bool hasCollideSound = true;     // 是否有碰撞音效
    public string collideSound = "GrenadeCollide"; // 碰撞音效键
    public int makeSoundCount = 3;          // 碰撞发声次数
    public bool isDangerForAi = true;       // 是否对AI危险

    [Header("投掷")]
    public float throwForce = 15f;          // 投掷力度
    public float throwAngle = 30f;          // 向上投掷角度
    public bool canControlCastDistance = true; // 可控制投掷距离
    public float grenadeVerticleSpeed = 10f;   // 垂直速度
    public bool canHurtSelf = true;         // 是否伤害自己

    // ========== 运行时状态 ==========
    protected Rigidbody rb;
    protected DamageInfo damageInfo;
    protected CharacterMainControl thrower;
    protected bool isThrown = false;
    protected bool hasExploded = false;
    protected bool collide = false;
}
```

### 36.2 投掷物发射逻辑

```csharp
/// <summary>
/// 发射投掷物
/// </summary>
public void Launch(Vector3 position, Vector3 velocity,
    CharacterMainControl throwerCharacter, bool hurtSelf)
{
    if (isThrown) return;

    thrower = throwerCharacter;
    isThrown = true;
    canHurtSelf = hurtSelf;

    // 设置初始位置
    transform.position = position;
    rb.isKinematic = false;

    // 应用速度
    rb.velocity = velocity;

    // 添加旋转效果
    rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

    // 开始计时（如果不是碰撞后才计时）
    if (!delayFromCollide)
    {
        StartCoroutine(FuseCountdown());
    }

    // 避免与投掷者碰撞
    if (thrower != null)
    {
        Collider throwerCollider = thrower.GetComponent<Collider>();
        Collider myCollider = GetComponent<Collider>();
        if (throwerCollider != null && myCollider != null)
        {
            Physics.IgnoreCollision(throwerCollider, myCollider, true);
        }
    }

    Debug.Log($"[投掷物] 已发射，位置: {position}, 速度: {velocity}");
}

/// <summary>
/// 引信倒计时协程
/// </summary>
private IEnumerator FuseCountdown()
{
    float delayTimer = 0f;

    while (!hasExploded)
    {
        yield return null;

        // 如果不是碰撞后计时，或者已经碰撞，则开始延迟计时
        if (!delayFromCollide || collide)
        {
            delayTimer += Time.deltaTime;
        }

        // 检查是否应该爆炸
        if (!isLandmine && delayTimer > delayTime)
        {
            Explode();
            break;
        }
    }
}
```

### 36.3 碰撞检测与音效

```csharp
/// <summary>
/// 碰撞检测
/// </summary>
void OnCollisionEnter(Collision collision)
{
    if (!isThrown) return;

    if (!collide)
    {
        collide = true;
    }

    // 速度衰减
    Vector3 velocity = rb.velocity;
    velocity.x *= 0.5f;
    velocity.z *= 0.5f;
    rb.velocity = velocity;
    rb.angularVelocity = rb.angularVelocity * 0.3f;

    // 播放碰撞音效
    if (hasCollideSound && soundMadeCount < makeSoundCount)
    {
        if (Time.time - lastSoundTime > 0.3f)
        {
            soundMadeCount++;
            lastSoundTime = Time.time;

            // AI 声音传播
            if (isDangerForAi)
            {
                AISound sound = default(AISound);
                sound.fromObject = gameObject;
                sound.pos = transform.position;
                sound.fromTeam = damageInfo.fromCharacter?.Team ?? Teams.all;
                sound.soundType = SoundTypes.grenadeDropSound;
                sound.radius = 20f;
                AIMainBrain.MakeSound(sound);
            }

            // 播放碰撞音效
            if (!string.IsNullOrEmpty(collideSound))
            {
                AudioManager.Post(collideSound, gameObject);
            }
        }
    }
}
```

### 36.4 爆炸与伤害逻辑

```csharp
/// <summary>
/// 引爆投掷物
/// </summary>
private void Explode()
{
    if (hasExploded) return;
    hasExploded = true;

    Debug.Log("[投掷物] 引爆！");

    // 创建爆炸特效
    if (createExplosion)
    {
        CreateExplosionEffect();
    }

    // 自定义特效
    if (fx != null)
    {
        Instantiate(fx, transform.position, Quaternion.identity);
    }

    // 爆炸时生成物体
    if (createOnExplode != null)
    {
        Instantiate(createOnExplode, transform.position, Quaternion.identity);
    }

    // 对范围内的角色造成伤害
    DealDamageInRange();

    // 执行爆炸事件
    onExplodeEvent?.Invoke();

    // 销毁
    if (destroyDelay > 0f)
    {
        Destroy(gameObject, destroyDelay);
    }
    else
    {
        Destroy(gameObject);
    }
}

/// <summary>
/// 对范围内的角色造成伤害
/// </summary>
private void DealDamageInRange()
{
    Vector3 explosionCenter = transform.position;

    // 使用 Physics.OverlapSphere 获取爆炸范围内的碰撞体
    Collider[] colliders = Physics.OverlapSphere(explosionCenter, damageRange);

    HashSet<DamageReceiver> damaged = new HashSet<DamageReceiver>();

    foreach (var collider in colliders)
    {
        DamageReceiver receiver = collider.GetComponentInParent<DamageReceiver>();
        if (receiver != null && !damaged.Contains(receiver))
        {
            // 检查是否可以伤害（排除自己）
            CharacterMainControl character = receiver.GetComponent<CharacterMainControl>();
            if (!canHurtSelf && character == thrower)
            {
                continue;
            }

            damaged.Add(receiver);

            // 计算距离衰减伤害
            float distance = Vector3.Distance(explosionCenter, receiver.transform.position);
            float damageMultiplier = 1f - (distance / damageRange);
            damageMultiplier = Mathf.Clamp01(damageMultiplier);

            // 创建伤害信息
            DamageInfo dmg = new DamageInfo(thrower);
            dmg.damageValue = damage * damageMultiplier;
            dmg.damageType = DamageTypes.explosive;
            dmg.damagePoint = explosionCenter;
            dmg.fromWeaponItemID = damageInfo?.fromWeaponItemID ?? 0;

            receiver.Hurt(dmg);

            Debug.Log($"[投掷物] 对 {receiver.name} 造成 {dmg.damageValue:F1} 点伤害");
        }
    }
}
```

### 36.5 爆炸特效生成

```csharp
/// <summary>
/// 创建爆炸特效
/// </summary>
private void CreateExplosionEffect()
{
    // 使用游戏的爆炸管理器
    if (LevelManager.Instance?.ExplosionManager != null)
    {
        // 获取爆炸类型
        ExplosionType explosionType = GetExplosionType(fxType);

        // 创建爆炸
        LevelManager.Instance.ExplosionManager.CreateExplosion(
            transform.position,
            explosionType,
            explosionShakeStrength
        );
    }
    else
    {
        // 手动创建简单爆炸特效
        CreateSimpleExplosionEffect();
    }
}

/// <summary>
/// 创建简单爆炸特效
/// </summary>
private void CreateSimpleExplosionEffect()
{
    GameObject explosionObj = new GameObject("Explosion");
    explosionObj.transform.position = transform.position;

    // 添加粒子系统
    ParticleSystem particles = explosionObj.AddComponent<ParticleSystem>();
    var main = particles.main;
    main.startColor = new Color(1f, 0.5f, 0.1f, 1f); // 橙色
    main.startSize = 2f;
    main.startLifetime = 0.5f;
    main.startSpeed = 5f;
    main.duration = 0.3f;
    main.loop = false;

    var emission = particles.emission;
    emission.SetBursts(new ParticleSystem.Burst[] {
        new ParticleSystem.Burst(0f, 30)
    });

    var shape = particles.shape;
    shape.shapeType = ParticleSystemShapeType.Sphere;
    shape.radius = 0.5f;

    particles.Play();

    // 添加爆炸光源
    Light explosionLight = explosionObj.AddComponent<Light>();
    explosionLight.type = LightType.Point;
    explosionLight.color = new Color(1f, 0.6f, 0.2f);
    explosionLight.intensity = 5f;
    explosionLight.range = damageRange * 2f;

    // 销毁
    Object.Destroy(explosionObj, 2f);
}
```

### 36.6 投掷物速度计算

```csharp
/// <summary>
/// 计算抛物线投掷速度
/// 用于让投掷物精确落到目标点
/// </summary>
public static Vector3 CalculateThrowVelocity(Vector3 start, Vector3 target, float verticleSpeed)
{
    float g = Physics.gravity.magnitude;
    float y = target.y - start.y;
    Vector3 horizontal = target - start;
    horizontal.y = 0f;
    float horizontalDistance = horizontal.magnitude;

    // 计算飞行时间
    float distance = Mathf.Sqrt(y * y + horizontalDistance * horizontalDistance);
    float upDistance = distance + y;
    float downDistance = distance - y;
    float t = 2f * upDistance / (g * downDistance);

    // 计算水平和垂直速度
    float horizontalSpeed = horizontalDistance / Mathf.Sqrt(t);
    float verticalSpeed = g * t / 2f;

    Vector3 result = horizontal.normalized * horizontalSpeed;
    result.y = verticalSpeed;
    return result;
}

/// <summary>
/// 简化版投掷速度计算
/// </summary>
public static Vector3 SimpleThrowVelocity(Vector3 direction, float force, float upAngle)
{
    // 将方向向上旋转指定角度
    Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
    Vector3 adjustedDirection = Quaternion.AngleAxis(-upAngle, right) * direction;
    return adjustedDirection.normalized * force;
}
```

### 36.7 完整投掷物使用示例

```csharp
/// <summary>
/// 在技能中使用投掷物
/// </summary>
public class GrenadeSkill : SkillBase
{
    public GameObject grenadePrefab;
    public float throwForce = 15f;
    public float damage = 100f;
    public float damageRange = 5f;

    public override void OnRelease()
    {
        if (fromCharacter == null) return;

        // 获取投掷位置和目标
        Vector3 spawnPosition = fromCharacter.CurrentUsingAimSocket.position;
        Vector3 targetPosition = skillReleaseContext.releasePoint;

        // 创建投掷物
        GameObject grenadeObj = Instantiate(grenadePrefab, spawnPosition, Quaternion.identity);
        CustomProjectile projectile = grenadeObj.GetComponent<CustomProjectile>();

        if (projectile == null)
        {
            projectile = grenadeObj.AddComponent<CustomProjectile>();
        }

        // 配置投掷物
        projectile.damage = damage;
        projectile.damageRange = damageRange;
        projectile.delayTime = 3f;

        // 设置伤害信息
        DamageInfo dmgInfo = new DamageInfo(fromCharacter);
        dmgInfo.damageValue = damage;
        projectile.SetDamageInfo(dmgInfo);

        // 计算投掷速度
        Vector3 velocity = CustomProjectile.CalculateThrowVelocity(
            spawnPosition, targetPosition, 10f);

        // 发射
        projectile.Launch(spawnPosition, velocity, fromCharacter, true);
    }
}
```

---

## 三十七、商店系统集成

### 37.1 StockShopDatabase 商店数据库

游戏使用 `StockShopDatabase` 管理所有商人的商品列表。通过操作这个数据库，可以将自定义物品添加到商店中。

```csharp
// 源码位置: TeamSoda.Duckov.Core/StockShopDatabase.cs
public class StockShopDatabase : ScriptableObject
{
    public static StockShopDatabase Instance { get; }

    // 商人配置列表
    public List<MerchantProfile> merchantProfiles;

    // 商人配置结构
    [Serializable]
    public class MerchantProfile
    {
        public string merchantID;           // 商人ID (如 "Albert")
        public List<ItemEntry> entries;     // 商品列表
    }

    // 商品条目结构
    [Serializable]
    public class ItemEntry
    {
        public int typeID;                  // 物品TypeID
        public int maxStock;                // 最大库存数量
        public float priceFactor;           // 价格倍率
        public float possibility;           // 刷新概率 (0-1)
        public bool forceUnlock;            // 强制解锁（忽略解锁条件）
        public bool lockInDemo;             // Demo版是否锁定
    }
}
```

### 37.2 将物品添加到商店

```csharp
/// <summary>
/// 注册武器到商店（自动售卖机）
/// </summary>
private void RegisterToShop()
{
    try
    {
        // 获取商店数据库实例
        var shopDatabase = StockShopDatabase.Instance;
        if (shopDatabase == null)
        {
            Debug.LogWarning("[MyMod] 无法获取商店数据库");
            return;
        }

        // 获取商人配置列表
        var merchantProfiles = shopDatabase.merchantProfiles;
        if (merchantProfiles == null || merchantProfiles.Count == 0)
        {
            Debug.LogWarning("[MyMod] 商店数据库中没有商人配置");
            return;
        }

        // 遍历所有商人，添加物品
        foreach (var profile in merchantProfiles)
        {
            if (profile == null || profile.entries == null) continue;

            // 检查是否已存在
            bool exists = false;
            foreach (var entry in profile.entries)
            {
                if (entry.typeID == MY_ITEM_TYPE_ID)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                // 创建新的商品条目
                var newEntry = new StockShopDatabase.ItemEntry
                {
                    typeID = MY_ITEM_TYPE_ID,
                    maxStock = 1,           // 库存数量
                    priceFactor = 1.0f,     // 价格倍率
                    possibility = 1.0f,     // 100%出现概率
                    forceUnlock = true,     // 强制解锁
                    lockInDemo = false
                };

                profile.entries.Add(newEntry);
                Debug.Log($"[MyMod] 已添加物品到商人 {profile.merchantID}");
            }
        }

        Debug.Log("[MyMod] 物品已添加到所有商店");
    }
    catch (System.Exception e)
    {
        Debug.LogError($"[MyMod] 注册商店失败: {e.Message}");
    }
}
```

### 37.3 商品参数说明

| 参数 | 类型 | 说明 | 推荐值 |
|------|------|------|--------|
| typeID | int | 物品的唯一标识 | 与物品Prefab中设置的一致 |
| maxStock | int | 每次刷新最大库存 | 普通物品3-5，稀有物品1 |
| priceFactor | float | 价格倍率 | 1.0正常价格，>1加价，<1打折 |
| possibility | float | 刷新时出现概率 | 普通物品0.5-1.0，稀有物品0.1-0.3 |
| forceUnlock | bool | 是否强制解锁 | Mod物品建议true |
| lockInDemo | bool | Demo版是否锁定 | 通常false |

---

## 三十八、战利品箱注入

### 38.1 LootBoxLoader 战利品生成器

`LootBoxLoader` 是控制场景中箱子物品生成的组件。通过修改其 `fixedItems` 列表，可以将自定义物品添加到箱子的掉落池中。

```csharp
/// <summary>
/// 注入到 LootBoxLoader（影响新生成的箱子）
/// </summary>
private void InjectToLootBoxLoaders()
{
    // 获取场景中所有 LootBoxLoader
    var lootBoxLoaders = FindObjectsOfType<Duckov.Utilities.LootBoxLoader>();

    foreach (var loader in lootBoxLoaders)
    {
        if (loader == null) continue;

        try
        {
            // 通过反射获取 fixedItems 列表
            var fixedItemsField = loader.GetType().GetField("fixedItems",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            if (fixedItemsField != null)
            {
                var fixedItems = fixedItemsField.GetValue(loader) as List<int>;
                if (fixedItems != null)
                {
                    // 按概率添加物品（5%概率，稀有物品）
                    if (!fixedItems.Contains(MY_ITEM_TYPE_ID) && Random.value < 0.05f)
                    {
                        fixedItems.Add(MY_ITEM_TYPE_ID);
                        Debug.Log($"[MyMod] 已修改 LootBoxLoader: {loader.gameObject.name}");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[MyMod] 修改 LootBoxLoader 失败: {e.Message}");
        }
    }
}
```

### 38.2 InteractableLootbox 已存在的箱子

对于场景中已经生成的箱子（`InteractableLootbox`），可以直接操作其背包来注入物品。

```csharp
/// <summary>
/// 注入到已存在的箱子背包
/// </summary>
private void InjectToExistingLootboxes(HashSet<int> processedBoxes)
{
    // 获取场景中所有 InteractableLootbox
    var lootboxes = FindObjectsOfType<InteractableLootbox>();

    foreach (var lootbox in lootboxes)
    {
        if (lootbox == null) continue;

        int instanceId = lootbox.GetInstanceID();

        // 跳过已处理的箱子
        if (processedBoxes.Contains(instanceId)) continue;

        try
        {
            // 尝试获取箱子的背包
            var getInventoryMethod = typeof(InteractableLootbox).GetMethod(
                "GetOrCreateInventory",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static);

            if (getInventoryMethod != null)
            {
                var inventory = getInventoryMethod.Invoke(null,
                    new object[] { lootbox }) as Inventory;

                if (inventory != null)
                {
                    // 按概率添加物品（5%概率）
                    if (Random.value < 0.05f)
                    {
                        bool success = TryAddItemToInventory(inventory);
                        if (success)
                        {
                            Debug.Log($"[MyMod] 已向箱子 {lootbox.gameObject.name} 注入物品");
                        }
                    }
                }
            }

            // 标记为已处理
            processedBoxes.Add(instanceId);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[MyMod] 注入到箱子失败: {e.Message}");
            processedBoxes.Add(instanceId);
        }
    }
}

/// <summary>
/// 尝试将物品添加到背包
/// </summary>
private bool TryAddItemToInventory(Inventory inventory)
{
    try
    {
        if (myItemPrefab == null) return false;

        // 创建物品实例
        var newItem = myItemPrefab.CreateInstance();
        if (newItem == null) return false;

        // 添加到背包
        bool success = inventory.AddItem(newItem);

        if (!success)
        {
            // 如果添加失败，销毁创建的物品
            Destroy(newItem.gameObject);
        }

        return success;
    }
    catch (System.Exception e)
    {
        Debug.LogWarning($"[MyMod] 添加物品到背包失败: {e.Message}");
        return false;
    }
}
```

### 38.3 箱子注入协程

使用协程定期扫描场景，持续注入物品：

```csharp
/// <summary>
/// 箱子物品注入协程
/// 定期扫描场景中的箱子，注入物品
/// </summary>
private IEnumerator LootBoxInjectionRoutine()
{
    // 等待场景完全加载
    yield return new WaitForSeconds(2f);

    Debug.Log("[MyMod] 开始箱子物品注入...");

    // 已处理的箱子集合，避免重复注入
    HashSet<int> processedBoxes = new HashSet<int>();

    while (true)
    {
        try
        {
            // 注入到 LootBoxLoader（影响箱子生成的物品池）
            InjectToLootBoxLoaders();

            // 注入到已存在的箱子背包
            InjectToExistingLootboxes(processedBoxes);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[MyMod] 箱子注入时发生错误: {e.Message}");
        }

        // 每隔一段时间检查一次
        yield return new WaitForSeconds(5f);
    }
}

// 在 Start() 中启动协程
void Start()
{
    // ... 其他初始化 ...
    StartCoroutine(LootBoxInjectionRoutine());
}
```

---

## 三十九、多语言本地化

### 39.1 LocalizationManager 本地化管理器

游戏使用 `LocalizationManager` 管理多语言文本。Mod可以通过事件监听实现语言切换时的动态更新。

```csharp
// 源码位置: SodaCraft.Localizations/LocalizationManager.cs
public static class LocalizationManager
{
    // 设置覆盖文本（Mod使用）
    public static void SetOverrideText(string key, string value);

    // 获取本地化文本
    public static string GetPlainText(string key);

    // 语言切换事件
    public static event Action<SystemLanguage> OnSetLanguage;

    // 当前语言
    public static SystemLanguage CurrentLanguage { get; }
}
```

### 39.2 多语言支持实现

```csharp
/// <summary>
/// 设置本地化文本（带多语言支持）
/// </summary>
private void SetupLocalization()
{
    // 设置默认语言（中文）的文本
    LocalizationManager.SetOverrideText("MyItem_Name", "我的物品");
    LocalizationManager.SetOverrideText("MyItem_Desc",
        "这是一个自定义物品。\n\n" +
        "<color=#87CEEB>特殊能力：</color>\n" +
        "• 功能一\n" +
        "• 功能二\n\n" +
        "<color=#FFD700>「传说描述」</color>");

    // 监听语言切换事件
    LocalizationManager.OnSetLanguage += OnLanguageChanged;

    // 根据当前语言立即更新
    OnLanguageChanged(LocalizationManager.CurrentLanguage);

    Debug.Log("[MyMod] 本地化设置完成");
}

/// <summary>
/// 语言切换回调
/// </summary>
private void OnLanguageChanged(SystemLanguage language)
{
    switch (language)
    {
        case SystemLanguage.English:
            LocalizationManager.SetOverrideText("MyItem_Name", "My Item");
            LocalizationManager.SetOverrideText("MyItem_Desc",
                "This is a custom item.\n\n" +
                "<color=#87CEEB>Special Abilities:</color>\n" +
                "• Feature one\n" +
                "• Feature two\n\n" +
                "<color=#FFD700>\"Legendary description\"</color>");
            break;

        case SystemLanguage.Japanese:
            LocalizationManager.SetOverrideText("MyItem_Name", "私のアイテム");
            LocalizationManager.SetOverrideText("MyItem_Desc",
                "これはカスタムアイテムです。\n\n" +
                "<color=#87CEEB>特殊能力：</color>\n" +
                "• 機能一\n" +
                "• 機能二");
            break;

        default: // 中文
            LocalizationManager.SetOverrideText("MyItem_Name", "我的物品");
            LocalizationManager.SetOverrideText("MyItem_Desc",
                "这是一个自定义物品。\n\n" +
                "<color=#87CEEB>特殊能力：</color>\n" +
                "• 功能一\n" +
                "• 功能二\n\n" +
                "<color=#FFD700>「传说描述」</color>");
            break;
    }

    Debug.Log($"[MyMod] 语言已切换到: {language}");
}

/// <summary>
/// Mod卸载时取消事件监听
/// </summary>
void OnDestroy()
{
    // ⚠️ 重要：必须取消事件监听
    LocalizationManager.OnSetLanguage -= OnLanguageChanged;
}
```

### 39.3 富文本格式支持

游戏支持 Unity 富文本标签，可用于物品描述：

| 标签 | 用途 | 示例 |
|------|------|------|
| `<color=#RRGGBB>` | 文字颜色 | `<color=#FFD700>金色文字</color>` |
| `<b>` | 粗体 | `<b>粗体文字</b>` |
| `<i>` | 斜体 | `<i>斜体文字</i>` |
| `<size=N>` | 字号 | `<size=14>小字</size>` |
| `\n` | 换行 | `第一行\n第二行` |

常用颜色参考：
- 普通: `#FFFFFF` (白色)
- 稀有: `#87CEEB` (天蓝色)
- 传说: `#FFD700` (金色)
- 警告: `#FF6B6B` (红色)
- 治疗: `#90EE90` (浅绿色)

---

## 四十、高级近战武器系统

### 40.1 Stat Hash 缓存优化

频繁调用 `GetHashCode()` 会影响性能。使用静态字段缓存 Hash 值：

```csharp
public class MeleeWeaponAttack : MonoBehaviour
{
    // Stat Hash 缓存（静态字段，只计算一次）
    private static readonly int DamageHash = "Damage".GetHashCode();
    private static readonly int CritRateHash = "CritRate".GetHashCode();
    private static readonly int CritDamageFactorHash = "CritDamageFactor".GetHashCode();
    private static readonly int ArmorPiercingHash = "ArmorPiercing".GetHashCode();
    private static readonly int AttackSpeedHash = "AttackSpeed".GetHashCode();
    private static readonly int AttackRangeHash = "AttackRange".GetHashCode();
    private static readonly int StaminaCostHash = "StaminaCost".GetHashCode();
    private static readonly int BleedChanceHash = "BleedChance".GetHashCode();

    private Item weaponItem;

    #region 标准 Stat 属性（从 Item.Stats 读取）

    /// <summary>基础伤害</summary>
    public float Damage => weaponItem != null ?
        weaponItem.GetStatValue(DamageHash) : 50f;

    /// <summary>暴击率</summary>
    public float CritRate => weaponItem != null ?
        weaponItem.GetStatValue(CritRateHash) : 0.05f;

    /// <summary>暴击伤害倍率</summary>
    public float CritDamageFactor => weaponItem != null ?
        weaponItem.GetStatValue(CritDamageFactorHash) : 1.5f;

    /// <summary>护甲穿透</summary>
    public float ArmorPiercing => weaponItem != null ?
        weaponItem.GetStatValue(ArmorPiercingHash) : 0f;

    /// <summary>攻击速度</summary>
    public float AttackSpeed => weaponItem != null ?
        Mathf.Max(0.1f, weaponItem.GetStatValue(AttackSpeedHash)) : 1f;

    /// <summary>攻击范围</summary>
    public float AttackRange => weaponItem != null ?
        weaponItem.GetStatValue(AttackRangeHash) : 2f;

    /// <summary>体力消耗</summary>
    public float StaminaCost => weaponItem != null ?
        weaponItem.GetStatValue(StaminaCostHash) : 5f;

    /// <summary>流血几率</summary>
    public float BleedChance => weaponItem != null ?
        weaponItem.GetStatValue(BleedChanceHash) : 0f;

    #endregion
}
```

### 40.2 连击系统实现

```csharp
public class MeleeWeaponAttack : MonoBehaviour
{
    [Header("连击系统")]
    private bool isAttacking = false;
    private int comboIndex = 0;              // 0=正手, 1=反手
    private float lastAttackTime;
    private float comboResetTime = 1.5f;     // 连击重置时间

    private CharacterMainControl character;
    private Animator playerAnimator;

    void Update()
    {
        // 自动重置连击
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboIndex = 0;
        }

        // 检测攻击输入
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            PerformNormalAttack();
        }
    }

    /// <summary>
    /// 执行普通攻击
    /// </summary>
    private void PerformNormalAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // 播放连击动画
        if (playerAnimator != null)
        {
            if (comboIndex == 0)
            {
                playerAnimator.SetTrigger("ForwardSlash");  // 正手挥击
                Debug.Log("[武器] 执行正手挥击");
            }
            else
            {
                playerAnimator.SetTrigger("BackhandSlash"); // 反手挥击
                Debug.Log("[武器] 执行反手挥击");
            }
        }

        // 启动攻击协程
        StartCoroutine(NormalAttackCoroutine());

        // 切换连击索引
        comboIndex = (comboIndex + 1) % 2;
    }

    /// <summary>
    /// 普通攻击协程
    /// </summary>
    private IEnumerator NormalAttackCoroutine()
    {
        // 等待攻击动画到达判定点（根据攻击速度调整）
        float attackDelay = 0.3f / AttackSpeed;
        yield return new WaitForSeconds(attackDelay);

        // 执行伤害判定
        PerformMeleeDamage();

        // 等待攻击动画结束
        yield return new WaitForSeconds(attackDelay);

        isAttacking = false;
    }
}
```

### 40.3 完整的伤害判定

```csharp
/// <summary>
/// 近战伤害判定（使用游戏标准 DamageInfo）
/// </summary>
public void PerformMeleeDamage()
{
    if (character == null) return;

    // 消耗体力
    if (StaminaCost > 0)
    {
        if (character.CurrentStamina < StaminaCost)
        {
            Debug.Log("[武器] 体力不足");
            return;
        }
        character.UseStamina(StaminaCost);
    }

    // 计算攻击原点和方向
    Vector3 attackOrigin = character.transform.position +
                           character.transform.forward;
    Vector3 attackDirection = character.transform.forward;

    // 扇形范围检测（使用标准 AttackRange）
    Collider[] hits = Physics.OverlapSphere(attackOrigin, AttackRange);

    int hitCount = 0;

    foreach (Collider hit in hits)
    {
        // 获取 DamageReceiver 组件
        DamageReceiver receiver = hit.GetComponent<DamageReceiver>();
        if (receiver == null) continue;

        // 检查是否是敌人
        if (!Team.IsEnemy(receiver.Team, character.Team))
        {
            continue;
        }

        // 检查是否在扇形范围内（90度）
        Vector3 directionToTarget =
            (hit.transform.position - character.transform.position).normalized;
        directionToTarget.y = 0;
        float angle = Vector3.Angle(attackDirection, directionToTarget);

        if (angle <= 90f)
        {
            // 使用游戏标准 DamageInfo 造成伤害
            DamageInfo damageInfo = new DamageInfo(character);
            damageInfo.damageValue = Damage;
            damageInfo.critRate = CritRate;
            damageInfo.critDamageFactor = CritDamageFactor;
            damageInfo.armorPiercing = ArmorPiercing;
            damageInfo.bleedChance = BleedChance;
            damageInfo.crit = -1; // 让游戏自动计算是否暴击
            damageInfo.damageNormal = -attackDirection;
            damageInfo.damagePoint = hit.transform.position;
            damageInfo.fromWeaponItemID = weaponItem?.TypeID ?? 0;

            receiver.Hurt(damageInfo);
            hitCount++;

            Debug.Log($"[武器] 命中 {hit.name}，伤害: {Damage}");
        }
    }

    if (hitCount > 0)
    {
        Debug.Log($"[武器] 攻击命中 {hitCount} 个敌人");
    }
}
```

### 40.4 特殊攻击：冲刺 + 剑气

```csharp
[Header("特殊攻击配置")]
public GameObject swordAuraPrefab;    // 剑气Prefab
public float specialDamage = 90f;     // 特殊攻击伤害
public float specialRange = 10f;      // 剑气飞行距离
public float specialCooldown = 8f;    // 特殊攻击冷却时间

private float lastSpecialTime;

/// <summary>
/// 尝试执行特殊攻击
/// </summary>
private void TrySpecialAttack()
{
    // 检查冷却时间
    float timeSinceLastSpecial = Time.time - lastSpecialTime;
    if (timeSinceLastSpecial < specialCooldown)
    {
        float remainingCooldown = specialCooldown - timeSinceLastSpecial;
        Debug.Log($"[武器] 特殊攻击冷却中，剩余: {remainingCooldown:F1}秒");
        return;
    }

    // 执行特殊攻击
    PerformSpecialAttack();
}

/// <summary>
/// 执行特殊攻击（冲刺 + 剑气）
/// </summary>
private void PerformSpecialAttack()
{
    isAttacking = true;
    lastSpecialTime = Time.time;
    lastAttackTime = Time.time;

    Debug.Log("[武器] 执行特殊攻击！");

    // 播放特殊攻击动画
    if (playerAnimator != null)
    {
        playerAnimator.SetTrigger("SpecialAttack");
    }

    // 启动特殊攻击协程
    StartCoroutine(SpecialAttackCoroutine());
}

/// <summary>
/// 特殊攻击协程：蓄力 -> 冲刺 -> 释放剑气
/// </summary>
private IEnumerator SpecialAttackCoroutine()
{
    // 阶段1: 蓄力 (0.3秒)
    Debug.Log("[武器] 阶段1: 蓄力");
    yield return new WaitForSeconds(0.3f);

    // 阶段2: 冲刺 (0.3秒)
    Debug.Log("[武器] 阶段2: 冲刺");
    yield return StartCoroutine(DashMovement(3f));

    // 阶段3: 释放剑气
    Debug.Log("[武器] 阶段3: 释放剑气");
    LaunchSwordAura();

    // 等待动画结束
    yield return new WaitForSeconds(0.6f);

    isAttacking = false;
    Debug.Log("[武器] 特殊攻击完成");
}

/// <summary>
/// 冲刺移动
/// </summary>
public IEnumerator DashMovement(float distance)
{
    if (character == null) yield break;

    Vector3 dashDirection = character.transform.forward;
    float dashDuration = 0.3f;
    Vector3 startPos = character.transform.position;
    Vector3 endPos = startPos + dashDirection * distance;

    // 检查终点是否有障碍物
    RaycastHit hit;
    if (Physics.Raycast(startPos, dashDirection, out hit, distance))
    {
        endPos = hit.point - dashDirection * 0.5f;
    }

    float elapsed = 0f;
    while (elapsed < dashDuration)
    {
        character.transform.position = Vector3.Lerp(startPos, endPos,
            elapsed / dashDuration);
        elapsed += Time.deltaTime;
        yield return null;
    }

    character.transform.position = endPos;
}

/// <summary>
/// 发射剑气
/// </summary>
public void LaunchSwordAura()
{
    if (swordAuraPrefab == null) return;

    // 在角色前方生成剑气
    Vector3 spawnPosition = character.transform.position + Vector3.up +
                            character.transform.forward * 1.5f;
    Quaternion spawnRotation = Quaternion.LookRotation(character.transform.forward);

    GameObject aura = Instantiate(swordAuraPrefab, spawnPosition, spawnRotation);

    // 配置剑气投射物
    var projectile = aura.GetComponent<SwordAuraProjectile>();
    if (projectile != null)
    {
        projectile.damage = specialDamage;
        projectile.speed = 15f;
        projectile.maxDistance = specialRange;
        projectile.owner = character.gameObject;
        projectile.Launch(character.transform.forward);

        Debug.Log("[武器] 剑气已发射!");
    }
}
```

---

## 四十一、高级投掷物系统

### 41.1 穿透型投掷物

支持穿透多个敌人的投掷物实现：

```csharp
/// <summary>
/// 剑气投掷物（支持穿透和子弹偏转）
/// </summary>
public class SwordAuraProjectile : MonoBehaviour
{
    [Header("投射物参数")]
    public float damage = 90f;              // 基础伤害
    public float speed = 15f;               // 飞行速度(米/秒)
    public float maxDistance = 10f;         // 最大飞行距离
    public int pierceCount = 3;             // 最大穿透数量

    [Header("运行状态")]
    public GameObject owner;                // 发射者
    private Vector3 direction;
    private float traveledDistance = 0f;
    private int currentPierceCount = 0;
    private bool isDestroying = false;

    [Header("碰撞检测")]
    private List<GameObject> hitEnemies = new List<GameObject>();
    private float detectRadius = 1.5f;

    /// <summary>
    /// 发射投掷物
    /// </summary>
    public void Launch(Vector3 launchDirection)
    {
        direction = launchDirection.normalized;
    }

    void Update()
    {
        if (direction == Vector3.zero || isDestroying) return;

        // 移动
        float moveDistance = speed * Time.deltaTime;
        transform.position += direction * moveDistance;
        traveledDistance += moveDistance;

        // 检查是否超出最大距离
        if (traveledDistance >= maxDistance)
        {
            DestroyProjectile();
        }
    }

    void FixedUpdate()
    {
        if (isDestroying) return;
        CheckCollisions();
    }

    /// <summary>
    /// 碰撞检测
    /// </summary>
    private void CheckCollisions()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject || hit.gameObject == owner) continue;

            // 检测敌人
            if (hit.CompareTag("Enemy") && !hitEnemies.Contains(hit.gameObject))
            {
                HitEnemy(hit.gameObject);
            }
            // 检测子弹（偏转）
            else if (hit.CompareTag("Projectile") || hit.CompareTag("Bullet"))
            {
                DeflectBullet(hit.gameObject);
            }
            // 检测障碍物（停止）
            else if (hit.CompareTag("Obstacle") || hit.CompareTag("Wall"))
            {
                DestroyProjectile();
            }
        }
    }

    /// <summary>
    /// 击中敌人（支持穿透）
    /// </summary>
    private void HitEnemy(GameObject enemy)
    {
        // 计算实际伤害（穿透衰减）
        float actualDamage = CalculateDamage();

        // 应用伤害
        ApplyDamageToEnemy(enemy, actualDamage);

        // 记录已击中的敌人
        hitEnemies.Add(enemy);
        currentPierceCount++;

        // 检查是否达到穿透上限
        if (currentPierceCount >= pierceCount)
        {
            DestroyProjectile();
        }
    }

    /// <summary>
    /// 计算伤害值（穿透衰减）
    /// </summary>
    private float CalculateDamage()
    {
        // 穿透伤害衰减（每次穿透减少10%，最多衰减30%）
        float pierceDamageMultiplier = 1f - (currentPierceCount * 0.1f);
        pierceDamageMultiplier = Mathf.Max(pierceDamageMultiplier, 0.7f);

        return damage * pierceDamageMultiplier;
    }
}
```

### 41.2 子弹偏转机制

```csharp
/// <summary>
/// 偏转子弹
/// </summary>
private void DeflectBullet(GameObject bullet)
{
    Debug.Log($"[投掷物] 偏转子弹: {bullet.name}");

    Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

    if (bulletRb != null)
    {
        // 获取当前速度
        Vector3 currentVelocity = bulletRb.velocity;
        float bulletSpeed = currentVelocity.magnitude;

        // 计算反向（子弹飞回去）
        Vector3 deflectDirection = -currentVelocity.normalized;

        // 应用新速度（增加50%）
        bulletRb.velocity = deflectDirection * bulletSpeed * 1.5f;

        // 尝试改变子弹的所有者
        ChangeProjectileOwner(bullet);

        Debug.Log("[投掷物] 子弹已反弹");
    }
    else
    {
        // 没有Rigidbody，直接销毁
        Destroy(bullet);
    }

    // 播放偏转特效
    SpawnDeflectEffect(bullet.transform.position);
}

/// <summary>
/// 改变投射物的所有者（使反弹子弹伤害敌人）
/// </summary>
private void ChangeProjectileOwner(GameObject projectile)
{
    var projectileScript = projectile.GetComponent("Projectile");
    if (projectileScript != null)
    {
        var ownerField = projectileScript.GetType().GetField("owner");
        if (ownerField != null)
        {
            ownerField.SetValue(projectileScript, owner);
            Debug.Log("[投掷物] 子弹所有权已改变");
        }
    }
}

/// <summary>
/// 生成偏转特效
/// </summary>
private void SpawnDeflectEffect(Vector3 position)
{
    GameObject spark = new GameObject("DeflectSpark");
    spark.transform.position = position;

    // 添加粒子系统（火花效果）
    ParticleSystem sparkParticles = spark.AddComponent<ParticleSystem>();
    var main = sparkParticles.main;
    main.startColor = Color.white;
    main.startSize = 0.2f;
    main.startLifetime = 0.3f;
    main.startSpeed = 5f;
    main.maxParticles = 30;

    var emission = sparkParticles.emission;
    emission.SetBursts(new ParticleSystem.Burst[] {
        new ParticleSystem.Burst(0f, 30)
    });

    var shape = sparkParticles.shape;
    shape.shapeType = ParticleSystemShapeType.Sphere;
    shape.radius = 0.3f;

    // 添加光闪
    Light flashLight = spark.AddComponent<Light>();
    flashLight.color = Color.white;
    flashLight.intensity = 3f;
    flashLight.range = 3f;

    Destroy(spark, 1f);
}
```

### 41.3 轨迹渲染器

为投掷物添加拖尾效果：

```csharp
/// <summary>
/// 初始化轨迹渲染器
/// </summary>
private void InitializeTrail()
{
    TrailRenderer trail = GetComponent<TrailRenderer>();
    if (trail == null)
    {
        trail = gameObject.AddComponent<TrailRenderer>();
    }

    // 配置轨迹
    trail.time = 0.5f;              // 轨迹持续时间
    trail.startWidth = 0.5f;        // 起始宽度
    trail.endWidth = 0.1f;          // 结束宽度
    trail.startColor = new Color(0.44f, 0.69f, 0.88f, 1f);  // 起始颜色
    trail.endColor = new Color(0.44f, 0.69f, 0.88f, 0f);    // 结束颜色（透明）
    trail.material = new Material(Shader.Find("Sprites/Default"));
}

/// <summary>
/// 更新轨迹效果（根据飞行距离渐弱）
/// </summary>
private void UpdateTrailEffect()
{
    TrailRenderer trail = GetComponent<TrailRenderer>();
    if (trail != null)
    {
        float distanceRatio = 1f - (traveledDistance / maxDistance);
        trail.startWidth = 0.5f * distanceRatio;
    }
}
```

### 41.4 投掷物销毁与渐隐

```csharp
/// <summary>
/// 销毁投掷物
/// </summary>
private void DestroyProjectile()
{
    if (isDestroying) return;
    isDestroying = true;

    Debug.Log($"[投掷物] 销毁 - 击中敌人: {hitEnemies.Count}");

    // 停止粒子发射（但让现有粒子播完）
    ParticleSystem particles = GetComponentInChildren<ParticleSystem>();
    if (particles != null)
    {
        var emission = particles.emission;
        emission.enabled = false;
    }

    // 渐隐主体
    StartCoroutine(FadeOut());

    // 延迟销毁
    Destroy(gameObject, 1f);
}

/// <summary>
/// 渐隐效果协程
/// </summary>
private IEnumerator FadeOut()
{
    Renderer renderer = GetComponent<Renderer>();
    if (renderer == null) yield break;

    Material mat = renderer.material;
    Color startColor = mat.color;
    float duration = 0.5f;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
        mat.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        elapsed += Time.deltaTime;
        yield return null;
    }
}
```

---

## 四十二、完整ModBehaviour生命周期参考

### 42.1 推荐的完整ModBehaviour模板

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Duckov.Modding;
using ItemStatsSystem;
using SodaCraft.Localizations;
using System.IO;

namespace MyCompleteMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // 物品TypeID
        private const int MY_ITEM_TYPE_ID = 990001;

        // 资源引用
        private Item myItemPrefab;
        private AssetBundle assetBundle;

        // 协程引用（用于停止）
        private Coroutine lootBoxCoroutine;

        // 事件注册标记
        private bool eventsRegistered = false;

        /// <summary>
        /// Mod启动入口
        /// </summary>
        void Start()
        {
            Debug.Log("[MyMod] 开始加载...");

            try
            {
                // 1. 设置本地化
                SetupLocalization();

                // 2. 加载资源
                LoadAssets();

                // 3. 配置物品
                ConfigureItem();

                // 4. 注册物品到游戏
                RegisterItem();

                // 5. 注册到商店
                RegisterToShop();

                // 6. 启动箱子注入协程
                lootBoxCoroutine = StartCoroutine(LootBoxInjectionRoutine());

                // 7. 注册游戏事件
                RegisterEvents();

                Debug.Log("[MyMod] 加载完成!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MyMod] 加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        #region 初始化方法

        private void SetupLocalization()
        {
            LocalizationManager.SetOverrideText("MyItem_Name", "我的物品");
            LocalizationManager.SetOverrideText("MyItem_Desc", "物品描述");
            LocalizationManager.OnSetLanguage += OnLanguageChanged;
        }

        private void OnLanguageChanged(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.English:
                    LocalizationManager.SetOverrideText("MyItem_Name", "My Item");
                    LocalizationManager.SetOverrideText("MyItem_Desc", "Item description");
                    break;
                default:
                    LocalizationManager.SetOverrideText("MyItem_Name", "我的物品");
                    LocalizationManager.SetOverrideText("MyItem_Desc", "物品描述");
                    break;
            }
        }

        private void LoadAssets()
        {
            string modFolder = Path.GetDirectoryName(info.dllPath);
            string bundlePath = Path.Combine(modFolder, "Assets", "mymod_assets");

            if (File.Exists(bundlePath))
            {
                assetBundle = AssetBundle.LoadFromFile(bundlePath);
                if (assetBundle != null)
                {
                    myItemPrefab = assetBundle.LoadAsset<Item>("MyItemPrefab");
                }
            }

            if (myItemPrefab == null)
            {
                Debug.LogWarning("[MyMod] 使用程序化生成物品");
                CreateProceduralItem();
            }
        }

        private void CreateProceduralItem()
        {
            // 程序化生成物品（备用方案）
        }

        private void ConfigureItem()
        {
            if (myItemPrefab == null) return;
            // 配置物品组件
        }

        private void RegisterItem()
        {
            if (myItemPrefab == null) return;
            bool success = ItemAssetsCollection.AddDynamicEntry(myItemPrefab);
            Debug.Log(success ? "[MyMod] 物品注册成功" : "[MyMod] 物品注册失败");
        }

        private void RegisterToShop()
        {
            // 注册到商店（参考第37章）
        }

        private IEnumerator LootBoxInjectionRoutine()
        {
            // 箱子注入逻辑（参考第38章）
            yield return new WaitForSeconds(2f);

            HashSet<int> processedBoxes = new HashSet<int>();
            while (true)
            {
                // 注入逻辑...
                yield return new WaitForSeconds(5f);
            }
        }

        private void RegisterEvents()
        {
            if (eventsRegistered) return;
            eventsRegistered = true;

            LevelManager.OnLevelInitialized += OnLevelLoaded;
            Item.onUseStatic += OnAnyItemUsed;
        }

        #endregion

        #region 事件回调

        private void OnLevelLoaded()
        {
            Debug.Log("[MyMod] 关卡加载完成");
        }

        private void OnAnyItemUsed(Item item, object user)
        {
            if (item.TypeID == MY_ITEM_TYPE_ID)
            {
                Debug.Log("[MyMod] 物品被使用");
            }
        }

        #endregion

        #region 清理

        /// <summary>
        /// Mod卸载时清理（必须实现）
        /// </summary>
        void OnDestroy()
        {
            Debug.Log("[MyMod] 开始卸载...");

            // 1. 停止协程
            if (lootBoxCoroutine != null)
            {
                StopCoroutine(lootBoxCoroutine);
            }

            // 2. 取消事件监听（⚠️重要）
            LocalizationManager.OnSetLanguage -= OnLanguageChanged;

            if (eventsRegistered)
            {
                LevelManager.OnLevelInitialized -= OnLevelLoaded;
                Item.onUseStatic -= OnAnyItemUsed;
            }

            // 3. 移除动态物品
            if (myItemPrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(myItemPrefab);
            }

            // 4. 卸载AssetBundle
            if (assetBundle != null)
            {
                assetBundle.Unload(true);
            }

            Debug.Log("[MyMod] 卸载完成");
        }

        #endregion
    }
}
```

---

## 四十三、开发检查清单汇总

### 物品开发检查清单

```
□ 基础配置
  □ TypeID 唯一（推荐 990000+）
  □ DisplayName 设置（本地化键名）
  □ Icon Sprite 配置
  □ 品质(Quality)和价值(Value)设置

□ 使用系统
  □ UsageBehavior 组件添加
  □ CanBeUsed() 逻辑正确
  □ OnUse() 逻辑正确

□ 显示系统
  □ ItemAgentUtilities 配置（武器必需）
  □ ItemGraphicInfo 配置（UI显示）

□ 商店/箱子系统
  □ 注册到 StockShopDatabase
  □ 箱子注入协程启动

□ 本地化
  □ 中文文本设置
  □ OnSetLanguage 事件监听
  □ 多语言支持（可选）

□ 清理
  □ OnDestroy 中取消所有事件监听
  □ 移除动态物品注册
  □ 卸载 AssetBundle
```

### 近战武器检查清单

```
□ Stats 配置
  □ Damage（伤害）
  □ CritRate（暴击率）
  □ CritDamageFactor（暴击伤害）
  □ ArmorPiercing（护甲穿透）
  □ AttackSpeed（攻击速度）
  □ AttackRange（攻击范围）
  □ StaminaCost（体力消耗）

□ 攻击系统
  □ Stat Hash 缓存
  □ 连击系统
  □ DamageInfo 正确配置
  □ Team.IsEnemy 敌我判断

□ 特殊攻击（可选）
  □ 冷却时间检查
  □ 冲刺逻辑
  □ 投掷物发射
```

### 投掷物检查清单

```
□ 基础属性
  □ damage（伤害）
  □ speed（速度）
  □ maxDistance（最大距离）

□ 穿透系统（可选）
  □ pierceCount（穿透数量）
  □ 穿透伤害衰减计算
  □ 已击中敌人记录

□ 子弹偏转（可选）
  □ Rigidbody 速度修改
  □ 所有者变更

□ 视觉效果
  □ TrailRenderer 配置
  □ 粒子系统
  □ 销毁渐隐效果
```

---

## 四十四、格挡/偏转系统

### 44.1 DamageReceiver 伤害接收器

游戏使用 `DamageReceiver` 组件接收伤害。通过监听其事件，可以在伤害生效前拦截：

```csharp
// DamageReceiver 关键事件
public class DamageReceiver : MonoBehaviour
{
    // 受伤事件（伤害生效前触发）
    public UnityEvent<DamageInfo> OnHurtEvent;

    // 获取 Health 组件
    public Health Health { get; }

    // 获取所属阵营
    public Teams Team { get; }
}
```

### 44.2 格挡系统完整实现

```csharp
using UnityEngine;
using ItemStatsSystem;
using System.Reflection;

/// <summary>
/// 格挡系统
/// 通过 DamageReceiver.OnHurtEvent 拦截伤害
/// </summary>
public class WeaponBlockSystem : MonoBehaviour
{
    [Header("格挡配置")]
    public float blockAngle = 120f;           // 格挡角度（正面120度）
    public float blockCooldown = 0.5f;        // 格挡冷却时间
    public float perfectBlockWindow = 0.2f;   // 完美格挡窗口
    public bool canDeflectBullets = true;     // 是否偏转子弹
    public float deflectDamageMultiplier = 0.5f; // 偏转子弹伤害倍率

    [Header("特效")]
    public GameObject blockEffectPrefab;
    public GameObject perfectBlockEffectPrefab;
    public GameObject deflectedBulletPrefab;

    // 运行时状态
    private bool isBlocking = false;
    private float blockStartTime;
    private float lastBlockTime;
    private bool pendingBlock = false;
    private DamageInfo pendingDamageInfo;

    // 组件引用
    private CharacterMainControl character;
    private Health health;
    private DamageReceiver damageReceiver;

    void Start()
    {
        character = GetComponentInParent<CharacterMainControl>();
        if (character != null)
        {
            health = character.Health;
            damageReceiver = character.GetComponent<DamageReceiver>();

            // 注册伤害事件
            if (damageReceiver != null)
            {
                damageReceiver.OnHurtEvent.AddListener(OnDamageReceiverHurt);
                Debug.Log("[格挡系统] 已注册伤害拦截");
            }
        }
    }

    void OnDestroy()
    {
        // 取消注册
        if (damageReceiver != null)
        {
            damageReceiver.OnHurtEvent.RemoveListener(OnDamageReceiverHurt);
        }
    }

    /// <summary>
    /// 开始格挡（由输入系统调用）
    /// </summary>
    public void StartBlock()
    {
        if (Time.time - lastBlockTime < blockCooldown) return;

        isBlocking = true;
        blockStartTime = Time.time;
        Debug.Log("[格挡系统] 开始格挡");
    }

    /// <summary>
    /// 结束格挡
    /// </summary>
    public void EndBlock()
    {
        isBlocking = false;
        Debug.Log("[格挡系统] 结束格挡");
    }

    /// <summary>
    /// 伤害接收回调（在 Health.Hurt 之前触发）
    /// </summary>
    private void OnDamageReceiverHurt(DamageInfo damageInfo)
    {
        if (!isBlocking) return;

        // 尝试格挡
        if (TryBlock(damageInfo))
        {
            // 设置临时无敌以阻止伤害
            SetInvincible(true);
            pendingBlock = true;
            pendingDamageInfo = damageInfo;

            Debug.Log($"[格挡系统] 成功格挡 {damageInfo.damageValue} 点伤害");
        }
    }

    void LateUpdate()
    {
        // 在 LateUpdate 中处理格挡后续
        if (pendingBlock)
        {
            pendingBlock = false;

            // 恢复非无敌状态
            SetInvincible(false);

            // 执行格挡效果
            ExecuteBlockEffect(pendingDamageInfo);

            lastBlockTime = Time.time;
        }
    }

    /// <summary>
    /// 判断是否可以格挡
    /// </summary>
    private bool TryBlock(DamageInfo damageInfo)
    {
        if (character == null) return false;

        // 检查攻击方向是否在格挡角度内
        Vector3 attackDirection = damageInfo.damageNormal;
        if (attackDirection == Vector3.zero && damageInfo.fromCharacter != null)
        {
            attackDirection = (character.transform.position -
                              damageInfo.fromCharacter.transform.position).normalized;
        }

        Vector3 facingDirection = character.transform.forward;
        float angle = Vector3.Angle(-attackDirection, facingDirection);

        if (angle > blockAngle / 2f)
        {
            Debug.Log($"[格挡系统] 攻击角度 {angle:F1}° 超出格挡范围");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 执行格挡效果
    /// </summary>
    private void ExecuteBlockEffect(DamageInfo damageInfo)
    {
        // 判断是否是完美格挡
        float blockDuration = Time.time - blockStartTime;
        bool isPerfectBlock = blockDuration <= perfectBlockWindow;

        if (isPerfectBlock)
        {
            Debug.Log("[格挡系统] 完美格挡！");

            // 完美格挡特效
            if (perfectBlockEffectPrefab != null)
            {
                Instantiate(perfectBlockEffectPrefab,
                    character.transform.position + Vector3.up, Quaternion.identity);
            }

            // 完美格挡可以偏转子弹
            if (canDeflectBullets && damageInfo.fromCharacter != null)
            {
                CreateDeflectedBullet(damageInfo);
            }

            // 显示提示
            character.PopText("完美格挡！");
        }
        else
        {
            // 普通格挡特效
            if (blockEffectPrefab != null)
            {
                Instantiate(blockEffectPrefab,
                    character.transform.position + Vector3.up, Quaternion.identity);
            }

            character.PopText("格挡！");
        }
    }

    /// <summary>
    /// 通过反射设置无敌状态
    /// </summary>
    private void SetInvincible(bool value)
    {
        if (health == null) return;

        var field = typeof(Health).GetField("invincible",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(health, value);
        }
    }

    /// <summary>
    /// 创建偏转的子弹
    /// </summary>
    private void CreateDeflectedBullet(DamageInfo damageInfo)
    {
        if (deflectedBulletPrefab == null || damageInfo.fromCharacter == null) return;

        // 计算反射方向
        Vector3 toAttacker = (damageInfo.fromCharacter.transform.position -
                              character.transform.position).normalized;

        // 在角色前方生成偏转子弹
        Vector3 spawnPos = character.transform.position + Vector3.up +
                           character.transform.forward * 0.5f;

        GameObject bullet = Instantiate(deflectedBulletPrefab, spawnPos,
            Quaternion.LookRotation(toAttacker));

        // 配置子弹
        var projectile = bullet.GetComponent<DeflectedBulletProjectile>();
        if (projectile != null)
        {
            projectile.damage = damageInfo.damageValue * deflectDamageMultiplier;
            projectile.owner = character.gameObject;
            projectile.targetCharacter = damageInfo.fromCharacter;
            projectile.Launch(toAttacker);
        }

        Debug.Log($"[格挡系统] 偏转子弹，目标：{damageInfo.fromCharacter.name}");
    }
}
```

### 44.3 偏转子弹投掷物

```csharp
/// <summary>
/// 偏转的子弹投掷物
/// </summary>
public class DeflectedBulletProjectile : MonoBehaviour
{
    public float damage = 30f;
    public float speed = 25f;
    public float maxDistance = 20f;
    public float homingStrength = 2f;      // 追踪强度

    public GameObject owner;
    public CharacterMainControl targetCharacter;

    private Vector3 direction;
    private float traveledDistance = 0f;
    private TrailRenderer trail;

    void Start()
    {
        // 添加轨迹渲染器
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.3f;
        trail.startWidth = 0.2f;
        trail.endWidth = 0.05f;
        trail.startColor = new Color(1f, 0.8f, 0.2f, 1f);
        trail.endColor = new Color(1f, 0.5f, 0.1f, 0f);
        trail.material = new Material(Shader.Find("Sprites/Default"));
    }

    public void Launch(Vector3 launchDirection)
    {
        direction = launchDirection.normalized;
    }

    void Update()
    {
        if (direction == Vector3.zero) return;

        // 追踪目标
        if (targetCharacter != null && homingStrength > 0)
        {
            Vector3 toTarget = (targetCharacter.transform.position + Vector3.up -
                               transform.position).normalized;
            direction = Vector3.Lerp(direction, toTarget,
                homingStrength * Time.deltaTime).normalized;
        }

        // 移动
        float moveDistance = speed * Time.deltaTime;
        transform.position += direction * moveDistance;
        transform.rotation = Quaternion.LookRotation(direction);
        traveledDistance += moveDistance;

        // 检查最大距离
        if (traveledDistance >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner) return;

        // 检测敌人
        DamageReceiver receiver = other.GetComponent<DamageReceiver>();
        if (receiver != null)
        {
            // 创建伤害信息
            CharacterMainControl ownerChar = owner?.GetComponent<CharacterMainControl>();
            DamageInfo damageInfo = new DamageInfo(ownerChar);
            damageInfo.damageValue = damage;
            damageInfo.damageType = DamageTypes.physics;
            damageInfo.damagePoint = transform.position;

            receiver.Hurt(damageInfo);

            Debug.Log($"[偏转子弹] 命中 {other.name}，伤害：{damage}");
        }

        // 销毁
        Destroy(gameObject);
    }
}
```

---

## 四十五、动画事件处理系统

### 45.1 动画事件基础

Unity Animator 可以在动画的特定帧触发事件。游戏中武器攻击通常使用动画事件来触发伤害判定。

### 45.2 动画事件处理器

```csharp
using UnityEngine;

/// <summary>
/// 动画事件处理器
/// 挂载到角色模型上，接收 Animator 的动画事件
/// </summary>
public class WeaponAnimationHandler : MonoBehaviour
{
    [Header("引用")]
    public MeleeWeaponAttack attackScript;  // 攻击脚本引用

    [Header("特效")]
    public GameObject slashEffectPrefab;    // 挥砍特效
    public Transform slashSpawnPoint;       // 特效生成点

    [Header("音效")]
    public string swingSound = "SwordSwing";
    public string hitSound = "SwordHit";

    [Header("相机震动")]
    public float attackShakeStrength = 0.3f;
    public float chargeShakeStrength = 0.5f;

    private CharacterMainControl character;
    private ParticleSystem chargeParticles;

    void Start()
    {
        character = GetComponentInParent<CharacterMainControl>();
    }

    #region 动画事件回调（由 Animator 调用）

    /// <summary>
    /// 攻击伤害帧（动画到达伤害判定点时调用）
    /// </summary>
    public void OnAttackDamageFrame(string attackType)
    {
        Debug.Log($"[动画事件] 攻击伤害帧: {attackType}");

        if (attackScript != null)
        {
            attackScript.PerformMeleeDamage();
        }
    }

    /// <summary>
    /// 播放挥砍音效
    /// </summary>
    public void PlaySlashSound()
    {
        if (!string.IsNullOrEmpty(swingSound))
        {
            AudioManager.Post(swingSound, gameObject);
        }
    }

    /// <summary>
    /// 显示挥砍特效
    /// </summary>
    public void ShowSlashEffect()
    {
        if (slashEffectPrefab == null) return;

        Transform spawnPoint = slashSpawnPoint != null ?
            slashSpawnPoint : transform;

        GameObject effect = Instantiate(slashEffectPrefab,
            spawnPoint.position, spawnPoint.rotation);

        Destroy(effect, 2f);
    }

    /// <summary>
    /// 开始蓄力特效
    /// </summary>
    public void StartChargeEffect()
    {
        if (chargeParticles != null)
        {
            chargeParticles.Play();
        }

        // 相机轻微震动
        if (character != null && character.IsMainCharacter)
        {
            CameraShaker.Shake(chargeShakeStrength, 0.3f);
        }
    }

    /// <summary>
    /// 停止蓄力特效
    /// </summary>
    public void StopChargeEffect()
    {
        if (chargeParticles != null)
        {
            chargeParticles.Stop();
        }
    }

    /// <summary>
    /// 开始冲刺移动
    /// </summary>
    public void StartDashMovement(float distance)
    {
        StopChargeEffect();

        if (attackScript != null)
        {
            StartCoroutine(attackScript.DashMovement(distance));
        }
    }

    /// <summary>
    /// 攻击命中反馈（命中敌人时调用）
    /// </summary>
    public void OnAttackHit()
    {
        // 播放命中音效
        if (!string.IsNullOrEmpty(hitSound))
        {
            AudioManager.Post(hitSound, gameObject);
        }

        // 相机震动
        if (character != null && character.IsMainCharacter)
        {
            CameraShaker.Shake(attackShakeStrength, 0.1f);
        }
    }

    /// <summary>
    /// 攻击动画结束
    /// </summary>
    public void OnAttackEnd()
    {
        if (attackScript != null)
        {
            attackScript.OnAttackAnimationEnd();
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 初始化蓄力粒子系统
    /// </summary>
    public void InitializeChargeParticles(ParticleSystem particles)
    {
        chargeParticles = particles;
    }

    #endregion
}
```

### 45.3 在 Unity 中配置动画事件

```
在 Unity 编辑器中配置动画事件：

1. 选择动画剪辑（Animation Clip）
2. 打开 Animation 窗口
3. 定位到目标帧
4. 点击 "Add Event" 按钮
5. 选择要调用的方法：
   - OnAttackDamageFrame (string attackType)
   - PlaySlashSound ()
   - ShowSlashEffect ()
   - StartDashMovement (float distance)
   - OnAttackEnd ()

常见动画事件配置：
┌─────────────────────────────────────────────────────────┐
│ 普通攻击动画                                              │
├──────────┬──────────────────────────────────────────────┤
│ 帧 0     │ PlaySlashSound()                             │
│ 帧 5     │ ShowSlashEffect()                            │
│ 帧 8     │ OnAttackDamageFrame("normal")                │
│ 帧 15    │ OnAttackEnd()                                │
└──────────┴──────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ 特殊攻击动画                                              │
├──────────┬──────────────────────────────────────────────┤
│ 帧 0     │ StartChargeEffect()                          │
│ 帧 10    │ StartDashMovement(3.0)                       │
│ 帧 15    │ OnAttackDamageFrame("special")               │
│ 帧 25    │ OnAttackEnd()                                │
└──────────┴──────────────────────────────────────────────┘
```

---

## 四十六、场景条件使用系统

### 46.1 LevelManager 场景状态

游戏使用 `LevelManager` 管理关卡状态，可用于限制物品使用场景：

```csharp
// LevelManager 重要属性
LevelManager.Instance.IsBaseLevel    // 是否在基地/家中
LevelManager.Instance.IsRaidMap      // 是否在突袭地图
LevelManager.Instance.IsTutorial     // 是否在教程关卡
LevelManager.Instance.CurrentScene   // 当前场景名称
```

### 46.2 场景限制的 UsageBehavior

```csharp
using UnityEngine;
using ItemStatsSystem;
using Duckov.Scenes;

/// <summary>
/// 只能在突袭地图使用的物品
/// 例如：虫洞撤离装置
/// </summary>
public class RaidMapOnlyUse : UsageBehavior
{
    [Header("配置")]
    public bool requireRaidMap = true;      // 需要在突袭地图
    public bool forbidInBase = true;        // 禁止在基地使用
    public string failMessageBase = "只能在战场使用！";
    public string failMessageNotRaid = "无法在此处使用！";

    public override DisplaySettingsData DisplaySettings
    {
        get
        {
            return new DisplaySettingsData
            {
                display = true,
                description = "仅限突袭地图使用"
            };
        }
    }

    public override bool CanBeUsed(Item item, object user)
    {
        // 基础检查
        CharacterMainControl character = user as CharacterMainControl;
        if (character == null) return false;

        // 检查 LevelManager
        if (LevelManager.Instance == null)
        {
            Debug.Log("[场景限制] LevelManager 为空");
            return false;
        }

        // 检查是否在基地
        if (forbidInBase && LevelManager.Instance.IsBaseLevel)
        {
            Debug.Log("[场景限制] 不能在基地使用");
            return false;
        }

        // 检查是否在突袭地图
        if (requireRaidMap && !LevelManager.Instance.IsRaidMap)
        {
            Debug.Log("[场景限制] 需要在突袭地图");
            return false;
        }

        return true;
    }

    protected override void OnUse(Item item, object user)
    {
        CharacterMainControl character = user as CharacterMainControl;
        if (character == null) return;

        // 再次验证（以防状态变化）
        if (forbidInBase && LevelManager.Instance.IsBaseLevel)
        {
            character.PopText(failMessageBase);
            return;
        }

        if (requireRaidMap && !LevelManager.Instance.IsRaidMap)
        {
            character.PopText(failMessageNotRaid);
            return;
        }

        // 执行实际逻辑
        ExecuteAction(character, item);
    }

    protected virtual void ExecuteAction(CharacterMainControl character, Item item)
    {
        // 子类实现具体逻辑
    }
}
```

### 46.3 只能在基地使用的物品

```csharp
/// <summary>
/// 只能在基地使用的物品
/// 例如：虫洞回溯装置
/// </summary>
public class BaseOnlyUse : UsageBehavior
{
    public override DisplaySettingsData DisplaySettings
    {
        get
        {
            return new DisplaySettingsData
            {
                display = true,
                description = "仅限基地使用"
            };
        }
    }

    public override bool CanBeUsed(Item item, object user)
    {
        CharacterMainControl character = user as CharacterMainControl;
        if (character == null) return false;

        if (LevelManager.Instance == null) return false;

        // 只能在基地使用
        if (!LevelManager.Instance.IsBaseLevel)
        {
            return false;
        }

        // 额外条件检查（可选）
        return CheckAdditionalConditions(character);
    }

    protected virtual bool CheckAdditionalConditions(CharacterMainControl character)
    {
        return true;
    }

    protected override void OnUse(Item item, object user)
    {
        CharacterMainControl character = user as CharacterMainControl;
        if (character == null) return;

        if (!LevelManager.Instance.IsBaseLevel)
        {
            character.PopText("只能在家中使用！");
            return;
        }

        ExecuteAction(character, item);
    }

    protected virtual void ExecuteAction(CharacterMainControl character, Item item)
    {
        // 子类实现
    }
}
```

---

## 四十七、撤离系统集成

### 47.1 EvacuationInfo 撤离信息

游戏使用 `EvacuationInfo` 结构体存储撤离相关数据：

```csharp
// 撤离信息结构
public struct EvacuationInfo
{
    public Vector3 position;         // 撤离位置
    public string evacuationID;      // 撤离点ID
    public bool success;             // 是否成功撤离
}
```

### 47.2 撤离功能物品实现

```csharp
using UnityEngine;
using ItemStatsSystem;
using Duckov.Scenes;

/// <summary>
/// 快速撤离物品
/// 消耗物品立即撤离当前地图
/// </summary>
public class EvacuationItemUse : UsageBehavior
{
    [Header("撤离配置")]
    public float evacuationDelay = 3f;       // 撤离延迟
    public bool consumeOnUse = true;         // 使用后消耗

    [Header("特效")]
    public GameObject evacuationEffectPrefab;

    private Item currentItem;
    private bool isEvacuating = false;

    public override DisplaySettingsData DisplaySettings
    {
        get
        {
            return new DisplaySettingsData
            {
                display = true,
                description = $"使用后 {evacuationDelay} 秒撤离"
            };
        }
    }

    public override bool CanBeUsed(Item item, object user)
    {
        CharacterMainControl character = user as CharacterMainControl;
        if (character == null) return false;

        // 已在撤离中
        if (isEvacuating) return false;

        // 检查场景
        if (LevelManager.Instance == null) return false;
        if (!LevelManager.Instance.IsRaidMap) return false;
        if (LevelManager.Instance.IsBaseLevel) return false;

        return true;
    }

    protected override void OnUse(Item item, object user)
    {
        CharacterMainControl character = user as CharacterMainControl;
        if (character == null) return;

        if (!LevelManager.Instance.IsRaidMap)
        {
            character.PopText("无法在此处撤离！");
            return;
        }

        currentItem = item;
        StartCoroutine(EvacuationSequence(character));
    }

    private System.Collections.IEnumerator EvacuationSequence(
        CharacterMainControl character)
    {
        isEvacuating = true;

        // 显示提示
        character.PopText($"撤离中... {evacuationDelay}秒");

        // 播放特效
        if (evacuationEffectPrefab != null)
        {
            GameObject effect = Instantiate(evacuationEffectPrefab,
                character.transform.position, Quaternion.identity);
            effect.transform.SetParent(character.transform);
        }

        // 等待延迟
        yield return new WaitForSeconds(evacuationDelay);

        // 执行撤离
        ExecuteEvacuation(character);

        isEvacuating = false;
    }

    /// <summary>
    /// 执行撤离
    /// </summary>
    private void ExecuteEvacuation(CharacterMainControl character)
    {
        try
        {
            // 创建撤离信息
            EvacuationInfo evacuationInfo = new EvacuationInfo
            {
                position = character.transform.position,
                evacuationID = "ModEvacuation",
                success = true
            };

            // 调用游戏撤离系统
            // 方式1：使用 LevelManager.NotifyEvacuated
            var notifyMethod = typeof(LevelManager).GetMethod("NotifyEvacuated",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);

            if (notifyMethod != null)
            {
                notifyMethod.Invoke(LevelManager.Instance,
                    new object[] { evacuationInfo });
                Debug.Log("[撤离物品] 已通知撤离系统");
            }
            else
            {
                // 方式2：直接加载基地场景
                LoadBaseScene();
            }

            // 消耗物品
            if (consumeOnUse && currentItem != null)
            {
                currentItem.Destroy();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[撤离物品] 撤离失败: {e.Message}");
            character.PopText("撤离失败！");
        }
    }

    /// <summary>
    /// 直接加载基地场景（备用方案）
    /// </summary>
    private void LoadBaseScene()
    {
        // 使用 SceneLoader 加载基地
        if (GameManager.SceneLoader != null)
        {
            GameManager.SceneLoader.LoadScene("BaseScene");
        }
        else
        {
            // 最后手段：使用 Unity 场景管理
            UnityEngine.SceneManagement.SceneManager.LoadScene("BaseScene");
        }
    }
}
```

### 47.3 撤离事件监听

```csharp
/// <summary>
/// 监听撤离事件
/// </summary>
public class EvacuationEventListener : MonoBehaviour
{
    void Start()
    {
        // 注册撤离事件
        LevelManager.OnEvacuated += OnPlayerEvacuated;
    }

    void OnDestroy()
    {
        LevelManager.OnEvacuated -= OnPlayerEvacuated;
    }

    private void OnPlayerEvacuated()
    {
        Debug.Log("[撤离监听] 玩家已撤离");
        // 在这里执行撤离后的逻辑
        // 例如：保存数据、清理临时状态等
    }
}
```

---

## 四十八、AgentUtilities 运行时修复

### 48.1 问题背景

动态创建的物品可能存在 `ItemAgentUtilities` 配置不完整的问题，导致手持物品不显示。

### 48.2 运行时修复方案

```csharp
using UnityEngine;
using ItemStatsSystem;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AgentUtilities 运行时修复器
/// 定期扫描并修复物品的手持显示问题
/// </summary>
public class AgentUtilitiesFixer : MonoBehaviour
{
    [Header("配置")]
    public float scanInterval = 5f;          // 扫描间隔（秒）
    public List<int> targetItemTypeIDs;      // 需要修复的物品TypeID列表

    private HashSet<int> fixedItems = new HashSet<int>();
    private Coroutine scanCoroutine;

    void Start()
    {
        scanCoroutine = StartCoroutine(ScanAndFixRoutine());
    }

    void OnDestroy()
    {
        if (scanCoroutine != null)
        {
            StopCoroutine(scanCoroutine);
        }
    }

    /// <summary>
    /// 定期扫描修复协程
    /// </summary>
    private IEnumerator ScanAndFixRoutine()
    {
        yield return new WaitForSeconds(2f); // 初始延迟

        while (true)
        {
            ScanAndFixInventoryItems();
            yield return new WaitForSeconds(scanInterval);
        }
    }

    /// <summary>
    /// 扫描并修复背包中的物品
    /// </summary>
    private void ScanAndFixInventoryItems()
    {
        CharacterMainControl mainChar = CharacterMainControl.Main;
        if (mainChar == null || mainChar.CharacterItem == null) return;

        Inventory inventory = mainChar.CharacterItem.Inventory;
        if (inventory == null) return;

        foreach (Item item in inventory)
        {
            if (item == null) continue;

            // 检查是否是目标物品
            if (!targetItemTypeIDs.Contains(item.TypeID)) continue;

            // 检查是否已修复
            int instanceId = item.GetInstanceID();
            if (fixedItems.Contains(instanceId)) continue;

            // 执行修复
            if (FixItemAgentUtilities(item))
            {
                fixedItems.Add(instanceId);
                Debug.Log($"[AgentFixer] 已修复物品: {item.DisplayName}");
            }
        }
    }

    /// <summary>
    /// 修复单个物品的 AgentUtilities
    /// </summary>
    private bool FixItemAgentUtilities(Item item)
    {
        try
        {
            ItemAgentUtilities agentUtils = item.AgentUtilities;
            if (agentUtils == null)
            {
                Debug.LogWarning($"[AgentFixer] 物品 {item.DisplayName} 没有 AgentUtilities");
                return false;
            }

            // 检查是否已经有有效的 handheld agent
            if (item.HasHandHeldAgent)
            {
                return true; // 已经有了，不需要修复
            }

            // 尝试从游戏设置获取默认的 handheld prefab
            GameObject handheldPrefab = GetDefaultHandheldPrefab();
            if (handheldPrefab == null)
            {
                Debug.LogWarning("[AgentFixer] 无法获取默认 handheld prefab");
                return false;
            }

            // 通过反射添加 agent entry
            AddAgentEntry(agentUtils, "handheld", handheldPrefab);

            // 重新初始化
            var initMethod = typeof(ItemAgentUtilities).GetMethod("Initialize",
                BindingFlags.Public | BindingFlags.Instance);
            if (initMethod != null)
            {
                initMethod.Invoke(agentUtils, new object[] { item });
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AgentFixer] 修复失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 获取默认的 handheld prefab
    /// </summary>
    private GameObject GetDefaultHandheldPrefab()
    {
        try
        {
            // 从 GameplayDataSettings 获取
            var prefabs = typeof(GameplayDataSettings).GetProperty("Prefabs",
                BindingFlags.Public | BindingFlags.Static)?.GetValue(null);

            if (prefabs != null)
            {
                var handheldField = prefabs.GetType().GetField("HandheldAgentPrefab");
                if (handheldField != null)
                {
                    return handheldField.GetValue(prefabs) as GameObject;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[AgentFixer] 获取默认 prefab 失败: {e.Message}");
        }

        return null;
    }

    /// <summary>
    /// 添加 agent entry
    /// </summary>
    private void AddAgentEntry(ItemAgentUtilities agentUtils,
        string key, GameObject prefab)
    {
        // 获取 agents 列表
        var agentsField = typeof(ItemAgentUtilities).GetField("agents",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (agentsField == null) return;

        var agents = agentsField.GetValue(agentUtils) as System.Collections.IList;
        if (agents == null) return;

        // 检查是否已存在
        foreach (var agent in agents)
        {
            var keyProp = agent.GetType().GetField("key");
            if (keyProp != null && (string)keyProp.GetValue(agent) == key)
            {
                return; // 已存在
            }
        }

        // 创建新的 entry
        var entryType = typeof(ItemAgentUtilities).GetNestedType("AgentEntry",
            BindingFlags.Public | BindingFlags.NonPublic);

        if (entryType != null)
        {
            var entry = System.Activator.CreateInstance(entryType);
            entryType.GetField("key")?.SetValue(entry, key);
            entryType.GetField("agentPrefab")?.SetValue(entry, prefab);
            agents.Add(entry);

            Debug.Log($"[AgentFixer] 已添加 agent entry: {key}");
        }
    }

    /// <summary>
    /// 手动触发修复（供外部调用）
    /// </summary>
    public void ForceFixAll()
    {
        fixedItems.Clear();
        ScanAndFixInventoryItems();
    }
}
```

### 48.3 在 ModBehaviour 中集成

```csharp
public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    private AgentUtilitiesFixer agentFixer;

    void Start()
    {
        // ... 其他初始化 ...

        // 添加 AgentUtilities 修复器
        agentFixer = gameObject.AddComponent<AgentUtilitiesFixer>();
        agentFixer.targetItemTypeIDs = new List<int>
        {
            990001, 990002, 990003 // 你的物品TypeID
        };
        agentFixer.scanInterval = 5f;

        Debug.Log("[MyMod] AgentUtilities 修复器已启动");
    }
}
```

---

## 四十九、多物品 Mod 架构

### 49.1 架构概述

一个 Mod 可以包含多个物品，需要统一管理它们的注册、配置和清理。

### 49.2 完整的多物品 Mod 模板

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Duckov.Modding;
using ItemStatsSystem;
using SodaCraft.Localizations;
using System.IO;
using System.Reflection;

namespace MultiItemMod
{
    /// <summary>
    /// 多物品 Mod 架构模板
    /// 管理多个物品的注册、配置和生命周期
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        #region 物品 TypeID 定义

        // 使用 const 定义所有物品的 TypeID
        private const int WEAPON_TYPE_ID = 990001;      // 武器
        private const int CONSUMABLE_TYPE_ID = 990002;  // 消耗品
        private const int GRENADE_TYPE_ID = 990003;     // 投掷物
        private const int PASSIVE_TYPE_ID = 990004;     // 被动装备

        #endregion

        #region 物品 Prefab 存储

        // 存储所有物品 Prefab
        private Dictionary<int, Item> itemPrefabs = new Dictionary<int, Item>();

        // AssetBundle
        private AssetBundle assetBundle;

        // 协程引用
        private Coroutine lootBoxCoroutine;
        private Coroutine agentFixCoroutine;

        // 事件注册标记
        private bool eventsRegistered = false;

        #endregion

        #region 生命周期

        void Start()
        {
            Debug.Log("[MultiItemMod] 开始加载...");

            try
            {
                // 1. 设置本地化
                SetupLocalization();

                // 2. 加载资源
                LoadAssets();

                // 3. 创建所有物品
                CreateAllItems();

                // 4. 注册所有物品
                RegisterAllItems();

                // 5. 注册到商店
                RegisterToShops();

                // 6. 启动后台任务
                StartBackgroundTasks();

                // 7. 注册事件
                RegisterEvents();

                Debug.Log($"[MultiItemMod] 加载完成，共 {itemPrefabs.Count} 个物品");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MultiItemMod] 加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        void OnDestroy()
        {
            Debug.Log("[MultiItemMod] 开始卸载...");

            // 1. 停止协程
            StopBackgroundTasks();

            // 2. 取消事件
            UnregisterEvents();

            // 3. 移除物品注册
            UnregisterAllItems();

            // 4. 卸载资源
            UnloadAssets();

            Debug.Log("[MultiItemMod] 卸载完成");
        }

        #endregion

        #region 本地化

        private void SetupLocalization()
        {
            // 武器
            LocalizationManager.SetOverrideText("Weapon_Name", "自定义武器");
            LocalizationManager.SetOverrideText("Weapon_Desc", "一把强力的自定义武器");

            // 消耗品
            LocalizationManager.SetOverrideText("Consumable_Name", "能量药剂");
            LocalizationManager.SetOverrideText("Consumable_Desc", "恢复生命值");

            // 投掷物
            LocalizationManager.SetOverrideText("Grenade_Name", "特殊手雷");
            LocalizationManager.SetOverrideText("Grenade_Desc", "投掷造成范围伤害");

            // 被动装备
            LocalizationManager.SetOverrideText("Passive_Name", "防护徽章");
            LocalizationManager.SetOverrideText("Passive_Desc", "被动：受伤时有几率闪避");

            // 监听语言切换
            LocalizationManager.OnSetLanguage += OnLanguageChanged;
        }

        private void OnLanguageChanged(SystemLanguage language)
        {
            // 根据语言更新文本
            switch (language)
            {
                case SystemLanguage.English:
                    LocalizationManager.SetOverrideText("Weapon_Name", "Custom Weapon");
                    // ... 其他英文文本
                    break;
                default:
                    // 中文
                    break;
            }
        }

        #endregion

        #region 资源加载

        private void LoadAssets()
        {
            string modPath = Path.GetDirectoryName(info.dllPath);
            string bundlePath = Path.Combine(modPath, "Assets", "multiitem_assets");

            if (File.Exists(bundlePath))
            {
                assetBundle = AssetBundle.LoadFromFile(bundlePath);
                Debug.Log("[MultiItemMod] AssetBundle 加载成功");
            }
            else
            {
                Debug.Log("[MultiItemMod] 未找到 AssetBundle，使用程序化创建");
            }
        }

        private void UnloadAssets()
        {
            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                assetBundle = null;
            }
        }

        #endregion

        #region 物品创建

        private void CreateAllItems()
        {
            // 创建武器
            CreateWeaponItem();

            // 创建消耗品
            CreateConsumableItem();

            // 创建投掷物
            CreateGrenadeItem();

            // 创建被动装备
            CreatePassiveItem();
        }

        private void CreateWeaponItem()
        {
            GameObject itemObj = new GameObject("CustomWeapon");
            itemObj.SetActive(false);
            DontDestroyOnLoad(itemObj);

            Item item = itemObj.AddComponent<Item>();
            SetItemField(item, "typeID", WEAPON_TYPE_ID);
            SetItemField(item, "displayName", "Weapon_Name");
            SetItemField(item, "description", "Weapon_Desc");
            SetItemField(item, "stackable", false);
            SetItemField(item, "quality", 4);
            SetItemField(item, "value", 50000);

            // 添加武器组件
            var attack = itemObj.AddComponent<CustomMeleeAttack>();
            // 配置攻击参数...

            itemObj.SetActive(true);
            itemPrefabs[WEAPON_TYPE_ID] = item;

            Debug.Log("[MultiItemMod] 武器创建完成");
        }

        private void CreateConsumableItem()
        {
            GameObject itemObj = new GameObject("EnergyPotion");
            itemObj.SetActive(false);
            DontDestroyOnLoad(itemObj);

            Item item = itemObj.AddComponent<Item>();
            SetItemField(item, "typeID", CONSUMABLE_TYPE_ID);
            SetItemField(item, "displayName", "Consumable_Name");
            SetItemField(item, "description", "Consumable_Desc");
            SetItemField(item, "stackable", true);
            SetItemField(item, "maxStackCount", 5);
            SetItemField(item, "quality", 2);
            SetItemField(item, "value", 1000);

            // 添加使用行为
            var useBehavior = itemObj.AddComponent<HealingUse>();
            useBehavior.healValue = 50;

            // 配置 UsageUtilities
            ConfigureUsageUtilities(item, useBehavior);

            itemObj.SetActive(true);
            itemPrefabs[CONSUMABLE_TYPE_ID] = item;

            Debug.Log("[MultiItemMod] 消耗品创建完成");
        }

        private void CreateGrenadeItem()
        {
            GameObject itemObj = new GameObject("SpecialGrenade");
            itemObj.SetActive(false);
            DontDestroyOnLoad(itemObj);

            Item item = itemObj.AddComponent<Item>();
            SetItemField(item, "typeID", GRENADE_TYPE_ID);
            SetItemField(item, "displayName", "Grenade_Name");
            SetItemField(item, "description", "Grenade_Desc");
            SetItemField(item, "stackable", true);
            SetItemField(item, "maxStackCount", 3);
            SetItemField(item, "quality", 3);
            SetItemField(item, "value", 5000);

            // 添加技能组件
            CreateGrenadeSkill(itemObj, item);

            itemObj.SetActive(true);
            itemPrefabs[GRENADE_TYPE_ID] = item;

            Debug.Log("[MultiItemMod] 投掷物创建完成");
        }

        private void CreatePassiveItem()
        {
            GameObject itemObj = new GameObject("ProtectionBadge");
            itemObj.SetActive(false);
            DontDestroyOnLoad(itemObj);

            Item item = itemObj.AddComponent<Item>();
            SetItemField(item, "typeID", PASSIVE_TYPE_ID);
            SetItemField(item, "displayName", "Passive_Name");
            SetItemField(item, "description", "Passive_Desc");
            SetItemField(item, "stackable", true);
            SetItemField(item, "maxStackCount", 5);
            SetItemField(item, "quality", 3);
            SetItemField(item, "value", 10000);

            // 创建被动效果系统
            CreatePassiveEffect(itemObj, item);

            itemObj.SetActive(true);
            itemPrefabs[PASSIVE_TYPE_ID] = item;

            Debug.Log("[MultiItemMod] 被动装备创建完成");
        }

        /// <summary>
        /// 创建被动效果系统
        /// </summary>
        private void CreatePassiveEffect(GameObject itemObj, Item item)
        {
            // 创建 Effect 容器
            GameObject effectObj = new GameObject("DodgeEffect");
            effectObj.transform.SetParent(itemObj.transform);
            effectObj.transform.localPosition = Vector3.zero;

            // 添加 Effect 组件
            Effect effect = effectObj.AddComponent<Effect>();
            SetFieldValue(effect, "display", true);
            SetFieldValue(effect, "description", "被动：受伤时有10%几率闪避");

            // 添加触发器（监听受伤事件）
            var trigger = effectObj.AddComponent<DodgeTrigger>();

            // 添加动作（执行闪避效果）
            var action = effectObj.AddComponent<DodgeAction>();
            action.healAmount = 5f;

            // 初始化 Effect 的 triggers 和 actions 列表
            InitializeEffectLists(effect, trigger, action);

            // 将 Effect 添加到物品
            item.Effects.Add(effect);

            Debug.Log("[MultiItemMod] 被动效果创建完成");
        }

        #endregion

        #region 物品注册

        private void RegisterAllItems()
        {
            foreach (var kvp in itemPrefabs)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(kvp.Value);
                if (success)
                {
                    Debug.Log($"[MultiItemMod] 物品 {kvp.Key} 注册成功");
                }
                else
                {
                    Debug.LogError($"[MultiItemMod] 物品 {kvp.Key} 注册失败");
                }
            }
        }

        private void UnregisterAllItems()
        {
            foreach (var kvp in itemPrefabs)
            {
                ItemAssetsCollection.RemoveDynamicEntry(kvp.Value);
            }
            itemPrefabs.Clear();
        }

        private void RegisterToShops()
        {
            // 参考第37章的商店注册逻辑
            // 将所有物品添加到商店
        }

        #endregion

        #region 后台任务

        private void StartBackgroundTasks()
        {
            // 箱子注入
            lootBoxCoroutine = StartCoroutine(LootBoxInjectionRoutine());

            // AgentUtilities 修复
            agentFixCoroutine = StartCoroutine(AgentFixRoutine());
        }

        private void StopBackgroundTasks()
        {
            if (lootBoxCoroutine != null)
            {
                StopCoroutine(lootBoxCoroutine);
            }
            if (agentFixCoroutine != null)
            {
                StopCoroutine(agentFixCoroutine);
            }
        }

        private IEnumerator LootBoxInjectionRoutine()
        {
            yield return new WaitForSeconds(2f);

            HashSet<int> processedBoxes = new HashSet<int>();

            while (true)
            {
                // 注入物品到箱子
                // 参考第38章的箱子注入逻辑

                yield return new WaitForSeconds(5f);
            }
        }

        private IEnumerator AgentFixRoutine()
        {
            yield return new WaitForSeconds(3f);

            HashSet<int> fixedItems = new HashSet<int>();

            while (true)
            {
                ScanAndFixInventoryItems(fixedItems);
                yield return new WaitForSeconds(5f);
            }
        }

        private void ScanAndFixInventoryItems(HashSet<int> fixedItems)
        {
            CharacterMainControl mainChar = CharacterMainControl.Main;
            if (mainChar == null || mainChar.CharacterItem == null) return;

            Inventory inventory = mainChar.CharacterItem.Inventory;
            if (inventory == null) return;

            foreach (Item item in inventory)
            {
                if (item == null) continue;
                if (!itemPrefabs.ContainsKey(item.TypeID)) continue;

                int instanceId = item.GetInstanceID();
                if (fixedItems.Contains(instanceId)) continue;

                // 检查并修复 AgentUtilities
                if (item.AgentUtilities != null && !item.HasHandHeldAgent)
                {
                    FixItemAgentUtilities(item);
                }

                fixedItems.Add(instanceId);
            }
        }

        private void FixItemAgentUtilities(Item item)
        {
            // 参考第48章的修复逻辑
        }

        #endregion

        #region 事件处理

        private void RegisterEvents()
        {
            if (eventsRegistered) return;
            eventsRegistered = true;

            LevelManager.OnLevelInitialized += OnLevelLoaded;
            Item.onUseStatic += OnAnyItemUsed;
        }

        private void UnregisterEvents()
        {
            LocalizationManager.OnSetLanguage -= OnLanguageChanged;

            if (eventsRegistered)
            {
                LevelManager.OnLevelInitialized -= OnLevelLoaded;
                Item.onUseStatic -= OnAnyItemUsed;
            }
        }

        private void OnLevelLoaded()
        {
            Debug.Log("[MultiItemMod] 关卡加载完成");
        }

        private void OnAnyItemUsed(Item item, object user)
        {
            if (itemPrefabs.ContainsKey(item.TypeID))
            {
                Debug.Log($"[MultiItemMod] Mod物品被使用: {item.DisplayName}");
            }
        }

        #endregion

        #region 工具方法

        private void SetItemField(Item item, string fieldName, object value)
        {
            var field = typeof(Item).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            field?.SetValue(item, value);
        }

        private void SetFieldValue(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        private void ConfigureUsageUtilities(Item item, UsageBehavior behavior)
        {
            // 获取或创建 UsageUtilities
            UsageUtilities usageUtils = item.gameObject.GetComponent<UsageUtilities>();
            if (usageUtils == null)
            {
                usageUtils = item.gameObject.AddComponent<UsageUtilities>();
            }

            // 获取 behaviors 列表
            var behaviorsField = typeof(UsageUtilities).GetField("behaviors",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var behaviors = behaviorsField?.GetValue(usageUtils) as List<UsageBehavior>;
            if (behaviors == null)
            {
                behaviors = new List<UsageBehavior>();
                behaviorsField?.SetValue(usageUtils, behaviors);
            }

            if (!behaviors.Contains(behavior))
            {
                behaviors.Add(behavior);
            }

            // 设置 item 的 usageUtilities
            SetItemField(item, "usageUtilities", usageUtils);
        }

        private void CreateGrenadeSkill(GameObject itemObj, Item item)
        {
            // 参考第32章的技能系统创建
        }

        private void InitializeEffectLists(Effect effect,
            EffectTrigger trigger, EffectAction action)
        {
            // 初始化 triggers 列表
            var triggersField = typeof(Effect).GetField("triggers",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var triggers = triggersField?.GetValue(effect) as List<EffectTrigger>;
            if (triggers == null)
            {
                triggers = new List<EffectTrigger>();
                triggersField?.SetValue(effect, triggers);
            }
            triggers.Add(trigger);

            // 初始化 actions 列表
            var actionsField = typeof(Effect).GetField("actions",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var actions = actionsField?.GetValue(effect) as List<EffectAction>;
            if (actions == null)
            {
                actions = new List<EffectAction>();
                actionsField?.SetValue(effect, actions);
            }
            actions.Add(action);
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 获取物品 Prefab
        /// </summary>
        public Item GetItemPrefab(int typeId)
        {
            return itemPrefabs.TryGetValue(typeId, out Item prefab) ? prefab : null;
        }

        /// <summary>
        /// 获取所有物品 TypeID
        /// </summary>
        public IEnumerable<int> GetAllItemTypeIDs()
        {
            return itemPrefabs.Keys;
        }

        #endregion
    }
}
```

---

## 五十、跨场景数据持久化

### 50.1 问题背景

Unity 场景切换时，非 `DontDestroyOnLoad` 的对象会被销毁。Mod 需要在场景间保持数据。

### 50.2 静态数据持久化

```csharp
using UnityEngine;
using System;

/// <summary>
/// 跨场景数据持久化
/// 使用静态字段保存需要跨场景的数据
/// </summary>
public static class CrossSceneData
{
    #region 数据结构

    /// <summary>
    /// 传送点数据
    /// </summary>
    [Serializable]
    public class TeleportData
    {
        public string sceneName;
        public Vector3 position;
        public Quaternion rotation;
        public DateTime timestamp;

        public bool IsValid => !string.IsNullOrEmpty(sceneName);

        public bool IsExpired(float maxAgeSeconds = 3600f)
        {
            return (DateTime.Now - timestamp).TotalSeconds > maxAgeSeconds;
        }
    }

    /// <summary>
    /// Mod 状态数据
    /// </summary>
    [Serializable]
    public class ModStateData
    {
        public int usageCount;
        public float lastUsedTime;
        public bool isEnabled;
    }

    #endregion

    #region 静态存储

    // 传送点数据
    private static TeleportData _teleportData = null;
    public static TeleportData TeleportPoint
    {
        get => _teleportData;
        set => _teleportData = value;
    }

    // 待处理标记（用于场景加载后执行操作）
    private static bool _pendingTeleport = false;
    public static bool PendingTeleport
    {
        get => _pendingTeleport;
        set => _pendingTeleport = value;
    }

    // Mod 状态
    private static ModStateData _modState = new ModStateData();
    public static ModStateData ModState => _modState;

    #endregion

    #region 公共方法

    /// <summary>
    /// 保存传送点
    /// </summary>
    public static void SaveTeleportPoint(string sceneName, Vector3 position,
        Quaternion rotation)
    {
        _teleportData = new TeleportData
        {
            sceneName = sceneName,
            position = position,
            rotation = rotation,
            timestamp = DateTime.Now
        };

        Debug.Log($"[跨场景数据] 保存传送点: {sceneName} @ {position}");
    }

    /// <summary>
    /// 清除传送点
    /// </summary>
    public static void ClearTeleportPoint()
    {
        _teleportData = null;
        _pendingTeleport = false;
        Debug.Log("[跨场景数据] 传送点已清除");
    }

    /// <summary>
    /// 检查是否有有效传送点
    /// </summary>
    public static bool HasValidTeleportPoint()
    {
        return _teleportData != null &&
               _teleportData.IsValid &&
               !_teleportData.IsExpired();
    }

    /// <summary>
    /// 重置所有数据
    /// </summary>
    public static void ResetAll()
    {
        _teleportData = null;
        _pendingTeleport = false;
        _modState = new ModStateData();
        Debug.Log("[跨场景数据] 所有数据已重置");
    }

    #endregion
}
```

### 50.3 在场景加载后处理待处理操作

```csharp
public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    void Start()
    {
        // 注册关卡加载事件
        LevelManager.OnLevelInitialized += OnLevelLoaded;
    }

    void OnDestroy()
    {
        LevelManager.OnLevelInitialized -= OnLevelLoaded;
    }

    /// <summary>
    /// 关卡加载完成后检查待处理操作
    /// </summary>
    private void OnLevelLoaded()
    {
        // 检查是否有待处理的传送
        if (CrossSceneData.PendingTeleport)
        {
            StartCoroutine(ExecutePendingTeleport());
        }
    }

    /// <summary>
    /// 执行待处理的传送
    /// </summary>
    private IEnumerator ExecutePendingTeleport()
    {
        // 等待一帧确保场景完全加载
        yield return null;

        // 检查数据有效性
        if (!CrossSceneData.HasValidTeleportPoint())
        {
            Debug.Log("[Mod] 传送数据无效或已过期");
            CrossSceneData.PendingTeleport = false;
            yield break;
        }

        var teleportData = CrossSceneData.TeleportPoint;
        string currentScene = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().name;

        // 验证场景
        if (currentScene != teleportData.sceneName)
        {
            Debug.Log($"[Mod] 场景不匹配: 当前={currentScene}, 目标={teleportData.sceneName}");
            CrossSceneData.PendingTeleport = false;
            yield break;
        }

        // 获取玩家
        CharacterMainControl player = CharacterMainControl.Main;
        if (player == null)
        {
            Debug.LogError("[Mod] 找不到玩家");
            CrossSceneData.PendingTeleport = false;
            yield break;
        }

        // 执行传送
        player.transform.position = teleportData.position;
        player.transform.rotation = teleportData.rotation;

        player.PopText("传送完成！");
        Debug.Log($"[Mod] 传送到: {teleportData.position}");

        // 清除标记
        CrossSceneData.PendingTeleport = false;
        CrossSceneData.ClearTeleportPoint();
    }
}
```

---

## 五十一、开发检查清单汇总（更新版）

### 完整 Mod 开发检查清单

```
□ 项目配置
  □ .NET Standard 2.1 目标框架
  □ 所有必需 DLL 正确引用
  □ ModBehaviour 继承自 Duckov.Modding.ModBehaviour

□ 物品系统
  □ TypeID 唯一（推荐 990000+）
  □ 本地化文本设置
  □ UsageBehavior 配置（如需使用）
  □ ItemAgentUtilities 配置（武器必需）
  □ Effect 系统配置（被动装备必需）

□ 商店/箱子
  □ StockShopDatabase 注册
  □ LootBox 注入协程

□ 战斗系统（武器）
  □ Stats 配置（Damage, CritRate 等）
  □ DamageInfo 正确创建
  □ 格挡系统（可选）
  □ 动画事件处理（可选）

□ 场景系统
  □ 场景条件检查（IsBaseLevel, IsRaidMap）
  □ 撤离系统集成（可选）
  □ 跨场景数据持久化

□ 运行时修复
  □ AgentUtilities 修复器
  □ 定期扫描协程

□ 事件与清理
  □ 所有事件在 OnDestroy 中取消
  □ 物品从 ItemAssetsCollection 移除
  □ AssetBundle 卸载
  □ 协程停止
```

---

## 五十二、技能系统深入：SkillContext详解

技能系统是实现复杂物品行为（如投掷物、特殊攻击）的核心。

### 52.1 SkillContext 配置参数

`SkillContext` 是技能的配置结构，决定技能的基本行为：

```csharp
/// <summary>
/// 技能上下文配置
/// 用于配置技能的释放参数
/// </summary>
public struct SkillContext
{
    /// <summary>施法范围（最大释放距离）</summary>
    public float castRange;

    /// <summary>效果范围（AOE半径）</summary>
    public float effectRange;

    /// <summary>是否是手雷类技能</summary>
    public bool isGrenade;

    /// <summary>手雷垂直速度（影响抛物线高度）</summary>
    public float grenageVerticleSpeed;

    /// <summary>瞄准时是否可以移动</summary>
    public bool movableWhileAim;

    /// <summary>技能准备时间</summary>
    public float skillReadyTime;

    /// <summary>是否检查障碍物</summary>
    public bool checkObsticle;

    /// <summary>是否在开始瞄准时释放</summary>
    public bool releaseOnStartAim;
}
```

### 52.2 SkillReleaseContext 运行时数据

`SkillReleaseContext` 在技能释放时由游戏填充，包含目标位置等信息：

```csharp
/// <summary>
/// 技能释放上下文
/// 包含释放时的运行时数据
/// </summary>
public struct SkillReleaseContext
{
    /// <summary>释放点（目标位置）</summary>
    public Vector3 releasePoint;

    /// <summary>释放方向</summary>
    public Vector3 releaseDirection;

    /// <summary>目标对象（如果有）</summary>
    public GameObject target;
}
```

### 52.3 完整技能实现示例

```csharp
using UnityEngine;
using Duckov;
using ItemStatsSystem;
using System.Reflection;

/// <summary>
/// 投掷类技能完整实现
/// 继承 SkillBase，实现 OnRelease
/// </summary>
public class ThrowableSkill : SkillBase
{
    [Header("投掷配置")]
    public float damageRange = 5f;
    public float delayTime = 2f;
    public float throwForce = 15f;
    public float verticleSpeed = 10f;
    public bool canHurtSelf = true;

    // 投掷物预制体
    public GameObject projectilePrefab;

    /// <summary>
    /// 技能释放时调用
    /// </summary>
    public override void OnRelease()
    {
        if (fromCharacter == null)
        {
            Debug.LogWarning("[技能] 没有释放者");
            return;
        }

        // 获取瞄准点位置
        Vector3 spawnPos = fromCharacter.CurrentUsingAimSocket.position;
        Vector3 targetPos = skillReleaseContext.releasePoint;

        // 计算投掷方向
        Vector3 direction = targetPos - fromCharacter.transform.position;
        direction.y = 0;
        float castDistance = direction.magnitude;
        direction.Normalize();

        // 计算抛物线速度
        Vector3 velocity = CalculateVelocity(spawnPos, targetPos, verticleSpeed);

        // 创建投掷物
        LaunchProjectile(spawnPos, velocity);
    }

    /// <summary>
    /// 计算抛物线速度
    /// 根据起点、终点和垂直速度计算初速度向量
    /// </summary>
    private Vector3 CalculateVelocity(Vector3 start, Vector3 target, float vSpeed)
    {
        float g = Physics.gravity.magnitude;
        float y = target.y - start.y;
        Vector3 horizontal = target - start;
        horizontal.y = 0f;

        float horizontalDist = horizontal.magnitude;
        float totalDist = Mathf.Sqrt(y * y + horizontalDist * horizontalDist);

        // 计算飞行时间
        float upDist = totalDist + y;
        float downDist = totalDist - y;
        float t = 2f * upDist / (g * downDist);

        // 计算水平和垂直分量
        float horizontalSpeed = horizontalDist / Mathf.Sqrt(t);
        float verticalSpeed = g * t / 2f;

        Vector3 velocity = horizontal.normalized * horizontalSpeed;
        velocity.y = verticalSpeed;

        return velocity;
    }

    /// <summary>
    /// 发射投掷物
    /// </summary>
    private void LaunchProjectile(Vector3 position, Vector3 velocity)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[技能] 投掷物预制体为空");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, position, Quaternion.identity);

        // 配置投掷物组件
        var grenadeComponent = projectile.GetComponent<WormholeGrenadeProjectile>();
        if (grenadeComponent != null)
        {
            grenadeComponent.delayTime = delayTime;
            grenadeComponent.damageRange = damageRange;

            // 设置伤害信息
            DamageInfo dmgInfo = new DamageInfo(fromCharacter);
            dmgInfo.damageValue = 0f; // 根据需要设置
            grenadeComponent.SetDamageInfo(dmgInfo);

            // 发射
            grenadeComponent.Launch(position, velocity, fromCharacter, canHurtSelf);
        }
    }
}
```

### 52.4 配置技能到物品

```csharp
/// <summary>
/// 在物品上配置技能
/// </summary>
private void ConfigureItemSkill(Item item, SkillBase skill)
{
    // 使用反射设置 skillContext
    var skillContextField = typeof(SkillBase).GetField("skillContext",
        BindingFlags.NonPublic | BindingFlags.Instance);

    if (skillContextField != null)
    {
        var context = new SkillContext
        {
            castRange = 15f,            // 最大投掷距离
            effectRange = 5f,            // 爆炸范围
            isGrenade = true,            // 标记为手雷
            grenageVerticleSpeed = 10f,  // 垂直速度
            movableWhileAim = true,      // 瞄准时可移动
            skillReadyTime = 0f,         // 无准备时间
            checkObsticle = true,        // 检查障碍物
            releaseOnStartAim = false    // 不在开始瞄准时释放
        };
        skillContextField.SetValue(skill, context);
    }

    // 添加 ItemSetting_Skill 组件
    ItemSetting_Skill skillSetting = item.gameObject.AddComponent<ItemSetting_Skill>();
    SetFieldValue(skillSetting, "Skill", skill);
    SetFieldValue(skillSetting, "onRelease", ItemSetting_Skill.OnReleaseAction.reduceCount);

    // 标记物品为技能物品
    item.SetBool("IsSkill", true, true);
}
```

---

## 五十三、物品实例化与CreateInstance

### 53.1 CreateInstance 方法

`Item.CreateInstance()` 是创建物品实例的标准方法：

```csharp
/// <summary>
/// 从预制体创建物品实例
/// </summary>
public Item CreateInstance()
{
    // 实例化 GameObject
    GameObject instance = Instantiate(this.gameObject);

    // 获取 Item 组件
    Item item = instance.GetComponent<Item>();

    // 初始化
    if (item != null)
    {
        item.Initialize();
    }

    return item;
}
```

### 53.2 实例化后初始化

创建物品实例后，需要进行额外初始化：

```csharp
/// <summary>
/// 添加物品到背包的完整流程
/// </summary>
private bool TryAddItemToInventory(Inventory inventory, int typeId)
{
    try
    {
        // 1. 获取预制体
        Item prefab = GetPrefabByTypeId(typeId);
        if (prefab == null) return false;

        // 2. 创建实例
        Item newItem = prefab.CreateInstance();
        if (newItem == null) return false;

        // 3. 初始化 Mod 物品（修复 AgentUtilities 等）
        InitializeModItem(newItem);

        // 4. 添加到背包
        bool success = inventory.AddItem(newItem);

        // 5. 失败时清理
        if (!success)
        {
            Destroy(newItem.gameObject);
        }

        return success;
    }
    catch (Exception e)
    {
        Debug.LogWarning($"添加物品失败: {e.Message}");
        return false;
    }
}

/// <summary>
/// 初始化 Mod 创建的物品
/// </summary>
private void InitializeModItem(Item item)
{
    if (item == null) return;

    // 初始化 AgentUtilities
    var agentUtils = item.AgentUtilities;
    if (agentUtils != null)
    {
        agentUtils.Initialize(item);
    }

    // 检查并修复手持代理
    if (!item.HasHandHeldAgent)
    {
        FixItemAgentUtilities(item);
    }
}
```

---

## 五十四、Mod信息与目录获取

### 54.1 info对象

`ModBehaviour` 基类提供 `info` 对象，包含 Mod 的元数据：

```csharp
/// <summary>
/// ModInfo 包含 Mod 的基本信息
/// </summary>
public class ModInfo
{
    /// <summary>DLL 文件路径</summary>
    public string dllPath;

    /// <summary>Mod 名称</summary>
    public string name;

    /// <summary>Mod 版本</summary>
    public string version;

    /// <summary>Mod 作者</summary>
    public string author;

    /// <summary>Mod 描述</summary>
    public string description;
}
```

### 54.2 获取Mod目录路径

```csharp
/// <summary>
/// 获取 Mod 所在目录路径
/// 用于加载 AssetBundle 等资源
/// </summary>
private string GetModFolderPath()
{
    // 方法1：通过 info.dllPath
    if (info != null && !string.IsNullOrEmpty(info.dllPath))
    {
        return Path.GetDirectoryName(info.dllPath);
    }

    // 方法2：通过程序集位置
    string assemblyPath = GetType().Assembly.Location;
    if (!string.IsNullOrEmpty(assemblyPath))
    {
        return Path.GetDirectoryName(assemblyPath);
    }

    return string.Empty;
}

/// <summary>
/// 加载 AssetBundle
/// </summary>
private void LoadAssetBundle()
{
    string modFolder = GetModFolderPath();
    string bundlePath = Path.Combine(modFolder, "Assets", "my_assets");

    if (File.Exists(bundlePath))
    {
        assetBundle = AssetBundle.LoadFromFile(bundlePath);

        if (assetBundle != null)
        {
            // 加载资源
            myPrefab = assetBundle.LoadAsset<GameObject>("MyPrefab");
            myIcon = LoadIconFromBundle("MyIcon");
        }
    }
}
```

---

## 五十五、Stats属性Hash缓存优化

在频繁读取物品属性时，使用 Hash 缓存可以提升性能：

```csharp
/// <summary>
/// Stats Hash 缓存优化示例
/// 避免每次调用时计算字符串 Hash
/// </summary>
public class WeaponComponent : MonoBehaviour
{
    // ========== Stat Hash 缓存（静态只读，编译时计算）==========
    private static readonly int DamageHash = "Damage".GetHashCode();
    private static readonly int CritRateHash = "CritRate".GetHashCode();
    private static readonly int CritDamageFactorHash = "CritDamageFactor".GetHashCode();
    private static readonly int ArmorPiercingHash = "ArmorPiercing".GetHashCode();
    private static readonly int AttackSpeedHash = "AttackSpeed".GetHashCode();
    private static readonly int AttackRangeHash = "AttackRange".GetHashCode();
    private static readonly int StaminaCostHash = "StaminaCost".GetHashCode();
    private static readonly int BleedChanceHash = "BleedChance".GetHashCode();

    private Item weaponItem;

    // ========== 属性访问器（从 Item.Stats 读取）==========

    /// <summary>基础伤害（默认值 50）</summary>
    public float Damage => weaponItem != null ?
        weaponItem.GetStatValue(DamageHash) : 50f;

    /// <summary>暴击率（默认值 5%）</summary>
    public float CritRate => weaponItem != null ?
        weaponItem.GetStatValue(CritRateHash) : 0.05f;

    /// <summary>暴击伤害倍率（默认值 1.5）</summary>
    public float CritDamageFactor => weaponItem != null ?
        weaponItem.GetStatValue(CritDamageFactorHash) : 1.5f;

    /// <summary>护甲穿透（默认值 0）</summary>
    public float ArmorPiercing => weaponItem != null ?
        weaponItem.GetStatValue(ArmorPiercingHash) : 0f;

    /// <summary>攻击速度（默认值 1，最小 0.1）</summary>
    public float AttackSpeed => weaponItem != null ?
        Mathf.Max(0.1f, weaponItem.GetStatValue(AttackSpeedHash)) : 1f;

    /// <summary>攻击范围（默认值 2 米）</summary>
    public float AttackRange => weaponItem != null ?
        weaponItem.GetStatValue(AttackRangeHash) : 2f;

    /// <summary>体力消耗（默认值 5）</summary>
    public float StaminaCost => weaponItem != null ?
        weaponItem.GetStatValue(StaminaCostHash) : 5f;

    /// <summary>流血几率（默认值 0）</summary>
    public float BleedChance => weaponItem != null ?
        weaponItem.GetStatValue(BleedChanceHash) : 0f;

    void Start()
    {
        weaponItem = GetComponent<Item>();
        if (weaponItem == null)
        {
            Debug.LogWarning("[武器] 未找到 Item 组件，使用默认属性值");
        }
        else
        {
            Debug.Log($"[武器] 属性加载: 伤害={Damage}, 暴击率={CritRate}, 范围={AttackRange}");
        }
    }
}
```

---

## 五十六、连击与冲刺系统实现

### 56.1 连击索引系统

```csharp
/// <summary>
/// 连击系统实现
/// 支持正手/反手交替攻击
/// </summary>
public class ComboSystem : MonoBehaviour
{
    [Header("连击配置")]
    public float comboResetTime = 1.5f;      // 连击重置时间
    public int maxComboSteps = 2;             // 最大连击段数

    // 运行时状态
    private int comboIndex = 0;               // 当前连击索引
    private float lastAttackTime = 0f;        // 上次攻击时间
    private bool isAttacking = false;         // 是否正在攻击

    private Animator animator;

    void Update()
    {
        // 自动重置连击
        if (Time.time - lastAttackTime > comboResetTime)
        {
            ResetCombo();
        }

        // 检测攻击输入
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            PerformComboAttack();
        }
    }

    /// <summary>
    /// 执行连击攻击
    /// </summary>
    private void PerformComboAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // 根据连击索引播放不同动画
        switch (comboIndex)
        {
            case 0:
                animator?.SetTrigger("ForwardSlash");
                Debug.Log("[连击] 正手挥击");
                break;
            case 1:
                animator?.SetTrigger("BackhandSlash");
                Debug.Log("[连击] 反手挥击");
                break;
        }

        // 启动攻击协程
        StartCoroutine(AttackCoroutine());

        // 切换到下一段连击
        comboIndex = (comboIndex + 1) % maxComboSteps;
    }

    /// <summary>
    /// 攻击协程
    /// </summary>
    private IEnumerator AttackCoroutine()
    {
        // 等待攻击动画到达判定点
        float attackDelay = 0.3f;
        yield return new WaitForSeconds(attackDelay);

        // 执行伤害判定
        PerformDamage();

        // 等待动画结束
        yield return new WaitForSeconds(attackDelay);

        isAttacking = false;
    }

    /// <summary>
    /// 重置连击
    /// </summary>
    private void ResetCombo()
    {
        if (comboIndex != 0)
        {
            comboIndex = 0;
            Debug.Log("[连击] 连击已重置");
        }
    }

    private void PerformDamage()
    {
        // 伤害判定逻辑...
    }
}
```

### 56.2 协程控制攻击时序

```csharp
/// <summary>
/// 使用协程精确控制攻击时序
/// </summary>
private IEnumerator NormalAttackCoroutine()
{
    // 阶段1: 起手
    Debug.Log("[攻击] 阶段1: 起手");
    yield return new WaitForSeconds(0.1f);

    // 阶段2: 挥动（可以在此播放音效）
    Debug.Log("[攻击] 阶段2: 挥动");
    PlaySwingSound();
    yield return new WaitForSeconds(0.2f);

    // 阶段3: 判定点（造成伤害）
    Debug.Log("[攻击] 阶段3: 判定");
    PerformMeleeDamage();
    yield return new WaitForSeconds(0.1f);

    // 阶段4: 收招
    Debug.Log("[攻击] 阶段4: 收招");
    yield return new WaitForSeconds(0.2f);

    // 攻击结束
    isAttacking = false;
}
```

### 56.3 冲刺移动实现

```csharp
/// <summary>
/// 冲刺移动协程
/// 平滑移动玩家到目标位置
/// </summary>
public IEnumerator DashMovement(float distance)
{
    if (player == null) yield break;

    Vector3 dashDirection = player.transform.forward;
    float dashDuration = 0.3f;

    Vector3 startPos = player.transform.position;
    Vector3 endPos = startPos + dashDirection * distance;

    // 障碍物检测
    RaycastHit hit;
    if (Physics.Raycast(startPos, dashDirection, out hit, distance))
    {
        // 在障碍物前 0.5 米停下
        endPos = hit.point - dashDirection * 0.5f;
    }

    // 平滑移动
    float elapsed = 0f;
    while (elapsed < dashDuration)
    {
        float t = elapsed / dashDuration;
        // 使用缓动函数使移动更自然
        float smoothT = Mathf.SmoothStep(0f, 1f, t);
        player.transform.position = Vector3.Lerp(startPos, endPos, smoothT);

        elapsed += Time.deltaTime;
        yield return null;
    }

    // 确保到达终点
    player.transform.position = endPos;
    Debug.Log($"[冲刺] 完成，移动了 {Vector3.Distance(startPos, endPos):F2} 米");
}

/// <summary>
/// 特殊攻击协程（包含冲刺）
/// </summary>
private IEnumerator SpecialAttackCoroutine()
{
    // 阶段1: 蓄力
    Debug.Log("[特殊攻击] 阶段1: 蓄力");
    yield return new WaitForSeconds(0.3f);

    // 阶段2: 冲刺
    Debug.Log("[特殊攻击] 阶段2: 冲刺");
    yield return StartCoroutine(DashMovement(3f));

    // 阶段3: 释放技能
    Debug.Log("[特殊攻击] 阶段3: 释放");
    LaunchSwordAura();

    // 等待结束
    yield return new WaitForSeconds(0.6f);
    isAttacking = false;
}
```

---

## 五十七、SceneLoader反射调用

游戏使用 `SceneLoader` 处理场景加载，Mod 需要通过反射调用：

```csharp
using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// SceneLoader 反射调用工具类
/// </summary>
public static class SceneLoaderHelper
{
    /// <summary>
    /// 加载场景（反射调用 SceneLoader）
    /// </summary>
    /// <param name="sceneId">场景ID</param>
    /// <param name="notifyEvacuation">是否通知撤离</param>
    public static bool LoadScene(string sceneId, bool notifyEvacuation = false)
    {
        try
        {
            // 获取 SceneLoader 类型
            var sceneLoaderType = Type.GetType("SceneLoader, TeamSoda.Duckov.Core");
            if (sceneLoaderType == null)
            {
                Debug.LogWarning("[SceneLoader] 找不到 SceneLoader 类型");
                return false;
            }

            // 获取 Instance 属性
            var instanceProp = sceneLoaderType.GetProperty("Instance",
                BindingFlags.Public | BindingFlags.Static);
            if (instanceProp == null)
            {
                Debug.LogWarning("[SceneLoader] 找不到 Instance 属性");
                return false;
            }

            // 获取实例
            var sceneLoader = instanceProp.GetValue(null);
            if (sceneLoader == null)
            {
                Debug.LogWarning("[SceneLoader] Instance 为空");
                return false;
            }

            // 查找 LoadScene 方法
            var methods = sceneLoaderType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (method.Name == "LoadScene")
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length >= 1 && parameters[0].ParameterType == typeof(string))
                    {
                        // 构建参数
                        var args = BuildLoadSceneArgs(parameters, sceneId, notifyEvacuation);

                        // 调用
                        method.Invoke(sceneLoader, args);
                        Debug.Log($"[SceneLoader] 已调用加载场景: {sceneId}");
                        return true;
                    }
                }
            }

            Debug.LogWarning("[SceneLoader] 找不到合适的 LoadScene 方法");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SceneLoader] 调用失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 构建 LoadScene 方法参数
    /// </summary>
    private static object[] BuildLoadSceneArgs(ParameterInfo[] parameters,
        string sceneId, bool notifyEvacuation)
    {
        var args = new object[parameters.Length];
        args[0] = sceneId;

        for (int i = 1; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;

            if (paramType == typeof(bool))
            {
                // 根据参数位置设置默认值
                switch (i)
                {
                    case 2: args[i] = false; break;           // clickToContinue
                    case 3: args[i] = notifyEvacuation; break; // notifyEvacuation
                    case 4: args[i] = true; break;            // doCircleFade
                    case 5: args[i] = false; break;           // useLocation
                    case 7: args[i] = true; break;            // saveToFile
                    case 8: args[i] = false; break;           // hideTips
                    default: args[i] = false; break;
                }
            }
            else if (paramType == typeof(MultiSceneLocation))
            {
                args[i] = default(MultiSceneLocation);
            }
            else
            {
                args[i] = null;
            }
        }

        return args;
    }
}
```

---

## 五十八、调试快捷键系统

在开发阶段，使用快捷键快速测试功能：

```csharp
/// <summary>
/// 调试快捷键系统
/// 仅在开发阶段使用
/// </summary>
public class DebugHotkeys : MonoBehaviour
{
    [Header("配置")]
    public bool enableDebugKeys = true;

    // 物品 TypeID
    private const int ITEM_TYPE_ID_1 = 990001;
    private const int ITEM_TYPE_ID_2 = 990002;

    // 物品预制体引用
    public Item itemPrefab1;
    public Item itemPrefab2;

    void Update()
    {
        if (!enableDebugKeys) return;

        // F9: 添加测试物品
        if (Input.GetKeyDown(KeyCode.F9))
        {
            AddTestItems();
        }

        // F10: 显示调试信息
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ShowDebugInfo();
        }

        // F11: 恢复生命/体力
        if (Input.GetKeyDown(KeyCode.F11))
        {
            RestorePlayerStats();
        }

        // F12: 传送到安全位置
        if (Input.GetKeyDown(KeyCode.F12))
        {
            TeleportToSafeZone();
        }
    }

    /// <summary>
    /// 添加测试物品到玩家背包
    /// </summary>
    private void AddTestItems()
    {
        try
        {
            var character = CharacterMainControl.Main;
            if (character == null || character.CharacterItem == null)
            {
                ShowMessage("无法找到玩家");
                return;
            }

            var inventory = character.CharacterItem.Inventory;
            if (inventory == null)
            {
                ShowMessage("无法找到背包");
                return;
            }

            int addedCount = 0;

            // 添加物品1
            if (itemPrefab1 != null && TryAddItem(inventory, itemPrefab1))
            {
                addedCount++;
            }

            // 添加物品2
            if (itemPrefab2 != null && TryAddItem(inventory, itemPrefab2))
            {
                addedCount++;
            }

            ShowMessage($"已添加 {addedCount} 个测试物品");
            Debug.Log($"[调试] 添加了 {addedCount} 个测试物品");
        }
        catch (Exception e)
        {
            Debug.LogError($"[调试] 添加物品失败: {e.Message}");
        }
    }

    private bool TryAddItem(Inventory inventory, Item prefab)
    {
        var newItem = prefab.CreateInstance();
        if (newItem == null) return false;

        bool success = inventory.AddItem(newItem);
        if (!success)
        {
            Destroy(newItem.gameObject);
        }
        return success;
    }

    /// <summary>
    /// 显示调试信息
    /// </summary>
    private void ShowDebugInfo()
    {
        var character = CharacterMainControl.Main;
        if (character == null) return;

        var health = character.Health;
        string info = $"HP: {health?.CurrentHealth:F0}/{health?.MaxHealth:F0}\n" +
                      $"Stamina: {character.CurrentStamina:F0}/{character.MaxStamina:F0}\n" +
                      $"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}\n" +
                      $"Position: {character.transform.position}";

        Debug.Log($"[调试信息]\n{info}");
        ShowMessage("调试信息已输出到控制台");
    }

    /// <summary>
    /// 恢复玩家状态
    /// </summary>
    private void RestorePlayerStats()
    {
        var character = CharacterMainControl.Main;
        if (character == null) return;

        // 恢复生命
        character.Health?.AddHealth(character.Health.MaxHealth);

        // 恢复体力
        character.UseStamina(-character.MaxStamina);

        ShowMessage("已恢复生命和体力");
        Debug.Log("[调试] 已恢复玩家状态");
    }

    /// <summary>
    /// 传送到安全区域
    /// </summary>
    private void TeleportToSafeZone()
    {
        var character = CharacterMainControl.Main;
        if (character == null) return;

        // 传送到当前位置上方（避免卡在地形中）
        Vector3 safePos = character.transform.position + Vector3.up * 2f;
        character.transform.position = safePos;

        ShowMessage("已传送到安全位置");
    }

    private void ShowMessage(string message)
    {
        CharacterMainControl.Main?.PopText(message);
    }
}
```

---

## 五十九、完整ModBehaviour模板

基于所有已知模式的完整 Mod 主类模板：

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using Duckov.Modding;
using Duckov.Utilities;
using Duckov.Scenes;
using ItemStatsSystem;
using SodaCraft.Localizations;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace MyMod
{
    /// <summary>
    /// Mod 主类完整模板
    /// 包含物品创建、注册、箱子注入、背包监控等完整功能
    /// </summary>
    public class MyModBehaviour : ModBehaviour
    {
        #region 字段定义

        // ========== 物品预制体 ==========
        private Item itemPrefab1;
        private Item itemPrefab2;
        private Dictionary<int, Item> prefabRegistry = new Dictionary<int, Item>();

        // ========== TypeID 定义 ==========
        private const int ITEM1_TYPE_ID = 990001;
        private const int ITEM2_TYPE_ID = 990002;

        // ========== AssetBundle ==========
        private AssetBundle assetBundle;
        private Sprite itemIcon1;
        private Sprite itemIcon2;

        // ========== 运行时状态 ==========
        private Coroutine lootBoxCoroutine;
        private Coroutine inventoryWatchCoroutine;
        private HashSet<int> fixedItems = new HashSet<int>();
        private HashSet<int> processedLootboxes = new HashSet<int>();

        // ========== 跨场景数据（静态）==========
        private static bool pendingTeleport = false;
        private static Vector3 pendingPosition;
        private static string pendingScene;

        #endregion

        #region 生命周期

        void Start()
        {
            Debug.Log("[MyMod] 开始加载...");

            try
            {
                // 1. 加载资源
                LoadAssetBundle();

                // 2. 设置本地化
                SetupLocalization();

                // 3. 创建物品
                CreateItems();

                // 4. 注册物品
                RegisterItems();

                // 5. 注册到商店
                RegisterToShop();

                // 6. 启动后台任务
                lootBoxCoroutine = StartCoroutine(LootBoxInjectionRoutine());
                inventoryWatchCoroutine = StartCoroutine(InventoryWatchRoutine());

                // 7. 检查待处理任务
                CheckPendingTasks();

                Debug.Log("[MyMod] 加载完成!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MyMod] 加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        void Update()
        {
            // 调试快捷键
            if (Input.GetKeyDown(KeyCode.F9))
            {
                AddTestItems();
            }

            // 定期修复物品
            PeriodicItemFix();
        }

        void OnDestroy()
        {
            Debug.Log("[MyMod] 开始卸载...");

            // 1. 停止协程
            if (lootBoxCoroutine != null)
            {
                StopCoroutine(lootBoxCoroutine);
            }
            if (inventoryWatchCoroutine != null)
            {
                StopCoroutine(inventoryWatchCoroutine);
            }

            // 2. 取消事件监听
            LocalizationManager.OnSetLanguage -= OnLanguageChanged;

            // 3. 移除物品注册
            foreach (var kvp in prefabRegistry)
            {
                if (kvp.Value != null)
                {
                    ItemAssetsCollection.RemoveDynamicEntry(kvp.Value);
                    Destroy(kvp.Value.gameObject);
                }
            }
            prefabRegistry.Clear();

            // 4. 卸载 AssetBundle
            if (assetBundle != null)
            {
                assetBundle.Unload(true);
            }

            Debug.Log("[MyMod] 卸载完成");
        }

        #endregion

        #region 资源加载

        /// <summary>
        /// 获取 Mod 目录路径
        /// </summary>
        private string GetModFolderPath()
        {
            if (info != null && !string.IsNullOrEmpty(info.dllPath))
            {
                return Path.GetDirectoryName(info.dllPath);
            }
            return Path.GetDirectoryName(GetType().Assembly.Location);
        }

        /// <summary>
        /// 加载 AssetBundle
        /// </summary>
        private void LoadAssetBundle()
        {
            string modFolder = GetModFolderPath();
            string bundlePath = Path.Combine(modFolder, "Assets", "my_assets");

            if (!File.Exists(bundlePath))
            {
                Debug.LogWarning($"[MyMod] AssetBundle 不存在: {bundlePath}");
                return;
            }

            assetBundle = AssetBundle.LoadFromFile(bundlePath);
            if (assetBundle != null)
            {
                itemIcon1 = LoadIconFromBundle("Item1Icon");
                itemIcon2 = LoadIconFromBundle("Item2Icon");
                Debug.Log("[MyMod] AssetBundle 加载成功");
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

        #endregion

        #region 本地化

        private void SetupLocalization()
        {
            LocalizationManager.SetOverrideText("Item1_Name", "物品1名称");
            LocalizationManager.SetOverrideText("Item1_Desc", "物品1描述");
            LocalizationManager.SetOverrideText("Item2_Name", "物品2名称");
            LocalizationManager.SetOverrideText("Item2_Desc", "物品2描述");

            LocalizationManager.OnSetLanguage += OnLanguageChanged;
        }

        private void OnLanguageChanged(SystemLanguage language)
        {
            // 根据语言更新文本...
        }

        #endregion

        #region 物品创建

        private void CreateItems()
        {
            itemPrefab1 = CreateItemPrefab("Item1", ITEM1_TYPE_ID,
                "Item1_Name", "Item1_Desc", itemIcon1);
            itemPrefab2 = CreateItemPrefab("Item2", ITEM2_TYPE_ID,
                "Item2_Name", "Item2_Desc", itemIcon2);

            // 注册到字典
            if (itemPrefab1 != null) prefabRegistry[ITEM1_TYPE_ID] = itemPrefab1;
            if (itemPrefab2 != null) prefabRegistry[ITEM2_TYPE_ID] = itemPrefab2;
        }

        private Item CreateItemPrefab(string name, int typeId,
            string nameKey, string descKey, Sprite icon)
        {
            GameObject obj = new GameObject(name);
            DontDestroyOnLoad(obj);
            obj.SetActive(false);

            // 添加 Item 组件
            Item item = obj.AddComponent<Item>();

            // 配置基础属性
            SetFieldValue(item, "typeID", typeId);
            SetFieldValue(item, "displayName", nameKey);
            SetFieldValue(item, "description", descKey);
            if (icon != null) SetFieldValue(item, "icon", icon);
            SetFieldValue(item, "stackable", true);
            SetFieldValue(item, "maxStackCount", 5);
            SetFieldValue(item, "quality", 4);
            SetFieldValue(item, "value", 10000);

            // 添加修复组件
            obj.AddComponent<AgentUtilitiesFixer>();

            return item;
        }

        #endregion

        #region 物品注册

        private void RegisterItems()
        {
            foreach (var kvp in prefabRegistry)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(kvp.Value);
                Debug.Log($"[MyMod] 注册物品 {kvp.Key}: {(success ? "成功" : "失败")}");
            }
        }

        private void RegisterToShop()
        {
            var shopDb = StockShopDatabase.Instance;
            if (shopDb == null) return;

            foreach (var profile in shopDb.merchantProfiles)
            {
                if (profile?.entries == null) continue;

                foreach (var kvp in prefabRegistry)
                {
                    AddItemToMerchant(profile, kvp.Key);
                }
            }
        }

        private void AddItemToMerchant(StockShopDatabase.MerchantProfile profile, int typeId)
        {
            foreach (var entry in profile.entries)
            {
                if (entry.typeID == typeId) return;
            }

            profile.entries.Add(new StockShopDatabase.ItemEntry
            {
                typeID = typeId,
                maxStock = 2,
                priceFactor = 1.0f,
                possibility = 1.0f,
                forceUnlock = true,
                lockInDemo = false
            });
        }

        #endregion

        #region 箱子注入

        private IEnumerator LootBoxInjectionRoutine()
        {
            yield return new WaitForSeconds(2f);

            while (true)
            {
                InjectToLootboxes();
                yield return new WaitForSeconds(5f);
            }
        }

        private void InjectToLootboxes()
        {
            var lootboxes = FindObjectsOfType<InteractableLootbox>();

            foreach (var lootbox in lootboxes)
            {
                if (lootbox == null) continue;

                int id = lootbox.GetInstanceID();
                if (processedLootboxes.Contains(id)) continue;

                try
                {
                    // 概率注入物品
                    if (UnityEngine.Random.value < 0.15f)
                    {
                        // 注入逻辑...
                    }
                }
                catch { }

                processedLootboxes.Add(id);
            }
        }

        #endregion

        #region 背包监控与修复

        private float fixTimer = 0f;
        private const float FIX_INTERVAL = 0.5f;

        private void PeriodicItemFix()
        {
            fixTimer += Time.deltaTime;
            if (fixTimer < FIX_INTERVAL) return;
            fixTimer = 0f;

            var character = CharacterMainControl.Main;
            if (character?.CharacterItem?.Inventory == null) return;

            foreach (Item item in character.CharacterItem.Inventory)
            {
                if (item == null) continue;

                int id = item.GetInstanceID();
                if (fixedItems.Contains(id)) continue;

                if (prefabRegistry.ContainsKey(item.TypeID))
                {
                    FixItemAgentUtilities(item);
                    fixedItems.Add(id);
                }
            }
        }

        private void FixItemAgentUtilities(Item item)
        {
            // AgentUtilities 修复逻辑...
        }

        private IEnumerator InventoryWatchRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);
                // 背包监控逻辑...
            }
        }

        #endregion

        #region 调试功能

        private void AddTestItems()
        {
            var character = CharacterMainControl.Main;
            if (character?.CharacterItem?.Inventory == null)
            {
                ShowMessage("无法找到背包");
                return;
            }

            int count = 0;
            foreach (var kvp in prefabRegistry)
            {
                if (TryAddToInventory(character.CharacterItem.Inventory, kvp.Value))
                {
                    count++;
                }
            }

            ShowMessage($"已添加 {count} 个测试物品");
        }

        private bool TryAddToInventory(Inventory inventory, Item prefab)
        {
            var newItem = prefab.CreateInstance();
            if (newItem == null) return false;

            bool success = inventory.AddItem(newItem);
            if (!success) Destroy(newItem.gameObject);
            return success;
        }

        #endregion

        #region 工具方法

        private void SetFieldValue(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            field?.SetValue(obj, value);
        }

        private void ShowMessage(string msg)
        {
            CharacterMainControl.Main?.PopText(msg);
        }

        private void CheckPendingTasks()
        {
            if (pendingTeleport)
            {
                StartCoroutine(ExecutePendingTeleport());
            }
        }

        private IEnumerator ExecutePendingTeleport()
        {
            yield return null;
            // 传送逻辑...
            pendingTeleport = false;
        }

        #endregion
    }
}
```

---

## 六十、开发检查清单汇总（完整版）

### Mod 项目配置检查

```
□ 项目配置
  □ .NET Standard 2.1 目标框架
  □ 必需 DLL 引用完整
    □ TeamSoda.Duckov.Core.dll
    □ UnityEngine.dll
    □ UnityEngine.CoreModule.dll
    □ Assembly-CSharp.dll
  □ ModBehaviour 继承自 Duckov.Modding.ModBehaviour
  □ info.ini 配置正确

□ AssetBundle（如需）
  □ Unity 2022.3.x 版本匹配
  □ 打包平台正确（StandaloneWindows64）
  □ 资源命名规范
  □ Prefab 组件完整
```

### 物品系统检查

```
□ 基础配置
  □ TypeID 唯一（推荐 990000+）
  □ DisplayName 设置（本地化键）
  □ Description 设置
  □ Icon Sprite 配置
  □ Quality 和 Value 设置
  □ Stackable 和 MaxStackCount（如需）

□ 使用系统（可用物品）
  □ UsageBehavior 组件
  □ DisplaySettings 属性重写
  □ CanBeUsed() 条件检查
  □ OnUse() 使用逻辑
  □ UsageUtilities 组件和配置

□ 技能系统（技能物品）
  □ SkillBase 继承
  □ SkillContext 配置
  □ OnRelease() 实现
  □ ItemSetting_Skill 组件

□ Effect 系统（被动物品）
  □ Effect 组件
  □ EffectTrigger 组件
  □ EffectAction 组件
  □ item.Effects 列表添加

□ 显示系统
  □ ItemAgentUtilities 配置（武器/手持物品）
  □ AgentUtilitiesFixer 组件
  □ Handheld 代理配置
```

### 武器系统检查

```
□ Stats 配置
  □ Damage（伤害）
  □ CritRate（暴击率）
  □ CritDamageFactor（暴击伤害）
  □ ArmorPiercing（护甲穿透）
  □ AttackSpeed（攻击速度）
  □ AttackRange（攻击范围）
  □ StaminaCost（体力消耗）

□ 攻击系统
  □ Stat Hash 缓存
  □ DamageInfo 正确配置
  □ DamageReceiver.Hurt() 调用
  □ Team.IsEnemy() 敌我判断

□ 连击系统（可选）
  □ comboIndex 索引
  □ comboResetTime 超时
  □ 动画触发器

□ 特殊攻击（可选）
  □ 冷却时间检查
  □ 冲刺协程
  □ 投掷物/剑气发射

□ 格挡系统（可选）
  □ DamageReceiver 事件监听
  □ 格挡角度检查
  □ 反射修改无敌状态
```

### 投掷物检查

```
□ 基础属性
  □ damage（伤害）
  □ speed（速度）
  □ maxDistance（最大距离）
  □ delayTime（引爆延迟）

□ 物理系统
  □ Rigidbody 配置
  □ Collider 配置
  □ Launch() 发射方法
  □ 速度计算（抛物线）

□ 碰撞处理
  □ OnCollisionEnter
  □ 音效播放
  □ AI 声音传播

□ 爆炸/效果
  □ Explode() 引爆方法
  □ 范围检测（OverlapSphere）
  □ 特效生成
  □ 销毁逻辑

□ 穿透系统（可选）
  □ pierceCount
  □ 已击中敌人记录
  □ 伤害衰减
```

### 商店/箱子系统检查

```
□ 商店注册
  □ StockShopDatabase.Instance 获取
  □ merchantProfiles 遍历
  □ ItemEntry 创建
  □ typeID / maxStock / priceFactor / possibility

□ 箱子注入
  □ 注入协程启动
  □ InteractableLootbox 查找
  □ 概率控制
  □ 已处理箱子记录（避免重复）
```

### 场景系统检查

```
□ 场景条件
  □ LevelManager.Instance 检查
  □ IsBaseLevel / IsRaidMap 判断
  □ 使用条件限制

□ 撤离系统
  □ EvacuationInfo 构造
  □ NotifyEvacuated() 调用

□ 场景加载
  □ SceneLoader 反射调用
  □ 参数构建正确
  □ 错误处理

□ 跨场景数据
  □ 静态变量/类
  □ 待处理标记
  □ Start() 中检查
```

### 事件与清理检查

```
□ 事件注册
  □ LocalizationManager.OnSetLanguage
  □ Item.onUseStatic
  □ Health.OnHurtEvent
  □ LevelManager.OnLevelInitialized

□ OnDestroy 清理
  □ 所有事件取消监听
  □ 协程停止
  □ 物品从 ItemAssetsCollection 移除
  □ AssetBundle 卸载
  □ 预制体销毁
```

---

*本手册基于游戏反编译代码、真实Mod项目和API文档分析生成*
*版本：5.4 | 新增技能系统深入、物品实例化、Mod目录获取、Stats缓存、连击冲刺、SceneLoader反射、调试快捷键、完整模板*
*适用于《逃离鸭科夫》(Escape from Duckov) Mod开发*
