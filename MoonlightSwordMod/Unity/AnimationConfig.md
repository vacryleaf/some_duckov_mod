# 名刀月影 - 动画配置文档

## 📋 概述

本文档定义名刀月影的所有动画状态和配置,包括普通攻击动作和特殊攻击动作。

---

## 🎬 动画状态机结构

### Animator Controller: MoonlightSwordAnimator

```
Base Layer
├── Idle (待机)
├── ForwardSlash (正手挥击)
├── BackhandSlash (反手挥击)
├── SpecialAttack (特殊攻击)
│   ├── Charge (蓄力)
│   ├── Dash (冲刺)
│   └── Release (释放剑气)
└── Sheathe (收刀)
```

---

## ⚔️ 普通攻击动作配置

### 1. 正手挥击 (ForwardSlash)

**动画描述**: 从右上至左下的斜劈动作

#### 动画参数
```yaml
名称: ForwardSlash
类型: Trigger
持续时间: 0.6秒
可打断时间: 0.4秒后
```

#### 关键帧时间轴
```
0.00s - 起始姿势(刀举至右肩上方)
0.15s - 开始挥动
0.30s - 伤害判定点(刀刃划过前方)
0.45s - 挥击结束(刀停在左腰侧)
0.60s - 恢复姿势
```

#### 动作细节
```
起始位置:
- 刀举至右肩上方,角度约45度
- 身体微微后倾,蓄力
- 双手握柄

挥击轨迹:
- 从右上至左下,角度约120度
- 身体随刀旋转,重心前移
- 刀刃划过扇形区域(前方3米,120度)

结束位置:
- 刀停在身体左侧腰部位置
- 身体前倾,呈攻击完成姿态
```

#### Animator配置
```yaml
State: ForwardSlash
Motion: forward_slash.anim
Speed: 1.0
TransitionsFrom:
  - Idle
  - BackhandSlash (连击)
TransitionsTo:
  - BackhandSlash (连击续接)
  - Idle (恢复)

Conditions:
  - Trigger: Attack
  - Bool: IsGrounded = true
  - Float: ComboIndex = 0
```

#### 动画事件
```csharp
// 在0.30秒触发伤害判定
AnimationEvent damageEvent = new AnimationEvent();
damageEvent.time = 0.3f;
damageEvent.functionName = "OnAttackDamageFrame";
damageEvent.stringParameter = "ForwardSlash";

// 在0.15秒播放音效
AnimationEvent soundEvent = new AnimationEvent();
soundEvent.time = 0.15f;
soundEvent.functionName = "PlaySlashSound";
soundEvent.intParameter = 1; // 音效ID

// 在0.30秒显示特效
AnimationEvent effectEvent = new AnimationEvent();
effectEvent.time = 0.3f;
effectEvent.functionName = "ShowSlashEffect";
effectEvent.stringParameter = "ForwardTrail";
```

---

### 2. 反手挥击 (BackhandSlash)

**动画描述**: 从左上至右下的回劈动作

#### 动画参数
```yaml
名称: BackhandSlash
类型: Trigger
持续时间: 0.6秒
可打断时间: 0.4秒后
```

#### 关键帧时间轴
```
0.00s - 起始姿势(承接正手挥击结束位置)
0.15s - 开始挥动
0.30s - 伤害判定点(刀刃划过前方)
0.45s - 挥击结束(刀停在右腰侧)
0.60s - 恢复姿势
```

#### 动作细节
```
起始位置:
- 刀在身体左侧腰部
- 借用正手挥击的惯性
- 迅速调整握姿

挥击轨迹:
- 从左上至右下,角度约120度
- 反向旋转,呈回旋之势
- 刀刃再次划过前方扇形区域

结束位置:
- 刀停在身体右侧
- 完成一个完整的连击循环
```

#### Animator配置
```yaml
State: BackhandSlash
Motion: backhand_slash.anim
Speed: 1.0
TransitionsFrom:
  - ForwardSlash (连击)
  - Idle
TransitionsTo:
  - ForwardSlash (连击重置)
  - Idle (恢复)

Conditions:
  - Trigger: Attack
  - Bool: IsGrounded = true
  - Float: ComboIndex = 1
```

