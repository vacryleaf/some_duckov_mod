using UnityEngine;
using System.Collections.Generic;

namespace MoonlightSwordMod
{
    /// <summary>
    /// 剑气投射物
    /// 处理剑气的飞行、碰撞检测、敌人击中和子弹偏转
    /// </summary>
    public class SwordAuraProjectile : MonoBehaviour
    {
        [Header("投射物参数")]
        public float damage = 90f;              // 基础伤害
        public float speed = 15f;               // 飞行速度(米/秒)
        public float maxDistance = 10f;         // 最大飞行距离
        public int pierceCount = 3;             // 穿透数量

        [Header("运行状态")]
        public GameObject owner;                // 发射者
        private Vector3 direction;              // 飞行方向
        private float traveledDistance = 0f;    // 已飞行距离
        private int currentPierceCount = 0;     // 当前穿透计数
        private bool isDestroying = false;      // 是否正在销毁

        [Header("碰撞检测")]
        private List<GameObject> hitEnemies = new List<GameObject>();    // 已击中的敌人
        private int deflectedBulletCount = 0;                            // 偏转的子弹数量
        private float detectRadius = 1.5f;                               // 检测半径

        [Header("特效组件")]
        private TrailRenderer trail;
        private ParticleSystem particles;
        private Light glowLight;
        private AudioSource audioSource;

        [Header("音效")]
        public AudioClip hitSound;              // 击中音效
        public AudioClip deflectSound;          // 偏转音效
        public AudioClip dissipateSound;        // 消散音效

        /// <summary>
        /// 初始化
        /// </summary>
        void Start()
        {
            // 初始化特效组件
            InitializeEffects();

            // 播放飞行音效
            PlayFlightSound();

            Debug.Log("[剑气] 剑气投射物已生成");
        }

        /// <summary>
        /// 初始化视觉特效
        /// </summary>
        private void InitializeEffects()
        {
            // 获取或添加轨迹渲染器
            trail = GetComponent<TrailRenderer>();
            if (trail == null)
            {
                trail = gameObject.AddComponent<TrailRenderer>();
                trail.time = 0.5f;
                trail.startWidth = 0.5f;
                trail.endWidth = 0.1f;
                trail.startColor = new Color(0.44f, 0.69f, 0.88f, 1f);
                trail.endColor = new Color(0.44f, 0.69f, 0.88f, 0f);
                trail.material = new Material(Shader.Find("Sprites/Default"));
            }

            // 获取粒子系统
            particles = GetComponentInChildren<ParticleSystem>();

            // 获取光源
            glowLight = GetComponentInChildren<Light>();

            // 获取音频源
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0.8f; // 3D音效
            }
        }

        /// <summary>
        /// 播放飞行音效
        /// </summary>
        private void PlayFlightSound()
        {
            if (audioSource != null)
            {
                audioSource.loop = true;
                audioSource.volume = 0.6f;
                audioSource.pitch = 1.0f;
                audioSource.Play();
            }
        }

