# 名刀月影 - 剑气系统详细配置

## 📋 概述

本文档详细描述剑气投射物系统的实现细节,包括物理行为、碰撞检测、特效系统和子弹偏转机制。

---

## 🌙 剑气投射物核心配置

### 基础参数

```yaml
名称: 月影剑气 (Moonlight Sword Aura)
类型: 投射物 (Projectile)
形状: 月牙形能量波
颜色: 冰蓝色 (RGB: 112, 176, 224)
```

### 物理属性

```yaml
速度: 15 米/秒
最大飞行距离: 10 米
宽度: 2 米
高度: 1 米
厚度: 0.3 米
碰撞层级: Projectile
```

### 伤害属性

```yaml
基础伤害: 90 (范围: 80-100)
伤害类型: 魔法伤害 (MagicDamage)
穿透数量: 3 (可击中3个敌人)
击退力度: 500 牛顿
击退距离: 2 米
```

---

## 🎯 碰撞检测系统

### 碰撞体配置

```csharp
// 主碰撞体 - 用于敌人检测
BoxCollider mainCollider = new BoxCollider();
mainCollider.size = new Vector3(2f, 1f, 0.3f);
mainCollider.center = Vector3.zero;
mainCollider.isTrigger = true;
mainCollider.layer = LayerMask.NameToLayer("Projectile");

// 子弹偏转碰撞体 - 稍大的范围
SphereCollider deflectCollider = new SphereCollider();
deflectCollider.radius = 1.2f;
deflectCollider.center = Vector3.zero;
deflectCollider.isTrigger = true;
deflectCollider.layer = LayerMask.NameToLayer("ProjectileDeflector");
```

### 碰撞检测逻辑

```csharp
void FixedUpdate()
{
    // 前向投射检测
    Vector3 origin = transform.position;
    Vector3 direction = transform.forward;
    float detectRadius = 1.5f; // 检测半径

    // 使用OverlapSphere进行范围检测
    Collider[] hits = Physics.OverlapSphere(origin, detectRadius, targetLayerMask);

    foreach (Collider hit in hits)
    {
        // 检查是否已经击中过
        if (hitObjects.Contains(hit.gameObject))
            continue;

        // 检查对象类型
        if (hit.CompareTag("Enemy"))
        {
            OnHitEnemy(hit.gameObject);
        }
        else if (hit.CompareTag("Projectile") || hit.CompareTag("Bullet"))
        {
            OnHitProjectile(hit.gameObject);
        }
        else if (hit.CompareTag("Obstacle"))
        {
            OnHitObstacle(hit.gameObject);
        }
    }

    // 检查飞行距离
    if (traveledDistance >= maxDistance)
    {
        OnReachMaxDistance();
    }
}
```

---

## ⚔️ 敌人击中系统

### 伤害计算

```csharp
/// <summary>
/// 击中敌人时的处理
/// </summary>
void OnHitEnemy(GameObject enemy)
{
    Debug.Log($"[剑气] 击中敌人: {enemy.name}");

    // 1. 计算实际伤害
    float actualDamage = CalculateDamage();

    // 2. 应用伤害
    IDamageable damageable = enemy.GetComponent<IDamageable>();
    if (damageable != null)
    {
        DamageInfo damageInfo = new DamageInfo
        {
            damage = actualDamage,
            damageType = DamageType.Magic,
            source = owner,
            hitPoint = enemy.transform.position,
            hitDirection = transform.forward
        };

        damageable.TakeDamage(damageInfo);
    }

    // 3. 应用击退
    ApplyKnockback(enemy, transform.forward);

    // 4. 播放击中特效
    SpawnHitEffect(enemy.transform.position);

    // 5. 记录击中对象
    hitObjects.Add(enemy);
    currentPierceCount++;

    // 6. 检查是否达到穿透上限
    if (currentPierceCount >= maxPierceCount)
    {
        DestroyAura();
    }
}

/// <summary>
/// 计算伤害值
/// </summary>
float CalculateDamage()
{
    // 基础伤害随机化
    float damage = Random.Range(minDamage, maxDamage);

    // 穿透伤害衰减
    float pierceDamageMultiplier = 1f - (currentPierceCount * 0.1f);
    damage *= Mathf.Max(pierceDamageMultiplier, 0.7f); // 最多衰减30%

    // 暴击判定
    if (Random.value < criticalChance)
    {
        damage *= criticalMultiplier;
        Debug.Log("[剑气] 暴击!");
    }

    return damage;
}

/// <summary>
/// 应用击退效果
/// </summary>
void ApplyKnockback(GameObject target, Vector3 direction)
{
    Rigidbody rb = target.GetComponent<Rigidbody>();
    if (rb != null)
    {
        // 计算击退力
        Vector3 knockbackForce = direction.normalized * knockbackStrength;

        // 添加向上的分量
        knockbackForce.y += knockbackUpwardForce;

        // 应用力
        rb.AddForce(knockbackForce, ForceMode.Impulse);

        Debug.Log($"[剑气] 击退 {target.name}");
    }
}
```

