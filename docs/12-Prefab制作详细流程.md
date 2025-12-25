# 12-Prefab制作详细流程

## 创建基础物品Prefab

### 步骤1：创建GameObject

1. 在 Hierarchy 窗口右键 → **Create Empty**
2. 重命名为你的物品名称（如 `HealingPotion`）

### 步骤2：添加Item组件

1. 选中 GameObject
2. Inspector 窗口点击 **Add Component**
3. 搜索 **Item**（来自 ItemStatsSystem.dll）
4. 添加组件

### 步骤3：配置Item属性

在 Inspector 中设置以下字段：

| 属性 | 说明 | 示例值 |
|------|------|--------|
| Type ID | 唯一ID（990000+） | `990001` |
| Display Name | 本地化键名 | `MyItem_Name` |
| Description | 描述本地化键名 | `MyItem_Desc` |
| Icon | 物品图标Sprite | （拖入图标） |
| Max Stack Count | 最大堆叠数 | `10` |
| Stackable | 是否可堆叠 | `true` |
| Value | 物品价值 | `500` |
| Quality | 品质(0-5) | `2` |
| Unit Self Weight | 物品重量 | `0.2` |
| Max Durability | 最大耐久度 | `100` |
| Use Durability | 是否消耗耐久 | `false` |

---

## 创建消耗品（可使用物品）

### 结构示例

```
HealingPotion (GameObject)
├── Item (Component)
│   ├── TypeID: 990001
│   ├── DisplayName: "Potion_Name"
│   ├── Icon: (Sprite)
│   └── UsageUtilities: (引用下方组件)
│
├── UsageUtilities (Component)
│   ├── UseTime: 2.0
│   ├── UseDurability: false
│   └── Behaviors: [HealingUse]
│
└── HealingUse (自定义UsageBehavior组件)
    ├── HealValue: 50
    └── ConsumeOnUse: true
```

### 配置UsageUtilities

1. 添加 **UsageUtilities** 组件
2. 设置属性：
   - **Use Time**: 使用时间（秒）
   - **Use Durability**: 是否消耗耐久
   - **Durability Usage**: 每次消耗量
3. 在 **Behaviors** 列表中添加你的 UsageBehavior 组件

### 自定义UsageBehavior脚本

```csharp
using UnityEngine;
using ItemStatsSystem;

namespace MyMod
{
    public class HealingUse : UsageBehavior
    {
        [Header("治疗设置")]
        public int healValue = 50;
        public bool consumeOnUse = true;

        public override DisplaySettingsData DisplaySettings => new DisplaySettingsData
        {
            display = true,
            description = $"恢复 {healValue} 点生命值"
        };

        public override bool CanBeUsed(Item item, object user)
        {
            CharacterMainControl character = user as CharacterMainControl;
            if (character == null) return false;

            // 满血时不能使用
            return character.Health.CurrentHealth < character.Health.MaxHealth;
        }

        protected override void OnUse(Item item, object user)
        {
            CharacterMainControl character = user as CharacterMainControl;
            if (character == null) return;

            character.Health.AddHealth(healValue);
            character.PopText($"+{healValue} HP");

            Debug.Log($"[Mod] 恢复了 {healValue} 点生命值");
        }
    }
}
```

---

## 创建被动装备（Effect系统）

### 结构示例

```
HealthBadge (GameObject)
├── Item (Component)
│   ├── TypeID: 990002
│   ├── DisplayName: "Badge_Name"
│   └── Effects: [Effect_Regen]
│
└── Effect_Regen (子GameObject)
    ├── Effect (Component)
    │   ├── Display: true
    │   ├── Description: "每5秒恢复5点生命"
    │   ├── Triggers: [TickTrigger]
    │   └── Actions: [HealAction]
    │
    ├── TickTrigger (Component)
    │   └── Period: 5.0
    │
    └── HealAction (Component)
        └── HealValue: 5
```

### 配置步骤

1. 创建 Item GameObject
2. 创建子对象 `Effect_Regen`
3. 在子对象上添加：
   - **Effect** 组件
   - **TickTrigger** 组件（周期触发）
   - **HealAction** 组件（治疗动作）
4. 在 Effect 组件中：
   - 将 TickTrigger 添加到 Triggers 列表
   - 将 HealAction 添加到 Actions 列表
5. 在 Item 组件中：
   - 将 Effect 添加到 Effects 列表

---

## 创建武器Prefab

### 结构示例

```
MoonlightSword (GameObject)
├── Item (Component)
│   ├── TypeID: 990003
│   ├── DisplayName: "Sword_Name"
│   ├── Stats: (配置伤害等属性)
│   └── AgentUtilities: (引用)
│
├── ItemAgentUtilities (Component)
│   └── Agents: [{key:"Handheld", prefab:HandheldModel}]
│
├── MeleeWeaponStats (自定义组件)
│   ├── Damage: 50
│   ├── AttackRange: 2.5
│   └── CritRate: 0.15
│
└── HandheldModel (子GameObject - 手持模型)
    ├── MeshFilter
    ├── MeshRenderer
    └── (碰撞体等)
```

### 配置Stats

在 Item 组件的 Stats 部分配置武器属性：

