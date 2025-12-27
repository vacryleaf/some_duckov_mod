using UnityEngine;
using UnityEngine.SceneManagement;
using Duckov.Modding;
using ItemStatsSystem;
using SodaCraft.Localizations;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
// 解决 UnityEngine.Object 与 System.Object 的冲突
// 解决 UnityEngine.Random 与 System.Random 的冲突
using Object = UnityEngine.Object;
using Random = System.Random;

namespace MoonlightSwordMod
{
    /// <summary>
    /// 名刀月影Mod主类
    /// 负责加载资源、创建武器并注册到游戏系统
    /// 注意：武器属性（TypeID、DisplayName、Stats等）必须在Unity Prefab中预设
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // ========== 魔数定义 ==========
        private const float BLADE_METALLIC = 0.95f;
        private const float BLADE_GLOSSINESS = 0.9f;
        private const float BLADE_EMISSION_INTENSITY = 0.5f;
        private const float GUARD_METALLIC = 0.8f;
        private const float AURA_EMISSION_INTENSITY = 2f;
        private const float AURA_TRANSPARENCY = 0.7f;
        private const float AURA_RENDER_QUEUE = 3000f;

        // 武器Prefab
        private Item moonlightSwordPrefab;

        // 剑气Prefab
        private GameObject swordAuraPrefab;

        // 武器图标
        private Sprite weaponIcon;

        // AssetBundle引用
        private AssetBundle weaponBundle;

        // 程序生成的物体引用（用于清理）
        private GameObject proceduralWeaponObj;
        private GameObject proceduralSwordAuraObj;

        // 程序生成的材质球引用（用于清理）
        private Material bladeMaterial;
        private Material guardMaterial;
        private Material handleMaterial;
        private Material auraMaterial;

        // 协程引用（用于停止）
        private Coroutine lootBoxInjectionCoroutine;

        // 武器TypeID（与Unity Prefab中设置的一致，Mod物品应使用990000+）
        // 注意：990001-990007 已被 WormholeTechMod 占用，使用 992001 避免冲突
        private const int WEAPON_TYPE_ID = 992001;

        /// <summary>
        /// 获取Mod所在目录路径
        /// </summary>
        private string GetModFolderPath()
        {
            // 通过info.dllPath获取DLL路径，然后取目录
            string dllPath = info.dllPath;
            if (!string.IsNullOrEmpty(dllPath))
            {
                return Path.GetDirectoryName(dllPath);
            }
            return string.Empty;
        }

