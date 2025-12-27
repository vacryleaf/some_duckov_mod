using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Duckov;
using Duckov.Utilities;
using ItemStatsSystem;

namespace WormholeTechMod
{
    /// <summary>
    /// 黑洞手雷投掷物组件
    /// 投掷后生成一个黑洞，吸引范围内敌人并造成持续伤害
    /// </summary>
    public class BlackHoleProjectile : MonoBehaviour
    {
        // ========== 黑洞属性 ==========

        /// <summary>
        /// 黑洞存在时间（秒）
        /// </summary>
        [Header("黑洞属性")]
        public float blackHoleDuration = 5f;

        /// <summary>
        /// 吸引范围半径
        /// </summary>
        public float pullRange = 10f;

        /// <summary>
        /// 吸引力强度
        /// </summary>
        public float pullForce = 15f;

        /// <summary>
        /// 每秒伤害值
        /// </summary>
        public float damagePerSecond = 10f;

        /// <summary>
        /// 伤害间隔（秒）
        /// </summary>
        public float damageInterval = 0.5f;

        /// <summary>
        /// 中心伤害范围（在此范围内伤害加倍）
        /// </summary>
        public float coreDamageRange = 2f;

        /// <summary>
        /// 引爆延迟时间（秒）
        /// </summary>
        [Header("投掷属性")]
        public float delayTime = 1.5f;

        /// <summary>
        /// 投掷力度
        /// </summary>
        public float throwForce = 12f;

        /// <summary>
        /// 向上投掷角度偏移
        /// </summary>
        public float throwAngle = 25f;

        /// <summary>
        /// 是否可以伤害自己
        /// </summary>
        public bool canHurtSelf = false;

        /// <summary>
        /// 碰撞音效键
        /// </summary>
        [Header("音效")]
        public string collideSound = "GrenadeCollide";

        /// <summary>
        /// 黑洞激活音效
        /// </summary>
        public string activateSound = "BlackHoleActivate";

        // ========== 内部状态 ==========

        private Rigidbody rb;
        private bool isThrown = false;
        private bool isActive = false;  // 黑洞是否已激活
        private bool collide = false;
        private CharacterMainControl thrower;
        private DamageInfo damageInfo;
        private float damageTimer = 0f;
        private float blackHoleTimer = 0f;
        private ParticleSystem warningParticles;
        private ParticleSystem blackHoleParticles;
        private Light blackHoleLight;
        private GameObject blackHoleVisual;

        // 已影响的角色列表
        private List<CharacterMainControl> affectedCharacters = new List<CharacterMainControl>();

        /// <summary>
        /// 初始化组件
        /// </summary>
        void Awake()
        {
            // 初始化刚体
            rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.mass = 0.5f;
            rb.drag = 0.5f;
            rb.angularDrag = 0.5f;
            rb.useGravity = true;
            rb.isKinematic = true;

            // 初始化碰撞体
            var collider = gameObject.GetComponent<SphereCollider>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<SphereCollider>();
                collider.radius = 0.15f;
            }

            // 初始化伤害信息
            damageInfo = new DamageInfo();
            damageInfo.damageType = DamageTypes.normal;
            damageInfo.damageValue = damagePerSecond * damageInterval;

            ModLogger.Log("[微型黑洞] 投掷物组件初始化完成");
        }

        /// <summary>
        /// 设置伤害信息
        /// </summary>
        public void SetDamageInfo(DamageInfo info)
        {
            damageInfo = info;
            damageInfo.damageValue = damagePerSecond * damageInterval;
        }

        /// <summary>
        /// 投掷黑洞发生器
        /// </summary>
        public void Throw(CharacterMainControl throwerCharacter, Vector3 throwDirection)
        {
            if (isThrown) return;

            thrower = throwerCharacter;
            isThrown = true;

            rb.isKinematic = false;

            // 计算投掷方向（带角度偏移）
            Vector3 adjustedDirection = Quaternion.AngleAxis(-throwAngle, Vector3.Cross(throwDirection, Vector3.up)) * throwDirection;
            adjustedDirection = adjustedDirection.normalized;

            // 施加投掷力
            rb.AddForce(adjustedDirection * throwForce, ForceMode.Impulse);

            // 添加旋转
            rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);

            // 开始计时
            StartCoroutine(ActivationCountdown());

            // 创建警告特效
            CreateWarningEffect();

