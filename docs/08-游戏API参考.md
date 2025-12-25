# 08-游戏API参考

## 管理器实例获取

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

---

## 常用角色访问

| 对象 | 获取方式 |
|------|----------|
| 主角 | `LevelManager.Instance.MainCharacter` |
| 主角生命值 | `LevelManager.Instance.MainCharacter.Health` |
| 主角Buff管理器 | `character.GetComponent<CharacterBuffManager>()` |
| 宠物 | `LevelManager.Instance.PetCharacter` |
| 游戏相机 | `LevelManager.Instance.GameCamera` |

---

## Health 生命值系统

### 属性

```csharp
public float MaxHealth;              // 最大生命值
public float CurrentHealth;          // 当前生命值
public bool IsDead;                  // 是否死亡
public bool Invincible;              // 是否无敌
public float BodyArmor;              // 身体护甲
public float HeadArmor;              // 头部护甲
```

### 事件

```csharp
public UnityEvent<DamageInfo> OnHurtEvent;     // 受伤
public UnityEvent<DamageInfo> OnDeadEvent;     // 死亡
public UnityEvent<Health> OnHealthChange;      // 血量变化
public UnityEvent<Health> OnMaxHealthChange;   // 最大血量变化
```

### 方法

```csharp
health.AddHealth(50f);              // 恢复生命值
health.SetHealth(100f);             // 设置生命值
health.SetInvincible(true);         // 设置无敌
health.Hurt(damageInfo);            // 造成伤害
```

---

## Buff 系统

### Buff 类属性

```csharp
public class Buff : MonoBehaviour
{
    public int ID;                      // Buff唯一ID
    public int maxLayers;               // 最大层数
    public BuffExclusiveTags ExclusiveTag;  // 互斥标签
    public bool limitedLifeTime;        // 是否限时
    public float totalLifeTime;         // 总持续时间
    public List<Effect> effects;        // 附带的效果
}
```

### Buff 生命周期方法

```csharp
protected virtual void OnSetup() { }           // Buff添加时
protected virtual void OnUpdate() { }          // 每帧更新
protected virtual void OnNotifiedOutOfTime() { }  // 时间到期时
```

### CharacterBuffManager 事件

```csharp
public event Action<CharacterBuffManager, Buff> onAddBuff;     // 添加Buff
public event Action<CharacterBuffManager, Buff> onRemoveBuff;  // 移除Buff
```

### 添加 Buff

```csharp
// 获取角色的 Buff 管理器
var buffManager = character.GetComponent<CharacterBuffManager>();

// 添加 Buff
character.AddBuff(buffPrefab, sourceCharacter, stackCount);

// 监听 Buff 变化
buffManager.onAddBuff += (manager, buff) => {
    Debug.Log($"获得Buff: {buff.DisplayName}");
};

buffManager.onRemoveBuff += (manager, buff) => {
    Debug.Log($"失去Buff: {buff.DisplayName}");
};
```

---

## 经济系统

### EconomyManager

```csharp
// 货币操作
EconomyManager.AddCurrency(1000);               // 添加货币
EconomyManager.RemoveCurrency(500);             // 移除货币
int money = EconomyManager.GetCurrency();       // 获取当前货币

// 物品解锁
EconomyManager.UnlockItem("Weapon_AK47");       // 解锁物品
bool unlocked = EconomyManager.IsItemUnlocked("Weapon_AK47");
List<string> items = EconomyManager.GetUnlockedItems();

// 事件
public static event Action OnEconomyManagerLoaded;
public static event Action<string> OnItemUnlocked;
public static event Action<int> OnCurrencyChanged;
```

---

## 制作系统

### CraftingManager

```csharp
// 配方管理
CraftingManager.UnlockFormula("Weapon_AK47");
bool unlocked = CraftingManager.IsFormulaUnlocked("Weapon_AK47");
CraftingFormula formula = CraftingManager.GetFormula("Weapon_AK47");

// 制作物品
List<Item> items = await CraftingManager.Instance.Craft("Weapon_AK47");

// 事件
public static event Action<Item> OnItemCrafted;
public static event Action<string> OnFormulaUnlocked;
```

### CraftingFormula

```csharp
public struct CraftingFormula
{
    public string id;                  // 配方ID
    public string result;              // 制作结果
    public string[] tags;              // 标签
    public CraftingCost[] cost;        // 制作成本
    public bool unlockByDefault;       // 默认解锁
    public string requirePerk;         // 需要技能点
}
```

---

## 任务系统

### QuestManager

```csharp
// 任务管理
QuestManager.ActivateQuest("MainQuest_01");
QuestManager.CompleteQuest("MainQuest_01");

bool active = QuestManager.IsQuestActive("MainQuest_01");
bool completed = QuestManager.IsQuestCompleted("MainQuest_01");

List<string> activeQuests = QuestManager.GetActiveQuests();
List<string> completedQuests = QuestManager.GetCompletedQuests();

// 事件
public static event Action<string> OnQuestActivated;
public static event Action<string> OnQuestCompleted;
public static event Action<string> OnTaskCompleted;
```

---

## 存档系统

### SavesSystem

