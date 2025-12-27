using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using ItemStatsSystem;
using SodaCraft.Localizations;

namespace WormholeTechMod
{
    /// <summary>
    /// 虫洞物品工厂
    /// 负责创建所有虫洞系列物品的 Prefab
    /// </summary>
    public static class WormholeItemFactory
    {
        // ========== 物品 TypeID 常量 ==========
        public const int WORMHOLE_TYPE_ID = 990001;   // 微型虫洞
        public const int RECALL_TYPE_ID = 990002;     // 回溯虫洞
        public const int GRENADE_TYPE_ID = 990003;    // 虫洞手雷
        public const int BADGE_TYPE_ID = 990004;      // 虫洞徽章
        public const int BLACKHOLE_TYPE_ID = 990005;  // 微型黑洞发生器
        public const int TIME_REWIND_TYPE_ID = 990006; // 时空回溯
        public const int WORMHOLE_NETWORK_TYPE_ID = 990007; // 虫洞网络

        #region 物品创建

        /// <summary>
        /// 创建微型虫洞物品
        /// </summary>
        public static Item CreateWormholeItem(AssetBundle assetBundle, Sprite icon)
        {
            ModLogger.Log("[微型虫洞] 开始创建微型虫洞Prefab...");

            GameObject itemObj = CreateItemGameObject("MicroWormhole", new Color(0.5f, 0.2f, 0.9f), assetBundle);

            UnityEngine.Object.DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            // 先添加 UsageBehavior 组件
            itemObj.AddComponent<MicroWormholeUse>();

            Item prefab = itemObj.AddComponent<Item>();
            ConfigureUsableItemProperties(prefab, WORMHOLE_TYPE_ID, "微型虫洞",
                "高科技传送装置。使用后会记录当前位置并撤离回家。\n\n<color=#FFD700>配合「回溯虫洞」使用，可返回记录的位置</color>",
                icon, typeof(MicroWormholeUse));

            // 添加 AgentUtilities 自动修复组件
            itemObj.AddComponent<AgentUtilitiesFixer>();

            ModLogger.Log("[微型虫洞] 微型虫洞Prefab创建完成");
            return prefab;
        }

        /// <summary>
        /// 创建虫洞回溯物品
        /// </summary>
        public static Item CreateRecallItem(Sprite icon)
        {
            ModLogger.Log("[微型虫洞] 开始创建虫洞回溯Prefab...");

            GameObject itemObj = CreateItemGameObject("WormholeRecall", new Color(0.2f, 0.8f, 0.5f), null);

            UnityEngine.Object.DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            // 先添加 UsageBehavior 组件
            itemObj.AddComponent<WormholeRecallUse>();

            Item prefab = itemObj.AddComponent<Item>();
            ConfigureUsableItemProperties(prefab, RECALL_TYPE_ID, "回溯虫洞",
                "虫洞传送的配套装置。在家中使用，可以传送回「微型虫洞」记录的位置。\n\n<color=#FFD700>只能在家中使用</color>",
                icon, typeof(WormholeRecallUse), 1000);

            // 添加 AgentUtilities 自动修复组件
            itemObj.AddComponent<AgentUtilitiesFixer>();

            ModLogger.Log("[微型虫洞] 虫洞回溯Prefab创建完成");
            return prefab;
        }

        /// <summary>
        /// 创建虫洞手雷物品
        /// </summary>
        public static Item CreateGrenadeItem(Sprite icon, out WormholeGrenadeSkill grenadeSkill)
        {
            ModLogger.Log("[微型虫洞] 开始创建虫洞手雷Prefab...");

            GameObject itemObj = CreateGrenadeGameObject("WormholeGrenade", new Color(1f, 0.5f, 0.2f));

            UnityEngine.Object.DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            Item prefab = itemObj.AddComponent<Item>();
            grenadeSkill = ConfigureGrenadeProperties(prefab, GRENADE_TYPE_ID, "虫洞手雷",
                "高科技空间扰乱装置。投掷后引爆，将范围内的所有生物随机传送到地图某处。\n\n<color=#87CEEB>特殊效果：</color>\n• 引信延迟：3秒\n• 传送范围：8米\n• 影响所有角色（包括自己）\n\n<color=#FFD700>「混乱是战场上最好的掩护」</color>",
                icon);

            // 添加 AgentUtilities 自动修复组件
            itemObj.AddComponent<AgentUtilitiesFixer>();

            ModLogger.Log("[微型虫洞] 虫洞手雷Prefab创建完成");
            return prefab;
        }

