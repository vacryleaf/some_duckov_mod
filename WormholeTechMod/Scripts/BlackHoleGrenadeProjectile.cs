using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Duckov;
using Duckov.Utilities;

namespace WormholeTechMod
{
    /// <summary>
    /// 黑洞手雷投掷物组�?
    /// 爆炸后产生黑洞引力，将范围内敌人聚集到中心并造成伤害
    /// </summary>
    public class BlackHoleGrenadeProjectile : MonoBehaviour
    {
        // ========== 核心属�?==========

        /// <summary>
        /// 伤害�?
        /// </summary>
        [Header("伤害")]
        public float damage = 25f;

        /// <summary>
        /// 爆炸/吸引范围半径
        /// </summary>
        [Header("范围")]
        public float pullRange = 12f;

        /// <summary>
        /// 引力强度（越高拉得越快越紧）
        /// </summary>
        [Header("引力")]
        public float pullForce = 3f;

        /// <summary>
        /// 引力持续时间（秒�?
        /// </summary>
        [Header("持续时间")]
        public float pullDuration = 3f;

        /// <summary>
        /// 引爆延迟时间（秒�?
        /// </summary>
        [Header("延迟")]
        public float delayTime = 2f;

        /// <summary>
        /// 是否碰撞后才开始计�?
        /// </summary>
        public bool delayFromCollide = false;

        /// <summary>
        /// 是否创建爆炸特效
        /// </summary>
        [Header("特效")]
        public bool createExplosion = true;

        /// <summary>
        /// 是否有碰撞音�?
        /// </summary>
        [Header("音效")]
        public bool hasCollideSound = true;

        /// <summary>
        /// 碰撞音效�?
        /// </summary>
        public string collideSound = "";

        /// <summary>
        /// 是否可以伤害自己
        /// </summary>
        public bool canHurtSelf = false;

        /// <summary>
        /// 投掷力度
        /// </summary>
        [Header("投掷")]
        public float throwForce = 15f;

        /// <summary>
        /// 向上投掷角度偏移
        /// </summary>
        public float throwAngle = 30f;

        // ========== 内部状�?==========

        private Rigidbody rb;
        private bool isThrown = false;
        private bool hasExploded = false;
        private bool isPulling = false;
        private bool collide = false;
        private float makeSoundTimeMarker = -1f;
        private int soundMadeCount = 0;
        private CharacterMainControl thrower;
        private Vector3 explosionCenter;
        private ParticleSystem warningParticles;
        private ParticleSystem blackHoleParticles;
        private List<CharacterMainControl> affectedCharacters = new List<CharacterMainControl>();
        private DamageInfo damageInfo;

        /// <summary>
        /// 初始化组�?
        /// </summary>
        void Awake()
        {
            // 初始化刚�?
            rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.mass = 0.35f;
            rb.drag = 0.5f;
            rb.angularDrag = 0.5f;
            rb.useGravity = true;
            rb.isKinematic = true;

            // 初始化碰撞体
            var collider = gameObject.GetComponent<SphereCollider>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<SphereCollider>();
                collider.radius = 0.12f;
            }

            // 初始化伤害信�?
            damageInfo = new DamageInfo();
            damageInfo.damageType = DamageTypes.normal;
            damageInfo.damageValue = damage;
            damageInfo.fromCharacter = thrower;

            }

        /// <summary>
        /// 设置伤害信息
        /// </summary>
        public void SetDamageInfo(DamageInfo info)
        {
            damageInfo = info;
        }

        /// <summary>
        /// 发射手雷
        /// </summary>
        public void Launch(Vector3 position, Vector3 velocity, CharacterMainControl throwerCharacter, bool hurtSelf)
        {
            if (isThrown) return;

            thrower = throwerCharacter;
            isThrown = true;
            canHurtSelf = hurtSelf;
            damageInfo.fromCharacter = throwerCharacter;

            // 设置初始位置
            transform.position = position;
            rb.isKinematic = false;

            // 应用速度
            rb.velocity = velocity;

            // 添加旋转
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

            // 开始计�?
            if (!delayFromCollide)
            {
                StartCoroutine(FuseCountdown());
            }

            // 创建警告特效（黑洞引力场的视觉效果）
            CreateWarningEffect();

            // 避免投掷者碰�?
            if (thrower != null)
            {
                Collider throwerCollider = thrower.GetComponent<Collider>();
                Collider myCollider = GetComponent<Collider>();
                if (throwerCollider != null && myCollider != null)
                {
                    Physics.IgnoreCollision(throwerCollider, myCollider, true);
                }
            }

            }