| Stat名称 | 说明 | 示例值 |
|----------|------|--------|
| Damage | 基础伤害 | `50` |
| CritRate | 暴击率 | `0.15` |
| CritDamageFactor | 暴击倍率 | `2.0` |
| AttackRange | 攻击范围 | `2.5` |
| AttackSpeed | 攻击速度 | `1.0` |
| StaminaCost | 体力消耗 | `10` |

### 配置ItemAgentUtilities

1. 添加 **ItemAgentUtilities** 组件
2. 在 Agents 列表中添加：
   - Key: `Handheld`
   - Agent Prefab: 手持模型的预制体

---

## 添加视觉模型

### 方式一：简单几何体

```
MyItem (GameObject)
├── Item (Component)
└── Model (子GameObject)
    ├── MeshFilter (选择 Cube/Sphere/Capsule)
    ├── MeshRenderer
    └── Material (创建材质并赋予颜色)
```

### 方式二：导入3D模型

1. 将模型文件（.fbx, .obj）拖入 Assets 目录
2. 在 Import Settings 中调整：
   - Scale Factor: 根据需要调整
   - Generate Colliders: 如果需要碰撞
3. 将模型拖入 Prefab 作为子对象
4. 调整 Transform（位置、旋转、缩放）

### 方式三：代码生成视觉

```csharp
private void CreateVisualModel(GameObject parent)
{
    // 创建主体
    GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    body.name = "Body";
    body.transform.SetParent(parent.transform);
    body.transform.localPosition = Vector3.zero;
    body.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

    // 设置材质
    Material mat = new Material(Shader.Find("Standard"));
    mat.color = Color.blue;
    body.GetComponent<Renderer>().material = mat;

    // 移除碰撞体（如果不需要）
    Object.Destroy(body.GetComponent<Collider>());
}
```

---

## 制作图标

### 图标规格

- **尺寸**: 128x128 或 256x256 像素
- **格式**: PNG（支持透明）
- **背景**: 透明或与游戏UI协调

### 导入设置

1. 选择图标文件
2. 在 Inspector 中设置：
   - Texture Type: **Sprite (2D and UI)**
   - Sprite Mode: **Single**
   - Pixels Per Unit: **100**
   - Filter Mode: **Bilinear**
3. 点击 **Apply**

### 在Prefab中使用

1. 选择 Item 组件
2. 将 Sprite 拖入 **Icon** 字段

---

## 保存Prefab

### 方法一：拖拽创建

1. 在 Hierarchy 中配置好物品
2. 将物品拖入 `Assets/Prefabs/` 目录
3. 选择 **Original Prefab**

### 方法二：右键创建

1. 选中配置好的物品
2. 右键 → **Prefab → Create Prefab**
3. 选择保存位置

### 设置AssetBundle

1. 选择保存的 Prefab
2. Inspector 底部 → AssetBundle
3. 选择或创建 AssetBundle 名称

---

## 验证Prefab

### 检查清单

```
□ Item组件已添加
□ TypeID已设置且唯一
□ DisplayName和Description已设置
□ Icon已设置（如果需要）
□ UsageUtilities已配置（如果是消耗品）
□ UsageBehavior已添加到Behaviors列表
□ ItemAgentUtilities已配置（如果是武器）
□ Stats已配置（如果有属性）
□ AssetBundle名称已设置
```

### 测试方法

在Unity中运行Play模式，使用以下脚本测试：

```csharp
// Assets/Editor/PrefabValidator.cs
using UnityEditor;
using UnityEngine;
using ItemStatsSystem;

public class PrefabValidator
{
    [MenuItem("Tools/Validate Item Prefab")]
    public static void ValidatePrefab()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogError("请先选择一个物品Prefab");
            return;
        }

        Item item = selected.GetComponent<Item>();
        if (item == null)
        {
            Debug.LogError("选中的对象没有Item组件");
            return;
        }

        Debug.Log($"=== 物品验证 ===");
        Debug.Log($"名称: {selected.name}");
        Debug.Log($"TypeID: {item.TypeID}");
        Debug.Log($"DisplayName: {item.DisplayName}");
        Debug.Log($"Icon: {(item.Icon != null ? "已设置" : "未设置")}");
        Debug.Log($"UsageUtilities: {(item.UsageUtilities != null ? "已配置" : "未配置")}");
        Debug.Log($"AgentUtilities: {(item.AgentUtilities != null ? "已配置" : "未配置")}");
        Debug.Log($"Effects数量: {item.Effects?.Count ?? 0}");
    }
}
```

---

## 常见问题

### 组件找不到

**原因**：DLL未正确导入

**解决**：检查 Assets/Plugins/ 目录

### Prefab打包后组件丢失

**原因**：Unity版本或DLL版本不匹配

**解决**：确保Unity项目和游戏使用相同版本

### 图标不显示

**原因**：Texture Type不正确

**解决**：设置为 Sprite (2D and UI)

---

## 下一步

Prefab制作完成后：
- 参考 [11-AssetBundle打包指南](11-AssetBundle打包指南.md) 打包资源
- 参考 [13-端到端完整教程](13-端到端完整教程.md) 完成整个开发流程