            // 避免投掷者碰撞
            if (thrower != null)
            {
                Collider throwerCollider = thrower.GetComponent<Collider>();
                Collider myCollider = GetComponent<Collider>();
                if (throwerCollider != null && myCollider != null)
                {
                    Physics.IgnoreCollision(throwerCollider, myCollider, true);
                }
            }

            // ModLogger.Log($"[微型黑洞] 已投掷，方向: {adjustedDirection}, 力度: {throwForce}");
        }

        /// <summary>
        /// 发射黑洞发生器（带速度参数）
        /// </summary>
        public void Launch(Vector3 position, Vector3 velocity, CharacterMainControl throwerCharacter, bool hurtSelf)
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

            // 添加旋转
            rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);

            // 开始计时
            StartCoroutine(ActivationCountdown());

            // 创建警告特效
            CreateWarningEffect();

            // 避免投掷者碰撞
            if (thrower != null)
            {
                Collider throwerCollider = thrower.GetComponent<Collider>();
                Collider myCollider = GetComponent<Collider>();
                if (throwerCollider != null && myCollider != null)
                {
                    Physics.IgnoreCollision(throwerCollider, myCollider, true);
                }
            }

            // ModLogger.Log($"[微型黑洞] 已发射，位置: {position}, 速度: {velocity}");
        }

        /// <summary>
        /// 激活倒计时协程
        /// </summary>
        private IEnumerator ActivationCountdown()
        {
            float timer = 0f;
            float activationTimer = 0f;

            while (true)
            {
                yield return null;

                timer += Time.deltaTime;

                // 等待碰撞或固定时间后激活
                if (collide || timer > delayTime)
                {
                    activationTimer += Time.deltaTime;
                }

                // 激活黑洞
                if (!isActive && (collide || timer > delayTime) && activationTimer > 0.3f)
                {
                    ActivateBlackHole();
                    break;
                }

                // 最后0.5秒加快警告特效
                if (delayTime - timer < 0.5f && warningParticles != null && !isActive)
                {
                    var emission = warningParticles.emission;
                    emission.rateOverTime = 80f;
                }
            }
        }

        /// <summary>
        /// 激活黑洞
        /// </summary>
        private void ActivateBlackHole()
        {
            if (isActive) return;
            isActive = true;

            ModLogger.Log("[微型黑洞] 黑洞激活！");

            // 停止移动
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
            }

            // 停止警告特效
            if (warningParticles != null)
            {
                warningParticles.Stop();
            }

            // 创建黑洞视觉效果
            CreateBlackHoleVisual();

            // 播放激活音效
            if (!string.IsNullOrEmpty(activateSound))
            {
                AudioManager.Post(activateSound, gameObject);
            }

            // 开始黑洞效果协程
            StartCoroutine(BlackHoleEffect());
        }

        /// <summary>
        /// 黑洞效果协程
        /// </summary>
        private IEnumerator BlackHoleEffect()
        {
            blackHoleTimer = 0f;
            damageTimer = 0f;

            while (blackHoleTimer < blackHoleDuration)
            {
                yield return null;

                blackHoleTimer += Time.deltaTime;
                damageTimer += Time.deltaTime;

                // 吸引范围内的角色
                PullCharactersInRange();

                // 造成伤害
                if (damageTimer >= damageInterval)
                {
                    DamageCharactersInRange();
                    damageTimer = 0f;
                }

                // 更新视觉效果（脉动）
                UpdateBlackHoleVisual();
            }

            // 黑洞消失
            DeactivateBlackHole();
        }

        /// <summary>
        /// 吸引范围内的角色
        /// </summary>
        private void PullCharactersInRange()
        {
            Vector3 center = transform.position;

            // 使用 Physics.OverlapSphere 获取范围内的碰撞体
            Collider[] colliders = Physics.OverlapSphere(center, pullRange);

            affectedCharacters.Clear();

            foreach (var col in colliders)
            {
                CharacterMainControl character = col.GetComponentInParent<CharacterMainControl>();
                if (character != null && !affectedCharacters.Contains(character))
                {
                    // 检查是否是投掷者
                    if (!canHurtSelf && character == thrower)
                    {
                        continue;
                    }

                    affectedCharacters.Add(character);

                    // 计算吸引方向和力度
                    Vector3 direction = center - character.transform.position;
                    float distance = direction.magnitude;
                    direction.Normalize();

                    // 距离越近，吸引力越强（但有最小值防止震荡）
                    float pullStrength = pullForce * (1f - (distance / pullRange));
                    pullStrength = Mathf.Max(pullStrength, pullForce * 0.3f);

                    // 应用位移（不使用物理力，直接移动）
                    Vector3 displacement = direction * pullStrength * Time.deltaTime;

                    // 防止穿越黑洞中心
                    if (distance > 0.5f)
                    {
                        character.transform.position += displacement;
                    }
                }
            }
        }

        /// <summary>
        /// 对范围内角色造成伤害
        /// </summary>
        private void DamageCharactersInRange()
        {
            Vector3 center = transform.position;

            foreach (var character in affectedCharacters)
            {
                if (character == null) continue;

                // 检查是否是投掷者
                if (!canHurtSelf && character == thrower)
                {
                    continue;
                }

                // 获取 DamageReceiver
                DamageReceiver receiver = character.GetComponent<DamageReceiver>();
                if (receiver == null)
                {
                    receiver = character.GetComponentInChildren<DamageReceiver>();
                }

                if (receiver != null)
                {
                    // 计算距离
                    float distance = Vector3.Distance(center, character.transform.position);

                    // 创建伤害信息
                    DamageInfo dmg = new DamageInfo(thrower);
                    dmg.damageType = DamageTypes.normal;

                    // 核心范围内伤害加倍
                    if (distance <= coreDamageRange)
                    {
                        dmg.damageValue = damagePerSecond * damageInterval * 2f;
                    }
                    else
                    {
                        dmg.damageValue = damagePerSecond * damageInterval;
                    }

                    // 应用伤害 (使用 Hurt 方法)
                    receiver.Hurt(dmg);

                    // ModLogger.Log($"[微型黑洞] 对 {character.name} 造成 {dmg.damageValue} 点伤害");
                }
            }
        }

        /// <summary>
        /// 碰撞检测
        /// </summary>
        void OnCollisionEnter(Collision collision)
        {
            if (!isThrown || isActive) return;

            if (!collide)
            {
                collide = true;

                // 速度减半
                Vector3 velocity = rb.velocity;
                velocity.x *= 0.3f;
                velocity.z *= 0.3f;
                rb.velocity = velocity;
                rb.angularVelocity = rb.angularVelocity * 0.2f;

                // 播放碰撞音效
                if (!string.IsNullOrEmpty(collideSound))
                {
                    AudioManager.Post(collideSound, gameObject);
                }

                // ModLogger.Log($"[微型黑洞] 碰撞到: {collision.gameObject.name}");
            }
        }

        /// <summary>
        /// 创建警告特效
        /// </summary>
        private void CreateWarningEffect()
        {
            GameObject effectObj = new GameObject("WarningEffect");
            effectObj.transform.SetParent(transform);
            effectObj.transform.localPosition = Vector3.zero;

            warningParticles = effectObj.AddComponent<ParticleSystem>();

            var main = warningParticles.main;
            main.startColor = new Color(0.2f, 0f, 0.5f, 0.8f);  // 深紫色
            main.startSize = 0.15f;
            main.startLifetime = 0.4f;
            main.startSpeed = 0.8f;
            main.loop = true;

            var emission = warningParticles.emission;
            emission.rateOverTime = 30f;

            var shape = warningParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            warningParticles.Play();
        }

        /// <summary>
        /// 创建黑洞视觉效果
        /// </summary>
        private void CreateBlackHoleVisual()
        {
            blackHoleVisual = new GameObject("BlackHoleVisual");
            blackHoleVisual.transform.SetParent(transform);
            blackHoleVisual.transform.localPosition = Vector3.zero;

            // 黑洞核心（黑色球体）
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "Core";
            core.transform.SetParent(blackHoleVisual.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            Material coreMaterial = new Material(Shader.Find("Standard"));
            coreMaterial.color = Color.black;
            coreMaterial.SetFloat("_Metallic", 1f);
            coreMaterial.SetFloat("_Glossiness", 1f);
            core.GetComponent<Renderer>().material = coreMaterial;
            Object.Destroy(core.GetComponent<Collider>());

            // 吸积盘（环形粒子）
            GameObject diskObj = new GameObject("AccretionDisk");
            diskObj.transform.SetParent(blackHoleVisual.transform);
            diskObj.transform.localPosition = Vector3.zero;

            blackHoleParticles = diskObj.AddComponent<ParticleSystem>();

            var main = blackHoleParticles.main;
            main.startColor = new Color(0.6f, 0.2f, 1f, 0.8f);  // 紫色
            main.startSize = 0.3f;
            main.startLifetime = 1f;
            main.startSpeed = 3f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.loop = true;

            var emission = blackHoleParticles.emission;
            emission.rateOverTime = 100f;

            var shape = blackHoleParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var velocityOverLifetime = blackHoleParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.orbitalX = 3f;
            velocityOverLifetime.orbitalY = 0f;
            velocityOverLifetime.orbitalZ = 0f;
            velocityOverLifetime.radial = -2f;

            var colorOverLifetime = blackHoleParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.5f, 0.2f), 0f),  // 橙色
                    new GradientColorKey(new Color(0.6f, 0.2f, 1f), 0.5f),  // 紫色
                    new GradientColorKey(new Color(0.2f, 0.1f, 0.3f), 1f)   // 深紫色
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            blackHoleParticles.Play();

            // 添加点光源
            blackHoleLight = blackHoleVisual.AddComponent<Light>();
            blackHoleLight.type = LightType.Point;
            blackHoleLight.color = new Color(0.6f, 0.2f, 1f);  // 紫色
            blackHoleLight.intensity = 2f;
            blackHoleLight.range = pullRange;

            // 创建范围指示器
            CreateRangeIndicator();
        }

        /// <summary>
        /// 创建范围指示器
        /// </summary>
        private void CreateRangeIndicator()
        {
            GameObject ringObj = new GameObject("RangeIndicator");
            ringObj.transform.SetParent(blackHoleVisual.transform);
            ringObj.transform.localPosition = Vector3.zero;

            ParticleSystem ringParticles = ringObj.AddComponent<ParticleSystem>();

            var main = ringParticles.main;
            main.startColor = new Color(0.4f, 0.2f, 0.8f, 0.3f);
            main.startSize = 0.2f;
            main.startLifetime = 2f;
            main.startSpeed = pullRange / 2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true;

            var emission = ringParticles.emission;
            emission.rateOverTime = 30f;

            var shape = ringParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = pullRange;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var velocityOverLifetime = ringParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.radial = -pullRange / 2f;

            ringParticles.Play();
        }

        /// <summary>
        /// 更新黑洞视觉效果
        /// </summary>
        private void UpdateBlackHoleVisual()
        {
            if (blackHoleVisual == null) return;

            // 脉动效果
            float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.1f;
            blackHoleVisual.transform.localScale = Vector3.one * pulse;

            // 旋转
            blackHoleVisual.transform.Rotate(Vector3.up, 60f * Time.deltaTime);

            // 光源强度随时间变化
            if (blackHoleLight != null)
            {
                blackHoleLight.intensity = 2f + Mathf.Sin(Time.time * 3f) * 0.5f;
            }

            // 接近消失时逐渐变小
            float remainingTime = blackHoleDuration - blackHoleTimer;
            if (remainingTime < 1f)
            {
                float scale = remainingTime;
                blackHoleVisual.transform.localScale = Vector3.one * scale * pulse;
            }
        }

        /// <summary>
        /// 黑洞消失
        /// </summary>
        private void DeactivateBlackHole()
        {
            ModLogger.Log("[微型黑洞] 黑洞消失");

            // 创建消失特效
            CreateDisappearEffect();

            // 销毁
            Destroy(gameObject, 0.5f);
        }

        /// <summary>
        /// 创建消失特效
        /// </summary>
        private void CreateDisappearEffect()
        {
            GameObject effectObj = new GameObject("DisappearEffect");
            effectObj.transform.position = transform.position;

            ParticleSystem particles = effectObj.AddComponent<ParticleSystem>();

            var main = particles.main;
            main.startColor = new Color(0.6f, 0.2f, 1f, 0.9f);
            main.startSize = 0.5f;
            main.startLifetime = 1f;
            main.startSpeed = 10f;
            main.duration = 0.3f;
            main.loop = false;

            var emission = particles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 80)
            });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.6f, 0.2f, 1f), 0f),
                    new GradientColorKey(new Color(0.2f, 0.1f, 0.3f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            particles.Play();

            Destroy(effectObj, 2f);
        }

        /// <summary>
        /// 显示消息提示
        /// </summary>
        private void ShowMessage(string message)
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter != null)
            {
                mainCharacter.PopText(message);
            }
        }
    }
}