### 击中特效

```csharp
/// <summary>
/// 生成击中特效
/// </summary>
void SpawnHitEffect(Vector3 position)
{
    if (hitEffectPrefab != null)
    {
        GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);

        // 设置特效颜色
        ParticleSystem particles = effect.GetComponent<ParticleSystem>();
        if (particles != null)
        {
            var main = particles.main;
            main.startColor = new Color(0.44f, 0.69f, 0.88f, 1f); // 冰蓝色
        }

        // 自动销毁
        Destroy(effect, 2f);
    }

    // 播放击中音效
    PlayHitSound(position);
}

/// <summary>
/// 播放击中音效
/// </summary>
void PlayHitSound(Vector3 position)
{
    if (hitSound != null)
    {
        AudioSource.PlayClipAtPoint(hitSound, position, 0.7f);
    }
}
```

---

## 🛡️ 子弹偏转系统

### 偏转机制

```csharp
/// <summary>
/// 击中投射物(子弹)时的处理
/// </summary>
void OnHitProjectile(GameObject projectile)
{
    Debug.Log($"[剑气] 偏转子弹: {projectile.name}");

    // 1. 获取子弹的运动组件
    Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

    if (projectileRb != null)
    {
        // 方案A: 反弹回去
        DeflectProjectile(projectileRb);
    }
    else
    {
        // 方案B: 直接销毁
        DestroyProjectile(projectile);
    }

    // 2. 播放偏转特效
    SpawnDeflectEffect(projectile.transform.position);

    // 3. 播放偏转音效
    PlayDeflectSound(projectile.transform.position);

    // 4. 统计偏转数量
    deflectedBulletCount++;
}

/// <summary>
/// 偏转子弹(反弹)
/// </summary>
void DeflectProjectile(Rigidbody projectileRb)
{
    // 获取当前速度
    Vector3 currentVelocity = projectileRb.velocity;
    float speed = currentVelocity.magnitude;

    // 计算反弹方向
    Vector3 deflectDirection = CalculateDeflectDirection(projectileRb.gameObject);

    // 应用新速度(增加50%速度)
    projectileRb.velocity = deflectDirection * speed * 1.5f;

    // 改变子弹的所有者标签(如果需要)
    var projectileScript = projectileRb.GetComponent<Projectile>();
    if (projectileScript != null)
    {
        projectileScript.owner = owner; // 变成玩家的子弹
        projectileScript.ChangeTeam(owner.team);
    }

    Debug.Log($"[剑气] 子弹已反弹,新速度: {projectileRb.velocity}");
}

/// <summary>
/// 计算偏转方向
/// </summary>
Vector3 CalculateDeflectDirection(GameObject projectile)
{
    // 方案1: 直接反向
    Vector3 direction = -projectile.transform.forward;

    // 方案2: 反射
    // Vector3 incomingDir = projectile.transform.forward;
    // Vector3 normal = -transform.forward;
    // direction = Vector3.Reflect(incomingDir, normal);

    // 方案3: 朝向最近的敌人
    // GameObject nearestEnemy = FindNearestEnemy(projectile.transform.position);
    // if (nearestEnemy != null)
    // {
    //     direction = (nearestEnemy.transform.position - projectile.transform.position).normalized;
    // }

    return direction.normalized;
}

/// <summary>
/// 销毁子弹
/// </summary>
void DestroyProjectile(GameObject projectile)
{
    // 播放销毁特效
    SpawnProjectileDestroyEffect(projectile.transform.position);

    // 销毁子弹
    Destroy(projectile);

    Debug.Log($"[剑气] 子弹已销毁: {projectile.name}");
}
```

