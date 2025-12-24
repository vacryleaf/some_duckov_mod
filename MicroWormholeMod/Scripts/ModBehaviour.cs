using UnityEngine;
using UnityEngine.SceneManagement;
using Duckov.Modding;
using ItemStatsSystem;
using SodaCraft.Localizations;
using System.IO;

namespace MicroWormholeMod
{
    /// <summary>
    /// 虫洞记录数据
    /// 保存使用微型虫洞时的位置和场景信息
    /// </summary>
    public class WormholeData
    {
        // 是否有有效的记录
        public bool IsValid { get; set; } = false;

        // 记录的位置
        public Vector3 Position { get; set; }

        // 记录的旋转
        public Quaternion Rotation { get; set; }

        // 场景名称
        public string SceneName { get; set; }

        // 清除记录
        public void Clear()
        {
            IsValid = false;
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
            SceneName = null;
        }
    }

    /// <summary>
    /// 微型虫洞Mod主类
    /// 提供两个物品：
    /// 1. 微型虫洞 - 记录当前位置并撤离回家
    /// 2. 虫洞回溯 - 在家中使用，传送回记录的位置
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // 物品Prefab
        private Item wormholePrefab;      // 微型虫洞
        private Item recallPrefab;        // 虫洞回溯
        private Item grenadePrefab;       // 虫洞手雷

        // 物品TypeID（使用较大的数值避免与游戏本体和其他Mod冲突）
        private const int WORMHOLE_TYPE_ID = 990001;  // 微型虫洞
        private const int RECALL_TYPE_ID = 990002;    // 虫洞回溯
        private const int GRENADE_TYPE_ID = 990003;   // 虫洞手雷

        // AssetBundle
        private AssetBundle assetBundle;

        // 物品图标
        private Sprite wormholeIcon;
        private Sprite recallIcon;
        private Sprite grenadeIcon;

        // 虫洞记录数据（静态，跨场景保持）
        private static WormholeData savedWormholeData = new WormholeData();

        // 是否正在等待场景加载完成后传送
        private static bool pendingTeleport = false;

