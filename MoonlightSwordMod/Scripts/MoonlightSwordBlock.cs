using UnityEngine;
using UnityEngine.Events;
using System;

namespace MoonlightSwordMod
{
    /// <summary>
    /// 名刀月影格挡系统
    /// 瞄准时被击中触发格挡，消耗体力使攻击无效并反弹子弹
    ///
    /// 伤害流程：
    /// 子弹碰撞 → DamageReceiver.Hurt() → OnHurtEvent → Health.Hurt() → 伤害结算
    ///
    /// 格挡接入点：DamageReceiver.OnHurtEvent（在Health.Hurt之前）
    /// 实现方式：临时设置 invincible = true 阻止伤害
    ///
    /// 自定义参数（格挡专用，在代码或 Inspector 中配置）:
    /// - staminaCost: 格挡消耗体力
    /// - deflectSpeed: 反弹子弹速度
    /// - deflectDamageRatio: 反弹伤害比例
    /// </summary>
    public class MoonlightSwordBlock : MonoBehaviour
    {
        [Header("格挡设置（自定义参数）")]
        [Tooltip("格挡消耗的体力值")]
        public float staminaCost = 5f;

        [Tooltip("反弹子弹的速度")]
        public float deflectSpeed = 30f;

        [Tooltip("反弹伤害比例（相对原伤害）")]
        public float deflectDamageRatio = 0.5f;

        [Tooltip("反弹子弹最大飞行距离")]
        public float deflectMaxDistance = 20f;

        [Header("特效配置")]
        [Tooltip("格挡成功的特效Prefab")]
        public GameObject blockEffectPrefab;

        [Tooltip("格挡成功的音效")]
        public AudioClip blockSound;

        // 角色引用
        private CharacterMainControl character;
        private Health health;
        private DamageReceiver[] damageReceivers;

        private float lastBlockTime;
        private float blockCooldown = 0.1f;

        private bool initialized = false;

        private bool pendingBlock = false;
        private DamageInfo pendingDamageInfo;

        private bool? cachedInvincibleState = null;

        void Start()
        {
            // 延迟初始化，等待武器被装备
            InvokeRepeating("TryInitialize", 0.5f, 1f);
        }

        /// <summary>
        /// 尝试初始化（找到装备此武器的角色）
        /// </summary>
        private void TryInitialize()
        {
            if (initialized) return;

            // 获取角色组件（从父物体查找）
            character = GetComponentInParent<CharacterMainControl>();
            if (character == null)
            {
                // 尝试从场景中找到主角
                character = CharacterMainControl.Main;
            }

            if (character == null)
            {
                return;
            }

            health = character.GetComponent<Health>();
            if (health == null)
            {
                return;
            }

            // 获取所有 DamageReceiver（角色可能有多个）
            damageReceivers = character.GetComponentsInChildren<DamageReceiver>();

            // 订阅 DamageReceiver.OnHurtEvent（在 Health.Hurt 之前触发）
            foreach (var receiver in damageReceivers)
            {
                if (receiver.OnHurtEvent == null)
                {
                    receiver.OnHurtEvent = new UnityEvent<DamageInfo>();
                }
                receiver.OnHurtEvent.AddListener(OnDamageReceiverHurt);
            }

            initialized = true;
            CancelInvoke("TryInitialize");
            Debug.Log($"[名刀月影] 格挡系统已启用，监听 {damageReceivers.Length} 个 DamageReceiver");
        }

        void OnDestroy()
        {
            UnsubscribeEvents();
        }

        void OnDisable()
        {
            UnsubscribeEvents();
            initialized = false;
        }

        void OnEnable()
        {
            if (!initialized)
            {
                InvokeRepeating("TryInitialize", 0.5f, 1f);
            }
        }

        private void UnsubscribeEvents()
        {
            if (damageReceivers != null)
            {
                foreach (var receiver in damageReceivers)
                {
                    if (receiver != null && receiver.OnHurtEvent != null)
                    {
                        receiver.OnHurtEvent.RemoveListener(OnDamageReceiverHurt);
                    }
                }
            }
        }

        /// <summary>
        /// DamageReceiver 伤害回调（在 Health.Hurt 之前触发）
        /// </summary>
        private void OnDamageReceiverHurt(DamageInfo damageInfo)
        {
            // 尝试格挡
            if (TryBlock(damageInfo))
            {
                // 格挡成功，临时设置无敌阻止伤害
                if (health != null)
                {
                    // 设置无敌状态
                    SetInvincible(true);

                    // 在下一帧恢复
                    pendingBlock = true;
                    pendingDamageInfo = damageInfo;
                }
            }
        }

        void LateUpdate()
        {
            // 在 LateUpdate 中恢复无敌状态（确保 Health.Hurt 已执行完）
            if (pendingBlock)
            {
                pendingBlock = false;
                SetInvincible(false);

                // 播放格挡效果
                PlayBlockEffect(pendingDamageInfo.damagePoint);
                PlayBlockSound();
                DeflectBullet(pendingDamageInfo);
            }
        }