### 偏转特效

```csharp
/// <summary>
/// 生成偏转特效
/// </summary>
void SpawnDeflectEffect(Vector3 position)
{
    // 创建火花特效
    GameObject spark = new GameObject("DeflectSpark");
    spark.transform.position = position;

    // 添加粒子系统
    ParticleSystem sparkParticles = spark.AddComponent<ParticleSystem>();
    var main = sparkParticles.main;
    main.startColor = Color.white;
    main.startSize = 0.2f;
    main.startLifetime = 0.3f;
    main.startSpeed = 5f;
    main.maxParticles = 30;

    var emission = sparkParticles.emission;
    emission.rateOverTime = 0;
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
    flashLight.shadows = LightShadows.None;

    // 自动销毁
    Destroy(spark, 1f);
}

/// <summary>
/// 播放偏转音效
/// </summary>
void PlayDeflectSound(Vector3 position)
{
    if (deflectSound != null)
    {
        AudioSource.PlayClipAtPoint(deflectSound, position, 0.8f);
    }
}
```

---

## ✨ 视觉特效系统

### 剑气本体特效

```csharp
/// <summary>
/// 初始化剑气视觉效果
/// </summary>
void InitializeVisualEffects()
{
    // 1. 主体发光
    CreateMainGlow();

    // 2. 粒子拖尾
    CreateParticleTrail();

    // 3. 边缘光晕
    CreateEdgeGlow();

    // 4. 能量波动
    CreateEnergyWave();
}

/// <summary>
/// 创建主体发光
/// </summary>
void CreateMainGlow()
{
    // 获取主体渲染器
    Renderer mainRenderer = GetComponent<Renderer>();
    if (mainRenderer != null)
    {
        Material mat = mainRenderer.material;

        // 设置发光
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", auraColor * emissionIntensity);

        // 动态调整发光强度
        StartCoroutine(AnimateEmission());
    }
}

/// <summary>
/// 发光动画
/// </summary>
IEnumerator AnimateEmission()
{
    Renderer renderer = GetComponent<Renderer>();
    float time = 0f;

    while (renderer != null)
    {
        // 呼吸效果
        float intensity = emissionIntensity * (1f + Mathf.Sin(time * 5f) * 0.3f);
        renderer.material.SetColor("_EmissionColor", auraColor * intensity);

        time += Time.deltaTime;
        yield return null;
    }
}

/// <summary>
/// 创建粒子拖尾
/// </summary>
void CreateParticleTrail()
{
    ParticleSystem trail = GetComponentInChildren<ParticleSystem>();
    if (trail == null) return;

    var main = trail.main;
    main.startColor = new Color(1f, 1f, 1f, 0.8f);
    main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
    main.startLifetime = 0.5f;
    main.startSpeed = 2f;
    main.maxParticles = 100;

    var emission = trail.emission;
    emission.rateOverTime = 50;

    var colorOverLifetime = trail.colorOverLifetime;
    colorOverLifetime.enabled = true;
    Gradient gradient = new Gradient();
    gradient.SetKeys(
        new GradientColorKey[] {
            new GradientColorKey(auraColor, 0f),
            new GradientColorKey(Color.white, 0.5f),
            new GradientColorKey(auraColor, 1f)
        },
        new GradientAlphaKey[] {
            new GradientAlphaKey(0f, 0f),
            new GradientAlphaKey(0.8f, 0.3f),
            new GradientAlphaKey(0f, 1f)
        }
    );
    colorOverLifetime.color = gradient;
}

/// <summary>
/// 创建边缘光晕
/// </summary>
void CreateEdgeGlow()
{
    // 创建外光晕对象
    GameObject glowObj = new GameObject("EdgeGlow");
    glowObj.transform.SetParent(transform);
    glowObj.transform.localPosition = Vector3.zero;
    glowObj.transform.localScale = Vector3.one * 1.2f;

    // 添加发光网格
    MeshFilter meshFilter = glowObj.AddComponent<MeshFilter>();
    meshFilter.mesh = GetComponent<MeshFilter>().mesh;

    MeshRenderer renderer = glowObj.AddComponent<MeshRenderer>();
    Material glowMat = new Material(Shader.Find("Standard"));
    glowMat.color = new Color(auraColor.r, auraColor.g, auraColor.b, 0.3f);
    glowMat.EnableKeyword("_EMISSION");
    glowMat.SetColor("_EmissionColor", auraColor * 1.5f);

    // 透明模式
    glowMat.SetFloat("_Mode", 3);
    glowMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
    glowMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
    glowMat.renderQueue = 3001;

    renderer.material = glowMat;
}
```