        /// <summary>
        /// 投掷手雷（兼容旧方法�?
        /// </summary>
        public void Throw(CharacterMainControl throwerCharacter, Vector3 throwDirection)
        {
            if (isThrown) return;

            thrower = throwerCharacter;
            isThrown = true;
            damageInfo.fromCharacter = throwerCharacter;

            rb.isKinematic = false;

            // 计算投掷方向
            Vector3 adjustedDirection = Quaternion.AngleAxis(-throwAngle, Vector3.Cross(throwDirection, Vector3.up)) * throwDirection;
            adjustedDirection = adjustedDirection.normalized;

            // 施加投掷�?
            rb.AddForce(adjustedDirection * throwForce, ForceMode.Impulse);

            // 添加旋转
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

            // 开始计时（如果需要碰撞后计时则等待碰撞）
            if (!delayFromCollide)
            {
                StartCoroutine(FuseCountdown());
            }

            // 创建警告特效
            CreateWarningEffect();

            // 避免投掷者碰�?
            if (thrower != null)
            {
                Collider throwerCollider = thrower.GetComponent<Collider>();
                Collider myCollider = GetComponent<Collider>();
                if (throwerCollider != null && myCollider != null)
                {
                    Physics.IgnoreCollision(throwerCollider, myCollider, true);
                }
            }

            }

        /// <summary>
        /// 引信倒计时协�?
        /// </summary>
        private IEnumerator FuseCountdown()
        {
            float timer = 0f;
            float delayTimer = 0f;

            while (true)
            {
                yield return null;

                timer += Time.deltaTime;

                // 如果不是碰撞后计时，或者已经碰撞，则开始延迟计�?
                if (!delayFromCollide || collide)
                {
                    delayTimer += Time.deltaTime;
                }

                // 检查是否爆�?
                if (!hasExploded)
                {
                    if (delayTimer > delayTime)
                    {
                        Explode();
                        break;
                    }
                }

                // 警告特效随着时间变化
                if (warningParticles != null && !isPulling)
                {
                    float progress = delayTimer / delayTime;
                    var emission = warningParticles.emission;
                    // 警告粒子逐渐变成深紫�?
                    emission.rateOverTime = 10f + progress * 30f;
                }
            }
        }

        /// <summary>
        /// 引爆手雷
        /// </summary>
        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;
            explosionCenter = transform.position;

            // 创建黑洞特效（包含独立的引力效果控制器）
            if (createExplosion)
            {
                CreateBlackHoleEffect();
            }

            // 物理约束
            if (rb != null)
            {
                rb.constraints = (RigidbodyConstraints)10;
            }

