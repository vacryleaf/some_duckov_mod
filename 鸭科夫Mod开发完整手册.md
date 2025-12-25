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

### 第四部分：游戏内部实现参考
24. [游戏内部实现参考](#二十四游戏内部实现参考)
    - [24.1 生命值与伤害系统 (Health.cs)](#241-生命值与伤害系统-healthcs)
    - [24.2 药物使用行为 (Drug.cs)](#242-药物使用行为-drugcs)
    - [24.3 Buff系统核心 (Buff.cs)](#243-buff系统核心-buffcs)
    - [24.4 添加Buff使用行为 (AddBuff.cs)](#244-添加buff使用行为-addbuffcs)
    - [24.5 商店系统 (StockShop.cs)](#245-商店系统-stockshopcs)
    - [24.6 制作系统 (CraftingManager.cs)](#246-制作系统-craftingmanagercs)
    - [24.7 枪械武器系统 (ItemAgent_Gun.cs)](#247-枪械武器系统-itemagent_guncs)
    - [24.8 近战武器系统 (ItemAgent_MeleeWeapon.cs)](#248-近战武器系统-itemagent_meleeweaponcs)
    - [24.9 AI药物使用行为 (UseDrug.cs)](#249-ai药物使用行为-usedrugcs)
25. [总结](#二十五总结)

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

## 二十四、游戏内部实现参考

> 以下是游戏反编译代码中的真实实现，供Mod开发者参考

### 24.1 生命值与伤害系统 (Health.cs)

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

### 24.2 药物使用行为 (Drug.cs)

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

### 24.3 Buff系统核心 (Buff.cs)

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

### 24.4 添加Buff使用行为 (AddBuff.cs)

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

### 24.5 商店系统 (StockShop.cs)

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

### 24.6 制作系统 (CraftingManager.cs)

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

### 24.7 枪械武器系统 (ItemAgent_Gun.cs)

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

### 24.8 近战武器系统 (ItemAgent_MeleeWeapon.cs)

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

### 24.9 AI药物使用行为 (UseDrug.cs)

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

## 二十五、总结

本手册涵盖了《逃离鸭科夫》Mod开发的所有核心内容：

1. **物品系统** - 创建自定义物品、使用行为、效果系统
2. **战斗系统** - 伤害计算、护甲、暴击、元素效果
3. **经济系统** - 货币、商店、交易
4. **制作系统** - 配方、解锁、制作流程
5. **Buff系统** - 状态效果、叠加、互斥
6. **武器系统** - 枪械、近战、伤害处理
7. **事件系统** - 所有可用事件的订阅与处理
8. **游戏实现参考** - 真实游戏代码示例

开发者可以参考游戏内部实现代码来理解各系统的工作原理，并据此创建自己的Mod。

---

*本手册基于游戏反编译代码和API文档分析生成，适用于《逃离鸭科夫》Mod开发*
*版本：3.0 | 包含完整游戏系统文档和内部实现参考*