### 飞行特效

```csharp
/// <summary>
/// 更新飞行特效
/// </summary>
void UpdateFlightEffects()
{
    // 1. 更新拖尾强度(根据速度)
    UpdateTrailIntensity();

    // 2. 更新粒子发射率
    UpdateParticleEmission();

    // 3. 更新音效音调
    UpdateSoundPitch();
}

/// <summary>
/// 更新拖尾强度
/// </summary>
void UpdateTrailIntensity()
{
    TrailRenderer trail = GetComponent<TrailRenderer>();
    if (trail != null)
    {
        float speedRatio = currentSpeed / maxSpeed;
        trail.startWidth = baseTrailWidth * speedRatio;
        trail.time = baseTrailTime / speedRatio;
    }
}

/// <summary>
/// 更新粒子发射率
/// </summary>
void UpdateParticleEmission()
{
    ParticleSystem particles = GetComponentInChildren<ParticleSystem>();
    if (particles != null)
    {
        var emission = particles.emission;
        float speedRatio = currentSpeed / maxSpeed;
        emission.rateOverTime = baseEmissionRate * speedRatio;
    }
}
```

---

## 🔊 音效系统

### 飞行音效

```csharp
/// <summary>
/// 播放飞行音效
/// </summary>
void PlayFlightSound()
{
    if (flightSound != null && audioSource != null)
    {
        audioSource.clip = flightSound;
        audioSource.loop = true;
        audioSource.volume = 0.6f;
        audioSource.pitch = 1.0f;
        audioSource.spatialBlend = 0.8f; // 3D音效
        audioSource.Play();
    }
}

/// <summary>
/// 更新音效音调(根据速度)
/// </summary>
void UpdateSoundPitch()
{
    if (audioSource != null && audioSource.isPlaying)
    {
        float speedRatio = currentSpeed / maxSpeed;
        audioSource.pitch = 0.8f + speedRatio * 0.4f; // 0.8 - 1.2
    }
}

/// <summary>
/// 停止飞行音效
/// </summary>
void StopFlightSound()
{
    if (audioSource != null)
    {
        audioSource.Stop();
    }
}
```

---

## 🎯 生命周期管理

### 剑气销毁