#### 动画事件
```csharp
// 伤害判定
AnimationEvent damageEvent = new AnimationEvent();
damageEvent.time = 0.3f;
damageEvent.functionName = "OnAttackDamageFrame";
damageEvent.stringParameter = "BackhandSlash";

// 音效
AnimationEvent soundEvent = new AnimationEvent();
soundEvent.time = 0.15f;
soundEvent.functionName = "PlaySlashSound";
soundEvent.intParameter = 2;

// 特效
AnimationEvent effectEvent = new AnimationEvent();
effectEvent.time = 0.3f;
effectEvent.functionName = "ShowSlashEffect";
effectEvent.stringParameter = "BackhandTrail";
```

---

## 🌙 特殊攻击动作配置

### 3. 月影剑气 (SpecialAttack)

**动画描述**: 瞄准后的特殊攻击,分为三个阶段

#### 整体动画参数
```yaml
名称: SpecialAttack
类型: Trigger
总持续时间: 1.2秒
不可打断
```

### 阶段1: 蓄力 (Charge)

#### 时间轴: 0.00s - 0.30s

```
动作描述:
- 角色稳定站立,双手紧握刀柄
- 刀横于胸前,刀刃向前
- 身体微微下蹲,重心下沉
- 刀身开始发出蓝色光芒
```

#### 特效
```csharp
// 0.10秒开始充能特效
AnimationEvent chargeStart = new AnimationEvent();
chargeStart.time = 0.1f;
chargeStart.functionName = "StartChargeEffect";
// 显示刀身周围聚集能量的粒子效果
```

### 阶段2: 冲刺 (Dash)

#### 时间轴: 0.30s - 0.60s

```
动作描述:
- 角色快速向前冲刺3米
- 刀保持在胸前准备姿势
- 身体前倾,呈突进姿态
- 地面留下蓝色轨迹特效
```

#### 实现细节
```csharp
// 0.30秒触发冲刺
AnimationEvent dashEvent = new AnimationEvent();
dashEvent.time = 0.3f;
dashEvent.functionName = "StartDashMovement";
dashEvent.floatParameter = 3f; // 冲刺距离

// 移动代码示例
IEnumerator DashMovement(float distance)
{
    Vector3 startPos = transform.position;
    Vector3 endPos = startPos + transform.forward * distance;
    float duration = 0.3f;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
        elapsed += Time.deltaTime;
        yield return null;
    }
}
```

### 阶段3: 释放剑气 (Release)

#### 时间轴: 0.60s - 1.20s

```
动作描述:
- 角色停止冲刺,站定身体
- 双手向前挥刀,大幅度斩击
- 刀刃划出,释放月牙形剑气
- 剑气向前飞行,角色保持挥击姿势
```

#### 关键时间点
```
0.60s - 开始挥刀
0.75s - 释放剑气(生成投射物)
0.90s - 挥击结束
1.20s - 恢复姿势
```

#### 动画事件
```csharp
// 0.75秒释放剑气
AnimationEvent releaseEvent = new AnimationEvent();
releaseEvent.time = 0.75f;
releaseEvent.functionName = "LaunchSwordAura";
// 生成剑气投射物,向前飞行10米

// 播放释放音效
AnimationEvent soundEvent = new AnimationEvent();
soundEvent.time = 0.75f;
soundEvent.functionName = "PlaySpecialSound";
soundEvent.stringParameter = "AuraRelease";

// 相机震动
AnimationEvent shakeEvent = new AnimationEvent();
shakeEvent.time = 0.75f;
shakeEvent.functionName = "CameraShake";
shakeEvent.floatParameter = 0.3f; // 震动强度
```

#### Animator配置
```yaml
State: SpecialAttack
Motion: special_attack.anim
Speed: 1.0
TransitionsFrom:
  - Idle (瞄准状态)
TransitionsTo:
  - Idle (完成后恢复)

Conditions:
  - Trigger: SpecialAttack
  - Bool: IsGrounded = true
  - Bool: IsAiming = true
  - Float: SpecialCooldown = 0
```