            // 销毁手雷物�?
            Destroy(gameObject, 0.05f);
        }

        /// <summary>
        /// 碰撞检�?
        /// </summary>
        void OnCollisionEnter(Collision collision)
        {
            if (!isThrown) return;

            if (!collide)
            {
                collide = true;
            }

            // 速度减半
            Vector3 velocity = rb.velocity;
            velocity.x *= 0.5f;
            velocity.z *= 0.5f;
            rb.velocity = velocity;
            rb.angularVelocity = rb.angularVelocity * 0.3f;

            // 处理音效
            if (hasCollideSound && soundMadeCount < 3)
            {
                if (Time.time - makeSoundTimeMarker > 0.3f)
                {
                    soundMadeCount++;
                    makeSoundTimeMarker = Time.time;

                    // 播放碰撞音效
                    if (!string.IsNullOrEmpty(collideSound))
                    {
                        try
                        {
                            AudioManager.Post(collideSound, gameObject);
                        }
                        catch (System.Exception e)
                        {
                            }
                    }
                }
            }

            }

        /// <summary>
        /// 拉取范围内的角色并造成伤害
        /// </summary>
        private IEnumerator PullCharactersAndApplyDamage()
        {
            float elapsed = 0f;
            float damageInterval = 0.5f; // �?.5秒造成一次伤�?
            float lastDamageTime = 0f;

            // 记录初始位置用于聚集效果（使用独立的变量�?
            Vector3 centerPoint = explosionCenter;
            // 持续吸引和伤�?
            while (elapsed < pullDuration && isPulling)
            {
                elapsed += Time.deltaTime;

                // 获取范围内的所有角�?
                Collider[] colliders = Physics.OverlapSphere(centerPoint, pullRange);

                foreach (var collider in colliders)
                {
                    CharacterMainControl character = collider.GetComponentInParent<CharacterMainControl>();
                    if (character != null)
                    {
                        // 检查是否可以伤害自�?
                        if (!canHurtSelf)
                        {
                            // 如果没有开启友伤，跳过投掷者和队友
                            if (character == thrower) continue;
                            if (thrower != null && character.Team == thrower.Team) continue;
                        }
                        // 开启友伤时，所有人（包括自己）都会被吸引和伤害

                        // 应用引力 - 将角色拉向中�?
                        Vector3 direction = centerPoint - character.transform.position;
                        float distance = direction.magnitude;

                        if (distance > 0.5f)
                        {
                            direction.Normalize();

                            // 距离越近引力越大
                            float forceMagnitude = pullForce * (1f + (pullRange - distance) / pullRange);

                            // 使用 CharacterController 移动角色
                            var characterController = character.GetComponent<CharacterController>();
                            if (characterController != null)
                            {
                                characterController.Move(direction * forceMagnitude * Time.deltaTime);
                            }
                            else
                            {
                                // 如果没有 CharacterController，直接移动位�?
                                character.transform.position += direction * forceMagnitude * Time.deltaTime;
                            }
                        }

                        // 定时造成伤害
                        if (Time.time - lastDamageTime >= damageInterval)
                        {
                            ApplyDamage(character);
                            lastDamageTime = Time.time;
                        }
                    }
                }

                // 黑洞粒子效果增强
                if (blackHoleParticles != null)
                {
                    var emission = blackHoleParticles.emission;
                    emission.rateOverTime = 20f + elapsed * 10f;
                }

                yield return null;
            }

            // 结束黑洞效果
            EndBlackHoleEffect();
        }

        /// <summary>
        /// 对角色造成伤害
        /// </summary>
        private void ApplyDamage(CharacterMainControl character)
        {
            if (character == null) return;

            try
            {
                // DamageReceiver 在角�?Collider 的父物体链上，需要从 Collider 向上查找
                DamageReceiver damageReceiver = character.GetComponentInChildren<DamageReceiver>();
                if (damageReceiver != null)
                {
                    DamageInfo dmgInfo = new DamageInfo(thrower);
                    dmgInfo.damageValue = damage;
                    dmgInfo.damageType = DamageTypes.normal;
                    dmgInfo.damagePoint = character.transform.position + Vector3.up * 0.6f;
                    dmgInfo.damageNormal = (dmgInfo.damagePoint - explosionCenter).normalized;

                    damageReceiver.Hurt(dmgInfo);
                    }
                else
                {
                    }
            }
            catch (System.Exception e)
            {
                }
        }

        /// <summary>
        /// 创建伤害数字显示
        /// </summary>
        private void CreateDamageNumber(Vector3 position, float damageAmount)
        {
            // 简化的伤害数字显示
            }

        /// <summary>
        /// 创建黑洞特效
        /// </summary>
        private void CreateBlackHoleEffect()
        {
            // 视觉上克制显示：粒子效果范围是实际范围的50%
            float visualRange = pullRange * 0.5f;

            // 黑洞核心球体（稍小一点）
            GameObject coreObj = new GameObject("BlackHoleCore");
            coreObj.transform.position = explosionCenter;
            coreObj.transform.localScale = Vector3.one * 0.5f;

            var coreRenderer = coreObj.AddComponent<MeshRenderer>();
            coreRenderer.sharedMaterial = CreateBlackHoleMaterial();
            SphereCollider coreCollider = coreObj.AddComponent<SphereCollider>();
            coreCollider.radius = 0.3f;

            coreObj.AddComponent<BlackHoleRotator>();

            // 黑洞粒子效果（克制显示）
            GameObject particleObj = new GameObject("BlackHoleParticles");
            particleObj.transform.position = explosionCenter;

            blackHoleParticles = particleObj.AddComponent<ParticleSystem>();

            var main = blackHoleParticles.main;
            main.startColor = new Color(0.4f, 0f, 0.6f, 0.8f); // 稍亮的紫�?
            main.startSize = 0.15f;
            main.startLifetime = 1.5f;
            main.startSpeed = -1.5f; // 稍慢的吸入速度
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = blackHoleParticles.emission;
            emission.rateOverTime = 20f; // 减少粒子数量

            var shape = blackHoleParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = visualRange; // 使用视觉范围

            var velocityOverLifetime = blackHoleParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.radial = -3f;

            blackHoleParticles.Play();

            // 实际影响范围指示器（半透明背景�?
            CreateRangeIndicator(explosionCenter, pullRange);

            // 创建独立的黑洞效果控制器（用于运行引力协程）
            GameObject controllerObj = new GameObject("BlackHoleEffectController");
            controllerObj.transform.position = explosionCenter;
            var effectController = controllerObj.AddComponent<BlackHoleEffectController>();
            effectController.Initialize(explosionCenter, pullRange, pullForce, pullDuration, damage, canHurtSelf, thrower, blackHoleParticles);

            // 清理黑洞对象
            Destroy(coreObj, pullDuration + 1f);
            Destroy(particleObj, pullDuration + 1f);
            Destroy(controllerObj, pullDuration + 2f);
        }

        /// <summary>
        /// 创建实际影响范围指示器（半透明背景�?
        /// </summary>
        private void CreateRangeIndicator(Vector3 center, float radius)
        {
            GameObject indicatorObj = new GameObject("RangeIndicator");
            indicatorObj.transform.position = center;

            // 使用圆柱体作为范围指示器
            GameObject cylinderObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinderObj.name = "RangeCylinder";
            cylinderObj.transform.SetParent(indicatorObj.transform);
            cylinderObj.transform.position = center + Vector3.up * 0.1f;
            cylinderObj.transform.localScale = new Vector3(radius * 2f, 0.05f, radius * 2f);

            // 创建半透明材质
            Material indicatorMaterial = new Material(Shader.Find("Standard"));
            indicatorMaterial.color = new Color(0.6f, 0f, 0.8f, 0.25f); // 紫色半透明
            indicatorMaterial.SetFloat("_Mode", 3); // 透明模式
            indicatorMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            indicatorMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            indicatorMaterial.SetInt("_ZWrite", 0);
            indicatorMaterial.DisableKeyword("_ALPHATEST_ON");
            indicatorMaterial.EnableKeyword("_ALPHABLEND_ON");
            indicatorMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            indicatorMaterial.renderQueue = 3000;
            indicatorMaterial.SetFloat("_Metallic", 0.3f);
            indicatorMaterial.SetFloat("_Glossiness", 0.8f);

            cylinderObj.GetComponent<Renderer>().material = indicatorMaterial;
            Object.Destroy(cylinderObj.GetComponent<Collider>());

            // 添加边缘发光�?
            CreateRangeRing(center, radius);

            Destroy(indicatorObj, pullDuration + 0.5f);
        }

        /// <summary>
        /// 创建范围边缘发光�?
        /// </summary>
        private void CreateRangeRing(Vector3 center, float radius)
        {
            GameObject ringObj = new GameObject("RangeRing");
            ringObj.transform.position = center;

            ParticleSystem ringParticles = ringObj.AddComponent<ParticleSystem>();

            var main = ringParticles.main;
            main.startColor = new Color(0.8f, 0.2f, 1f, 0.6f); // 亮紫色边�?
            main.startSize = 0.3f;
            main.startLifetime = 0.8f;
            main.startSpeed = 0f;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ringParticles.emission;
            emission.rateOverTime = 60f;

            var shape = ringParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var colorOverLifetime = ringParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.8f, 0.2f, 1f), 0f),
                    new GradientColorKey(new Color(0.4f, 0f, 0.8f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.6f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            ringParticles.Play();

            Destroy(ringObj, pullDuration + 0.5f);
        }

        /// <summary>
        /// 创建黑洞材质
        /// </summary>
        private Material CreateBlackHoleMaterial()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.1f, 0f, 0.2f, 0.95f);
            mat.SetFloat("_Metallic", 0.9f);
            mat.SetFloat("_Glossiness", 0.95f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.3f, 0f, 0.5f) * 2f);
            return mat;
        }

        /// <summary>
        /// 结束黑洞效果
        /// </summary>
        private void EndBlackHoleEffect()
        {
            isPulling = false;
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
            main.startColor = new Color(0.4f, 0f, 0.6f, 0.8f); // 深紫色警�?
            main.startSize = 0.2f;
            main.startLifetime = 0.5f;
            main.startSpeed = 0.5f;
            main.loop = true;

            var emission = warningParticles.emission;
            emission.rateOverTime = 15f;

            var shape = warningParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            warningParticles.Play();
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

        /// <summary>
        /// 通过反射调用方法
        /// </summary>
        private object CallMethod(object target, string methodName, object[] parameters)
        {
            if (target == null) return null;

            try
            {
                var type = target.GetType();
                var method = type.GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    return method.Invoke(target, parameters);
                }
            }
            catch (System.Exception e)
            {
                }

            return null;
        }
    }

    /// <summary>
    /// 黑洞旋转动画组件
    /// </summary>
    public class BlackHoleRotator : MonoBehaviour
    {
        public float rotationSpeed = 100f;

        void Update()
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 黑洞效果控制�?
    /// 独立运行引力效果，确保手雷销毁后效果继续
    /// </summary>
    public class BlackHoleEffectController : MonoBehaviour
    {
        private Vector3 centerPoint;
        private float pullRange;
        private float pullForce;
        private float pullDuration;
        private float damage;
        private CharacterMainControl thrower;
        private ParticleSystem blackHoleParticles;
        private bool isPulling = true;

        // 伤害相关：每个角色独立计�?
        private Dictionary<CharacterMainControl, float> lastDamageTimes = new Dictionary<CharacterMainControl, float>();
        private const float damageInterval = 0.5f;

        public void Initialize(Vector3 center, float range, float force, float duration, float dmg,
            bool hurtSelf, CharacterMainControl throwerChar, ParticleSystem particles)
        {
            centerPoint = center;
            pullRange = range;
            pullForce = force;
            pullDuration = duration;
            damage = dmg;
            thrower = throwerChar;
            blackHoleParticles = particles;

            // 开始引力协�?
            StartCoroutine(PullCharactersAndApplyDamage());
        }

        private IEnumerator PullCharactersAndApplyDamage()
        {
            float elapsed = 0f;

            // 持续吸引和伤�?
            while (elapsed < pullDuration && isPulling)
            {
                elapsed += Time.deltaTime;

                // 获取范围内的所有角�?
                Collider[] colliders = Physics.OverlapSphere(centerPoint, pullRange);

                int affectedCount = 0;
                foreach (var collider in colliders)
                {
                    CharacterMainControl character = collider.GetComponentInParent<CharacterMainControl>();
                    if (character != null)
                    {
                        // 所有角色都会被吸引和伤害（包括投掷者自己）
                        ApplyGravity(character);
                        ApplyDamagePerCharacter(character);
                        affectedCount++;
                    }
                }

                // 黑洞粒子效果增强
                if (blackHoleParticles != null)
                {
                    var emission = blackHoleParticles.emission;
                    emission.rateOverTime = 20f + elapsed * 10f;
                }

                yield return null;
            }

            EndBlackHoleEffect();
        }

        /// <summary>
        /// 应用引力（匀速靠近中心）
        /// </summary>
        private void ApplyGravity(CharacterMainControl character)
        {
            if (character == null) return;

            Vector3 direction = centerPoint - character.transform.position;
            float distance = direction.magnitude;

            // 到达中心附近停止吸引
            if (distance <= 0.5f) return;

            direction.Normalize();

            // 匀速吸引，保持恒定速度
            float actualSpeed = pullForce;

            // 使用 CharacterController 移动
            var characterController = character.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.Move(direction * actualSpeed * Time.deltaTime);
            }
            else
            {
                character.transform.position += direction * actualSpeed * Time.deltaTime;
            }
        }

        /// <summary>
        /// 对每个角色独立计时造成伤害
        /// </summary>
        private void ApplyDamagePerCharacter(CharacterMainControl character)
        {
            if (character == null) return;

            // 初始化角色的伤害计时�?
            if (!lastDamageTimes.ContainsKey(character))
            {
                lastDamageTimes[character] = -damageInterval;
            }

            // 检查是否可以对角色造成伤害
            if (Time.time - lastDamageTimes[character] >= damageInterval)
            {
                ApplyDamage(character);
                lastDamageTimes[character] = Time.time;
            }
        }

        private void ApplyDamage(CharacterMainControl character)
        {
            if (character == null) return;

            try
            {
                // DamageReceiver 在角�?Collider 的父物体链上，需要从 Collider 向上查找
                DamageReceiver damageReceiver = character.GetComponentInChildren<DamageReceiver>();
                if (damageReceiver != null)
                {
                    DamageInfo dmgInfo = new DamageInfo(thrower);
                    dmgInfo.damageValue = damage;
                    dmgInfo.damageType = DamageTypes.normal;
                    dmgInfo.damagePoint = character.transform.position + Vector3.up * 0.6f;
                    dmgInfo.damageNormal = (dmgInfo.damagePoint - centerPoint).normalized;

                    damageReceiver.Hurt(dmgInfo);
                    }
                else
                {
                    }
            }
            catch (System.Exception e)
            {
                }
        }

        private void EndBlackHoleEffect()
        {
            isPulling = false;
            lastDamageTimes.Clear();
            }
    }
}

