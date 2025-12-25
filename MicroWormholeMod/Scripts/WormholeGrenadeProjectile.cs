using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Duckov;
using Duckov.Utilities;

namespace MicroWormholeMod
{
    /// <summary>
    /// 虫洞手雷投掷物组件
    /// 以鸭科夫原版手雷(Grenade)为基准
    /// </summary>
    public class WormholeGrenadeProjectile : MonoBehaviour
    {
        // ========== 核心属性（与原版Grenade对齐）==========

        /// <summary>
        /// 伤害值（虫洞手雷不造成伤害，固定为0）
        /// </summary>
        [Header("伤害")]
        public float damage = 0f;

        /// <summary>
        /// 爆炸/传送范围半径
        /// </summary>
        [Header("范围")]
        public float damageRange = 16f;

        /// <summary>
        /// 是否是地雷模式
        /// </summary>
        [Header("地雷模式")]
        public bool isLandmine = false;

        /// <summary>
        /// 地雷触发范围
        /// </summary>
        public float landmineTriggerRange = 0.5f;

        /// <summary>
        /// 引爆延迟时间（秒）
        /// </summary>
        [Header("延迟")]
        public float delayTime = 3f;

        /// <summary>
        /// 是否碰撞后才开始计时
        /// </summary>
        public bool delayFromCollide = false;

        /// <summary>
        /// 爆炸延迟（多投掷物间隔）
        /// </summary>
        public float blastDelayTimeSpace = 0.2f;

        /// <summary>
        /// 投掷物数量
        /// </summary>
        public int blastCount = 1;

        /// <summary>
        /// 爆炸角度（多投掷物）
        /// </summary>
        public float blastAngle = 0f;

        /// <summary>
        /// 是否创建爆炸特效
        /// </summary>
        [Header("特效")]
        public bool createExplosion = true;

        /// <summary>
        /// 爆炸震动强度
        /// </summary>
        [Range(0f, 1f)]
        public float explosionShakeStrength = 1f;

        /// <summary>
        /// 特效类型
        /// </summary>
        public ExplosionFxTypes fxType = ExplosionFxTypes.custom;

        /// <summary>
        /// 自定义特效预制体
        /// </summary>
        public GameObject fx;

        /// <summary>
        /// 爆炸时生成的物体
        /// </summary>
        public GameObject createOnExlode;

        /// <summary>
        /// 销毁延迟
        /// </summary>
        public float destroyDelay = 0.5f;

        /// <summary>
        /// 爆炸事件
        /// </summary>
        public UnityEngine.Events.UnityEvent onExplodeEvent;

        /// <summary>
        /// 是否有碰撞音效
        /// </summary>
        [Header("音效")]
        public bool hasCollideSound = true;

        /// <summary>
        /// 碰撞音效键
        /// </summary>
        public string collideSound = "GrenadeCollide";

        /// <summary>
        /// 碰撞发声次数
        /// </summary>
        public int makeSoundCount = 3;

        /// <summary>
        /// 是否对AI危险（会产生声音）
        /// </summary>
        public bool isDangerForAi = true;

        /// <summary>
        /// 是否可以伤害自己
        /// </summary>
        public bool canHurtSelf = true;

        /// <summary>
        /// 投掷力度
        /// </summary>
        [Header("投掷")]
        public float throwForce = 15f;

        /// <summary>
        /// 向上投掷角度偏移
        /// </summary>
        public float throwAngle = 30f;

        /// <summary>
        /// 是否可以控制投掷距离
        /// </summary>
        public bool canControlCastDistance = true;

        /// <summary>
        /// 垂直速度
        /// </summary>
        public float grenadeVerticleSpeed = 10f;

        // ========== 内部状态 ==========

        private Rigidbody rb;
        private bool isThrown = false;
        private bool hasExploded = false;
        private bool landmineActived = false;
        private bool landmineTriggerd = false;
        private bool collide = false;
        private float makeSoundTimeMarker = -1f;
        private int soundMadeCount = 0;
        private CharacterMainControl thrower;
        private CharacterMainControl selfTeam;
        private ParticleSystem warningParticles;
        private DamageInfo damageInfo;

        /// <summary>
        /// 特效类型枚举
        /// </summary>
        public enum ExplosionFxTypes
        {
            normal,
            fire,
            smoke,
            custom,
            none
        }

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
            rb.mass = 0.3f;
            rb.drag = 0.5f;
            rb.angularDrag = 0.5f;
            rb.useGravity = true;
            rb.isKinematic = true;

