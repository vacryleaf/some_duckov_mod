using UnityEngine;
using ItemStatsSystem;

namespace WormholeTechMod
{
    /// <summary>
    /// 黑洞手雷使用行为
    /// 继承�?UsageBehavior，通过 UsageUtilities 系统调用
    /// </summary>
    public class BlackHoleGrenadeUse : UsageBehavior
    {
        // 关联的物�?
        private Item item;

        // 使用冷却时间
        private float useCooldown = 1f;
        private float lastUseTime = 0f;

        // 投掷物预制体（动态创建）
        private GameObject projectilePrefab;

        // 重写 DisplaySettings - 让UI显示使用信息
        public override DisplaySettingsData DisplaySettings
        {
            get
            {
                return new DisplaySettingsData
                {
                    display = true,
                    description = "投掷黑洞手雷"
                };
            }
        }

        void Awake()
        {
            item = GetComponent<Item>();
            }

        /// <summary>
        /// 确保投掷物预制体已创�?
        /// </summary>
        private void EnsureProjectilePrefab()
        {
            if (projectilePrefab == null)
            {
                CreateProjectilePrefab();
            }
        }

        /// <summary>
        /// 创建投掷物预制体
        /// </summary>
        private void CreateProjectilePrefab()
        {
            projectilePrefab = new GameObject("BlackHoleGrenadeProjectile");
            projectilePrefab.SetActive(false);
            DontDestroyOnLoad(projectilePrefab);

            // 添加视觉效果
            CreateProjectileVisual(projectilePrefab);

            // 添加投掷物组�?
            var projectile = projectilePrefab.AddComponent<BlackHoleGrenadeProjectile>();
            projectile.delayTime = 2f;
            projectile.damage = 25f;
            projectile.pullRange = 5f;
            projectile.pullForce = 5f;
            projectile.pullDuration = 3f;
            projectile.throwForce = 15f;
            projectile.throwAngle = 30f;
            projectile.hasCollideSound = false;
            projectile.canHurtSelf = true; // 开启友�?

            }

        /// <summary>
        /// 创建投掷物的视觉效果
        /// </summary>
        private void CreateProjectileVisual(GameObject parent)
        {
            Color color = new Color(0.3f, 0f, 0.4f); // 深紫�?

            // 手雷主体
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(parent.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            Material bodyMaterial = new Material(Shader.Find("Standard"));
            bodyMaterial.color = color;
            bodyMaterial.SetFloat("_Metallic", 0.8f);
            bodyMaterial.SetFloat("_Glossiness", 0.9f);
            bodyMaterial.EnableKeyword("_EMISSION");
            bodyMaterial.SetColor("_EmissionColor", new Color(0.4f, 0.1f, 0.6f) * 2f);

            body.GetComponent<Renderer>().material = bodyMaterial;
            Object.Destroy(body.GetComponent<Collider>());

            // 引力�?
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Ring";
            ring.transform.SetParent(parent.transform);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(0.12f, 0.005f, 0.12f);

            Material ringMaterial = new Material(Shader.Find("Standard"));
            ringMaterial.color = new Color(0.4f, 0.1f, 0.6f);
            ringMaterial.SetFloat("_Metallic", 0.8f);
            ringMaterial.SetFloat("_Glossiness", 0.9f);
            ringMaterial.EnableKeyword("_EMISSION");
            ringMaterial.SetColor("_EmissionColor", new Color(0.5f, 0.2f, 0.8f) * 1.5f);

            ring.GetComponent<Renderer>().material = ringMaterial;
            Object.Destroy(ring.GetComponent<Collider>());

            // 发光效果
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(parent.transform);
            glow.transform.localPosition = Vector3.zero;

            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.4f, 0.1f, 0.6f);
            light.intensity = 0.8f;
            light.range = 1.5f;
        }

        public override bool CanBeUsed(Item item, object user)
        {
            // 检查冷�?
            if (Time.time - lastUseTime < useCooldown)
            {
                return false;
            }

            // 检查使用�?
            if (user is CharacterMainControl character)
            {
                return character != null;
            }

            return false;
        }

        protected override void OnUse(Item item, object user)
        {
            lastUseTime = Time.time;

            if (user is CharacterMainControl character)
            {
                ThrowGrenade(character);
            }
            else
            {
                }
        }

        /// <summary>
        /// 投掷手雷
        /// </summary>
        private void ThrowGrenade(CharacterMainControl thrower)
        {
            // 确保投掷物预制体已创�?
            EnsureProjectilePrefab();

            if (projectilePrefab == null)
            {
                return;
            }

            // 计算投掷起始位置（角色前方稍高处�?
            Vector3 throwPosition = thrower.transform.position +
                                    thrower.transform.forward * 0.5f +
                                    Vector3.up * 1.5f;

            // 计算投掷方向（角色面向方向）
            Vector3 throwDirection = thrower.transform.forward;

            // 创建投掷物实�?
            GameObject grenadeInstance = Instantiate(projectilePrefab, throwPosition, Quaternion.identity);
            grenadeInstance.SetActive(true);

            // 获取投掷物组件并投掷
            var projectile = grenadeInstance.GetComponent<BlackHoleGrenadeProjectile>();
            if (projectile != null)
            {
                projectile.Throw(thrower, throwDirection);
            }

            // 显示提示
            ShowMessage("黑洞手雷已投掷！");

            }

        /// <summary>
        /// 显示消息提示
        /// </summary>
        private void ShowMessage(string message)
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter != null)
            {
                // 使用角色�?PopText 方法显示文字
                mainCharacter.PopText(message);
            }
        }

        void OnDestroy()
        {
            if (projectilePrefab != null)
            {
                Destroy(projectilePrefab);
            }
        }
    }
}