        /// <summary>
        /// 创建虫洞徽章物品
        /// </summary>
        public static Item CreateBadgeItem(Sprite icon)
        {
            ModLogger.Log("[微型虫洞] 开始创建虫洞徽章Prefab...");

            GameObject itemObj = CreateBadgeGameObject("WormholeBadge", new Color(0.2f, 0.8f, 1f));

            UnityEngine.Object.DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            Item prefab = itemObj.AddComponent<Item>();

            CreateBadgeEffect(itemObj, prefab);

            ConfigureBadgeProperties(prefab, BADGE_TYPE_ID, "虫洞徽章",
                "蕴含虫洞能量的神秘徽章。被动效果：受到伤害时有10%概率闪避伤害。\n\n<color=#87CEEB>被动效果：</color>\n• 10%伤害闪避概率\n• 多个徽章乘法叠加\n\n<color=#FFD700>「空间裂缝是最好的盾牌」</color>",
                icon);

            itemObj.AddComponent<AgentUtilitiesFixer>();

            ModLogger.Log("[微型虫洞] 虫洞徽章Prefab创建完成");
            return prefab;
        }

        /// <summary>
        /// 创建黑洞手雷物品（技能系统，装备后蓄力投掷）
        /// </summary>
        public static Item CreateBlackHoleItem(Sprite icon)
        {
            // 创建物品根对象
            GameObject itemObj = new GameObject("BlackHoleGrenade");
            UnityEngine.Object.DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            // 创建视觉效果
            CreateBlackHoleVisual(itemObj, new Color(0.2f, 0f, 0.3f));

            // 添加 Item 组件
            Item prefab = itemObj.AddComponent<Item>();
            ConfigureBlackHoleGrenadeProperties(prefab, BLACKHOLE_TYPE_ID, "黑洞手雷",
                "高科技引力武器。装备后按射击键蓄力投掷，产生黑洞引力场将敌人聚集到中心并造成持续伤害。\n\n<color=#87CEEB>操作：</color>\n• 装备到副手\n• 按住射击键蓄力\n• 松开投掷\n\n<color=#87CEEB>效果：</color>\n• 引力持续时间：3秒\n• 吸引范围：5米\n• 每0.5秒造成25点伤害\n\n<color=#FFD700>「引力是战场的主宰」</color>",
                icon);

            // 添加 AgentUtilities 自动修复组件
            itemObj.AddComponent<AgentUtilitiesFixer>();

            return prefab;
        }

        #endregion

        #region GameObject 创建

        /// <summary>
        /// 创建物品 GameObject
        /// </summary>
        private static GameObject CreateItemGameObject(string name, Color color, AssetBundle assetBundle)
        {
            GameObject itemObj = null;

            // 尝试从 AssetBundle 加载模型
            if (assetBundle != null)
            {
                GameObject prefab = assetBundle.LoadAsset<GameObject>("MicroWormhole");
                if (prefab != null)
                {
                    itemObj = UnityEngine.Object.Instantiate(prefab);
                    itemObj.name = name;
                }
            }

            // 如果加载失败，程序生成模型
            if (itemObj == null)
            {
                itemObj = new GameObject(name);
                CreateWormholeVisual(itemObj, color);
            }

            return itemObj;
        }

        /// <summary>
        /// 创建虫洞手雷 GameObject
        /// </summary>
        private static GameObject CreateGrenadeGameObject(string name, Color color)
        {
            GameObject itemObj = new GameObject(name);
            CreateGrenadeVisual(itemObj, color);
            return itemObj;
        }

