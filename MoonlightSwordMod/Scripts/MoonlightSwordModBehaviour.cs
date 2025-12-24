using UnityEngine;
using UnityEngine.SceneManagement;
using Duckov.Modding;
using ItemStatsSystem;
using SodaCraft.Localizations;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace MoonlightSwordMod
{
    /// <summary>
    /// 名刀月影Mod主类
    /// 负责加载资源、创建武器并注册到游戏系统
    /// 注意：武器属性（TypeID、DisplayName、Stats等）必须在Unity Prefab中预设
    /// </summary>
    public class MoonlightSwordModBehaviour : ModBehaviour
    {
        // 武器Prefab
        private Item moonlightSwordPrefab;

        // 剑气Prefab
        private GameObject swordAuraPrefab;

        // 武器图标
        private Sprite weaponIcon;

        // AssetBundle引用
        private AssetBundle weaponBundle;

        // 武器TypeID（与Unity Prefab中设置的一致）
        private const int WEAPON_TYPE_ID = 10001;

        // 获取Mod所在目录路径
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
            Debug.Log("[名刀月影] 开始加载Mod...");

            try
            {
                // 设置本地化文本
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
                StartCoroutine(LootBoxInjectionRoutine());

                Debug.Log("[名刀月影] Mod加载完成!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[名刀月影] Mod加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 设置本地化文本
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

            // 监听语言切换事件
            LocalizationManager.OnSetLanguage += OnLanguageChanged;

            Debug.Log("[名刀月影] 本地化设置完成");
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

                    Debug.Log("[名刀月影] AssetBundle加载成功");

                    if (moonlightSwordPrefab == null)
                    {
                        Debug.LogWarning("[名刀月影] 武器Prefab未找到，使用程序化生成");
                        CreateProceduralWeapon();
                    }

                    if (swordAuraPrefab == null)
                    {
                        Debug.LogWarning("[名刀月影] 剑气Prefab未找到，使用程序化生成");
                        swordAuraPrefab = CreateSwordAuraEffect();
                    }
                }
                else
                {
                    Debug.LogError("[名刀月影] AssetBundle加载失败");
                    CreateProceduralWeapon();
                }
            }
            else
            {
                Debug.LogWarning($"[名刀月影] AssetBundle文件不存在: {bundlePath}");
                Debug.LogWarning("[名刀月影] 使用程序化生成武器");
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
                Debug.LogError("[名刀月影] 武器Prefab为空，无法配置");
                return;
            }

            Debug.Log("[名刀月影] 开始配置武器Prefab");

            // 设置武器图标
            if (weaponIcon != null)
            {
                SetFieldValue(moonlightSwordPrefab, "icon", weaponIcon);
                Debug.Log("[名刀月影] 武器图标已设置");
            }

            // displayName 和 description 存储本地化键，游戏会自动调用 LocalizationManager.GetPlainText 查找
            SetFieldValue(moonlightSwordPrefab, "displayName", "MoonlightSword_Name");
            SetFieldValue(moonlightSwordPrefab, "description", "MoonlightSword_Desc");
            Debug.Log("[名刀月影] 本地化键已设置");

            // 添加自定义攻击组件
            var attackComponent = moonlightSwordPrefab.gameObject.GetComponent<MoonlightSwordAttack>();
            if (attackComponent == null)
            {
                attackComponent = moonlightSwordPrefab.gameObject.AddComponent<MoonlightSwordAttack>();
            }

            // 配置特殊攻击参数（自定义参数，不从Stats读取）
            attackComponent.swordAuraPrefab = swordAuraPrefab;
            attackComponent.specialDamage = 90f;       // 剑气伤害
            attackComponent.specialRange = 10f;        // 剑气飞行距离
            attackComponent.specialCooldown = 8f;      // 特殊攻击冷却

            // 添加格挡组件
            var blockComponent = moonlightSwordPrefab.gameObject.GetComponent<MoonlightSwordBlock>();
            if (blockComponent == null)
            {
                blockComponent = moonlightSwordPrefab.gameObject.AddComponent<MoonlightSwordBlock>();
            }

            // 配置格挡参数（自定义参数）
            blockComponent.staminaCost = 5f;           // 格挡消耗体力
            blockComponent.deflectSpeed = 30f;         // 反弹速度
            blockComponent.deflectDamageRatio = 0.5f;  // 反弹伤害比例
            blockComponent.deflectMaxDistance = 20f;   // 反弹最大距离

            Debug.Log("[名刀月影] 武器Prefab配置完成（含格挡系统）");
            Debug.Log("[名刀月影] 注意：基础参数（Damage, CritRate等）需在 Unity Prefab 的 Item.Stats 中配置");
        }

        /// <summary>
        /// 注册武器到游戏系统
        /// </summary>
        private void RegisterWeapon()
        {
            if (moonlightSwordPrefab == null)
            {
                Debug.LogError("[名刀月影] 无法注册空武器");
                return;
            }

            Debug.Log($"[名刀月影] 正在注册武器，TypeID: {moonlightSwordPrefab.TypeID}");

            // 注册到动态物品系统
            bool success = ItemAssetsCollection.AddDynamicEntry(moonlightSwordPrefab);

            if (success)
            {
                Debug.Log($"[名刀月影] 武器注册成功！");
                Debug.Log($"  - 名称: {moonlightSwordPrefab.DisplayName}");
                Debug.Log($"  - ID: {moonlightSwordPrefab.TypeID}");
            }
            else
            {
                Debug.LogError("[名刀月影] 武器注册失败！可能是TypeID冲突");
            }
        }

        /// <summary>
        /// 程序化生成武器（备用方案）
        /// 当AssetBundle不存在时使用
        /// 注意：程序化生成的武器功能有限，建议使用AssetBundle
        /// </summary>
        private void CreateProceduralWeapon()
        {
            Debug.Log("[名刀月影] 开始程序化生成武器");

            // 创建基础GameObject
            GameObject weaponObj = new GameObject("MoonlightSword");

            // 添加Item组件（注意：属性在Prefab中是只读的，这里只能使用默认值）
            moonlightSwordPrefab = weaponObj.AddComponent<Item>();

            // 创建简单的刀模型
            CreateSimpleSwordModel(weaponObj);

            // 创建剑气特效
            if (swordAuraPrefab == null)
            {
                swordAuraPrefab = CreateSwordAuraEffect();
            }

            Debug.Log("[名刀月影] 程序化武器生成完成");
            Debug.LogWarning("[名刀月影] 注意：程序化生成的武器属性使用默认值，建议提供AssetBundle");
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

            // 创建刀身材质
            Material bladeMaterial = new Material(Shader.Find("Standard"));
            bladeMaterial.color = new Color(0.91f, 0.91f, 0.94f); // 银白色
            bladeMaterial.SetFloat("_Metallic", 0.95f);
            bladeMaterial.SetFloat("_Glossiness", 0.9f);
            bladeMaterial.EnableKeyword("_EMISSION");
            bladeMaterial.SetColor("_EmissionColor", new Color(0.63f, 0.78f, 0.91f) * 0.5f);
            blade.GetComponent<Renderer>().material = bladeMaterial;

            // 移除默认碰撞体
            Object.Destroy(blade.GetComponent<Collider>());

            // 护手
            GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            guard.name = "Guard";
            guard.transform.SetParent(parent.transform);
            guard.transform.localPosition = new Vector3(0, 0.1f, 0);
            guard.transform.localScale = new Vector3(0.15f, 0.02f, 0.05f);

            Material guardMaterial = new Material(Shader.Find("Standard"));
            guardMaterial.color = new Color(0.1f, 0.17f, 0.29f); // 深蓝色
            guardMaterial.SetFloat("_Metallic", 0.8f);
            guard.GetComponent<Renderer>().material = guardMaterial;
            Object.Destroy(guard.GetComponent<Collider>());

            // 刀柄
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(parent.transform);
            handle.transform.localPosition = new Vector3(0, -0.05f, 0);
            handle.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);

            Material handleMaterial = new Material(Shader.Find("Standard"));
            handleMaterial.color = new Color(0.23f, 0.18f, 0.35f); // 深紫色
            handle.GetComponent<Renderer>().material = handleMaterial;
            Object.Destroy(handle.GetComponent<Collider>());

            // 添加拾取碰撞体
            BoxCollider collider = parent.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.2f, 1.5f, 0.2f);
            collider.center = new Vector3(0, 0.75f, 0);

            Debug.Log("[名刀月影] 简易刀模型创建完成");
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

            Debug.Log("[名刀月影] 简易剑气特效创建完成");

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
                    Debug.LogWarning("[名刀月影] 无法获取商店数据库，武器将不会出现在商店中");
                    return;
                }

                // 获取商人配置列表
                var merchantProfiles = shopDatabase.merchantProfiles;
                if (merchantProfiles == null || merchantProfiles.Count == 0)
                {
                    Debug.LogWarning("[名刀月影] 商店数据库中没有商人配置");
                    return;
                }

                // 遍历所有商人，添加武器
                foreach (var profile in merchantProfiles)
                {
                    if (profile == null || profile.entries == null) continue;

                    // 检查是否已存在
                    bool exists = false;
                    foreach (var entry in profile.entries)
                    {
                        if (entry.typeID == WEAPON_TYPE_ID)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        // 创建新的物品条目
                        var newEntry = new StockShopDatabase.ItemEntry
                        {
                            typeID = WEAPON_TYPE_ID,
                            maxStock = 1,           // 传说武器每次只刷新1把
                            priceFactor = 1.0f,     // 价格倍率（商店本身会涨价）
                            possibility = 1.0f,     // 100%出现概率
                            forceUnlock = true,     // 强制解锁
                            lockInDemo = false
                        };

                        profile.entries.Add(newEntry);
                        Debug.Log($"[名刀月影] 已添加武器到商人 {profile.merchantID}");
                    }
                }

                Debug.Log("[名刀月影] 武器已添加到商店");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[名刀月影] 注册商店失败: {e.Message}");
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

            Debug.Log("[名刀月影] 开始箱子物品注入...");

            // 已处理的箱子集合，避免重复注入
            HashSet<int> processedBoxes = new HashSet<int>();

            while (true)
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
            // 获取场景中所有 LootBoxLoader
            var lootBoxLoaders = FindObjectsOfType<Duckov.Utilities.LootBoxLoader>();

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

                    if (fixedItemsField != null)
                    {
                        var fixedItems = fixedItemsField.GetValue(loader) as List<int>;
                        if (fixedItems != null)
                        {
                            // 5%概率添加名刀月影（传说武器稀有度较高）
                            if (!fixedItems.Contains(WEAPON_TYPE_ID) && Random.value < 0.05f)
                            {
                                fixedItems.Add(WEAPON_TYPE_ID);
                                Debug.Log($"[名刀月影] 已修改 LootBoxLoader: {loader.gameObject.name}");
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[名刀月影] 修改 LootBoxLoader 失败: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 注入到已存在的箱子背包
        /// </summary>
        private void InjectToExistingLootboxes(HashSet<int> processedBoxes)
        {
            // 获取场景中所有 InteractableLootbox
            var lootboxes = FindObjectsOfType<InteractableLootbox>();

            foreach (var lootbox in lootboxes)
            {
                if (lootbox == null) continue;

                int instanceId = lootbox.GetInstanceID();

                // 跳过已处理的箱子
                if (processedBoxes.Contains(instanceId)) continue;

                try
                {
                    // 尝试获取或创建箱子的背包
                    var getInventoryMethod = typeof(InteractableLootbox).GetMethod("GetOrCreateInventory",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (getInventoryMethod != null)
                    {
                        var inventory = getInventoryMethod.Invoke(null, new object[] { lootbox }) as Inventory;

                        if (inventory != null)
                        {
                            // 5%概率添加名刀月影（传说武器稀有度较高）
                            if (Random.value < 0.05f)
                            {
                                bool success = TryAddWeaponToInventory(inventory);
                                if (success)
                                {
                                    Debug.Log($"[名刀月影] 已向箱子 {lootbox.gameObject.name} 注入武器");
                                }
                            }
                        }
                    }

                    // 标记为已处理
                    processedBoxes.Add(instanceId);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[名刀月影] 注入到箱子失败: {e.Message}");
                    // 仍然标记为已处理，避免重复尝试
                    processedBoxes.Add(instanceId);
                }
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
                Debug.LogWarning($"[名刀月影] 添加武器到背包失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Mod卸载时清理资源
        /// </summary>
        void OnDestroy()
        {
            Debug.Log("[名刀月影] 开始卸载Mod");

            // 取消本地化事件监听
            LocalizationManager.OnSetLanguage -= OnLanguageChanged;

            // 移除动态物品
            if (moonlightSwordPrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(moonlightSwordPrefab);
                Debug.Log("[名刀月影] 武器已从游戏中移除");
            }

            // 卸载AssetBundle
            if (weaponBundle != null)
            {
                weaponBundle.Unload(true);
                Debug.Log("[名刀月影] AssetBundle已卸载");
            }

            Debug.Log("[名刀月影] Mod卸载完成");
        }
    }
}
