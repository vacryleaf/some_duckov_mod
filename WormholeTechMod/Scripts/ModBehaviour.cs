using UnityEngine;
using System;
using System.IO;
using System.Linq;
using ItemStatsSystem;
using SodaCraft.Localizations;

namespace WormholeTechMod
{
    /// <summary>
    /// 虫洞记录数据
    /// 保存使用微型虫洞时的位置和场景信息
    /// </summary>
    public class WormholeData
    {
        public bool IsValid { get; set; } = false;
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public string SceneName { get; set; }

        public void Clear()
        {
            IsValid = false;
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
            SceneName = null;
        }
    }

    /// <summary>
    /// 微型虫洞 Mod 主入口
    /// 协调各个子模块的初始化和运行
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // ========== 物品 Prefab ==========
        private Item wormholePrefab;
        private Item recallPrefab;
        private Item grenadePrefab;
        private Item badgePrefab;
        private Item blackHolePrefab;

        // ========== 技能引用 ==========
        private WormholeGrenadeSkill grenadeSkill;
        private BlackHoleGrenadeSkill blackHoleSkill;

        // ========== 子模块 ==========
        private WormholeTeleportManager teleportManager;
        private WormholeInventoryHelper inventoryHelper;
        private WormholeLootInjector lootInjector;
        private WormholeShopTerminal shopTerminal;
        private WormholeBadgeManager badgeManager;

        // ========== 资源 ==========
        private AssetBundle assetBundle;
        private Sprite wormholeIcon;
        private Sprite recallIcon;
        private Sprite grenadeIcon;
        private Sprite badgeIcon;
        private Sprite blackHoleIcon;

        /// <summary>
        /// Mod 启动入口
        /// </summary>
        void Start()
        {
            ModLogger.Log("开始加载Mod...");

            try
            {
                // 加载配置文件
                LoadConfiguration();

                // 加载 AssetBundle
                LoadAssetBundle();

                // 设置本地化文本
                SetupLocalization();

                // 初始化子模块
                InitializeSubModules();

                // 创建物品
                CreateAllItems();

                // 注册物品到游戏系统
                RegisterItems();

                // 配置子模块
                ConfigureSubModules();

                // 监听事件
                RegisterEvents();

                // 检查待传送
                teleportManager.CheckPendingTeleport();

                // 启动箱子注入
                lootInjector.StartInjection();

                // 监听背包变化
                inventoryHelper.StartWatchInventoryChanges();

                // 初始化商店终端
                InitializeShopTerminal();

                ModLogger.Log("Mod加载完成!");
            }
            catch (Exception e)
            {
                ModLogger.LogError(string.Format("Mod加载失败: {0}\n{1}", e.Message, e.StackTrace));
            }
        }

        /// <summary>
        /// 每帧更新 - 测试快捷键
        /// </summary>
        void Update()
        {
            // 按 F9 添加测试物品
            if (UnityEngine.Input.GetKeyDown(KeyCode.F9))
            {
                AddTestItems();
            }
        }