        /// <summary>
        /// 发射剑气
        /// </summary>
        /// <param name="launchDirection">发射方向</param>
        public void Launch(Vector3 launchDirection)
        {
            direction = launchDirection.normalized;
            Debug.Log($"[剑气] 发射方向: {direction}");
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        void Update()
        {
            if (direction == Vector3.zero || isDestroying) return;

            // 移动剑气
            float moveDistance = speed * Time.deltaTime;
            transform.position += direction * moveDistance;
            traveledDistance += moveDistance;

            // 更新特效
            UpdateEffects();

            // 检查是否超出最大距离
            if (traveledDistance >= maxDistance)
            {
                Debug.Log($"[剑气] 达到最大距离: {maxDistance}米");
                DestroyAura();
            }
        }

        /// <summary>
        /// 固定时间步长更新（用于物理检测）
        /// </summary>
        void FixedUpdate()
        {
            if (isDestroying) return;

            // 碰撞检测
            CheckCollisions();
        }

        /// <summary>
        /// 碰撞检测
        /// </summary>
        private void CheckCollisions()
        {
            // 使用OverlapSphere进行范围检测
            Vector3 checkOrigin = transform.position;
            Collider[] hits = Physics.OverlapSphere(checkOrigin, detectRadius);

            foreach (Collider hit in hits)
            {
                // 跳过自己的碰撞体
                if (hit.gameObject == gameObject) continue;

                // 跳过发射者
                if (hit.gameObject == owner) continue;

                // 检测敌人
                if (hit.CompareTag("Enemy"))
                {
                    if (!hitEnemies.Contains(hit.gameObject))
                    {
                        HitEnemy(hit.gameObject);
                    }
                }
                // 检测子弹
                else if (hit.CompareTag("Projectile") || hit.CompareTag("Bullet"))
                {
                    DeflectBullet(hit.gameObject);
                }
                // 检测障碍物
                else if (hit.CompareTag("Obstacle") || hit.CompareTag("Wall"))
                {
                    OnHitObstacle(hit.gameObject);
                }
            }
        }

        /// <summary>
        /// 击中敌人
        /// </summary>
        private void HitEnemy(GameObject enemy)
        {
            Debug.Log($"[剑气] 击中敌人: {enemy.name}");

            // 计算实际伤害
            float actualDamage = CalculateDamage();

            // 应用伤害
            ApplyDamageToEnemy(enemy, actualDamage);

            // 应用击退
            ApplyKnockback(enemy, direction);

            // 播放击中特效
            SpawnHitEffect(enemy.transform.position);

            // 记录已击中的敌人
            hitEnemies.Add(enemy);
            currentPierceCount++;

            // 检查是否达到穿透上限
            if (currentPierceCount >= pierceCount)
            {
                Debug.Log($"[剑气] 达到穿透上限: {pierceCount}");
                DestroyAura();
            }
        }

        /// <summary>
        /// 计算伤害值（考虑穿透衰减）
        /// </summary>
        private float CalculateDamage()
        {
            // 穿透伤害衰减（每次穿透减少10%）
            float pierceDamageMultiplier = 1f - (currentPierceCount * 0.1f);
            pierceDamageMultiplier = Mathf.Max(pierceDamageMultiplier, 0.7f); // 最多衰减30%

            float actualDamage = damage * pierceDamageMultiplier;

            // 暴击判定 (15%几率)
            if (Random.value < 0.15f)
            {
                actualDamage *= 1.8f;
                Debug.Log("[剑气] 暴击!");
            }

            return actualDamage;
        }

        /// <summary>
        /// 对敌人应用伤害
        /// </summary>
        private void ApplyDamageToEnemy(GameObject enemy, float damageAmount)
        {
            // 尝试使用IDamageable接口
            var damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damageAmount);
            }
            else
            {
                // 备用方案: 使用SendMessage
                enemy.SendMessage("TakeDamage", damageAmount, SendMessageOptions.DontRequireReceiver);
            }

