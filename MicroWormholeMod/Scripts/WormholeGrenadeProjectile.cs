using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MicroWormholeMod
{
    /// <summary>
    /// 虫洞手雷投掷物组件
    /// 处理投掷、落地、延迟爆炸和范围传送逻辑
    /// </summary>
    public class WormholeGrenadeProjectile : MonoBehaviour
    {
        // ========== 配置参数 ==========

        /// <summary>
        /// 引爆延迟时间（秒）
        /// </summary>
        public float fuseTime = 3f;

        /// <summary>
        /// 传送范围半径
        /// </summary>
        public float teleportRadius = 8f;

        /// <summary>
        /// 投掷力度
        /// </summary>
        public float throwForce = 15f;

        /// <summary>
        /// 向上投掷角度偏移
        /// </summary>
        public float throwAngle = 30f;

        // ========== 内部状态 ==========

        // 刚体组件
        private Rigidbody rb;

        // 是否已投掷
        private bool isThrown = false;

        // 是否已引爆
        private bool hasExploded = false;

        // 投掷者引用
        private CharacterMainControl thrower;

        // 特效引用
        private ParticleSystem warningParticles;

        /// <summary>
        /// 初始化组件
        /// </summary>
        void Awake()
        {
            // 添加刚体
            rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            rb.mass = 0.3f;
            rb.drag = 0.5f;
            rb.angularDrag = 0.5f;
            rb.useGravity = true;
            rb.isKinematic = true; // 初始时禁用物理，等待投掷

            // 添加碰撞体
            var collider = gameObject.GetComponent<SphereCollider>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<SphereCollider>();
                collider.radius = 0.1f;
            }

            Debug.Log("[虫洞手雷] 投掷物组件初始化完成");
        }

        /// <summary>
        /// 投掷手雷
        /// </summary>
        /// <param name="throwerCharacter">投掷者</param>
        /// <param name="throwDirection">投掷方向</param>
        public void Throw(CharacterMainControl throwerCharacter, Vector3 throwDirection)
        {
            if (isThrown) return;

            thrower = throwerCharacter;
            isThrown = true;

            // 启用物理
            rb.isKinematic = false;

            // 计算投掷方向（加入向上的角度）
            Vector3 adjustedDirection = Quaternion.AngleAxis(-throwAngle, Vector3.Cross(throwDirection, Vector3.up)) * throwDirection;
            adjustedDirection = adjustedDirection.normalized;

            // 施加投掷力
            rb.AddForce(adjustedDirection * throwForce, ForceMode.Impulse);

            // 添加旋转
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

            // 开始引信倒计时
            StartCoroutine(FuseCountdown());

            // 创建警告特效
            CreateWarningEffect();

            Debug.Log($"[虫洞手雷] 已投掷，方向: {adjustedDirection}, 力度: {throwForce}");
        }

        /// <summary>
        /// 引信倒计时协程
        /// </summary>
        private IEnumerator FuseCountdown()
        {
            float elapsed = 0f;

            // 倒计时期间增强警告效果
            while (elapsed < fuseTime)
            {
                elapsed += Time.deltaTime;

                // 最后1秒加快闪烁
                if (fuseTime - elapsed < 1f && warningParticles != null)
                {
                    var emission = warningParticles.emission;
                    emission.rateOverTime = 100f;
                }

                yield return null;
            }

            // 引爆
            Explode();
        }

        /// <summary>
        /// 引爆手雷
        /// </summary>
        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;

            Debug.Log("[虫洞手雷] 引爆！");

            // 播放爆炸特效
            CreateExplosionEffect();

            // 获取范围内的所有角色并传送
            TeleportCharactersInRange();

            // 销毁手雷
            Destroy(gameObject, 0.5f);
        }

        /// <summary>
        /// 传送范围内的所有角色
        /// </summary>
        private void TeleportCharactersInRange()
        {
            Vector3 explosionCenter = transform.position;

            // 获取范围内的所有碰撞体
            Collider[] colliders = Physics.OverlapSphere(explosionCenter, teleportRadius);

            List<CharacterMainControl> affectedCharacters = new List<CharacterMainControl>();

            foreach (var collider in colliders)
            {
                // 尝试获取角色组件
                CharacterMainControl character = collider.GetComponentInParent<CharacterMainControl>();
                if (character != null && !affectedCharacters.Contains(character))
                {
                    affectedCharacters.Add(character);
                }

                // 也检查AI角色
                var aiController = collider.GetComponentInParent<AICharacterController>();
                if (aiController != null)
                {
                    var aiCharacter = aiController.GetComponentInParent<CharacterMainControl>();
                    if (aiCharacter != null && !affectedCharacters.Contains(aiCharacter))
                    {
                        affectedCharacters.Add(aiCharacter);
                    }
                }
            }

            Debug.Log($"[虫洞手雷] 范围内发现 {affectedCharacters.Count} 个角色");

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

            // 获取随机位置
            Vector3 randomPosition = GetRandomPositionOnMap();

            if (randomPosition != Vector3.zero)
            {
                // 保存原位置用于特效
                Vector3 originalPosition = character.transform.position;

                // 传送角色
                character.transform.position = randomPosition;

                // 在原位置播放消失特效
                CreateTeleportEffect(originalPosition, new Color(0.6f, 0.2f, 1f, 0.8f));

                // 在新位置播放出现特效
                CreateTeleportEffect(randomPosition, new Color(0.2f, 0.8f, 1f, 0.8f));

                Debug.Log($"[虫洞手雷] 角色 {character.name} 从 {originalPosition} 传送到 {randomPosition}");

                // 显示提示
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
            // 尝试多次寻找有效位置
            for (int attempt = 0; attempt < 20; attempt++)
            {
                // 在一定范围内生成随机位置
                Vector3 randomDirection = Random.insideUnitSphere;
                randomDirection.y = 0; // 保持水平
                randomDirection = randomDirection.normalized;

                // 随机距离（20-100米）
                float randomDistance = Random.Range(20f, 100f);
                Vector3 candidatePosition = transform.position + randomDirection * randomDistance;

                // 使用射线检测找到地面
                RaycastHit hit;
                if (Physics.Raycast(candidatePosition + Vector3.up * 50f, Vector3.down, out hit, 100f))
                {
                    // 检查是否是有效的地面
                    if (hit.collider != null && !hit.collider.isTrigger)
                    {
                        Vector3 groundPosition = hit.point + Vector3.up * 0.5f;

                        // 检查该位置是否有足够空间
                        if (!Physics.CheckSphere(groundPosition, 0.5f))
                        {
                            return groundPosition;
                        }
                    }
                }
            }

            // 如果找不到有效位置，返回当前位置附近
            Debug.LogWarning("[虫洞手雷] 无法找到有效的随机位置");
            return transform.position + Random.insideUnitSphere * 10f;
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
            main.startColor = new Color(1f, 0.5f, 0f, 0.8f); // 橙色警告
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

            // 主爆炸粒子
            ParticleSystem particles = effectObj.AddComponent<ParticleSystem>();

            var main = particles.main;
            main.startColor = new Color(0.5f, 0.2f, 1f, 0.9f); // 紫色虫洞效果
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

            // 添加颜色渐变
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

            // 创建范围指示器（环形扩散）
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
            main.startSpeed = teleportRadius * 2f;
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
        /// 显示消息提示（使用 CharacterMainControl.PopText 方法）
        /// </summary>
        private void ShowMessage(string message)
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter != null)
            {
                // 使用角色的 PopText 方法显示文字
                mainCharacter.PopText(message);
            }
        }

        /// <summary>
        /// 碰撞检测
        /// </summary>
        void OnCollisionEnter(Collision collision)
        {
            if (!isThrown) return;

            // 落地后增加阻力，减缓滚动
            rb.drag = 2f;
            rb.angularDrag = 2f;

            Debug.Log($"[虫洞手雷] 碰撞到: {collision.gameObject.name}");
        }
    }
}
