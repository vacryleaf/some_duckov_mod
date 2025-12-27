using UnityEngine;
using ItemStatsSystem;

namespace WormholeTechMod
{
    /// <summary>
    /// 黑洞手雷使用行为
    /// 继承自 UsageBehavior，通过 UsageUtilities 系统调用
    /// </summary>
    public class BlackHoleGeneratorUse : UsageBehavior
    {
        // 关联的物品
        private Item item;

        // 使用冷却时间
        private float useCooldown = 2f;
        private float lastUseTime = 0f;

        // 投掷物预制体（动态创建）
        private GameObject projectilePrefab;

        // 黑洞属性配置
        [Header("黑洞属性")]
        public float blackHoleDuration = 5f;
        public float pullRange = 10f;
        public float pullForce = 15f;
        public float damagePerSecond = 10f;
        public bool canHurtSelf = false;

        // 重写 DisplaySettings - 让UI显示使用信息
        public override DisplaySettingsData DisplaySettings
        {
            get
            {
                return new DisplaySettingsData
                {
                    display = true,
                    description = $"投掷后生成黑洞，吸引范围{pullRange}米内的敌人并造成每秒{damagePerSecond}点伤害，持续{blackHoleDuration}秒"
                };
            }
        }

        void Awake()
        {
            item = GetComponent<Item>();

            // 创建投掷物预制体
            CreateProjectilePrefab();

            ModLogger.Log("[微型黑洞] BlackHoleGeneratorUse行为初始化完成");
        }

        /// <summary>
        /// 创建投掷物预制体
        /// </summary>
        private void CreateProjectilePrefab()
        {
            projectilePrefab = new GameObject("BlackHoleGeneratorProjectile");
            projectilePrefab.SetActive(false);
            DontDestroyOnLoad(projectilePrefab);

            // 添加视觉效果
            CreateProjectileVisual(projectilePrefab);

            // 添加投掷物组件
            var projectile = projectilePrefab.AddComponent<BlackHoleProjectile>();
            projectile.blackHoleDuration = blackHoleDuration;
            projectile.pullRange = pullRange;
            projectile.pullForce = pullForce;
            projectile.damagePerSecond = damagePerSecond;
            projectile.canHurtSelf = canHurtSelf;
            projectile.delayTime = 1.5f;
            projectile.throwForce = 12f;
            projectile.throwAngle = 25f;
            projectile.collideSound = "GrenadeCollide";

            ModLogger.Log("[微型黑洞] 投掷物预制体创建完成");
        }

        /// <summary>
        /// 创建投掷物的视觉效果
        /// </summary>
        private void CreateProjectileVisual(GameObject parent)
        {
            Color primaryColor = new Color(0.4f, 0.1f, 0.6f);  // 深紫色
            Color secondaryColor = new Color(0.8f, 0.4f, 1f);  // 亮紫色

            // 发生器主体（球形）
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(parent.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);

            Material bodyMaterial = new Material(Shader.Find("Standard"));
            bodyMaterial.color = primaryColor;
            bodyMaterial.SetFloat("_Metallic", 0.8f);
            bodyMaterial.SetFloat("_Glossiness", 0.9f);
            bodyMaterial.EnableKeyword("_EMISSION");
            bodyMaterial.SetColor("_EmissionColor", secondaryColor * 1.5f);

            body.GetComponent<Renderer>().material = bodyMaterial;
            Object.Destroy(body.GetComponent<Collider>());

            // 外环
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Ring";
            ring.transform.SetParent(parent.transform);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(0.15f, 0.01f, 0.15f);

            Material ringMaterial = new Material(Shader.Find("Standard"));
            ringMaterial.color = Color.black;
            ringMaterial.SetFloat("_Metallic", 1f);
            ringMaterial.SetFloat("_Glossiness", 1f);
            ring.GetComponent<Renderer>().material = ringMaterial;
            Object.Destroy(ring.GetComponent<Collider>());

            // 内部光芯
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "Core";
            core.transform.SetParent(parent.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            Material coreMaterial = new Material(Shader.Find("Standard"));
            coreMaterial.color = Color.white;
            coreMaterial.EnableKeyword("_EMISSION");
            coreMaterial.SetColor("_EmissionColor", Color.white * 3f);
            core.GetComponent<Renderer>().material = coreMaterial;
            Object.Destroy(core.GetComponent<Collider>());

            // 添加发光效果
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(parent.transform);
            glow.transform.localPosition = Vector3.zero;

            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = secondaryColor;
            light.intensity = 0.8f;
            light.range = 1.5f;
        }

        /// <summary>
        /// 实现 CanBeUsed - 决定物品是否可用
        /// </summary>
        public override bool CanBeUsed(Item item, object user)
        {
            // 检查冷却
            if (Time.time - lastUseTime < useCooldown)
            {
                return false;
            }

            // 检查使用者
            if (user is CharacterMainControl character)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 实现 OnUse - 执行投掷逻辑
        /// </summary>
        protected override void OnUse(Item item, object user)
        {
            lastUseTime = Time.time;

            // Debug.Log($"[微型黑洞] 发生器被使用，使用者: {user}");

            // 获取投掷者
            CharacterMainControl thrower = user as CharacterMainControl;
            if (thrower == null)
            {
                thrower = CharacterMainControl.Main;
            }

            if (thrower == null)
            {
                ModLogger.LogWarning("[微型黑洞] 找不到投掷者");
                return;
            }

            // 投掷黑洞发生器
            ThrowBlackHoleGenerator(thrower);
        }

        /// <summary>
        /// 投掷黑洞发生器
        /// </summary>
        private void ThrowBlackHoleGenerator(CharacterMainControl thrower)
        {
            if (projectilePrefab == null)
            {
                ModLogger.LogError("[微型黑洞] 投掷物预制体为空");
                return;
            }

            // 计算投掷起始位置（角色前方稍高处）
            Vector3 throwPosition = thrower.transform.position +
                                    thrower.transform.forward * 0.5f +
                                    Vector3.up * 1.5f;

            // 计算投掷方向（角色面向方向）
            Vector3 throwDirection = thrower.transform.forward;

            // 创建投掷物实例
            GameObject generatorInstance = Object.Instantiate(projectilePrefab, throwPosition, Quaternion.identity);
            generatorInstance.SetActive(true);

            // 获取投掷物组件并投掷
            var projectile = generatorInstance.GetComponent<BlackHoleProjectile>();
            if (projectile != null)
            {
                // 同步属性
                projectile.blackHoleDuration = blackHoleDuration;
                projectile.pullRange = pullRange;
                projectile.pullForce = pullForce;
                projectile.damagePerSecond = damagePerSecond;
                projectile.canHurtSelf = canHurtSelf;

                // 设置伤害信息
                DamageInfo dmgInfo = new DamageInfo(thrower);
                dmgInfo.damageType = DamageTypes.normal;
                projectile.SetDamageInfo(dmgInfo);

                // 投掷
                projectile.Throw(thrower, throwDirection);
            }

            // 显示提示
            ShowMessage("微型黑洞已投出！");

            // Debug.Log($"[微型黑洞] 发生器投掷成功，位置: {throwPosition}, 方向: {throwDirection}");
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

        void OnDestroy()
        {
            if (projectilePrefab != null)
            {
                Object.Destroy(projectilePrefab);
            }
        }
    }
}