```csharp
/// <summary>
/// 销毁剑气
/// </summary>
void DestroyAura()
{
    if (isDestroying) return;
    isDestroying = true;

    Debug.Log("[剑气] 销毁");

    // 1. 停止所有音效
    StopFlightSound();

    // 2. 播放消散特效
    PlayDissipateEffect();

    // 3. 停止粒子发射(但让现有粒子播完)
    StopParticleEmission();

    // 4. 渐隐主体
    StartCoroutine(FadeOut());

    // 5. 延迟销毁
    Destroy(gameObject, 1f);
}

/// <summary>
/// 播放消散特效
/// </summary>
void PlayDissipateEffect()
{
    if (dissipateEffectPrefab != null)
    {
        GameObject effect = Instantiate(dissipateEffectPrefab, transform.position, Quaternion.identity);
        Destroy(effect, 2f);
    }
}

/// <summary>
/// 停止粒子发射
/// </summary>
void StopParticleEmission()
{
    ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
    foreach (var ps in particles)
    {
        var emission = ps.emission;
        emission.enabled = false;
    }
}

/// <summary>
/// 渐隐效果
/// </summary>
IEnumerator FadeOut()
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

## 📊 性能优化

### 对象池

```csharp
/// <summary>
/// 剑气对象池
/// </summary>
public class SwordAuraPool : MonoBehaviour
{
    public GameObject auraPrefab;
    public int poolSize = 5;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        // 预生成对象
        for (int i = 0; i < poolSize; i++)
        {
            GameObject aura = Instantiate(auraPrefab);
            aura.SetActive(false);
            pool.Enqueue(aura);
        }
    }

    /// <summary>
    /// 获取剑气对象
    /// </summary>
    public GameObject GetAura()
    {
        if (pool.Count > 0)
        {
            GameObject aura = pool.Dequeue();
            aura.SetActive(true);
            return aura;
        }
        else
        {
            // 池为空,创建新对象
            return Instantiate(auraPrefab);
        }
    }

    /// <summary>
    /// 归还剑气对象
    /// </summary>
    public void ReturnAura(GameObject aura)
    {
        aura.SetActive(false);
        pool.Enqueue(aura);
    }
}
```

### 碰撞优化

```csharp
// 使用LayerMask减少不必要的碰撞检测
LayerMask enemyLayer = LayerMask.GetMask("Enemy");
LayerMask projectileLayer = LayerMask.GetMask("Projectile", "Bullet");
LayerMask targetLayerMask = enemyLayer | projectileLayer;

// 使用Physics.OverlapSphereNonAlloc避免GC
Collider[] hitBuffer = new Collider[20];

void FixedUpdate()
{
    int hitCount = Physics.OverlapSphereNonAlloc(
        transform.position,
        detectRadius,
        hitBuffer,
        targetLayerMask
    );

    for (int i = 0; i < hitCount; i++)
    {
        ProcessHit(hitBuffer[i]);
    }
}
```

---

## 📋 配置文件

### SwordAuraConfig.json

```json
{
  "auraName": "月影剑气",
  "physics": {
    "speed": 15.0,
    "maxDistance": 10.0,
    "width": 2.0,
    "height": 1.0
  },
  "damage": {
    "baseDamage": 90.0,
    "minDamage": 80.0,
    "maxDamage": 100.0,
    "damageType": "Magic",
    "pierceCount": 3,
    "pierceDamageDecay": 0.1
  },
  "knockback": {
    "strength": 500.0,
    "upwardForce": 100.0,
    "distance": 2.0
  },
  "deflection": {
    "enabled": true,
    "deflectRadius": 1.2,
    "deflectSpeedMultiplier": 1.5,
    "redirectToEnemy": false
  },
  "visuals": {
    "color": [112, 176, 224, 180],
    "emissionIntensity": 2.0,
    "particleCount": 100,
    "trailDuration": 0.5
  },
  "audio": {
    "flightSound": "aura_fly.wav",
    "hitSound": "aura_hit.wav",
    "deflectSound": "metal_clash.wav",
    "dissipateSound": "aura_fade.wav"
  }
}
```

---

## 📝 检查清单

### 核心功能
- [ ] 剑气正确生成
- [ ] 飞行速度和距离正确
- [ ] 伤害计算正确
- [ ] 穿透系统工作正常
- [ ] 击退效果正确

### 子弹偏转
- [ ] 可以偏转敌方子弹
- [ ] 偏转方向正确
- [ ] 偏转后子弹伤害敌人
- [ ] 偏转特效正常

### 视觉特效
- [ ] 剑气颜色和形状正确
- [ ] 发光效果正常
- [ ] 粒子拖尾正常
- [ ] 击中特效显示正确
- [ ] 消散动画流畅

### 音效
- [ ] 飞行音效循环播放
- [ ] 击中音效正确触发
- [ ] 偏转音效正确触发
- [ ] 音效音调随速度变化

### 性能
- [ ] 帧率保持稳定
- [ ] 无内存泄漏
- [ ] 对象池工作正常
- [ ] 碰撞检测优化生效

---

**创建日期**: 2025-12-22
**版本**: 1.0
**状态**: 配置完成 ✅