        /// <summary>
        /// 创建虫洞徽章 GameObject
        /// </summary>
        private static GameObject CreateBadgeGameObject(string name, Color color)
        {
            GameObject itemObj = new GameObject(name);
            CreateBadgeVisual(itemObj, color);
            return itemObj;
        }

        /// <summary>
        /// 创建黑洞发生器 GameObject
        /// </summary>
        private static GameObject CreateBlackHoleGameObject(string name, Color color)
        {
            GameObject itemObj = new GameObject(name);
            CreateBlackHoleVisual(itemObj, color);
            return itemObj;
        }

        #endregion

        #region 视觉效果创建

        /// <summary>
        /// 创建虫洞的视觉效果
        /// </summary>
        private static void CreateWormholeVisual(GameObject parent, Color color)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(parent.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(color.r, color.g, color.b, 0.9f);
            material.SetFloat("_Metallic", 0.8f);
            material.SetFloat("_Glossiness", 0.9f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2f);

            visual.GetComponent<Renderer>().material = material;
            UnityEngine.Object.Destroy(visual.GetComponent<Collider>());

            SphereCollider collider = parent.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
        }

        /// <summary>
        /// 创建手雷的视觉效果
        /// </summary>
        private static void CreateGrenadeVisual(GameObject parent, Color color)
        {
            // 手雷主体（椭圆形）
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(parent.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);

            Material bodyMaterial = new Material(Shader.Find("Standard"));
            bodyMaterial.color = new Color(color.r, color.g, color.b, 1f);
            bodyMaterial.SetFloat("_Metallic", 0.6f);
            bodyMaterial.SetFloat("_Glossiness", 0.7f);
            bodyMaterial.EnableKeyword("_EMISSION");
            bodyMaterial.SetColor("_EmissionColor", color * 1.5f);

            body.GetComponent<Renderer>().material = bodyMaterial;
            UnityEngine.Object.Destroy(body.GetComponent<Collider>());

            // 手雷环带
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Ring";
            ring.transform.SetParent(parent.transform);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(0.1f, 0.01f, 0.1f);

            Material ringMaterial = new Material(Shader.Find("Standard"));
            ringMaterial.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            ringMaterial.SetFloat("_Metallic", 0.9f);
            ring.GetComponent<Renderer>().material = ringMaterial;
            UnityEngine.Object.Destroy(ring.GetComponent<Collider>());

            // 添加发光效果
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(parent.transform);
            glow.transform.localPosition = Vector3.zero;

            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = 0.5f;
            light.range = 1f;

            // 添加拾取碰撞体
            SphereCollider collider = parent.AddComponent<SphereCollider>();
            collider.radius = 0.08f;
        }

        /// <summary>
        /// 创建徽章的视觉效果
        /// </summary>
        private static void CreateBadgeVisual(GameObject parent, Color color)
        {
            // 徽章主体（圆盘形）
            GameObject badge = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            badge.name = "Badge";
            badge.transform.SetParent(parent.transform);
            badge.transform.localPosition = Vector3.zero;
            badge.transform.localScale = new Vector3(0.12f, 0.015f, 0.12f);

            Material badgeMaterial = new Material(Shader.Find("Standard"));
            badgeMaterial.color = new Color(color.r, color.g, color.b, 1f);
            badgeMaterial.SetFloat("_Metallic", 0.9f);
            badgeMaterial.SetFloat("_Glossiness", 0.85f);
            badgeMaterial.EnableKeyword("_EMISSION");
            badgeMaterial.SetColor("_EmissionColor", color * 1.2f);

            badge.GetComponent<Renderer>().material = badgeMaterial;
            UnityEngine.Object.Destroy(badge.GetComponent<Collider>());

            // 中心宝石
            GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gem.name = "Gem";
            gem.transform.SetParent(parent.transform);
            gem.transform.localPosition = new Vector3(0, 0.02f, 0);
            gem.transform.localScale = new Vector3(0.05f, 0.03f, 0.05f);

            Material gemMaterial = new Material(Shader.Find("Standard"));
            gemMaterial.color = new Color(0.6f, 0.3f, 1f, 1f); // 紫色
            gemMaterial.SetFloat("_Metallic", 0.2f);
            gemMaterial.SetFloat("_Glossiness", 1f);
            gemMaterial.EnableKeyword("_EMISSION");
            gemMaterial.SetColor("_EmissionColor", new Color(0.6f, 0.3f, 1f) * 2f);

            gem.GetComponent<Renderer>().material = gemMaterial;
            UnityEngine.Object.Destroy(gem.GetComponent<Collider>());

            // 添加发光效果
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(parent.transform);
            glow.transform.localPosition = Vector3.zero;

            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = 0.3f;
            light.range = 0.5f;

            // 添加拾取碰撞体
            SphereCollider collider = parent.AddComponent<SphereCollider>();
            collider.radius = 0.08f;
        }