        /// <summary>
        /// Mod启动入口
        /// </summary>
        void Start()
        {
            Debug.Log("[微型虫洞] 开始加载Mod...");

            try
            {
                // 加载 AssetBundle
                LoadAssetBundle();

                // 设置本地化文本
                SetupLocalization();

                // 创建物品Prefab
                CreateWormholeItem();
                CreateRecallItem();
                CreateGrenadeItem();

                // 注册到游戏系统
                RegisterItems();

                // 注册到商店（自动售卖机）
                RegisterToShop();

                // 监听物品使用事件
                RegisterEvents();

                // 检查是否需要传送（场景加载后）
                CheckPendingTeleport();

                // 启动箱子物品注入协程
                StartCoroutine(LootBoxInjectionRoutine());

                Debug.Log("[微型虫洞] Mod加载完成!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[微型虫洞] Mod加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 检查是否有待处理的传送
        /// </summary>
        private void CheckPendingTeleport()
        {
            if (pendingTeleport && savedWormholeData.IsValid)
            {
                // 延迟执行传送，等待场景完全初始化
                StartCoroutine(DelayedTeleport());
            }
        }

        /// <summary>
        /// 延迟传送协程
        /// </summary>
        private System.Collections.IEnumerator DelayedTeleport()
        {
            // 等待几帧，确保场景完全加载
            yield return new WaitForSeconds(1f);

            // 执行传送
            TeleportToSavedPosition();

            // 清除标记
            pendingTeleport = false;
        }

        /// <summary>
        /// 传送到保存的位置
        /// </summary>
        private void TeleportToSavedPosition()
        {
            if (!savedWormholeData.IsValid)
            {
                Debug.LogWarning("[微型虫洞] 没有有效的传送数据");
                return;
            }

            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter == null)
            {
                Debug.LogWarning("[微型虫洞] 找不到主角");
                return;
            }

            Debug.Log($"[微型虫洞] 正在传送到: {savedWormholeData.Position}");

            // 传送角色
            mainCharacter.transform.position = savedWormholeData.Position;
            mainCharacter.transform.rotation = savedWormholeData.Rotation;

            // 播放特效
            PlayWormholeEffect();

            // 显示提示
            ShowMessage("虫洞回溯成功！");

            // 清除保存的数据
            savedWormholeData.Clear();

            Debug.Log("[微型虫洞] 传送完成");
        }

        /// <summary>
        /// 加载 AssetBundle
        /// </summary>
        private void LoadAssetBundle()
        {
            string modPath = Path.GetDirectoryName(GetType().Assembly.Location);
            string bundlePath = Path.Combine(modPath, "Assets", "micro_wormhole");

            Debug.Log($"[微型虫洞] 正在加载 AssetBundle: {bundlePath}");

            if (!File.Exists(bundlePath))
            {
                Debug.LogWarning($"[微型虫洞] AssetBundle 文件不存在: {bundlePath}，将使用程序生成的模型");
                return;
            }

            assetBundle = AssetBundle.LoadFromFile(bundlePath);

            if (assetBundle == null)
            {
                Debug.LogWarning("[微型虫洞] AssetBundle 加载失败，将使用程序生成的模型");
                return;
            }

            // 加载图标
            wormholeIcon = LoadIconFromBundle("MicroWormholeIcon");
            recallIcon = LoadIconFromBundle("WormholeRecallIcon");
            grenadeIcon = LoadIconFromBundle("WormholeGrenadeIcon");

            Debug.Log("[微型虫洞] AssetBundle 加载完成");
        }

        /// <summary>
        /// 从 AssetBundle 加载图标
        /// </summary>
        private Sprite LoadIconFromBundle(string iconName)
        {
            if (assetBundle == null) return null;

            Sprite icon = assetBundle.LoadAsset<Sprite>(iconName);
            if (icon == null)
            {
                Texture2D iconTex = assetBundle.LoadAsset<Texture2D>(iconName);
                if (iconTex != null)
                {
                    icon = Sprite.Create(iconTex, new Rect(0, 0, iconTex.width, iconTex.height), new Vector2(0.5f, 0.5f));
                }
            }
            return icon;
        }

        /// <summary>
        /// 设置本地化文本
        /// </summary>
        private void SetupLocalization()
        {
            // 微型虫洞
            LocalizationManager.SetOverrideText("MicroWormhole_Name", "微型虫洞");
            LocalizationManager.SetOverrideText("MicroWormhole_Desc", "高科技传送装置。使用后会记录当前位置并撤离回家。\n\n<color=#FFD700>配合「回溯虫洞」使用，可返回记录的位置</color>");

            // 虫洞回溯
            LocalizationManager.SetOverrideText("WormholeRecall_Name", "回溯虫洞");
            LocalizationManager.SetOverrideText("WormholeRecall_Desc", "虫洞传送的配套装置。在家中使用，可以传送回「微型虫洞」记录的位置。\n\n<color=#FFD700>只能在家中使用</color>");

            // 虫洞手雷
            LocalizationManager.SetOverrideText("WormholeGrenade_Name", "虫洞手雷");
            LocalizationManager.SetOverrideText("WormholeGrenade_Desc", "高科技空间扰乱装置。投掷后引爆，将范围内的所有生物随机传送到地图某处。\n\n<color=#87CEEB>特殊效果：</color>\n• 引信延迟：3秒\n• 传送范围：8米\n• 影响所有角色（包括自己）\n\n<color=#FFD700>「混乱是战场上最好的掩护」</color>");

            // 监听语言切换事件
            LocalizationManager.OnSetLanguage += OnLanguageChanged;

            Debug.Log("[微型虫洞] 本地化设置完成");
        }

        /// <summary>
        /// 语言切换时更新本地化文本
        /// </summary>
        private void OnLanguageChanged(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.English:
                    LocalizationManager.SetOverrideText("MicroWormhole_Name", "Micro Wormhole");
                    LocalizationManager.SetOverrideText("MicroWormhole_Desc", "High-tech teleportation device. Records current position and evacuates home.\n\n<color=#FFD700>Use with 'Wormhole Recall' to return to the recorded position</color>");
                    LocalizationManager.SetOverrideText("WormholeRecall_Name", "Wormhole Recall");
                    LocalizationManager.SetOverrideText("WormholeRecall_Desc", "Companion device for wormhole teleportation. Use at home to teleport back to the position recorded by 'Micro Wormhole'.\n\n<color=#FFD700>Can only be used at home</color>");
                    LocalizationManager.SetOverrideText("WormholeGrenade_Name", "Wormhole Grenade");
                    LocalizationManager.SetOverrideText("WormholeGrenade_Desc", "High-tech spatial disruption device. Throw and detonate to teleport all creatures in range to random locations on the map.\n\n<color=#87CEEB>Special Effects:</color>\n• Fuse Delay: 3 seconds\n• Teleport Range: 8 meters\n• Affects all characters (including yourself)\n\n<color=#FFD700>\"Chaos is the best cover on the battlefield\"</color>");
                    break;
                default:
                    LocalizationManager.SetOverrideText("MicroWormhole_Name", "微型虫洞");
                    LocalizationManager.SetOverrideText("MicroWormhole_Desc", "高科技传送装置。使用后会记录当前位置并撤离回家。\n\n<color=#FFD700>配合「回溯虫洞」使用，可返回记录的位置</color>");
                    LocalizationManager.SetOverrideText("WormholeRecall_Name", "回溯虫洞");
                    LocalizationManager.SetOverrideText("WormholeRecall_Desc", "虫洞传送的配套装置。在家中使用，可以传送回「微型虫洞」记录的位置。\n\n<color=#FFD700>只能在家中使用</color>");
                    LocalizationManager.SetOverrideText("WormholeGrenade_Name", "虫洞手雷");
                    LocalizationManager.SetOverrideText("WormholeGrenade_Desc", "高科技空间扰乱装置。投掷后引爆，将范围内的所有生物随机传送到地图某处。\n\n<color=#87CEEB>特殊效果：</color>\n• 引信延迟：3秒\n• 传送范围：8米\n• 影响所有角色（包括自己）\n\n<color=#FFD700>「混乱是战场上最好的掩护」</color>");
                    break;
            }
        }

        /// <summary>
        /// 创建微型虫洞物品
        /// </summary>
        private void CreateWormholeItem()
        {
            Debug.Log("[微型虫洞] 开始创建微型虫洞Prefab...");

            GameObject itemObj = CreateItemGameObject("MicroWormhole", new Color(0.5f, 0.2f, 0.9f));

            DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            wormholePrefab = itemObj.AddComponent<Item>();
            ConfigureItemProperties(wormholePrefab, WORMHOLE_TYPE_ID, "MicroWormhole_Name", "MicroWormhole_Desc", wormholeIcon);

            itemObj.AddComponent<MicroWormholeUse>();

            Debug.Log("[微型虫洞] 微型虫洞Prefab创建完成");
        }

        /// <summary>
        /// 创建虫洞回溯物品
        /// </summary>
        private void CreateRecallItem()
        {
            Debug.Log("[微型虫洞] 开始创建虫洞回溯Prefab...");

            GameObject itemObj = CreateItemGameObject("WormholeRecall", new Color(0.2f, 0.8f, 0.5f)); // 绿色

            DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            recallPrefab = itemObj.AddComponent<Item>();
            ConfigureItemProperties(recallPrefab, RECALL_TYPE_ID, "WormholeRecall_Name", "WormholeRecall_Desc", recallIcon);

            Debug.Log("[微型虫洞] 虫洞回溯Prefab创建完成");
        }

        /// <summary>
        /// 创建虫洞手雷物品
        /// </summary>
        private void CreateGrenadeItem()
        {
            Debug.Log("[微型虫洞] 开始创建虫洞手雷Prefab...");

            GameObject itemObj = CreateGrenadeGameObject("WormholeGrenade", new Color(1f, 0.5f, 0.2f)); // 橙色

            DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            grenadePrefab = itemObj.AddComponent<Item>();
            ConfigureGrenadeProperties(grenadePrefab, GRENADE_TYPE_ID, "WormholeGrenade_Name", "WormholeGrenade_Desc", grenadeIcon);

            // 添加虫洞手雷使用组件
            itemObj.AddComponent<WormholeGrenadeUse>();

            Debug.Log("[微型虫洞] 虫洞手雷Prefab创建完成");
        }

        /// <summary>
        /// 创建虫洞手雷GameObject
        /// </summary>
        private GameObject CreateGrenadeGameObject(string name, Color color)
        {
            GameObject itemObj = new GameObject(name);

            // 创建手雷的视觉效果
            CreateGrenadeVisual(itemObj, color);

            return itemObj;
        }

        /// <summary>
        /// 创建手雷的视觉效果
        /// </summary>
        private void CreateGrenadeVisual(GameObject parent, Color color)
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
            Object.Destroy(body.GetComponent<Collider>());

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
            Object.Destroy(ring.GetComponent<Collider>());

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
        /// 配置虫洞手雷物品属性
        /// </summary>
        private void ConfigureGrenadeProperties(Item item, int typeId, string nameKey, string descKey, Sprite icon)
        {
            SetFieldValue(item, "typeID", typeId);

            // displayName 和 description 存储本地化键，游戏会自动调用 LocalizationManager.GetPlainText 查找
            SetFieldValue(item, "displayName", nameKey);
            SetFieldValue(item, "description", descKey);

            if (icon != null)
            {
                SetFieldValue(item, "icon", icon);
            }

            SetFieldValue(item, "stackable", true);
            SetFieldValue(item, "maxStackCount", 3);    // 手雷堆叠数较少
            SetFieldValue(item, "usable", true);
            SetFieldValue(item, "quality", 5);          // 传说级
            SetFieldValue(item, "value", 25000);        // 更贵
            SetFieldValue(item, "weight", 0.3f);        // 比虫洞重

            // 添加 UsageUtilities 组件，使物品可使用
            item.AddUsageUtilitiesComponent();
            Debug.Log($"[微型虫洞] 已为虫洞手雷 {typeId} 添加 UsageUtilities 组件，本地化键: {nameKey}");
        }

        /// <summary>
        /// 创建物品GameObject
        /// </summary>
        private GameObject CreateItemGameObject(string name, Color color)
        {
            GameObject itemObj = null;

            // 尝试从 AssetBundle 加载模型
            if (assetBundle != null)
            {
                GameObject prefab = assetBundle.LoadAsset<GameObject>("MicroWormhole");
                if (prefab != null)
                {
                    itemObj = Instantiate(prefab);
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
        /// 配置物品属性
        /// </summary>
        private void ConfigureItemProperties(Item item, int typeId, string nameKey, string descKey, Sprite icon)
        {
            SetFieldValue(item, "typeID", typeId);

            // displayName 和 description 存储本地化键，游戏会自动调用 LocalizationManager.GetPlainText 查找
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
            SetFieldValue(item, "value", 10000);
            SetFieldValue(item, "weight", 0.1f);

            // 添加 UsageUtilities 组件，使物品可使用
            item.AddUsageUtilitiesComponent();
            Debug.Log($"[微型虫洞] 已为物品 {typeId} 添加 UsageUtilities 组件，本地化键: {nameKey}");
        }

        /// <summary>
        /// 使用反射设置字段值
        /// </summary>
        private void SetFieldValue(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                var prop = type.GetProperty(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(obj, value);
                }
            }
        }

        /// <summary>
        /// 创建虫洞的视觉效果
        /// </summary>
        private void CreateWormholeVisual(GameObject parent, Color color)
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
            Object.Destroy(visual.GetComponent<Collider>());

            SphereCollider collider = parent.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
        }

        /// <summary>
        /// 注册物品到游戏系统
        /// </summary>
        private void RegisterItems()
        {
            if (wormholePrefab != null)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(wormholePrefab);
                Debug.Log($"[微型虫洞] 微型虫洞注册: {(success ? "成功" : "失败")}");
            }

            if (recallPrefab != null)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(recallPrefab);
                Debug.Log($"[微型虫洞] 虫洞回溯注册: {(success ? "成功" : "失败")}");
            }

            if (grenadePrefab != null)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(grenadePrefab);
                Debug.Log($"[微型虫洞] 虫洞手雷注册: {(success ? "成功" : "失败")}");
            }
        }

        /// <summary>
        /// 注册物品到商店（自动售卖机）
        /// </summary>
        private void RegisterToShop()
        {
            try
            {
                // 获取商店数据库实例
                var shopDatabase = StockShopDatabase.Instance;
                if (shopDatabase == null)
                {
                    Debug.LogWarning("[微型虫洞] 无法获取商店数据库，物品将不会出现在商店中");
                    return;
                }

                // 获取商人配置列表
                var merchantProfiles = shopDatabase.merchantProfiles;
                if (merchantProfiles == null || merchantProfiles.Count == 0)
                {
                    Debug.LogWarning("[微型虫洞] 商店数据库中没有商人配置");
                    return;
                }

                // 遍历所有商人，添加物品
                foreach (var profile in merchantProfiles)
                {
                    if (profile == null || profile.entries == null) continue;

                    // 添加微型虫洞（价格倍率1.0，商店本身会涨价）
                    AddItemToMerchant(profile, WORMHOLE_TYPE_ID, 2, 1.0f, 1.0f);

                    // 添加回溯虫洞（价格倍率1.0，商店本身会涨价）
                    AddItemToMerchant(profile, RECALL_TYPE_ID, 2, 1.0f, 1.0f);

                    // 添加虫洞手雷（价格倍率1.0，商店本身会涨价）
                    AddItemToMerchant(profile, GRENADE_TYPE_ID, 1, 1.0f, 1.0f);
                }

                Debug.Log("[微型虫洞] 物品已添加到商店");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[微型虫洞] 注册商店失败: {e.Message}");
            }
        }

        /// <summary>
        /// 向商人添加物品
        /// </summary>
        /// <param name="profile">商人配置</param>
        /// <param name="typeId">物品TypeID</param>
        /// <param name="maxStock">最大库存</param>
        /// <param name="priceFactor">价格倍率</param>
        /// <param name="possibility">出现概率 (0-1)</param>
        private void AddItemToMerchant(StockShopDatabase.MerchantProfile profile, int typeId, int maxStock, float priceFactor, float possibility)
        {
            // 检查是否已存在
            foreach (var entry in profile.entries)
            {
                if (entry.typeID == typeId)
                {
                    Debug.Log($"[微型虫洞] 物品 {typeId} 已存在于商人 {profile.merchantID} 中");
                    return;
                }
            }

            // 创建新的物品条目
            var newEntry = new StockShopDatabase.ItemEntry
            {
                typeID = typeId,
                maxStock = maxStock,
                priceFactor = priceFactor,
                possibility = possibility,
                forceUnlock = true,  // 强制解锁，不需要完成任务
                lockInDemo = false
            };

            profile.entries.Add(newEntry);
            Debug.Log($"[微型虫洞] 已添加物品 {typeId} 到商人 {profile.merchantID}");
        }

        /// <summary>
        /// 注册事件监听
        /// </summary>
        private void RegisterEvents()
        {
            Item.onUseStatic += OnItemUsed;
            Debug.Log("[微型虫洞] 事件监听注册完成");
        }

        /// <summary>
        /// 物品使用事件处理
        /// </summary>
        private void OnItemUsed(Item item, object user)
        {
            if (item == null) return;

            if (item.TypeID == WORMHOLE_TYPE_ID)
            {
                Debug.Log("[微型虫洞] 检测到微型虫洞被使用！");
                OnWormholeUsed(item);
            }
            else if (item.TypeID == RECALL_TYPE_ID)
            {
                Debug.Log("[微型虫洞] 检测到虫洞回溯被使用！");
                OnRecallUsed(item);
            }
        }

        /// <summary>
        /// 微型虫洞使用逻辑
        /// </summary>
        private void OnWormholeUsed(Item item)
        {
            if (LevelManager.Instance == null)
            {
                Debug.LogWarning("[微型虫洞] LevelManager未初始化");
                return;
            }

            if (LevelManager.Instance.IsBaseLevel)
            {
                ShowMessage("你已经在家中了！");
                return;
            }

            if (!LevelManager.Instance.IsRaidMap)
            {
                ShowMessage("只能在突袭任务中使用！");
                return;
            }

            // 记录当前位置和场景
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter != null)
            {
                savedWormholeData.IsValid = true;
                savedWormholeData.Position = mainCharacter.transform.position;
                savedWormholeData.Rotation = mainCharacter.transform.rotation;
                savedWormholeData.SceneName = SceneManager.GetActiveScene().name;

                Debug.Log($"[微型虫洞] 位置已记录: {savedWormholeData.Position}, 场景: {savedWormholeData.SceneName}");
                ShowMessage("位置已记录！正在撤离...");
            }

            // 播放特效
            PlayWormholeEffect();

            // 消耗物品
            ConsumeItem(item);

            // 触发撤离
            EvacuationInfo evacuationInfo = new EvacuationInfo();
            LevelManager.Instance.NotifyEvacuated(evacuationInfo);

            Debug.Log("[微型虫洞] 撤离成功！");
        }

        /// <summary>
        /// 虫洞回溯使用逻辑
        /// </summary>
        private void OnRecallUsed(Item item)
        {
            if (LevelManager.Instance == null)
            {
                Debug.LogWarning("[微型虫洞] LevelManager未初始化");
                return;
            }

            // 检查是否在家中
            if (!LevelManager.Instance.IsBaseLevel)
            {
                ShowMessage("只能在家中使用！");
                return;
            }

            // 检查是否有有效的记录（是否使用过微型虫洞）
            if (!savedWormholeData.IsValid)
            {
                ShowMessage("没有可回溯的虫洞残留");
                return;
            }

            Debug.Log($"[微型虫洞] 正在回溯到: {savedWormholeData.SceneName} - {savedWormholeData.Position}");

            // 播放特效
            PlayWormholeEffect();

            // 消耗物品
            ConsumeItem(item);

            // 设置待传送标记
            pendingTeleport = true;

            // 加载目标场景
            ShowMessage($"正在打开虫洞通道...");

            // 使用 SceneLoader 加载场景
            try
            {
                // 获取 SceneLoader 实例并加载场景
                var sceneLoaderType = System.Type.GetType("SceneLoader, TeamSoda.Duckov.Core");
                if (sceneLoaderType != null)
                {
                    var instanceProp = sceneLoaderType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (instanceProp != null)
                    {
                        var sceneLoader = instanceProp.GetValue(null);
                        if (sceneLoader != null)
                        {
                            Debug.Log($"[微型虫洞] 找到 SceneLoader，准备加载场景: {savedWormholeData.SceneName}");
                            // 场景加载将在 Start 中通过 pendingTeleport 标记处理
                            SceneManager.LoadScene(savedWormholeData.SceneName);
                        }
                    }
                }
                else
                {
                    // 直接使用 Unity 的场景管理器
                    Debug.Log($"[微型虫洞] 使用 Unity SceneManager 加载场景: {savedWormholeData.SceneName}");
                    SceneManager.LoadScene(savedWormholeData.SceneName);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[微型虫洞] 场景加载失败: {e.Message}");
                ShowMessage("虫洞通道开启失败！");
                pendingTeleport = false;
            }
        }

        /// <summary>
        /// 播放虫洞特效
        /// </summary>
        private void PlayWormholeEffect()
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter == null) return;

            Vector3 position = mainCharacter.transform.position;

            GameObject effectObj = new GameObject("WormholeEffect");
            effectObj.transform.position = position;

            ParticleSystem particles = effectObj.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(0.6f, 0.3f, 1f, 0.8f);
            main.startSize = 0.5f;
            main.startLifetime = 1f;
            main.startSpeed = 5f;
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
        /// 消耗物品
        /// </summary>
        private void ConsumeItem(Item item)
        {
            if (item == null) return;

            if (item.Stackable && item.StackCount > 1)
            {
                Debug.Log("[微型虫洞] 减少堆叠数量");
            }
            else
            {
                item.Detach();
                Destroy(item.gameObject);
                Debug.Log("[微型虫洞] 物品已消耗");
            }
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
        /// 箱子物品注入协程
        /// 定期扫描场景中的箱子，注入虫洞物品
        /// </summary>
        private System.Collections.IEnumerator LootBoxInjectionRoutine()
        {
            // 等待场景完全加载
            yield return new WaitForSeconds(2f);

            Debug.Log("[微型虫洞] 开始箱子物品注入...");

            // 已处理的箱子集合，避免重复注入
            System.Collections.Generic.HashSet<int> processedBoxes = new System.Collections.Generic.HashSet<int>();

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
                    Debug.LogWarning($"[微型虫洞] 箱子注入时发生错误: {e.Message}");
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
                        var fixedItems = fixedItemsField.GetValue(loader) as System.Collections.Generic.List<int>;
                        if (fixedItems != null)
                        {
                            bool modified = false;

                            // 有一定概率添加虫洞物品
                            if (!fixedItems.Contains(WORMHOLE_TYPE_ID) && Random.value < 0.15f)
                            {
                                fixedItems.Add(WORMHOLE_TYPE_ID);
                                modified = true;
                            }

                            if (!fixedItems.Contains(RECALL_TYPE_ID) && Random.value < 0.15f)
                            {
                                fixedItems.Add(RECALL_TYPE_ID);
                                modified = true;
                            }

                            if (!fixedItems.Contains(GRENADE_TYPE_ID) && Random.value < 0.10f)
                            {
                                fixedItems.Add(GRENADE_TYPE_ID);
                                modified = true;
                            }

                            if (modified)
                            {
                                Debug.Log($"[微型虫洞] 已修改 LootBoxLoader: {loader.gameObject.name}");
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[微型虫洞] 修改 LootBoxLoader 失败: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 注入到已存在的箱子背包
        /// </summary>
        private void InjectToExistingLootboxes(System.Collections.Generic.HashSet<int> processedBoxes)
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
                        var inventory = getInventoryMethod.Invoke(null, new object[] { lootbox }) as ItemStatsSystem.Inventory;

                        if (inventory != null)
                        {
                            // 三个物品独立计算概率，一个箱子可以同时刷出多个
                            int addedCount = 0;

                            // 18%概率添加微型虫洞（概率略高）
                            if (Random.value < 0.18f)
                            {
                                if (TryAddItemToInventory(inventory, WORMHOLE_TYPE_ID))
                                {
                                    addedCount++;
                                    Debug.Log($"[微型虫洞] 向箱子添加了微型虫洞");
                                }
                            }

                            // 15%概率添加回溯虫洞
                            if (Random.value < 0.15f)
                            {
                                if (TryAddItemToInventory(inventory, RECALL_TYPE_ID))
                                {
                                    addedCount++;
                                    Debug.Log($"[微型虫洞] 向箱子添加了回溯虫洞");
                                }
                            }

                            // 10%概率添加虫洞手雷（更稀有）
                            if (Random.value < 0.10f)
                            {
                                if (TryAddItemToInventory(inventory, GRENADE_TYPE_ID))
                                {
                                    addedCount++;
                                    Debug.Log($"[微型虫洞] 向箱子添加了虫洞手雷");
                                }
                            }

                            if (addedCount > 0)
                            {
                                Debug.Log($"[微型虫洞] 已向箱子 {lootbox.gameObject.name} 注入 {addedCount} 个物品");
                            }
                        }
                    }

                    // 标记为已处理
                    processedBoxes.Add(instanceId);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[微型虫洞] 注入到箱子失败: {e.Message}");
                    // 仍然标记为已处理，避免重复尝试
                    processedBoxes.Add(instanceId);
                }
            }
        }

        /// <summary>
        /// 尝试将物品添加到背包
        /// </summary>
        private bool TryAddItemToInventory(ItemStatsSystem.Inventory inventory, int typeId)
        {
            try
            {
                // 根据 TypeID 获取物品 prefab
                Item prefab = null;
                string itemName = "";

                if (typeId == WORMHOLE_TYPE_ID)
                {
                    prefab = wormholePrefab;
                    itemName = "微型虫洞";
                }
                else if (typeId == RECALL_TYPE_ID)
                {
                    prefab = recallPrefab;
                    itemName = "回溯虫洞";
                }
                else if (typeId == GRENADE_TYPE_ID)
                {
                    prefab = grenadePrefab;
                    itemName = "虫洞手雷";
                }

                if (prefab == null)
                {
                    Debug.LogWarning($"[微型虫洞] {itemName} prefab 为空，无法添加到背包");
                    return false;
                }

                // 创建物品实例
                var newItem = prefab.CreateInstance();
                if (newItem == null)
                {
                    Debug.LogWarning($"[微型虫洞] 无法创建 {itemName} 实例");
                    return false;
                }

                // 添加到背包
                bool success = inventory.AddItem(newItem);

                if (!success)
                {
                    // 如果添加失败，销毁创建的物品
                    Destroy(newItem.gameObject);
                    Debug.LogWarning($"[微型虫洞] 添加 {itemName} 到背包失败");
                }
                else
                {
                    Debug.Log($"[微型虫洞] 成功添加 {itemName} 到背包");
                }

                return success;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[微型虫洞] 添加物品到背包失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Mod卸载时清理资源
        /// </summary>
        void OnDestroy()
        {
            Debug.Log("[微型虫洞] 开始卸载Mod");

            Item.onUseStatic -= OnItemUsed;
            LocalizationManager.OnSetLanguage -= OnLanguageChanged;

            if (wormholePrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(wormholePrefab);
                Destroy(wormholePrefab.gameObject);
            }

            if (recallPrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(recallPrefab);
                Destroy(recallPrefab.gameObject);
            }

            if (grenadePrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(grenadePrefab);
                Destroy(grenadePrefab.gameObject);
            }

            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                assetBundle = null;
            }

            Debug.Log("[微型虫洞] Mod卸载完成");
        }
    }
}