        #region 初始化

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                string modPath = Path.GetDirectoryName(GetType().Assembly.Location);
                ModConfig.Instance.Initialize(modPath);
                ModLogger.Log("配置文件加载完成");
            }
            catch (Exception e)
            {
                ModLogger.LogWarning(string.Format("配置文件加载失败，使用默认值: {0}", e.Message));
            }
        }

        /// <summary>
        /// 加载 AssetBundle
        /// </summary>
        private void LoadAssetBundle()
        {
            string modPath = Path.GetDirectoryName(GetType().Assembly.Location);
            string bundlePath = Path.Combine(modPath, "Assets", "micro_wormhole");

            ModLogger.Log(string.Format("正在加载 AssetBundle: {0}", bundlePath));

            if (!File.Exists(bundlePath))
            {
                ModLogger.LogWarning(string.Format("AssetBundle 文件不存在，将使用程序生成的模型"));
                return;
            }

            assetBundle = AssetBundle.LoadFromFile(bundlePath);

            if (assetBundle == null)
            {
                ModLogger.LogWarning("AssetBundle 加载失败，将使用程序生成的模型");
                return;
            }

            // 加载图标
            wormholeIcon = LoadIconFromBundle("MicroWormholeIcon");
            recallIcon = LoadIconFromBundle("WormholeRecallIcon");
            grenadeIcon = LoadIconFromBundle("WormholeGrenadeIcon");
            badgeIcon = LoadIconFromBundle("WormholeBadgeIcon");
            blackHoleIcon = LoadIconFromBundle("BlackHoleIcon");

            ModLogger.Log("AssetBundle 加载完成");
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
        /// 初始化子模块
        /// </summary>
        private void InitializeSubModules()
        {
            ModLogger.Log("[虫洞科技] 开始初始化子模块...");

            // 传送管理器
            GameObject teleportObj = new GameObject("WormholeTeleportManager");
            teleportObj.transform.SetParent(transform);
            DontDestroyOnLoad(teleportObj);
            teleportManager = teleportObj.AddComponent<WormholeTeleportManager>();

            // 背包辅助
            GameObject inventoryObj = new GameObject("WormholeInventoryHelper");
            inventoryObj.transform.SetParent(transform);
            DontDestroyOnLoad(inventoryObj);
            inventoryHelper = inventoryObj.AddComponent<WormholeInventoryHelper>();
            ModLogger.Log("[虫洞科技] WormholeInventoryHelper 已创建");

            // 箱子注入器
            GameObject lootObj = new GameObject("WormholeLootInjector");
            lootObj.transform.SetParent(transform);
            DontDestroyOnLoad(lootObj);
            lootInjector = lootObj.AddComponent<WormholeLootInjector>();

            // 徽章管理器
            GameObject badgeObj = new GameObject("WormholeBadgeManager");
            badgeObj.transform.SetParent(transform);
            DontDestroyOnLoad(badgeObj);
            badgeManager = badgeObj.AddComponent<WormholeBadgeManager>();

            ModLogger.Log("[虫洞科技] 子模块初始化完成");
        }

        /// <summary>
        /// 创建所有物品
        /// </summary>
        private void CreateAllItems()
        {
            wormholePrefab = WormholeItemFactory.CreateWormholeItem(assetBundle, wormholeIcon);
            recallPrefab = WormholeItemFactory.CreateRecallItem(recallIcon);
            grenadePrefab = WormholeItemFactory.CreateGrenadeItem(grenadeIcon, out grenadeSkill);
            badgePrefab = WormholeItemFactory.CreateBadgeItem(badgeIcon);
            blackHolePrefab = WormholeItemFactory.CreateBlackHoleItem(blackHoleIcon, out blackHoleSkill);

            // 修复 prefab 的 master 问题（关键！）
            // 将 prefab 的 agentUtilities.master 设置为 null
            // 这样 Instantiate 后的克隆品也会是 master = null
            // 然后在 Awake 中正确初始化
            FixPrefabMaster(wormholePrefab);
            FixPrefabMaster(recallPrefab);
            FixPrefabMaster(grenadePrefab);
            FixPrefabMaster(badgePrefab);
            FixPrefabMaster(blackHolePrefab);

            ModLogger.Log("所有物品创建完成");
        }

        /// <summary>
        /// 修复 prefab 的 AgentUtilities master 问题
        ///
        /// 问题根源：
        /// 1. Mod 在代码中创建 prefab 时，调用 Configure 方法设置 agentUtilities
        /// 2. prefab 的 Item.Awake() 被触发，调用 Initialize()，设置 initialized = true 和 master = prefab
        /// 3. Instantiate 克隆时，initialized 和 master 都被复制给克隆品
        /// 4. 克隆品的 Awake 检查 initialized == true，跳过 Initialize()
        /// 5. 最终克隆品的 master 仍然指向原始 prefab，不是自身
        ///
        /// 解决方案：
        /// 1. 将 prefab 的 agentUtilities.master 设置为 null
        /// 2. 将 prefab 的 initialized 设置为 false
        /// 这样 Instantiate 后的克隆品也会是 initialized = false 和 master = null
        /// 克隆品 Awake 时会调用 Initialize() 正确设置 master
        /// </summary>
        private void FixPrefabMaster(Item prefab)
        {
            if (prefab == null) return;

            try
            {
                var agentUtils = prefab.AgentUtilities;
                if (agentUtils == null) return;

                // 通过反射获取字段
                var masterField = typeof(ItemAgentUtilities).GetField("master",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                // 读取当前值用于日志
                var currentMaster = masterField?.GetValue(agentUtils);

                // 1. 设置 master = null
                masterField?.SetValue(agentUtils, null);

                // 2. 重置 initialized = false（关键步骤！）
                // 这样 Instantiate 后的克隆品也会是 initialized = false
                // 克隆品 Awake 时会调用 Initialize() 正确设置 master
                var initializedField = typeof(Item).GetField("initialized",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                initializedField?.SetValue(prefab, false);

                ModLogger.Log(string.Format("已修复 prefab {0}: master={1}→null, initialized=false",
                    prefab.DisplayName,
                    currentMaster == null ? "null" : "已设置"));
            }
            catch (Exception e)
            {
                ModLogger.LogWarning(string.Format("修复 prefab master 失败: {0}", e.Message));
            }
        }

        /// <summary>
        /// 注册物品到游戏系统
        /// </summary>
        private void RegisterItems()
        {
            if (wormholePrefab != null)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(wormholePrefab);
                ModLogger.Log(string.Format("微型虫洞注册: {0}", success ? "成功" : "失败"));
            }

            if (recallPrefab != null)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(recallPrefab);
                ModLogger.Log(string.Format("虫洞回溯注册: {0}", success ? "成功" : "失败"));
            }

            if (grenadePrefab != null)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(grenadePrefab);
                ModLogger.Log(string.Format("虫洞手雷注册: {0}", success ? "成功" : "失败"));
            }

            if (badgePrefab != null)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(badgePrefab);
                ModLogger.Log(string.Format("虫洞徽章注册: {0}", success ? "成功" : "失败"));
            }

            if (blackHolePrefab != null)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(blackHolePrefab);
                ModLogger.Log(string.Format("黑洞手雷注册: {0}", success ? "成功" : "失败"));
            }
        }

        /// <summary>
        /// 配置子模块
        /// </summary>
        private void ConfigureSubModules()
        {
            // 配置背包辅助的 prefab 引用
            inventoryHelper.SetPrefabs(wormholePrefab, recallPrefab, grenadePrefab, badgePrefab, blackHolePrefab);

            // 配置箱子注入器
            lootInjector.SetInventoryHelper(inventoryHelper);

            ModLogger.Log("子模块配置完成");
        }

        /// <summary>
        /// 注册事件监听
        /// </summary>
        private void RegisterEvents()
        {
            Item.onUseStatic += OnItemUsed;
            ModLogger.Log("事件监听注册完成");
        }

        /// <summary>
        /// 初始化商店终端
        /// </summary>
        private void InitializeShopTerminal()
        {
            try
            {
                GameObject terminalObj = new GameObject("WormholeShopTerminal");
                terminalObj.transform.SetParent(transform);
                DontDestroyOnLoad(terminalObj);

                // 添加独立商店组件
                var wormholeShop = terminalObj.AddComponent<WormholeTechShop>();
                wormholeShop.Initialize();

                // 添加商店终端组件
                shopTerminal = terminalObj.AddComponent<WormholeShopTerminal>();
                shopTerminal.openShopKey = ModConfig.Instance.ShopTerminalHotkey;
                shopTerminal.cooldownTime = ModConfig.Instance.ShopTerminalCooldown;
                shopTerminal.onlyInBase = ModConfig.Instance.ShopTerminalOnlyInBase;
                shopTerminal.useExclusiveShop = true;
                shopTerminal.SetWormholeShop(wormholeShop);

                ModLogger.Log(string.Format("商店终端初始化完成，快捷键: {0}", shopTerminal.openShopKey));
            }
            catch (Exception e)
            {
                ModLogger.LogError(string.Format("商店终端初始化失败: {0}", e.Message));
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 物品使用事件
        /// </summary>
        private void OnItemUsed(Item item, object user)
        {
            if (item == null) return;

            if (item.TypeID == WormholeItemFactory.WORMHOLE_TYPE_ID)
            {
                ModLogger.Log("检测到微型虫洞被使用！");
                OnWormholeUsed(item);
            }
            else if (item.TypeID == WormholeItemFactory.RECALL_TYPE_ID)
            {
                ModLogger.Log("检测到回溯虫洞被使用！");
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
                ModLogger.LogWarning("LevelManager未初始化");
                return;
            }

            if (LevelManager.Instance.IsBaseLevel)
            {
                teleportManager.ShowMessage("你已经在家中了！");
                return;
            }

            if (!LevelManager.Instance.IsRaidMap)
            {
                teleportManager.ShowMessage("只能在突袭任务中使用！");
                return;
            }

            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter != null)
            {
                var data = new WormholeData
                {
                    IsValid = true,
                    Position = mainCharacter.transform.position,
                    Rotation = mainCharacter.transform.rotation,
                    SceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                };
                teleportManager.SetWormholeData(data);

                ModLogger.Log(string.Format("位置已记录: {0}, 场景: {1}", data.Position, data.SceneName));
                teleportManager.ShowMessage("位置已记录！正在撤离...");
            }

            teleportManager.PlayWormholeEffect();
            ConsumeItem(item);

            EvacuationInfo evacuationInfo = new EvacuationInfo();
            LevelManager.Instance.NotifyEvacuated(evacuationInfo);

            ModLogger.Log("撤离成功！");
        }

        /// <summary>
        /// 虫洞回溯使用逻辑
        /// </summary>
        private void OnRecallUsed(Item item)
        {
            if (LevelManager.Instance == null)
            {
                ModLogger.LogWarning("LevelManager未初始化");
                return;
            }

            if (!LevelManager.Instance.IsBaseLevel)
            {
                teleportManager.ShowMessage("只能在家中使用！");
                return;
            }

            if (!teleportManager.HasValidWormholeData())
            {
                teleportManager.ShowMessage("没有可回溯的虫洞残留");
                return;
            }

            var data = teleportManager.GetWormholeData();
            ModLogger.Log(string.Format("正在回溯到: {0} - {1}", data.SceneName, data.Position));

            teleportManager.PlayWormholeEffect();
            ConsumeItem(item);

            teleportManager.ShowMessage("正在打开虫洞通道...");
            teleportManager.ExecuteRecall(CharacterMainControl.Main);
        }

        /// <summary>
        /// 语言切换事件
        /// </summary>
        private void OnLanguageChanged(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.English:
                    SetEnglishLocalization();
                    break;
                default:
                    SetChineseLocalization();
                    break;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 添加测试物品
        /// </summary>
        private void AddTestItems()
        {
            try
            {
                var character = CharacterMainControl.Main;
                if (character == null || character.CharacterItem == null)
                {
                    teleportManager.ShowMessage("无法找到玩家");
                    return;
                }

                var inventory = character.CharacterItem.Inventory;
                if (inventory == null)
                {
                    teleportManager.ShowMessage("无法找到背包");
                    return;
                }

                int addedCount = 0;

                if (inventoryHelper.TryAddItemToInventory(inventory, WormholeItemFactory.WORMHOLE_TYPE_ID))
                    addedCount++;
                if (inventoryHelper.TryAddItemToInventory(inventory, WormholeItemFactory.RECALL_TYPE_ID))
                    addedCount++;
                if (inventoryHelper.TryAddItemToInventory(inventory, WormholeItemFactory.GRENADE_TYPE_ID))
                    addedCount++;
                if (inventoryHelper.TryAddItemToInventory(inventory, WormholeItemFactory.BADGE_TYPE_ID))
                    addedCount++;
                if (inventoryHelper.TryAddItemToInventory(inventory, WormholeItemFactory.BLACKHOLE_TYPE_ID))
                    addedCount++;

                teleportManager.ShowMessage(string.Format("已添加 {0} 个虫洞物品到背包", addedCount));
                ModLogger.Log(string.Format("测试：添加了 {0} 个虫洞物品", addedCount));
            }
            catch (Exception e)
            {
                ModLogger.LogError(string.Format("添加测试物品失败: {0}", e.Message));
            }
        }

        /// <summary>
        /// 消耗物品
        /// </summary>
        private void ConsumeItem(Item item)
        {
            if (item == null) return;

            if (item.Stackable && item.StackCount > 1)
            {
                // 减少堆叠数量
                SetFieldValue(item, "stackCount", item.StackCount - 1);
                ModLogger.Log(string.Format("堆叠数量减少为: {0}", item.StackCount));
            }
            else
            {
                item.Detach();
                Destroy(item.gameObject);
                ModLogger.Log("物品已消耗");
            }
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
            field?.SetValue(obj, value);
        }

        /// <summary>
        /// 创建默认图标（当 AssetBundle 不存在时使用）
        /// </summary>
        private Sprite CreateDefaultIcon(string iconName, Color color)
        {
            try
            {
                int size = 64;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[size * size];

                // 绘制圆形图标
                Vector2 center = new Vector2(size / 2f, size / 2f);
                float radius = size / 2f - 4f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), center);
                        if (dist <= radius)
                        {
                            // 边缘发光效果
                            float edge = dist / radius;
                            float alpha = 1f;
                            if (edge > 0.8f)
                            {
                                alpha = 1f - (edge - 0.8f) / 0.2f;
                            }
                            pixels[y * size + x] = new Color(color.r, color.g, color.b, alpha * 0.8f);
                        }
                        else
                        {
                            pixels[y * size + x] = Color.clear;
                        }
                    }
                }

                texture.SetPixels(pixels);
                texture.Apply();

                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
                sprite.name = iconName;

                ModLogger.Log(string.Format("已创建默认图标: {0}", iconName));
                return sprite;
            }
            catch (Exception e)
            {
                ModLogger.LogWarning(string.Format("创建默认图标失败: {0}", e.Message));
                return null;
            }
        }

        #endregion

        #region 本地化

        /// <summary>
        /// 设置本地化
        /// </summary>
        private void SetupLocalization()
        {
            SetChineseLocalization();

            // 只在首次设置时注册事件
            var eventField = typeof(SodaCraft.Localizations.LocalizationManager).GetField("OnSetLanguage",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            bool alreadyRegistered = false;
            if (eventField != null)
            {
                var currentDelegate = eventField.GetValue(null) as System.Delegate;
                if (currentDelegate != null && currentDelegate.GetInvocationList().Contains((System.Action<SystemLanguage>)OnLanguageChanged))
                {
                    alreadyRegistered = true;
                }
            }

            if (!alreadyRegistered)
            {
                LocalizationManager.OnSetLanguage += OnLanguageChanged;
            }

            ModLogger.Log("本地化设置完成");
        }

        private void SetChineseLocalization()
        {
            LocalizationManager.SetOverrideText("MicroWormhole_Name", "微型虫洞");
            LocalizationManager.SetOverrideText("MicroWormhole_Desc", "高科技传送装置。使用后会记录当前位置并撤离回家。\n\n<color=#FFD700>配合「回溯虫洞」使用，可返回记录的位置</color>");
            LocalizationManager.SetOverrideText("WormholeRecall_Name", "回溯虫洞");
            LocalizationManager.SetOverrideText("WormholeRecall_Desc", "虫洞传送的配套装置。在家中使用，可以传送回「微型虫洞」记录的位置。\n\n<color=#FFD700>只能在家使用</color>");
            LocalizationManager.SetOverrideText("WormholeGrenade_Name", "虫洞手雷");
            LocalizationManager.SetOverrideText("WormholeGrenade_Desc", "高科技空间扰乱装置。投掷后引爆，将范围内的所有生物随机传送到地图某处。\n\n<color=#87CEEB>特殊效果：</color>\n• 引信延迟：3秒\n• 传送范围：8米\n• 影响所有角色（包括自己）\n\n<color=#FFD700>「混乱是战场上最好的掩护」</color>");
            LocalizationManager.SetOverrideText("WormholeBadge_Name", "虫洞徽章");
            LocalizationManager.SetOverrideText("WormholeBadge_Desc", "蕴含虫洞能量的神秘徽章。放在物品栏中即可生效。\n\n<color=#87CEEB>被动效果：</color>\n• 被击中时有10%概率使伤害无效化\n• 多个徽章乘法叠加，最多5个生效\n\n<color=#FFD700>「空间的裂缝，是最好的护盾」</color>");
            LocalizationManager.SetOverrideText("BlackHoleGenerator_Name", "黑洞手雷");
            LocalizationManager.SetOverrideText("BlackHoleGenerator_Desc", "高科技引力装置。投掷后生成一个黑洞，吸引范围内的敌人并造成持续伤害。\n\n<color=#87CEEB>特效：</color>\n• 持续时间：5秒\n• 吸引范围：5米\n• 每秒伤害：25点\n\n<color=#FFD700>「引力是宇宙最强大的力量」</color>");
        }

        private void SetEnglishLocalization()
        {
            LocalizationManager.SetOverrideText("MicroWormhole_Name", "Micro Wormhole");
            LocalizationManager.SetOverrideText("MicroWormhole_Desc", "High-tech teleportation device. Records current position and evacuates home.\n\n<color=#FFD700>Use with 'Wormhole Recall' to return to the recorded position</color>");
            LocalizationManager.SetOverrideText("WormholeRecall_Name", "Wormhole Recall");
            LocalizationManager.SetOverrideText("WormholeRecall_Desc", "Companion device for wormhole teleportation. Use at home to teleport back to the position recorded by 'Micro Wormhole'.\n\n<color=#FFD700>Can only be used at home</color>");
            LocalizationManager.SetOverrideText("WormholeGrenade_Name", "Wormhole Grenade");
            LocalizationManager.SetOverrideText("WormholeGrenade_Desc", "High-tech spatial disruption device. Throw and detonate to teleport all creatures in range to random locations.\n\n<color=#FFD700>\"Chaos is the best cover on the battlefield\"</color>");
            LocalizationManager.SetOverrideText("WormholeBadge_Name", "Wormhole Badge");
            LocalizationManager.SetOverrideText("WormholeBadge_Desc", "A mysterious badge infused with wormhole energy. Works passively in your inventory.\n\n<color=#87CEEB>Passive Effect:</color>\n• 10% chance to negate damage when hit\n• Multiple badges stack multiplicatively, max 5 effective\n\n<color=#FFD700>\"Crocks in space make the best shields\"</color>");
            LocalizationManager.SetOverrideText("BlackHoleGenerator_Name", "Micro Black Hole Generator");
            LocalizationManager.SetOverrideText("BlackHoleGenerator_Desc", "High-tech gravity device. Generates a micro black hole that pulls and damages enemies.\n\n<color=#FFD700>\"Gravity is the most powerful force in the universe\"</color>");
        }

        #endregion

        #region 清理

        void OnDestroy()
        {
            ModLogger.Log("开始卸载Mod");

            // 停止协程
            lootInjector?.StopInjection();
            inventoryHelper?.StopWatchInventoryChanges();

            // 取消事件监听
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

            if (badgePrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(badgePrefab);
                Destroy(badgePrefab.gameObject);
            }

            if (blackHolePrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(blackHolePrefab);
                Destroy(blackHolePrefab.gameObject);
            }

            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                assetBundle = null;
            }

            ModLogger.Log("Mod卸载完成");
        }

        #endregion
    }
}