        /// <summary>
        /// 创建黑洞发生器的视觉效果
        /// </summary>
        private static void CreateBlackHoleVisual(GameObject parent, Color color)
        {
            Color primaryColor = color;
            Color secondaryColor = new Color(0.8f, 0.4f, 1f);

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
            UnityEngine.Object.Destroy(body.GetComponent<Collider>());

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Ring";
            ring.transform.SetParent(parent.transform);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(0.15f, 0.01f, 0.15f);

            Material ringMaterial = new Material(Shader.Find("Standard"));
            ringMaterial.color = Color.black;
            ringMaterial.SetFloat("_Metallic", 1f);
            ring.GetComponent<Renderer>().material = ringMaterial;
            UnityEngine.Object.Destroy(ring.GetComponent<Collider>());

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
            UnityEngine.Object.Destroy(core.GetComponent<Collider>());

            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(parent.transform);
            glow.transform.localPosition = Vector3.zero;

            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = secondaryColor;
            light.intensity = 0.8f;
            light.range = 1.5f;

            SphereCollider collider = parent.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
        }

        #endregion

        #region 属性配置

        /// <summary>
        /// 配置可使用物品属性
        /// </summary>
        private static void ConfigureUsableItemProperties(Item item, int typeId, string nameKey, string descKey, Sprite icon, Type usageBehaviorType, int price = 500)
        {
            SetFieldValue(item, "typeID", typeId);
            SetFieldValue(item, "displayName", nameKey);
            SetFieldValue(item, "description", descKey);

            if (icon != null)
            {
                SetFieldValue(item, "icon", icon);
            }

            SetFieldValue(item, "stackable", true);
            SetFieldValue(item, "maxStackCount", 5);
            SetFieldValue(item, "usable", true);
            SetFieldValue(item, "quality", 4);
            SetFieldValue(item, "value", price);
            SetFieldValue(item, "weight", 0.1f);

            // 手动添加 UsageUtilities 组件
            UsageUtilities usageUtilities = item.gameObject.AddComponent<UsageUtilities>();
            SetFieldValue(usageUtilities, "useTime", 1.5f);
            SetFieldValue(usageUtilities, "useDurability", false);

            // 添加 UsageBehavior
            var useBehavior = item.gameObject.GetComponent(usageBehaviorType) as UsageBehavior;
            if (useBehavior != null && usageUtilities.behaviors != null)
            {
                usageUtilities.behaviors.Add(useBehavior);
            }

            // 设置 usageUtilities 字段到 Item
            SetFieldValue(item, "usageUtilities", usageUtilities);

            // 初始化物品
            item.Initialize();

            // Debug.Log($"[微型虫洞] 物品 {typeId} 配置完成");
        }