---

## 🎯 Animator Controller完整配置

### Parameters (参数)

```yaml
Parameters:
  # Triggers (触发器)
  - Attack: Trigger # 普通攻击
  - SpecialAttack: Trigger # 特殊攻击

  # Bools (布尔值)
  - IsGrounded: Bool, Default: true # 是否在地面
  - IsAiming: Bool, Default: false # 是否瞄准
  - IsAttacking: Bool, Default: false # 是否正在攻击

  # Floats (浮点数)
  - ComboIndex: Float, Default: 0 # 连击索引(0或1)
  - MoveSpeed: Float, Default: 0 # 移动速度
  - AttackSpeed: Float, Default: 1.2 # 攻击速度
  - SpecialCooldown: Float, Default: 0 # 特殊攻击冷却

  # Ints (整数)
  - WeaponState: Int, Default: 1 # 武器状态(1:持刀,0:收刀)
```

### State Transitions (状态转换)

```yaml
# 待机 → 正手挥击
Idle → ForwardSlash:
  Conditions:
    - Attack (trigger)
    - ComboIndex = 0
    - IsGrounded = true
  Settings:
    HasExitTime: false
    TransitionDuration: 0.1s

# 正手挥击 → 反手挥击 (连击)
ForwardSlash → BackhandSlash:
  Conditions:
    - Attack (trigger)
    - ComboIndex = 1
  Settings:
    HasExitTime: true
    ExitTime: 0.7 (70%动画完成)
    TransitionDuration: 0.1s

# 反手挥击 → 正手挥击 (连击重置)
BackhandSlash → ForwardSlash:
  Conditions:
    - Attack (trigger)
    - ComboIndex = 0
  Settings:
    HasExitTime: true
    ExitTime: 0.7
    TransitionDuration: 0.1s

# 任意状态 → 待机 (恢复)
Any State → Idle:
  Conditions:
    - IsAttacking = false
  Settings:
    HasExitTime: true
    ExitTime: 0.9
    TransitionDuration: 0.2s

# 待机 → 特殊攻击
Idle → SpecialAttack:
  Conditions:
    - SpecialAttack (trigger)
    - IsAiming = true
    - SpecialCooldown = 0
  Settings:
    HasExitTime: false
    TransitionDuration: 0.1s

# 特殊攻击 → 待机
SpecialAttack → Idle:
  Conditions:
    - None (automatic)
  Settings:
    HasExitTime: true
    ExitTime: 1.0 (完全结束)
    TransitionDuration: 0.2s
```

---

## 🎨 动画曲线 (Animation Curves)

### 攻击力度曲线

用于控制攻击动画的速度变化,创造更真实的打击感。

```csharp
// ForwardSlash力度曲线
AnimationCurve forwardSlashCurve = new AnimationCurve(
    new Keyframe(0f, 0f),      // 起始缓慢
    new Keyframe(0.25f, 0.3f), // 加速
    new Keyframe(0.5f, 1f),    // 最快点(伤害判定)
    new Keyframe(0.75f, 0.6f), // 减速
    new Keyframe(1f, 0f)       // 停止
);

// BackhandSlash力度曲线(类似但稍快)
AnimationCurve backhandSlashCurve = new AnimationCurve(
    new Keyframe(0f, 0.2f),    // 借用前一次惯性
    new Keyframe(0.3f, 1f),    // 快速到达最快点
    new Keyframe(0.7f, 0.5f),  // 减速
    new Keyframe(1f, 0f)       // 停止
);

// SpecialAttack冲刺曲线
AnimationCurve dashCurve = new AnimationCurve(
    new Keyframe(0f, 0f),      // 静止
    new Keyframe(0.1f, 2f),    // 爆发加速
    new Keyframe(0.8f, 1.5f),  // 保持高速
    new Keyframe(1f, 0f)       // 急停
);
```