            // 初始化碰撞体
            var collider = gameObject.GetComponent<SphereCollider>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<SphereCollider>();
                collider.radius = 0.1f;
            }

            // 初始化伤害信息
            damageInfo = new DamageInfo();
            damageInfo.damageType = DamageTypes.normal;
            damageInfo.damageValue = 0f; // 虫洞手雷不造成伤害

            Debug.Log("[虫洞手雷] 投掷物组件初始化完成");
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
            selfTeam = throwerCharacter;
            isThrown = true;
            canHurtSelf = hurtSelf;

            // 设置初始位置
            transform.position = position;
            rb.isKinematic = false;

            // 应用速度
            rb.velocity = velocity;

            // 添加旋转
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

            // 开始计时
            if (!delayFromCollide)
            {
                StartCoroutine(FuseCountdown());
            }

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

            Debug.Log($"[虫洞手雷] 已发射，位置: {position}, 速度: {velocity}");
        }

        /// <summary>
        /// 投掷手雷（兼容旧方法）
        /// </summary>
        public void Throw(CharacterMainControl throwerCharacter, Vector3 throwDirection)
        {
            if (isThrown) return;

            thrower = throwerCharacter;
            isThrown = true;
            selfTeam = throwerCharacter;

            rb.isKinematic = false;

            // 计算投掷方向
            Vector3 adjustedDirection = Quaternion.AngleAxis(-throwAngle, Vector3.Cross(throwDirection, Vector3.up)) * throwDirection;
            adjustedDirection = adjustedDirection.normalized;

            // 施加投掷力
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

            Debug.Log($"[虫洞手雷] 已投掷，方向: {adjustedDirection}, 力度: {throwForce}");
        }

        /// <summary>
        /// 引信倒计时协程
        /// </summary>
        private IEnumerator FuseCountdown()
        {
            float timer = 0f;
            float delayTimer = 0f;

            while (true)
            {
                yield return null;

                timer += Time.deltaTime;

                // 如果不是碰撞后计时，或者已经碰撞，则开始延迟计时
                if (!delayFromCollide || collide)
                {
                    delayTimer += Time.deltaTime;
                }

                // 检查是否爆炸
                if (!hasExploded)
                {
                    if (!isLandmine)
                    {
                        if (delayTimer > delayTime)
                        {
                            Explode();
                            break;
                        }
                    }
                    else
                    {
                        if (delayTimer > delayTime)
                        {
                            if (!landmineActived)
                            {
                                landmineActived = true;
                                ActiveLandmine();
                            }
                        }
                    }
                }

                // 最后1秒加快警告特效
                if (delayTime - delayTimer < 1f && warningParticles != null && !landmineActived)
                {
                    var emission = warningParticles.emission;
                    emission.rateOverTime = 100f;
                }
            }
        }

        /// <summary>
        /// 激活地雷
        /// </summary>
        private void ActiveLandmine()
        {
            Debug.Log("[虫洞手雷] 地雷已激活");
        }

        /// <summary>
        /// 引爆手雷
        /// </summary>
        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;

            Debug.Log("[虫洞手雷] 引爆！");

            // 创建爆炸特效
            if (createExplosion)
            {
                CreateExplosionEffect();
            }

            // 自定义特效
            if (createExplosion && needCustomFx && fx != null)
            {
                Instantiate(fx, transform.position, Quaternion.identity);
            }

            // 爆炸时生成物体
            if (createOnExlode != null)
            {
                Instantiate(createOnExlode, transform.position, Quaternion.identity);
            }

            // 执行爆炸事件
            onExplodeEvent?.Invoke();

            // 传送范围内的角色
            TeleportCharactersInRange();

            // 物理约束
            if (rb != null)
            {
                rb.constraints = (RigidbodyConstraints)10;
            }

            // 销毁
            if (destroyDelay <= 0f)
            {
                Destroy(gameObject);
            }
            else if (destroyDelay < 999f)
            {
                Destroy(gameObject, destroyDelay);
            }
        }

        /// <summary>
        /// 是否需要自定义特效
        /// </summary>
        private bool needCustomFx => fxType == ExplosionFxTypes.custom;

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

            // 速度减半
            Vector3 velocity = rb.velocity;
            velocity.x *= 0.5f;
            velocity.z *= 0.5f;
            rb.velocity = velocity;
            rb.angularVelocity = rb.angularVelocity * 0.3f;

            // 处理音效
            if (hasCollideSound && makeSoundCount > 0)
            {
                if (Time.time - makeSoundTimeMarker > 0.3f)
                {
                    soundMadeCount++;
                    makeSoundTimeMarker = Time.time;

                    // AI 声音传播
                    if (isDangerForAi)
                    {
                        AISound sound = default(AISound);
                        sound.fromObject = gameObject;
                        sound.pos = transform.position;
                        if (damageInfo.fromCharacter != null)
                        {
                            sound.fromTeam = damageInfo.fromCharacter.Team;
                        }
                        else
                        {
                            sound.fromTeam = Teams.all;
                        }
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

            Debug.Log($"[虫洞手雷] 碰撞到: {collision.gameObject.name}");
        }

        /// <summary>
        /// 传送范围内的所有角色（原版手雷逻辑）
        /// </summary>
        private void TeleportCharactersInRange()
        {
            Vector3 explosionCenter = transform.position;

            // 使用 Physics.OverlapSphere 获取爆炸范围内的碰撞体（与原版一致）
            Collider[] colliders = Physics.OverlapSphere(explosionCenter, damageRange);

            List<CharacterMainControl> affectedCharacters = new List<CharacterMainControl>();

            foreach (var collider in colliders)
            {
                // 从碰撞体获取角色
                CharacterMainControl character = collider.GetComponentInParent<CharacterMainControl>();
                if (character != null && !affectedCharacters.Contains(character))
                {
                    affectedCharacters.Add(character);
                    Debug.Log($"[虫洞手雷] 角色 {character.name} 在爆炸范围内");
                }
            }

            Debug.Log($"[虫洞手雷] 爆炸范围内共有 {affectedCharacters.Count} 个角色将被传送");

            // 传送每个角色
            foreach (var character in affectedCharacters)
            {
                TeleportCharacterToRandomPosition(character);
            }
        }

        /// <summary>
        /// 将角色传送到地图上的随机位置
        /// </summary>
        private void TeleportCharacterToRandomPosition(CharacterMainControl character)
        {
            if (character == null) return;

            Vector3 randomPosition = GetRandomPositionOnMap();

            if (randomPosition != Vector3.zero)
            {
                Vector3 originalPosition = character.transform.position;
                character.transform.position = randomPosition;

                CreateTeleportEffect(originalPosition, new Color(0.6f, 0.2f, 1f, 0.8f));
                CreateTeleportEffect(randomPosition, new Color(0.2f, 0.8f, 1f, 0.8f));

                Debug.Log($"[虫洞手雷] 角色 {character.name} 从 {originalPosition} 传送到 {randomPosition}");

                if (character == CharacterMainControl.Main)
                {
                    ShowMessage("被虫洞手雷传送！");
                }
            }
        }

        /// <summary>
        /// 获取地图上的随机有效位置
        /// </summary>
        private Vector3 GetRandomPositionOnMap()
        {
            for (int attempt = 0; attempt < 50; attempt++)
            {
                Vector3 randomDirection = Random.insideUnitSphere;
                randomDirection.y = 0;
                randomDirection = randomDirection.normalized;

                float randomDistance = Random.Range(15f, 80f);
                Vector3 candidatePosition = transform.position + randomDirection * randomDistance;

                RaycastHit hit;
                if (Physics.Raycast(candidatePosition + Vector3.up * 100f, Vector3.down, out hit, 200f))
                {
                    if (hit.collider != null && !hit.collider.isTrigger)
                    {
                        Vector3 groundPosition = hit.point + Vector3.up * 0.5f;

                        if (!Physics.CheckSphere(groundPosition, 1f) && groundPosition.y > -20f)
                        {
                            if (IsPositionValid(groundPosition))
                            {
                                NavMeshHit navHit;
                                if (NavMesh.SamplePosition(groundPosition, out navHit, 3f, NavMesh.AllAreas))
                                {
                                    if (IsNavMeshPositionValid(navHit.position))
                                    {
                                        return navHit.position;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Debug.LogWarning("[虫洞手雷] 无法找到有效的NavMesh位置，尝试安全回退位置");
            return GetSafeFallbackPosition();
        }

        /// <summary>
        /// 检查坐标是否有效（在地图内）
        /// </summary>
        private bool IsPositionValid(Vector3 position)
        {
            if (position.y < -50f || position.y > 200f)
            {
                return false;
            }

            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 20f, Vector3.down, out hit, 50f))
            {
                return hit.collider != null && !hit.collider.isTrigger;
            }

            return false;
        }

        /// <summary>
        /// 检查NavMesh位置是否有效可行走
        /// </summary>
        private bool IsNavMeshPositionValid(Vector3 position)
        {
            if (LevelManager.Instance != null)
            {
                object isValidPos = CallMethod(LevelManager.Instance, "IsValidPosition", new object[] { position });
                if (isValidPos is bool && !(bool)isValidPos)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取安全回退位置（爆炸点附近开阔区域）
        /// </summary>
        private Vector3 GetSafeFallbackPosition()
        {
            Vector3[] directions = new Vector3[]
            {
                Vector3.forward * 10f,
                Vector3.back * 10f,
                Vector3.left * 10f,
                Vector3.right * 10f,
                (Vector3.forward + Vector3.right).normalized * 10f,
                (Vector3.forward + Vector3.left).normalized * 10f,
                (Vector3.back + Vector3.right).normalized * 10f,
                (Vector3.back + Vector3.left).normalized * 10f
            };

            foreach (Vector3 offset in directions)
            {
                Vector3 candidate = transform.position + offset;
                RaycastHit hit;
                if (Physics.Raycast(candidate + Vector3.up * 50f, Vector3.down, out hit, 100f))
                {
                    Vector3 groundPos = hit.point + Vector3.up * 0.5f;
                    if (!Physics.CheckSphere(groundPos, 1.5f))
                    {
                        NavMeshHit navHit;
                        if (NavMesh.SamplePosition(groundPos, out navHit, 3f, NavMesh.AllAreas))
                        {
                            Debug.Log($"[虫洞手雷] 使用回退位置: {navHit.position}");
                            return navHit.position;
                        }
                    }
                }
            }

            Vector3 centerPos = transform.position;
            NavMeshHit centerNav;
            if (NavMesh.SamplePosition(centerPos, out centerNav, 10f, NavMesh.AllAreas))
            {
                Debug.Log($"[虫洞手雷] 使用中心位置回退: {centerNav.position}");
                return centerNav.position;
            }

            Debug.LogWarning("[虫洞手雷] 所有回退位置均无效，保持在原位");
            return transform.position + Vector3.up * 2f;
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
            main.startColor = new Color(1f, 0.5f, 0f, 0.8f);
            main.startSize = 0.2f;
            main.startLifetime = 0.5f;
            main.startSpeed = 1f;
            main.loop = true;

            var emission = warningParticles.emission;
            emission.rateOverTime = 20f;

            var shape = warningParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            warningParticles.Play();
        }

        /// <summary>
        /// 创建爆炸特效
        /// </summary>
        private void CreateExplosionEffect()
        {
            GameObject effectObj = new GameObject("ExplosionEffect");
            effectObj.transform.position = transform.position;

            ParticleSystem particles = effectObj.AddComponent<ParticleSystem>();

            var main = particles.main;
            main.startColor = new Color(0.5f, 0.2f, 1f, 0.9f);
            main.startSize = 2f;
            main.startLifetime = 1.5f;
            main.startSpeed = 8f;
            main.duration = 0.3f;
            main.loop = false;

            var emission = particles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 50)
            });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.5f, 0.2f, 1f), 0f),
                    new GradientColorKey(new Color(0.2f, 0.8f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            particles.Play();

            CreateRangeIndicator();

            Destroy(effectObj, 3f);
        }

        /// <summary>
        /// 创建范围指示器
        /// </summary>
        private void CreateRangeIndicator()
        {
            GameObject ringObj = new GameObject("RangeIndicator");
            ringObj.transform.position = transform.position;

            ParticleSystem ringParticles = ringObj.AddComponent<ParticleSystem>();

            var main = ringParticles.main;
            main.startColor = new Color(0.3f, 0.6f, 1f, 0.5f);
            main.startSize = 0.3f;
            main.startLifetime = 1f;
            main.startSpeed = damageRange * 2f;
            main.duration = 0.1f;
            main.loop = false;

            var emission = ringParticles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 100)
            });

            var shape = ringParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            ringParticles.Play();

            Destroy(ringObj, 2f);
        }

        /// <summary>
        /// 创建传送特效
        /// </summary>
        private void CreateTeleportEffect(Vector3 position, Color color)
        {
            GameObject effectObj = new GameObject("TeleportEffect");
            effectObj.transform.position = position;

            ParticleSystem particles = effectObj.AddComponent<ParticleSystem>();

            var main = particles.main;
            main.startColor = color;
            main.startSize = 0.5f;
            main.startLifetime = 1f;
            main.startSpeed = 3f;
            main.duration = 0.5f;
            main.loop = false;

            var emission = particles.emission;
            emission.rateOverTime = 50f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

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