        /// <summary>
        /// 配置虫洞手雷物品属性（使用技能系统）
        /// </summary>
        private static WormholeGrenadeSkill ConfigureGrenadeProperties(Item item, int typeId, string nameKey, string descKey, Sprite icon)
        {
            SetFieldValue(item, "typeID", typeId);
            SetFieldValue(item, "displayName", nameKey);
            SetFieldValue(item, "description", descKey);

            if (icon != null)
            {
                SetFieldValue(item, "icon", icon);
            }

            SetFieldValue(item, "stackable", true);
            SetFieldValue(item, "maxStackCount", 3);
            SetFieldValue(item, "quality", 5);
            SetFieldValue(item, "value", 500);
            SetFieldValue(item, "weight", 0.3f);
            SetFieldValue(item, "soundKey", "Grenade");

            // 在物品对象下创建技能子对象
            GameObject skillObj = new GameObject("WormholeGrenadeSkill");
            skillObj.transform.SetParent(item.gameObject.transform);
            skillObj.transform.localPosition = Vector3.zero;

            // 添加技能组件
            var grenadeSkill = skillObj.AddComponent<WormholeGrenadeSkill>();
            grenadeSkill.staminaCost = 0f;
            grenadeSkill.coolDownTime = 0.5f;
            grenadeSkill.damageRange = 4f;
            grenadeSkill.delayTime = 1f;
            grenadeSkill.throwForce = 15f;
            grenadeSkill.throwAngle = 30f;
            grenadeSkill.canControlCastDistance = true;
            grenadeSkill.grenadeVerticleSpeed = 10f;
            grenadeSkill.canHurtSelf = true;

            // 设置 SkillContext
            var skillContextField = typeof(SkillBase).GetField("skillContext",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (skillContextField != null)
            {
                var skillContext = new SkillContext
                {
                    castRange = 8f,       // 瞄准范围
                    effectRange = 4f,     // 效果范围 = 实际爆炸范围
                    isGrenade = true,
                    grenageVerticleSpeed = 10f,
                    movableWhileAim = true,
                    skillReadyTime = 0f,
                    checkObsticle = true,
                    releaseOnStartAim = false
                };
                skillContextField.SetValue(grenadeSkill, skillContext);
            }

            // 添加 ItemSetting_Skill 组件
            ItemSetting_Skill skillSetting = item.gameObject.AddComponent<ItemSetting_Skill>();
            SetFieldValue(skillSetting, "Skill", grenadeSkill);
            SetFieldValue(skillSetting, "onRelease", ItemSetting_Skill.OnReleaseAction.reduceCount);

            // 设置物品为技能物品
            item.SetBool("IsSkill", true, true);

            // 初始化物品
            item.Initialize();

            // Debug.Log($"[微型虫洞] 虫洞手雷 {typeId} 配置完成");
            return grenadeSkill;
        }

        /// <summary>
        /// 配置虫洞徽章物品属性
        /// </summary>
        private static void ConfigureBadgeProperties(Item item, int typeId, string nameKey, string descKey, Sprite icon)
        {
            SetFieldValue(item, "typeID", typeId);
            SetFieldValue(item, "displayName", nameKey);
            SetFieldValue(item, "description", descKey);

            if (icon != null)
            {
                SetFieldValue(item, "icon", icon);
            }

            SetFieldValue(item, "stackable", true);
            SetFieldValue(item, "maxStackCount", 5);
            SetFieldValue(item, "usable", false);
            SetFieldValue(item, "quality", 4);
            SetFieldValue(item, "value", 5000);
            SetFieldValue(item, "weight", 0.05f);

            item.Initialize();

            ConfigureAvailability(item, 15, 5f);

            // Debug.Log($"[微型虫洞] 已配置虫洞徽章 {typeId}");
        }

        /// <summary>
        /// 配置黑洞手雷物品属性（使用技能系统，装备后蓄力投掷）
        /// </summary>
        private static void ConfigureBlackHoleGrenadeProperties(Item item, int typeId, string nameKey, string descKey, Sprite icon)
        {
            SetFieldValue(item, "typeID", typeId);
            SetFieldValue(item, "displayName", nameKey);
            SetFieldValue(item, "description", descKey);

            if (icon != null)
            {
                SetFieldValue(item, "icon", icon);
            }

            SetFieldValue(item, "stackable", true);
            SetFieldValue(item, "maxStackCount", 3);
            SetFieldValue(item, "quality", 5);
            SetFieldValue(item, "value", 1000);
            SetFieldValue(item, "weight", 0.5f);
            SetFieldValue(item, "soundKey", "Grenade");

            // 在物品对象下创建技能子对象
            GameObject skillObj = new GameObject("BlackHoleGrenadeSkill");
            skillObj.transform.SetParent(item.gameObject.transform);
            skillObj.transform.localPosition = Vector3.zero;

            // 添加技能组件
            var blackHoleSkill = skillObj.AddComponent<BlackHoleGrenadeSkill>();
            blackHoleSkill.staminaCost = 0f;
            blackHoleSkill.coolDownTime = 0.5f;
            blackHoleSkill.damageRange = 5f;
            blackHoleSkill.delayTime = 2f;
            blackHoleSkill.throwForce = 15f;
            blackHoleSkill.throwAngle = 30f;
            blackHoleSkill.canControlCastDistance = true;
            blackHoleSkill.grenadeVerticleSpeed = 10f;
            blackHoleSkill.canHurtSelf = true;

            // 黑洞参数（内置固定值）
            blackHoleSkill.pullRange = 5f;
            blackHoleSkill.pullForce = 3f;
            blackHoleSkill.pullDuration = 5f;
            blackHoleSkill.pullDamage = 10f;

            // 设置 SkillContext
            var skillContextField = typeof(SkillBase).GetField("skillContext",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (skillContextField != null)
            {
                var skillContext = new SkillContext
                {
                    castRange = 8f,      // 瞄准范围
                    effectRange = 5f,    // 效果范围 = 实际影响范围
                    isGrenade = true,
                    grenageVerticleSpeed = 10f,
                    movableWhileAim = true,
                    skillReadyTime = 0f,
                    checkObsticle = true,
                    releaseOnStartAim = false
                };
                skillContextField.SetValue(blackHoleSkill, skillContext);
            }

            // 添加 ItemSetting_Skill 组件
            ItemSetting_Skill skillSetting = item.gameObject.AddComponent<ItemSetting_Skill>();
            SetFieldValue(skillSetting, "Skill", blackHoleSkill);
            SetFieldValue(skillSetting, "onRelease", ItemSetting_Skill.OnReleaseAction.reduceCount);

            // 设置物品为技能物品
            item.SetBool("IsSkill", true, true);

            // 初始化物品
            item.Initialize();

            ConfigureAvailability(item, 20, 3f);

            // Debug.Log($"[微型虫洞] 已配置黑洞手雷 {typeId}");
        }

        /// <summary>
        /// 配置物品的 Availability
        /// </summary>
        private static void ConfigureAvailability(Item item, int minLevel, float dropWeight)
        {
            try
            {
                var availabilityField = item.GetType().GetField("availability",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (availabilityField != null)
                {
                    var availabilityType = availabilityField.FieldType;
                    var availability = Activator.CreateInstance(availabilityType);

                    SetFieldValue(availability, "canSpawnInLoot", true);
                    SetFieldValue(availability, "canSpawnInShop", true);
                    SetFieldValue(availability, "canSpawnInCraft", false);
                    SetFieldValue(availability, "canDropFromEnemy", true);
                    SetFieldValue(availability, "canBeGivenAsQuestReward", true);
                    SetFieldValue(availability, "minPlayerLevel", minLevel);
                    SetFieldValue(availability, "randomDropWeight", dropWeight);

                    availabilityField.SetValue(item, availability);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[微型虫洞] 配置 Availability 失败: {e.Message}");
            }
        }

        /// <summary>
        /// 为徽章物品添加 Effect 系统
        /// </summary>
        private static void CreateBadgeEffect(GameObject itemObj, Item badgeItem)
        {
            GameObject effectObj = new GameObject("BadgeDodgeEffect");
            effectObj.transform.SetParent(itemObj.transform);
            effectObj.transform.localPosition = Vector3.zero;

            Effect effect = effectObj.AddComponent<Effect>();
            SetFieldValue(effect, "display", true);
            SetFieldValue(effect, "description", "虫洞徽章被动：受到伤害时有概率闪避并恢复生命");

            effectObj.AddComponent<WormholeDodgeTrigger>();
            effectObj.AddComponent<WormholeDodgeAction>();

            badgeItem.Effects.Add(effect);

            ModLogger.Log("[微型虫洞] 已为徽章添加 Effect 系统");
        }

        #endregion

        #region 新机制物品

        /// <summary>
        /// 创建时空回溯物品
        /// </summary>
        public static Item CreateTimeRewindItem(Sprite icon)
        {
            ModLogger.Log("[时空回溯] 开始创建时空回溯Prefab...");

            GameObject itemObj = CreateItemGameObject("TimeRewind", new Color(0.2f, 0.8f, 1f), null);

            UnityEngine.Object.DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            // 添加使用行为
            itemObj.AddComponent<TimeRewindUse>();

            Item prefab = itemObj.AddComponent<Item>();
            ConfigureUsableItemProperties(prefab, TIME_REWIND_TYPE_ID, "时空回溯",
                "高阶虫洞科技产品。\n\n" +
                "<color=#87CEEB>功能：</color>\n" +
                "• 记录玩家状态\n" +
                "• 可回溯到5秒前的状态\n" +
                "• 恢复位置/生命/弹药\n\n" +
                "<color=#FF6B6B>消耗：</color>\n" +
                "• 15%最大生命值\n" +
                "• 30秒冷却\n\n" +
                "<color=#FFD700>「时间是最锋利的武器」</color>",
                icon, typeof(TimeRewindUse), 1);

            // 添加 AgentUtilities 自动修复组件
            itemObj.AddComponent<AgentUtilitiesFixer>();

            ModLogger.Log("[时空回溯] 时空回溯Prefab创建完成");
            return prefab;
        }

        /// <summary>
        /// 创建虫洞网络物品
        /// </summary>
        public static Item CreateWormholeNetworkItem(Sprite icon)
        {
            ModLogger.Log("[虫洞网络] 开始创建虫洞网络Prefab...");

            GameObject itemObj = CreateItemGameObject("WormholeNetwork", new Color(0.5f, 1f, 0.5f), null);

            UnityEngine.Object.DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            // 添加使用行为
            itemObj.AddComponent<WormholeNetworkUse>();

            Item prefab = itemObj.AddComponent<Item>();
            ConfigureUsableItemProperties(prefab, WORMHOLE_NETWORK_TYPE_ID, "虫洞网络",
                "高级虫洞科技产品。\n\n" +
                "<color=#87CEEB>功能：</color>\n" +
                "• 在当前位置放置蓝色传送门\n" +
                "• 在前方生成橙色传送门\n" +
                "• 两个传送门可双向传送\n\n" +
                "<color=#4CAF50>特性：</color>\n" +
                "• 传送延迟1秒\n" +
                "• 传送冷却5秒\n" +
                "• 持续60秒\n\n" +
                "<color=#FFD700>「连接两个空间，掌控战场」</color>",
                icon, typeof(WormholeNetworkUse), 1);

            // 添加 AgentUtilities 自动修复组件
            itemObj.AddComponent<AgentUtilitiesFixer>();

            ModLogger.Log("[虫洞网络] 虫洞网络Prefab创建完成");
            return prefab;
        }

        #endregion

        #region 辅助方法

        // ========== 反射缓存 ==========
        private static readonly Dictionary<(Type, string), FieldInfo> fieldCache = new Dictionary<(Type, string), FieldInfo>();
        private static readonly Dictionary<(Type, string), PropertyInfo> propertyCache = new Dictionary<(Type, string), PropertyInfo>();

        /// <summary>
        /// 使用反射设置字段值（带缓存优化）
        /// </summary>
        public static void SetFieldValue(object obj, string fieldName, object value)
        {
            if (obj == null) return;

            var type = obj.GetType();
            var key = (type, fieldName);

            // 尝试从缓存获取 FieldInfo
            if (!fieldCache.TryGetValue(key, out var field))
            {
                field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                fieldCache[key] = field;
            }

            if (field != null)
            {
                field.SetValue(obj, value);
                return;
            }

            // 尝试从缓存获取 PropertyInfo
            if (!propertyCache.TryGetValue(key, out var prop))
            {
                prop = type.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                propertyCache[key] = prop;
            }

            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(obj, value);
            }
        }

        #endregion
    }
}