        /// <summary>
        /// Mod启动入口
        /// </summary>
        void Start()
        {
            ModLogger.Log("[名刀月影] 开始加载Mod...");

            try
            {
                // 设置本地化文本（只注册一次事件）
                SetupLocalization();

                // 同步加载AssetBundle资源
                LoadAssets();

                // 配置武器Prefab组件
                ConfigureWeaponPrefab();

                // 注册到游戏系统
                RegisterWeapon();

                // 注册到商店（自动售卖机）
                RegisterToShop();

                // 启动箱子物品注入协程
                lootBoxInjectionCoroutine = StartCoroutine(LootBoxInjectionRoutine());

                ModLogger.Log("[名刀月影] Mod加载完成!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[名刀月影] Mod加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 设置本地化文本（带重复注册检查）
        /// </summary>
        private void SetupLocalization()
        {
            // 武器名称和描述
            LocalizationManager.SetOverrideText("MoonlightSword_Name", "名刀月影");
            LocalizationManager.SetOverrideText("MoonlightSword_Desc",
                "传说级近战武器，蕴含月之精华。\n\n" +
                "<color=#87CEEB>特殊能力：</color>\n" +
                "• 正手/反手连击系统\n" +
                "• 瞄准后冲刺并释放剑气\n" +
                "• 剑气可穿透敌人\n" +
                "• 格挡可偏转子弹\n\n" +
                "<color=#FFD700>「月华如水，斩尽黑暗」</color>");

            // 只在首次设置时注册事件
            var eventField = typeof(LocalizationManager).GetField("OnSetLanguage",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (eventField != null)
            {
                var currentDelegate = eventField.GetValue(null) as System.Delegate;
                if (currentDelegate == null || !currentDelegate.GetInvocationList().Contains((System.Action<SystemLanguage>)OnLanguageChanged))
                {
                    LocalizationManager.OnSetLanguage += OnLanguageChanged;
                }
            }
            else
            {
                LocalizationManager.OnSetLanguage += OnLanguageChanged;
            }

            ModLogger.Log("[名刀月影] 本地化设置完成");
        }

        /// <summary>
        /// 语言切换时更新本地化文本
        /// </summary>
        private void OnLanguageChanged(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.English:
                    LocalizationManager.SetOverrideText("MoonlightSword_Name", "Moonlight Sword");
                    LocalizationManager.SetOverrideText("MoonlightSword_Desc",
                        "Legendary melee weapon imbued with the essence of the moon.\n\n" +
                        "<color=#87CEEB>Special Abilities:</color>\n" +
                        "• Forehand/Backhand combo system\n" +
                        "• Dash and release sword aura when aiming\n" +
                        "• Sword aura pierces enemies\n" +
                        "• Block can deflect bullets\n\n" +
                        "<color=#FFD700>\"Moonlight flows like water, cleaving through darkness\"</color>");
                    break;
                default: // 中文
                    LocalizationManager.SetOverrideText("MoonlightSword_Name", "名刀月影");
                    LocalizationManager.SetOverrideText("MoonlightSword_Desc",
                        "传说级近战武器，蕴含月之精华。\n\n" +
                        "<color=#87CEEB>特殊能力：</color>\n" +
                        "• 正手/反手连击系统\n" +
                        "• 瞄准后冲刺并释放剑气\n" +
                        "• 剑气可穿透敌人\n" +
                        "• 格挡可偏转子弹\n\n" +
                        "<color=#FFD700>「月华如水，斩尽黑暗」</color>");
                    break;
            }
        }

        /// <summary>
        /// 加载AssetBundle资源文件
        /// </summary>
        private void LoadAssets()
        {
            // 构建AssetBundle路径
            string modFolder = GetModFolderPath();
            string bundlePath = Path.Combine(modFolder, "Assets", "moonlight_sword");

            Debug.Log($"[名刀月影] 尝试加载AssetBundle: {bundlePath}");

            if (File.Exists(bundlePath))
            {
                // 同步加载AssetBundle
                weaponBundle = AssetBundle.LoadFromFile(bundlePath);

                if (weaponBundle != null)
                {
                    // 加载武器Prefab
                    moonlightSwordPrefab = weaponBundle.LoadAsset<Item>("MoonlightSwordPrefab");

                    // 加载剑气Prefab
                    swordAuraPrefab = weaponBundle.LoadAsset<GameObject>("SwordAuraPrefab");

                    // 加载武器图标
                    weaponIcon = LoadIconFromBundle("MoonlightSwordIcon");

                    ModLogger.Log("[名刀月影] AssetBundle加载成功");

                    if (moonlightSwordPrefab == null)
                    {
                        ModLogger.LogWarning("[名刀月影] 武器Prefab未找到，使用程序化生成");
                        CreateProceduralWeapon();
                    }

                    if (swordAuraPrefab == null)
                    {
                        ModLogger.LogWarning("[名刀月影] 剑气Prefab未找到，使用程序化生成");
                        swordAuraPrefab = CreateSwordAuraEffect();
                    }
                }
                else
                {
                    ModLogger.LogError("[名刀月影] AssetBundle加载失败");
                    CreateProceduralWeapon();
                }
            }
            else
            {
                Debug.LogWarning($"[名刀月影] AssetBundle文件不存在: {bundlePath}");
                ModLogger.LogWarning("[名刀月影] 使用程序化生成武器");
                CreateProceduralWeapon();
            }
        }

        /// <summary>
        /// 从 AssetBundle 加载图标
        /// </summary>
        private Sprite LoadIconFromBundle(string iconName)
        {
            if (weaponBundle == null) return null;

            // 尝试直接加载 Sprite
            Sprite icon = weaponBundle.LoadAsset<Sprite>(iconName);
            if (icon != null)
            {
                Debug.Log($"[名刀月影] 图标 {iconName} 加载成功（Sprite）");
                return icon;
            }

            // 尝试加载 Texture2D 并转换为 Sprite
            Texture2D iconTex = weaponBundle.LoadAsset<Texture2D>(iconName);
            if (iconTex != null)
            {
                icon = Sprite.Create(iconTex,
                    new Rect(0, 0, iconTex.width, iconTex.height),
                    new Vector2(0.5f, 0.5f));
                Debug.Log($"[名刀月影] 图标 {iconName} 加载成功（Texture2D -> Sprite）");
                return icon;
            }

            Debug.LogWarning($"[名刀月影] 图标 {iconName} 未找到");
            return null;
        }

        /// <summary>
        /// 配置武器Prefab组件
        /// 注意：基础参数（Damage, CritRate, AttackRange等）应在 Unity Prefab 的 Item.Stats 中配置
        /// 这里只配置特殊攻击和格挡的自定义参数
        /// </summary>
        private void ConfigureWeaponPrefab()
        {
            if (moonlightSwordPrefab == null)
            {
                ModLogger.LogError("[名刀月影] 武器Prefab为空，无法配置");
                return;
            }

            ModLogger.Log("[名刀月影] 开始配置武器Prefab");

            // 添加自定义攻击组件
            var attackComponent = moonlightSwordPrefab.gameObject.GetComponent<MoonlightSwordAttack>();
            if (attackComponent == null)
            {
                attackComponent = moonlightSwordPrefab.gameObject.AddComponent<MoonlightSwordAttack>();
            }

            // 配置特殊攻击参数（自定义参数，不从Stats读取）
            attackComponent.swordAuraPrefab = swordAuraPrefab;
            attackComponent.specialDamage = 90f;
            attackComponent.specialRange = 10f;
            attackComponent.specialCooldown = 8f;

            // 添加格挡组件
            var blockComponent = moonlightSwordPrefab.gameObject.GetComponent<MoonlightSwordBlock>();
            if (blockComponent == null)
            {
                blockComponent = moonlightSwordPrefab.gameObject.AddComponent<MoonlightSwordBlock>();
            }

            // 配置格挡参数（自定义参数）
            blockComponent.staminaCost = 5f;
            blockComponent.deflectSpeed = 30f;
            blockComponent.deflectDamageRatio = 0.5f;
            blockComponent.deflectMaxDistance = 20f;

            // 在添加所有组件后调用 Initialize()，确保组件正确注册到游戏系统
            moonlightSwordPrefab.Initialize();
            ModLogger.Log("[名刀月影] 武器Prefab组件添加完成并已初始化");

            // 设置物品图标
            if (weaponIcon != null)
            {
                SetFieldValue(moonlightSwordPrefab, "icon", weaponIcon);
                ModLogger.Log("[名刀月影] 武器图标已设置");
            }

            // displayName 和 description 存储本地化键，游戏会自动调用 LocalizationManager.GetPlainText 查找
            SetFieldValue(moonlightSwordPrefab, "displayName", "MoonlightSword_Name");
            SetFieldValue(moonlightSwordPrefab, "description", "MoonlightSword_Desc");
            ModLogger.Log("[名刀月影] 本地化键已设置");

            // 配置 Availability（物品可用性配置）
            ConfigureAvailability();

            ModLogger.Log("[名刀月影] 武器Prefab配置完成（含格挡系统和Availability）");
        }

        /// <summary>
        /// 配置物品的 Availability（决定物品何时可被获取）
        /// </summary>
        private void ConfigureAvailability()
        {
            if (moonlightSwordPrefab == null) return;

            try
            {
                var availabilityField = moonlightSwordPrefab.GetType().GetField("availability",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                // 如果找不到字段，尝试查找公共字段
                if (availabilityField == null)
                {
                    availabilityField = moonlightSwordPrefab.GetType().GetField("availability",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                }

                if (availabilityField == null)
                {
                    ModLogger.LogWarning("[名刀月影] 无法访问 Availability 字段，跳过配置");
                    return;
                }

                object availability = availabilityField.GetValue(moonlightSwordPrefab);
                if (availability == null)
                {
                    var availabilityType = availabilityField.FieldType;
                    availability = System.Activator.CreateInstance(availabilityType);
                    availabilityField.SetValue(moonlightSwordPrefab, availability);
                }

                // 配置属性
                var canSpawnInLootField = availability.GetType().GetField("canSpawnInLoot",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var canSpawnInShopField = availability.GetType().GetField("canSpawnInShop",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var canSpawnInCraftField = availability.GetType().GetField("canSpawnInCraft",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var canDropFromEnemyField = availability.GetType().GetField("canDropFromEnemy",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var canBeGivenAsQuestRewardField = availability.GetType().GetField("canBeGivenAsQuestReward",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var minPlayerLevelField = availability.GetType().GetField("minPlayerLevel",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var randomDropWeightField = availability.GetType().GetField("randomDropWeight",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (canSpawnInLootField != null) canSpawnInLootField.SetValue(availability, true);
                if (canSpawnInShopField != null) canSpawnInShopField.SetValue(availability, true);
                if (canSpawnInCraftField != null) canSpawnInCraftField.SetValue(availability, false);
                if (canDropFromEnemyField != null) canDropFromEnemyField.SetValue(availability, true);
                if (canBeGivenAsQuestRewardField != null) canBeGivenAsQuestRewardField.SetValue(availability, true);
                if (minPlayerLevelField != null) minPlayerLevelField.SetValue(availability, 10);
                if (randomDropWeightField != null) randomDropWeightField.SetValue(availability, 10f);

                ModLogger.Log("[名刀月影] Availability配置完成: 可在战利品/商店/敌人掉落中出现");
            }
            catch (System.Exception e)
            {
                ModLogger.LogWarning($"[名刀月影] 配置Availability失败: {e.Message}");
            }
        }

        /// <summary>
        /// 注册武器到游戏系统
        /// </summary>
        private void RegisterWeapon()
        {
            if (moonlightSwordPrefab == null)
            {
                ModLogger.LogError("[名刀月影] 无法注册空武器");
                return;
            }

            Debug.Log($"[名刀月影] 正在注册武器，TypeID: {moonlightSwordPrefab.TypeID}");

            // 注册到动态物品系统
            bool success = ItemAssetsCollection.AddDynamicEntry(moonlightSwordPrefab);

            if (success)
            {
                ModLogger.Log("[名刀月影] 武器注册成功！");
                // 修复：先检查 moonlightSwordPrefab 是否为 null
                if (moonlightSwordPrefab != null)
                {
                    Debug.Log($"  - 名称: {moonlightSwordPrefab.DisplayName}");
                    Debug.Log($"  - ID: {moonlightSwordPrefab.TypeID}");
                }
                else
                {
                    ModLogger.Log("  - 名称: (prefab为空)");
                    ModLogger.Log("  - ID: N/A");
                }
            }
            else
            {
                ModLogger.LogError("[名刀月影] 武器注册失败！可能是TypeID冲突");
            }
        }

        /// <summary>
        /// 程序化生成武器（备用方案）
        /// 当AssetBundle不存在时使用
        /// 注意：程序化生成的武器功能有限，建议使用AssetBundle
        /// </summary>
        private void CreateProceduralWeapon()
        {
            ModLogger.Log("[名刀月影] 开始程序化生成武器");

            // 创建基础GameObject
            GameObject weaponObj = new GameObject("MoonlightSword");
            proceduralWeaponObj = weaponObj; // 保存引用以便卸载时清理

            // 添加Item组件
            moonlightSwordPrefab = weaponObj.AddComponent<Item>();

            // 设置TypeID（使用反射，因为typeID是私有字段）
            SetFieldValue(moonlightSwordPrefab, "typeID", WEAPON_TYPE_ID);

            // 设置其他必要属性
            SetFieldValue(moonlightSwordPrefab, "displayName", "MoonlightSword_Name");
            SetFieldValue(moonlightSwordPrefab, "description", "MoonlightSword_Desc");

            // 设置物品属性
            SetFieldValue(moonlightSwordPrefab, "stackable", false);
            SetFieldValue(moonlightSwordPrefab, "maxStackCount", 1);
            SetFieldValue(moonlightSwordPrefab, "quality", 5); // 传说品质
            SetFieldValue(moonlightSwordPrefab, "value", 5000);
            SetFieldValue(moonlightSwordPrefab, "weight", 1.5f);

            // 配置武器 Stats（重要：否则伤害、暴击率等都为0）
            ConfigureWeaponStats();

            // 初始化物品
            moonlightSwordPrefab.Initialize();

            ModLogger.Log($"[名刀月影] 程序化武器 TypeID 设置为: {WEAPON_TYPE_ID}");

            // 创建简单的刀模型
            CreateSimpleSwordModel(weaponObj);

            // 创建剑气特效
            if (swordAuraPrefab == null)
            {
                swordAuraPrefab = CreateSwordAuraEffect();
                proceduralSwordAuraObj = swordAuraPrefab; // 保存引用
            }

            ModLogger.Log("[名刀月影] 程序化武器生成完成");
        }

        /// <summary>
        /// 配置程序化武器的 Stats
        /// </summary>
        private void ConfigureWeaponStats()
        {
            if (moonlightSwordPrefab == null) return;

            try
            {
                // 创建 Stats 字典
                var statsField = moonlightSwordPrefab.GetType().GetField("stats",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (statsField != null)
                {
                    var statsDictType = typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(float));
                    var statsDict = Activator.CreateInstance(statsDictType) as System.Collections.IDictionary;

                    if (statsDict != null)
                    {
                        // 配置武器属性
                        statsDict["Damage"] = 50f;              // 基础伤害
                        statsDict["CritRate"] = 0.15f;          // 暴击率 15%
                        statsDict["CritDamageFactor"] = 2f;     // 暴击伤害倍率 200%
                        statsDict["ArmorPiercing"] = 0.25f;     // 护甲穿透 25%
                        statsDict["AttackSpeed"] = 1.2f;        // 攻击速度 1.2
                        statsDict["AttackRange"] = 2.5f;        // 攻击范围 2.5米
                        statsDict["StaminaCost"] = 8f;          // 体力消耗 8点
                        statsDict["BleedChance"] = 0.2f;        // 流血几率 20%

                        statsField.SetValue(moonlightSwordPrefab, statsDict);
                        ModLogger.Log("[名刀月影] 武器 Stats 配置完成");
                    }
                }
            }
            catch (System.Exception e)
            {
                ModLogger.LogWarning($"[名刀月影] 配置 Stats 失败: {e.Message}");
            }
        }

        /// <summary>
        /// 创建简单的刀模型（备用）
        /// </summary>
        private void CreateSimpleSwordModel(GameObject parent)
        {
            // 刀身
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade";
            blade.transform.SetParent(parent.transform);
            blade.transform.localPosition = new Vector3(0, 0.75f, 0);
            blade.transform.localScale = new Vector3(0.08f, 1.2f, 0.02f);

            // 创建刀身材质（带 Shader null 检查）
            Shader bladeShader = Shader.Find("Standard");
            if (bladeShader == null)
            {
                bladeShader = Shader.Find("Mobile/Diffuse"); // 备用 Shader
            }
            bladeMaterial = new Material(bladeShader);
            bladeMaterial.color = new Color(0.91f, 0.91f, 0.94f); // 银白色
            bladeMaterial.SetFloat("_Metallic", BLADE_METALLIC);
            bladeMaterial.SetFloat("_Glossiness", BLADE_GLOSSINESS);
            bladeMaterial.EnableKeyword("_EMISSION");
            bladeMaterial.SetColor("_EmissionColor", new Color(0.63f, 0.78f, 0.91f) * BLADE_EMISSION_INTENSITY);
            blade.GetComponent<Renderer>().material = bladeMaterial;

            // 移除默认碰撞体
            Object.Destroy(blade.GetComponent<Collider>());

            // 护手
            GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            guard.name = "Guard";
            guard.transform.SetParent(parent.transform);
            guard.transform.localPosition = new Vector3(0, 0.1f, 0);
            guard.transform.localScale = new Vector3(0.15f, 0.02f, 0.05f);

            Shader guardShader = Shader.Find("Standard");
            guardMaterial = new Material(guardShader != null ? guardShader : Shader.Find("Mobile/Diffuse"));
            guardMaterial.color = new Color(0.1f, 0.17f, 0.29f); // 深蓝色
            guardMaterial.SetFloat("_Metallic", GUARD_METALLIC);
            guard.GetComponent<Renderer>().material = guardMaterial;
            Object.Destroy(guard.GetComponent<Collider>());

            // 刀柄
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(parent.transform);
            handle.transform.localPosition = new Vector3(0, -0.05f, 0);
            handle.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);

            Shader handleShader = Shader.Find("Standard");
            handleMaterial = new Material(handleShader != null ? handleShader : Shader.Find("Mobile/Diffuse"));
            handleMaterial.color = new Color(0.23f, 0.18f, 0.35f); // 深紫色
            handle.GetComponent<Renderer>().material = handleMaterial;
            Object.Destroy(handle.GetComponent<Collider>());

            // 添加拾取碰撞体
            BoxCollider collider = parent.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.2f, 1.5f, 0.2f);
            collider.center = new Vector3(0, 0.75f, 0);

            ModLogger.Log("[名刀月影] 简易刀模型创建完成");
        }

        /// <summary>
        /// 创建剑气特效（备用）
        /// </summary>
        private GameObject CreateSwordAuraEffect()
        {
            GameObject aura = new GameObject("SwordAura");

            // 创建月牙形状
            GameObject crescent = GameObject.CreatePrimitive(PrimitiveType.Quad);
            crescent.name = "CrescentShape";
            crescent.transform.SetParent(aura.transform);
            crescent.transform.localScale = new Vector3(2f, 1f, 1f);

            // 创建发光材质
            Material auraMaterial = new Material(Shader.Find("Standard"));
            auraMaterial.color = new Color(0.44f, 0.69f, 0.88f, 0.7f);
            auraMaterial.EnableKeyword("_EMISSION");
            auraMaterial.SetColor("_EmissionColor", new Color(0.44f, 0.69f, 0.88f) * 2f);

            // 设置透明模式
            auraMaterial.SetFloat("_Mode", 3);
            auraMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            auraMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            auraMaterial.renderQueue = 3000;

            crescent.GetComponent<Renderer>().material = auraMaterial;
            Object.Destroy(crescent.GetComponent<Collider>());

            // 添加粒子系统
            ParticleSystem particles = aura.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(1f, 1f, 1f, 0.8f);
            main.startSize = 0.1f;
            main.startLifetime = 0.5f;
            main.startSpeed = 2f;

            // 添加剑气投射物组件
            SwordAuraProjectile projectile = aura.AddComponent<SwordAuraProjectile>();
            projectile.damage = 90f;
            projectile.speed = 15f;
            projectile.maxDistance = 10f;
            projectile.pierceCount = 3;

            ModLogger.Log("[名刀月影] 简易剑气特效创建完成");

            return aura;
        }

        /// <summary>
        /// 使用反射设置字段值
        /// </summary>
        private void SetFieldValue(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                var prop = type.GetProperty(fieldName,
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(obj, value);
                }
            }
        }

        /// <summary>
        /// 注册武器到商店（自动售卖机）
        /// </summary>
        private void RegisterToShop()
        {
            try
            {
                // 获取商店数据库实例
                var shopDatabase = StockShopDatabase.Instance;
                if (shopDatabase == null)
                {
                    ModLogger.LogWarning("[名刀月影] 无法获取商店数据库，武器将不会出现在商店中");
                    return;
                }

                // 获取商人配置列表
                var merchantProfiles = shopDatabase.merchantProfiles;
                if (merchantProfiles == null || merchantProfiles.Count == 0)
                {
                    ModLogger.LogWarning("[名刀月影] 商店数据库中没有商人配置");
                    return;
                }

                // 遍历所有商人，添加武器
                int addedCount = 0;
                foreach (var profile in merchantProfiles)
                {
                    if (profile == null)
                    {
                        ModLogger.LogWarning("[名刀月影] 遇到空的商人配置");
                        continue;
                    }

                    // 安全获取 entries 字段
                    var entriesField = profile.GetType().GetField("entries",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (entriesField == null)
                    {
                        ModLogger.LogWarning("[名刀月影] 无法获取 entries 字段");
                        continue;
                    }

                    var entries = entriesField.GetValue(profile) as System.Collections.IList;
                    if (entries == null)
                    {
                        ModLogger.LogWarning("[名刀月影] 商人 entries 为 null");
                        continue;
                    }

                    // 检查是否已存在
                    bool exists = false;
                    foreach (var entry in entries)
                    {
                        if (entry == null) continue;

                        var typeIdField = entry.GetType().GetField("typeID",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (typeIdField != null)
                        {
                            int typeId = (int)typeIdField.GetValue(entry);
                            if (typeId == WEAPON_TYPE_ID)
                            {
                                exists = true;
                                break;
                            }
                        }
                    }

                    if (!exists)
                    {
                        // 创建新的物品条目 - 确保所有字段都正确初始化
                        var newEntry = new StockShopDatabase.ItemEntry
                        {
                            typeID = WEAPON_TYPE_ID,
                            maxStock = 1,           // 传说武器每次只刷新1把
                            priceFactor = 1.0f,     // 价格倍率（商店本身会涨价）
                            possibility = 1.0f,     // 100%出现概率
                            forceUnlock = true,     // 强制解锁
                            lockInDemo = false
                        };

                        // 安全地添加到列表
                        var addMethod = entries.GetType().GetMethod("Add");
                        if (addMethod != null)
                        {
                            addMethod.Invoke(entries, new[] { newEntry });
                            addedCount++;
                            Debug.Log($"[名刀月影] 已添加武器到商人配置");
                        }
                        else
                        {
                            ModLogger.LogWarning("[名刀月影] 无法调用 Add 方法");
                        }
                    }
                }

                if (addedCount > 0)
                {
                    ModLogger.Log($"[名刀月影] 武器已添加到 {addedCount} 个商人配置");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[名刀月影] 注册商店失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 箱子物品注入协程
        /// 定期扫描场景中的箱子，注入武器
        /// </summary>
        private IEnumerator LootBoxInjectionRoutine()
        {
            // 等待场景完全加载
            yield return new WaitForSeconds(2f);

            ModLogger.Log("[名刀月影] 开始箱子物品注入...");

            // 已处理的箱子集合，避免重复注入
            HashSet<int> processedBoxes = new HashSet<int>();

            while (this != null && this.gameObject != null)
            {
                try
                {
                    // 注入到 LootBoxLoader（影响箱子生成的物品池）
                    InjectToLootBoxLoaders();

                    // 注入到已存在的箱子背包
                    InjectToExistingLootboxes(processedBoxes);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[名刀月影] 箱子注入时发生错误: {e.Message}");
                }

                // 每隔一段时间检查一次
                yield return new WaitForSeconds(5f);
            }
        }

        /// <summary>
        /// 注入到 LootBoxLoader（影响新生成的箱子）
        /// </summary>
        private void InjectToLootBoxLoaders()
        {
            try
            {
                // 获取场景中所有 LootBoxLoader
                var lootBoxLoaders = FindObjectsOfType<Duckov.Utilities.LootBoxLoader>();

                if (lootBoxLoaders.Length == 0)
                {
                    // 正常情况：不是每个场景都有 LootBoxLoader
                    return;
                }

                foreach (var loader in lootBoxLoaders)
                {
                    if (loader == null) continue;

                    try
                    {
                        // 获取 fixedItems 列表
                        var fixedItemsField = loader.GetType().GetField("fixedItems",
                            System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Public);

                        if (fixedItemsField == null)
                        {
                            continue;
                        }

                        var fixedItems = fixedItemsField.GetValue(loader) as List<int>;
                        if (fixedItems == null)
                        {
                            continue;
                        }

                        // 5%概率添加名刀月影（传说武器稀有度较高）
                        if (!fixedItems.Contains(WEAPON_TYPE_ID) && UnityEngine.Random.value < 0.05f)
                        {
                            fixedItems.Add(WEAPON_TYPE_ID);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[名刀月影] 修改 LootBoxLoader 失败: {e.Message}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[名刀月影] 遍历 LootBoxLoader 失败: {e.Message}");
            }
        }

        /// <summary>
        /// 注入到已存在的箱子背包
        /// </summary>
        private void InjectToExistingLootboxes(HashSet<int> processedBoxes)
        {
            try
            {
                // 获取场景中所有 InteractableLootbox
                var lootboxes = FindObjectsOfType<InteractableLootbox>();

                foreach (var lootbox in lootboxes)
                {
                    if (lootbox == null) continue;

                    int instanceId;
                    try
                    {
                        instanceId = lootbox.GetInstanceID();
                    }
                    catch (System.Exception)
                    {
                        continue;
                    }

                    // 跳过已处理的箱子
                    if (processedBoxes.Contains(instanceId)) continue;

                    try
                    {
                        // 尝试获取或创建箱子的背包
                        var getInventoryMethod = typeof(InteractableLootbox).GetMethod("GetOrCreateInventory",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                        if (getInventoryMethod == null) continue;

                        var inventory = getInventoryMethod.Invoke(null, new object[] { lootbox }) as Inventory;
                        if (inventory == null) continue;

                        // 5%概率添加名刀月影（传说武器稀有度较高）
                        if (UnityEngine.Random.value < 0.05f)
                        {
                            bool success = TryAddWeaponToInventory(inventory);
                            if (success)
                            {
                                Debug.Log($"[名刀月影] 已向箱子注入武器");
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[名刀月影] 注入到箱子失败: {e.Message}");
                    }
                    finally
                    {
                        // 标记为已处理，避免重复尝试
                        processedBoxes.Add(instanceId);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[名刀月影] 遍历箱子失败: {e.Message}");
            }
        }

        /// <summary>
        /// 尝试将武器添加到背包
        /// </summary>
        private bool TryAddWeaponToInventory(Inventory inventory)
        {
            try
            {
                if (moonlightSwordPrefab == null) return false;

                // 创建武器实例
                var newWeapon = moonlightSwordPrefab.CreateInstance();
                if (newWeapon == null) return false;

                // 添加到背包
                bool success = inventory.AddItem(newWeapon);

                if (!success)
                {
                    // 如果添加失败，销毁创建的武器
                    Destroy(newWeapon.gameObject);
                }

                return success;
            }
            catch (System.Exception e)
            {
                ModLogger.LogWarning(string.Format("[名刀月影] 添加武器到背包失败: {0}", e.Message));
                return false;
            }
        }

        /// <summary>
        /// Mod卸载时清理资源
        /// </summary>
        void OnDestroy()
        {
            ModLogger.Log("[名刀月影] 开始卸载Mod");

            // 停止箱子注入协程
            if (lootBoxInjectionCoroutine != null)
            {
                StopCoroutine(lootBoxInjectionCoroutine);
                lootBoxInjectionCoroutine = null;
                ModLogger.Log("[名刀月影] 箱子注入协程已停止");
            }

            // 取消本地化事件监听
            LocalizationManager.OnSetLanguage -= OnLanguageChanged;

            // 移除动态物品
            if (moonlightSwordPrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(moonlightSwordPrefab);
                ModLogger.Log("[名刀月影] 武器已从游戏中移除");
            }

            // 卸载AssetBundle
            if (weaponBundle != null)
            {
                weaponBundle.Unload(true);
                weaponBundle = null;
                ModLogger.Log("[名刀月影] AssetBundle已卸载");
            }

            // 清理程序生成的物体
            if (proceduralWeaponObj != null)
            {
                Destroy(proceduralWeaponObj);
                proceduralWeaponObj = null;
                ModLogger.Log("[名刀月影] 程序化武器已清理");
            }

            if (proceduralSwordAuraObj != null)
            {
                Destroy(proceduralSwordAuraObj);
                proceduralSwordAuraObj = null;
                ModLogger.Log("[名刀月影] 程序化剑气特效已清理");
            }

            // 清理程序生成的材质球
            DestroyMaterial(ref bladeMaterial);
            DestroyMaterial(ref guardMaterial);
            DestroyMaterial(ref handleMaterial);
            DestroyMaterial(ref auraMaterial);

            ModLogger.Log("[名刀月影] Mod卸载完成");
        }

        /// <summary>
        /// 安全销毁材质球
        /// </summary>
        private void DestroyMaterial(ref Material material)
        {
            if (material != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(material);
                }
                material = null;
            }
        }
    }
}