---

## 🔊 音效配置

### 普通攻击音效

```yaml
ForwardSlash:
  - 挥刀音效: "swoosh_heavy_01.wav"
    时间: 0.15s
    音量: 0.8
    音调: 1.0

  - 破空音效: "air_cut_01.wav"
    时间: 0.30s
    音量: 0.6
    音调: 1.2

BackhandSlash:
  - 挥刀音效: "swoosh_heavy_02.wav"
    时间: 0.15s
    音量: 0.8
    音调: 1.1

  - 破空音效: "air_cut_02.wav"
    时间: 0.30s
    音量: 0.6
    音调: 1.3
```

### 特殊攻击音效

```yaml
SpecialAttack:
  Charge阶段:
    - 充能音效: "power_charge.wav"
      时间: 0.10s
      音量: 0.7
      循环: true
      停止时间: 0.30s

  Dash阶段:
    - 冲刺音效: "dash_whoosh.wav"
      时间: 0.30s
      音量: 0.9
      音调: 0.9

  Release阶段:
    - 释放音效: "aura_release.wav"
      时间: 0.75s
      音量: 1.0
      音调: 1.0

    - 剑气飞行音效: "aura_fly.wav"
      时间: 0.80s
      音量: 0.8
      循环: true (由剑气投射物控制)
```

---

## ✨ 视觉特效配置

### 挥击轨迹特效

```yaml
ForwardSlash轨迹:
  TrailRenderer:
    时间: 0.3秒
    起始宽度: 0.08米
    结束宽度: 0.02米
    起始颜色: RGBA(160, 200, 232, 200)
    结束颜色: RGBA(255, 255, 255, 0)
    材质: "trail_blade_material"

BackhandSlash轨迹:
  TrailRenderer:
    时间: 0.3秒
    起始宽度: 0.08米
    结束宽度: 0.02米
    起始颜色: RGBA(160, 200, 232, 200)
    结束颜色: RGBA(255, 255, 255, 0)
    材质: "trail_blade_material"
```

### 特殊攻击特效

```yaml
充能特效:
  ParticleSystem:
    发射速率: 20/秒
    粒子生命: 1秒
    起始大小: 0.05米
    起始颜色: RGBA(112, 176, 224, 128)
    形状: 刀身周围螺旋

冲刺轨迹:
  TrailRenderer:
    时间: 0.5秒
    起始宽度: 0.5米
    结束宽度: 0.1米
    起始颜色: RGBA(112, 176, 224, 150)
    结束颜色: RGBA(112, 176, 224, 0)

剑气释放闪光:
  时间: 0.75秒
  持续: 0.2秒
  闪光强度: 2.0
  颜色: RGB(200, 230, 255)
```

---

## 🎮 实现代码示例

### AnimationEventHandler.cs