        /// <summary>
        /// 设置角色无敌状态（使用安全的属性访问）
        /// </summary>
        private void SetInvincible(bool value)
        {
            if (health == null) return;

            try
            {
                var prop = typeof(Health).GetProperty("invincible",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(health, value);
                }
                else
                {
                    var field = typeof(Health).GetField("invincible",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance);

                    if (field != null)
                    {
                        field.SetValue(health, value);
                    }
                    else
                    {
                        Debug.LogWarning("[名刀月影] 无法访问 Health.invincible 字段，格挡可能无法完全阻止伤害");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[名刀月影] 设置无敌状态失败: {e.Message}");
            }
        }

        /// <summary>
        /// 尝试格挡
        /// </summary>
        private bool TryBlock(DamageInfo damageInfo)
        {
            // 检查冷却
            if (Time.time - lastBlockTime < blockCooldown)
            {
                return false;
            }

            // 检查是否正在瞄准
            if (character == null || !character.IsAiming())
            {
                return false;
            }

            // 检查体力是否足够
            if (character.CurrentStamina < staminaCost)
            {
                Debug.Log("[名刀月影] 体力不足，无法格挡");
                return false;
            }

            // 格挡成功
            lastBlockTime = Time.time;

            // 消耗体力
            character.UseStamina(staminaCost);

            Debug.Log($"[名刀月影] 格挡成功！消耗 {staminaCost} 点体力");
            return true;
        }

        /// <summary>
        /// 反弹子弹到发射方向
        /// </summary>
        private void DeflectBullet(DamageInfo damageInfo)
        {
            Vector3 deflectDirection;

            if (damageInfo.fromCharacter != null)
            {
                deflectDirection = (damageInfo.fromCharacter.transform.position - character.transform.position).normalized;
            }
            else
            {
                deflectDirection = -damageInfo.damageNormal;
                if (deflectDirection == Vector3.zero)
                {
                    deflectDirection = character.transform.forward;
                }
            }

            CreateDeflectedBullet(damageInfo.damagePoint, deflectDirection, damageInfo);
        }

        private void CreateDeflectedBullet(Vector3 position, Vector3 direction, DamageInfo originalDamage)
        {
            if (position == Vector3.zero)
            {
                position = character.transform.position + Vector3.up;
            }

            GameObject deflectedBullet = new GameObject("DeflectedBullet");
            deflectedBullet.transform.position = position;
            deflectedBullet.transform.rotation = Quaternion.LookRotation(direction);

            DeflectedBulletProjectile projectile = deflectedBullet.AddComponent<DeflectedBulletProjectile>();
            projectile.direction = direction;
            projectile.speed = deflectSpeed;
            projectile.damage = originalDamage.damageValue * deflectDamageRatio;
            projectile.owner = character;
            projectile.maxDistance = deflectMaxDistance;

            CreateDeflectVisual(deflectedBullet);

            Debug.Log($"[名刀月影] 子弹已反弹，方向: {direction}，伤害: {projectile.damage}");
        }

        private void CreateDeflectVisual(GameObject bullet)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "BulletVisual";
            visual.transform.SetParent(bullet.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            Collider collider = visual.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.44f, 0.69f, 0.88f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.44f, 0.69f, 0.88f) * 3f);
            visual.GetComponent<Renderer>().material = material;

            TrailRenderer trail = bullet.AddComponent<TrailRenderer>();
            trail.startWidth = 0.05f;
            trail.endWidth = 0f;
            trail.time = 0.2f;
            trail.material = material;
            trail.startColor = new Color(0.44f, 0.69f, 0.88f, 1f);
            trail.endColor = new Color(0.44f, 0.69f, 0.88f, 0f);
        }

        private void PlayBlockEffect(Vector3 position)
        {
            if (position == Vector3.zero)
            {
                position = character.transform.position + Vector3.up;
            }

            if (blockEffectPrefab != null)
            {
                GameObject effect = Instantiate(blockEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            else
            {
                CreateSimpleBlockEffect(position);
            }
        }

        private void CreateSimpleBlockEffect(Vector3 position)
        {
            GameObject effect = new GameObject("BlockEffect");
            effect.transform.position = position;

            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(0.44f, 0.69f, 0.88f, 1f);
            main.startSize = 0.3f;
            main.startLifetime = 0.5f;
            main.startSpeed = 5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 20)
            });
            emission.rateOverTime = 0;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            Destroy(effect, 1f);
        }

        private void PlayBlockSound()
        {
            if (blockSound != null)
            {
                AudioSource.PlayClipAtPoint(blockSound, character.transform.position);
            }
        }
    }

    /// <summary>
    /// 反弹子弹投射物
    /// </summary>
    public class DeflectedBulletProjectile : MonoBehaviour
    {
        public Vector3 direction;
        public float speed = 30f;
        public float damage = 25f;
        public float maxDistance = 20f;
        public CharacterMainControl owner;

        private Vector3 startPosition;

        void Start()
        {
            startPosition = transform.position;

            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
            collider.isTrigger = true;

            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            Destroy(gameObject, 5f);
        }

        void Update()
        {
            transform.position += direction * speed * Time.deltaTime;

            if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (owner != null && other.transform.IsChildOf(owner.transform))
            {
                return;
            }

            DamageReceiver receiver = other.GetComponent<DamageReceiver>();
            if (receiver != null)
            {
                if (owner != null && receiver.Team == owner.Team)
                {
                    return;
                }

                DamageInfo damageInfo = new DamageInfo(owner);
                damageInfo.damageValue = damage;
                damageInfo.damagePoint = transform.position;
                damageInfo.damageNormal = -direction;

                receiver.Hurt(damageInfo);
                Debug.Log($"[名刀月影] 反弹子弹命中目标，造成 {damage} 点伤害");

                Destroy(gameObject);
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Default") ||
                other.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                Destroy(gameObject);
            }
        }
    }
}