```csharp
// 属性
SavesSystem.CurrentSlot              // 当前存档槽
SavesSystem.CurrentFilePath          // 当前存档文件路径
SavesSystem.SavesFolder              // 存档文件夹路径

// 保存和加载
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
SavesSystem.SaveToFile();
SavesSystem.LoadFromFile();

// 事件
public static event Action OnCollectSaveData;
public static event Action OnLoadSaveData;
```

---

## 音频系统

### AudioManager

```csharp
// 播放声音
AudioManager.PlaySound("GunShot");
AudioManager.PlaySound("Explosion", position);  // 3D声音
AudioManager.StopSound("GunShot");

// 设置角色声音类型
AudioManager.SetVoiceType(character.gameObject, AudioManager.VoiceType.Male);

// 设置脚步声材质
AudioManager.SetFootStepMaterialType(character.gameObject,
    AudioManager.FootStepMaterialType.Concrete);
```

### FootStepMaterialType

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

## 成就系统

### AchievementManager

```csharp
// 成就管理
AchievementManager.UnlockAchievement("Kill_100_Enemies");
bool unlocked = AchievementManager.IsAchievementUnlocked("Kill_100_Enemies");
float progress = AchievementManager.GetAchievementProgress("Kill_100_Enemies");

// 事件
public static event Action<string> OnAchievementUnlocked;
public static event Action OnAchievementDataLoaded;
```

### StatisticsManager

```csharp
StatisticsManager.IncrementStat("EnemiesKilled", 1);
StatisticsManager.SetStat("PlayerLevel", 5);
int kills = StatisticsManager.GetStat("EnemiesKilled");
StatisticsManager.ResetAllStats();
```

---

## 难度系统

### GameRulesManager

```csharp
GameRulesManager.Instance              // 获取实例
GameRulesManager.Current               // 当前规则集
GameRulesManager.SelectedRuleIndex     // 选中的规则索引

public static event Action OnRuleChanged;  // 规则改变事件
```

### RuleIndex 枚举

```csharp
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

### Ruleset 属性

```csharp
Ruleset rules = GameRulesManager.Current;

rules.DisplayName                    // 显示名称
rules.Description                    // 描述
rules.SpawnDeadBody                  // 是否生成尸体
rules.FogOfWar                       // 战争迷雾
rules.RecoilMultiplier               // 后坐力倍数
rules.DamageFactor_ToPlayer          // 对玩家伤害倍数
rules.EnemyHealthFactor              // 敌人生命值倍数
rules.EnemyReactionTimeFactor        // 敌人反应时间
rules.EnemyAttackTimeFactor          // 敌人攻击时间
```

---

## 完整事件列表

| 系统 | 事件名 | 说明 |
|------|--------|------|
| **角色系统** | CharacterMainControl.OnMainCharacterStartUseItem | 主角使用物品 |
| | CharacterMainControl.OnMainCharacterInventoryChangedEvent | 背包改变 |
| | CharacterMainControl.OnMainCharacterSlotContentChangedEvent | 插槽内容改变 |
| | character.OnShootEvent | 射击事件 |
| | character.OnAttackEvent | 近战攻击事件 |
| | character.OnTeamChanged | 阵营改变 |
| **生命值** | health.OnHurtEvent | 受伤事件 |
| | health.OnDeadEvent | 死亡事件 |
| | health.OnHealthChange | 血量变化 |
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

---

## 物品系统快速操作

| 操作 | 代码 |
|------|------|
| 注册物品 | `ItemAssetsCollection.AddDynamicEntry(prefab)` |
| 移除物品 | `ItemAssetsCollection.RemoveDynamicEntry(prefab)` |
| 实例化物品 | `await ItemAssetsCollection.InstantiateAsync(typeID)` |
| 同步实例化 | `ItemAssetsCollection.InstantiateSync(typeID)` |
| 发送给玩家 | `ItemUtilities.SendToPlayer(item)` |

---

## AI 系统简介

### AI 行为组件

```csharp
// AI 使用物品
public class UseDrug : AIBehavior
{
    // AI 检查是否可以使用药品
    public override bool CanExecute();

    // AI 执行使用药品
    public override void Execute();
}
```

### AI 状态

```csharp
public enum AIState
{
    Idle,           // 空闲
    Patrol,         // 巡逻
    Chase,          // 追击
    Attack,         // 攻击
    Flee,           // 逃跑
    Search,         // 搜索
    TakeCover       // 掩护
}
```

---

## 枪械系统 (ItemAgent_Gun)

### 重要属性

```csharp
public float FireRate;               // 射速
public int MagazineCapacity;         // 弹匣容量
public int CurrentAmmo;              // 当前弹药
public float ReloadTime;             // 换弹时间
public float RecoilMultiplier;       // 后坐力倍数
public bool IsAutomatic;             // 是否自动
```

### 重要方法

```csharp
gun.Shoot();                         // 射击
gun.Reload();                        // 换弹
gun.SetFireMode(FireMode.Auto);      // 设置开火模式
```

---

## 日志位置

- **Windows**: `%USERPROFILE%\AppData\LocalLow\TeamSoda\Escape from Duckov\Player.log`
- **macOS**: `~/Library/Logs/TeamSoda/Escape from Duckov/Player.log`

---

## 版本检查

```csharp
// 获取游戏版本
string gameVersion = Application.version;
Debug.Log($"Game Version: {gameVersion}");

// 获取 Unity 版本
string unityVersion = Application.unityVersion;
Debug.Log($"Unity Version: {unityVersion}");
```