```csharp
using UnityEngine;

/// <summary>
/// 处理动画事件的脚本
/// 挂载在角色对象上
/// </summary>
public class MoonlightSwordAnimationHandler : MonoBehaviour
{
    [Header("音效")]
    public AudioClip swooshSound;
    public AudioClip airCutSound;
    public AudioClip chargeSound;
    public AudioClip dashSound;
    public AudioClip releaseSound;

    [Header("特效")]
    public GameObject slashTrailPrefab;
    public GameObject chargeEffectPrefab;
    public GameObject dashTrailPrefab;

    [Header("引用")]
    private MoonlightSwordAttack attackScript;
    private AudioSource audioSource;

    void Start()
    {
        attackScript = GetComponent<MoonlightSwordAttack>();
        audioSource = GetComponent<AudioSource>();
    }

    // 普通攻击伤害判定
    public void OnAttackDamageFrame(string attackType)
    {
        Debug.Log($"触发伤害判定: {attackType}");
        attackScript?.PerformMeleeDamage(attackScript.normalDamage);
    }

    // 播放挥刀音效
    public void PlaySlashSound(int soundId)
    {
        if (audioSource != null && swooshSound != null)
        {
            audioSource.PlayOneShot(swooshSound, 0.8f);
        }
    }

    // 显示挥击特效
    public void ShowSlashEffect(string effectType)
    {
        // 在刀身位置生成轨迹特效
        // 实际实现依赖TrailRenderer或粒子系统
        Debug.Log($"显示特效: {effectType}");
    }

    // 开始充能特效
    public void StartChargeEffect()
    {
        if (chargeEffectPrefab != null)
        {
            GameObject effect = Instantiate(chargeEffectPrefab, transform.position, Quaternion.identity, transform);
            Destroy(effect, 0.3f); // 充能阶段持续0.3秒
        }

        if (audioSource != null && chargeSound != null)
        {
            audioSource.PlayOneShot(chargeSound, 0.7f);
        }
    }

    // 开始冲刺移动
    public void StartDashMovement(float distance)
    {
        attackScript?.StartCoroutine(attackScript.DashMovement(distance));

        if (audioSource != null && dashSound != null)
        {
            audioSource.PlayOneShot(dashSound, 0.9f);
        }
    }

    // 发射剑气
    public void LaunchSwordAura()
    {
        attackScript?.LaunchSwordAura();

        if (audioSource != null && releaseSound != null)
        {
            audioSource.PlayOneShot(releaseSound, 1.0f);
        }
    }

    // 播放特殊音效
    public void PlaySpecialSound(string soundType)
    {
        Debug.Log($"播放音效: {soundType}");
    }

    // 相机震动
    public void CameraShake(float intensity)
    {
        // 触发相机震动效果
        Camera.main.GetComponent<CameraShake>()?.Shake(intensity, 0.2f);
    }
}
```

### 动画控制脚本

```csharp
using UnityEngine;

/// <summary>
/// 控制Animator参数的脚本
/// </summary>
public class MoonlightSwordAnimatorController : MonoBehaviour
{
    private Animator animator;
    private int comboIndex = 0;
    private float lastAttackTime = 0f;
    private float comboResetTime = 1.5f;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 自动重置连击
        if (Time.time - lastAttackTime > comboResetTime)
        {
            ResetCombo();
        }

        // 更新Animator参数
        animator.SetFloat("ComboIndex", comboIndex);
        animator.SetBool("IsAttacking", Time.time - lastAttackTime < 0.6f);
    }

    /// <summary>
    /// 触发普通攻击
    /// </summary>
    public void TriggerAttack()
    {
        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;

        // 切换连击索引
        comboIndex = (comboIndex + 1) % 2;
    }

    /// <summary>
    /// 触发特殊攻击
    /// </summary>
    public void TriggerSpecialAttack()
    {
        animator.SetTrigger("SpecialAttack");
        animator.SetBool("IsAiming", true);
        lastAttackTime = Time.time;
    }

    /// <summary>
    /// 重置连击
    /// </summary>
    public void ResetCombo()
    {
        comboIndex = 0;
        animator.SetFloat("ComboIndex", 0);
    }

    /// <summary>
    /// 设置瞄准状态
    /// </summary>
    public void SetAiming(bool aiming)
    {
        animator.SetBool("IsAiming", aiming);
    }
}
```

---

## 📋 检查清单

### 动画文件
- [ ] forward_slash.anim 已创建
- [ ] backhand_slash.anim 已创建
- [ ] special_attack.anim 已创建
- [ ] 所有动画长度正确

### Animator Controller
- [ ] MoonlightSwordAnimator.controller 已创建
- [ ] 所有Parameters已配置
- [ ] 所有State已添加
- [ ] 所有Transition已设置
- [ ] Transition条件正确

### 动画事件
- [ ] 伤害判定事件已添加
- [ ] 音效事件已添加
- [ ] 特效事件已添加
- [ ] 事件触发时间正确

### 脚本
- [ ] AnimationEventHandler脚本已创建
- [ ] AnimatorController脚本已创建
- [ ] 事件函数已实现
- [ ] 脚本已挂载到角色

---

**创建日期**: 2025-12-22
**版本**: 1.0
**状态**: 配置完成 ✅