            Debug.Log($"[剑气] 对 {enemy.name} 造成 {damageAmount:F1} 伤害");
        }

        /// <summary>
        /// 应用击退效果
        /// </summary>
        private void ApplyKnockback(GameObject target, Vector3 knockbackDirection)
        {
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 计算击退力
                Vector3 knockbackForce = knockbackDirection.normalized * 500f;

                // 添加向上的分量
                knockbackForce.y += 100f;

                // 应用力
                rb.AddForce(knockbackForce, ForceMode.Impulse);

                Debug.Log($"[剑气] 击退 {target.name}");
            }
        }

        /// <summary>
        /// 生成击中特效
        /// </summary>
        private void SpawnHitEffect(Vector3 position)
        {
            // 创建击中特效
            GameObject effect = new GameObject("HitEffect");
            effect.transform.position = position;

            // 添加粒子系统
            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.44f, 0.69f, 0.88f, 1f);
            main.startSize = 0.3f;
            main.startLifetime = 0.5f;
            main.maxParticles = 30;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 30)
            });

            // 播放音效
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, position, 0.7f);
            }

            // 自动销毁
            Destroy(effect, 2f);
        }

        /// <summary>
        /// 偏转子弹
        /// </summary>
        private void DeflectBullet(GameObject bullet)
        {
            Debug.Log($"[剑气] 偏转子弹: {bullet.name}");

            // 获取子弹的Rigidbody
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

            if (bulletRb != null)
            {
                // 反弹子弹
                Vector3 currentVelocity = bulletRb.velocity;
                float bulletSpeed = currentVelocity.magnitude;

                // 计算反向
                Vector3 deflectDirection = -currentVelocity.normalized;

                // 应用新速度（增加50%）
                bulletRb.velocity = deflectDirection * bulletSpeed * 1.5f;

                // 尝试改变子弹的所有者
                ChangeProjectileOwner(bullet);

                Debug.Log($"[剑气] 子弹已反弹");
            }
            else
            {
                // 没有Rigidbody，直接销毁
                Destroy(bullet);
                Debug.Log($"[剑气] 子弹已销毁");
            }

            // 播放偏转特效
            SpawnDeflectEffect(bullet.transform.position);

            // 统计
            deflectedBulletCount++;
        }

        /// <summary>
        /// 改变投射物的所有者（如果可能）
        /// </summary>
        private void ChangeProjectileOwner(GameObject projectile)
        {
            // 尝试查找投射物脚本并改变所有者
            // 这需要根据实际游戏的投射物系统来调整
            var projectileScript = projectile.GetComponent("Projectile");
            if (projectileScript != null)
            {
                // 使用反射设置owner字段
                var ownerField = projectileScript.GetType().GetField("owner");
                if (ownerField != null)
                {
                    ownerField.SetValue(projectileScript, owner);
                    Debug.Log("[剑气] 子弹所有权已改变");
                }
            }
        }

        /// <summary>
        /// 生成偏转特效
        /// </summary>
        private void SpawnDeflectEffect(Vector3 position)
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

            // 播放音效
            if (deflectSound != null)
            {
                AudioSource.PlayClipAtPoint(deflectSound, position, 0.8f);
            }

            // 自动销毁
            Destroy(spark, 1f);
        }

        /// <summary>
        /// 击中障碍物
        /// </summary>
        private void OnHitObstacle(GameObject obstacle)
        {
            Debug.Log($"[剑气] 击中障碍物: {obstacle.name}");
            DestroyAura();
        }

        /// <summary>
        /// 更新特效
        /// </summary>
        private void UpdateEffects()
        {
            // 更新轨迹宽度（根据剩余距离）
            if (trail != null)
            {
                float distanceRatio = 1f - (traveledDistance / maxDistance);
                trail.startWidth = 0.5f * distanceRatio;
            }

            // 更新发光强度
            if (glowLight != null)
            {
                float distanceRatio = 1f - (traveledDistance / maxDistance);
                glowLight.intensity = 2f * distanceRatio;
            }
        }

        /// <summary>
        /// 销毁剑气
        /// </summary>
        private void DestroyAura()
        {
            if (isDestroying) return;
            isDestroying = true;

            Debug.Log($"[剑气] 销毁 - 击中敌人: {hitEnemies.Count}, 偏转子弹: {deflectedBulletCount}");

            // 停止音效
            if (audioSource != null)
            {
                audioSource.Stop();
            }

            // 播放消散音效
            if (dissipateSound != null)
            {
                AudioSource.PlayClipAtPoint(dissipateSound, transform.position, 0.5f);
            }

            // 停止粒子发射（但让现有粒子播完）
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
        /// 渐隐效果
        /// </summary>
        private System.Collections.IEnumerator FadeOut()
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

        /// <summary>
        /// 调试可视化
        /// </summary>
        void OnDrawGizmos()
        {
            // 绘制检测范围
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectRadius);

            // 绘制剩余飞行距离
            if (direction != Vector3.zero)
            {
                float remainingDistance = maxDistance - traveledDistance;
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, direction * remainingDistance);
            }
        }
    }
}
