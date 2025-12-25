using UnityEngine;
using UnityEngine.SceneManagement;
using Duckov.Modding;
using Duckov.Utilities;
using Duckov.Scenes;
using ItemStatsSystem;
using SodaCraft.Localizations;
using System;
using System.IO;
using System.Reflection;

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
        private Item badgePrefab;         // 虫洞徽章

        // 虫洞手雷技能Prefab
        private WormholeGrenadeSkill grenadeSkill;

        // 物品TypeID（使用较大的数值避免与游戏本体和其他Mod冲突）
        private const int WORMHOLE_TYPE_ID = 990001;  // 微型虫洞
        private const int RECALL_TYPE_ID = 990002;    // 虫洞回溯
        private const int GRENADE_TYPE_ID = 990003;   // 虫洞手雷
        private const int BADGE_TYPE_ID = 990004;     // 虫洞徽章

        // AssetBundle
        private AssetBundle assetBundle;

        // 物品图标
        private Sprite wormholeIcon;
        private Sprite recallIcon;
        private Sprite grenadeIcon;
        private Sprite badgeIcon;

        // 虫洞记录数据（静态，跨场景保持）
        private static WormholeData savedWormholeData = new WormholeData();

        // 是否正在等待场景加载完成后传送
        private static bool pendingTeleport = false;

        // 待传送的场景名称
        private static string pendingTeleportScene = null;

        // 待传送的位置
        private static Vector3 pendingTeleportPosition = Vector3.zero;

        // 待传送的旋转
        private static Quaternion pendingTeleportRotation = Quaternion.identity;

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
                CreateBadgeItem();

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

                // 监听背包内容变化，自动修复 AgentUtilities
                StartCoroutine(WatchInventoryChanges());

                Debug.Log("[微型虫洞] Mod加载完成!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[微型虫洞] Mod加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        // 修复计时器
        private float inventoryCheckTimer = 0f;
        private float inventoryCheckInterval = 0.5f; // 每0.5秒检查一次
        private System.Collections.Generic.HashSet<int> fixedItems = new System.Collections.Generic.HashSet<int>();

        /// <summary>
        /// 每帧更新 - 监听测试快捷键和修复物品
        /// </summary>
        void Update()
        {
            // 按 F9 添加四个虫洞物品用于测试
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F9))
            {
                AddTestItems();
            }

            // 每隔一段时间扫描背包，修复虫洞物品的 AgentUtilities
            inventoryCheckTimer += Time.deltaTime;
            if (inventoryCheckTimer >= inventoryCheckInterval)
            {
                inventoryCheckTimer = 0f;
                ScanAndFixInventoryItems();
            }
        }

        /// <summary>
        /// 添加测试物品（F9快捷键）
        /// </summary>
        private void AddTestItems()
        {
            try
            {
                var character = CharacterMainControl.Main;
                if (character == null || character.CharacterItem == null)
                {
                    ShowMessage("无法找到玩家");
                    return;
                }

                var inventory = character.CharacterItem.Inventory;
                if (inventory == null)
                {
                    ShowMessage("无法找到背包");
                    return;
                }

                int addedCount = 0;

                // 添加微型虫洞
                if (TryAddItemToInventory(inventory, WORMHOLE_TYPE_ID))
                {
                    addedCount++;
                }

                // 添加回溯虫洞
                if (TryAddItemToInventory(inventory, RECALL_TYPE_ID))
                {
                    addedCount++;
                }

                // 添加虫洞手雷
                if (TryAddItemToInventory(inventory, GRENADE_TYPE_ID))
                {
                    addedCount++;
                }

                // 添加虫洞徽章
                if (TryAddItemToInventory(inventory, BADGE_TYPE_ID))
                {
                    addedCount++;
                }

                ShowMessage($"已添加 {addedCount} 个虫洞物品到背包");
                Debug.Log($"[微型虫洞] 测试：添加了 {addedCount} 个虫洞物品");
            }
            catch (Exception e)
            {
                Debug.LogError($"[微型虫洞] 添加测试物品失败: {e.Message}");
            }
        }

        /// <summary>
        /// 扫描并修复背包中的虫洞物品
        /// </summary>
        private void ScanAndFixInventoryItems()
        {
            try
            {
                // 获取玩家角色
                var character = CharacterMainControl.Main;
                if (character == null || character.CharacterItem == null)
                {
                    return;
                }

                // 获取玩家背包
                var characterItem = character.CharacterItem;
                if (characterItem.Inventory == null)
                {
                    return;
                }

                var inventory = characterItem.Inventory;

                // 获取背包内容
                var inventoryType = typeof(Inventory);
                var contentField = inventoryType.GetField("content",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var content = contentField?.GetValue(inventory) as System.Collections.IList;

                if (content == null) return;

                // 扫描所有物品
                foreach (var obj in content)
                {
                    var item = obj as Item;
                    if (item == null) continue;

                    int instanceId = item.GetInstanceID();
                    if (fixedItems.Contains(instanceId)) continue;

                    // 检查是否是虫洞物品
                    if (item.TypeID == WORMHOLE_TYPE_ID || item.TypeID == RECALL_TYPE_ID ||
                        item.TypeID == GRENADE_TYPE_ID || item.TypeID == BADGE_TYPE_ID)
                    {
                        // 修复 UsageUtilities（对于可使用的物品）
                        if (item.TypeID == WORMHOLE_TYPE_ID || item.TypeID == RECALL_TYPE_ID)
                        {
                            FixItemUsageUtilities(item);
                        }

                        // 修复 AgentUtilities
                        if (FixItemAgentUtilities(item))
                        {
                            fixedItems.Add(instanceId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // 静默失败，避免日志刷屏
            }
        }

        /// <summary>
        /// 修复物品的 AgentUtilities（返回是否成功）
        /// </summary>
        private bool FixItemAgentUtilities(Item item)
        {
            if (item == null) return false;

            try
            {
                var agentUtils = item.AgentUtilities;
                if (agentUtils == null) return false;

                // 设置 Master
                var masterField = typeof(ItemAgentUtilities).GetField("master",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (masterField != null)
                {
                    masterField.SetValue(agentUtils, item);
                }

                // 获取手持代理预制体
                var handheldPrefab = GameplayDataSettings.Prefabs.HandheldAgentPrefab;
                if (handheldPrefab == null) return false;

                // 获取 agents 列表
                var agentsField = typeof(ItemAgentUtilities).GetField("agents",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var agents = agentsField?.GetValue(agentUtils) as System.Collections.IList;

                // 检查是否已有 Handheld
                bool hasHandheld = false;
                if (agents != null)
                {
                    foreach (var agent in agents)
                    {
                        if (agent == null) continue;

                        // 检查 key 字段
                        var keyField = agent.GetType().GetField("key",
                            BindingFlags.Public | BindingFlags.Instance);
                        var keyValue = keyField?.GetValue(agent) as string;
                        if (keyValue == "Handheld")
                        {
                            hasHandheld = true;

                            // 确保 prefab 字段已设置
                            var prefabField = agent.GetType().GetField("agentPrefab",
                                BindingFlags.Public | BindingFlags.Instance);
                            if (prefabField != null)
                            {
                                var currentPrefab = prefabField.GetValue(agent);
                                if (currentPrefab == null)
                                {
                                    prefabField.SetValue(agent, handheldPrefab);
                                    Debug.Log($"[微型虫洞] 已补全 {item.DisplayName} 的 Handheld prefab");
                                }
                            }
                            break;
                        }
                    }
                }

                // 如果没有 Handheld，尝试使用 Unity 序列化直接添加
                if (!hasHandheld)
                {
                    Debug.Log($"[微型虫洞] {item.DisplayName} 的 agents 列表: {(agents?.Count.ToString() ?? "null")}");

                    // 方案：直接调用游戏内部方法或使用 Unity 序列化
                    // 由于反射无法创建嵌套类型，我们尝试使用 Unity API

                    // 方案1: 使用 ScriptableObject 绕过类型问题
                    // 方案2: 使用 UnityEngine.Random 判断尝试次数

                    // 方案3: 尝试通过 GetPrefab 设置
                    var existingPrefab = agentUtils.GetPrefab("Handheld");
                    if (existingPrefab == null)
                    {
                        Debug.Log($"[微型虫洞] {item.DisplayName} 没有 Handheld prefab，尝试通过 SetPrefab...");

                        // 使用 Unity 序列化直接修改
                        // 注意：这需要访问私有方法 SetPrefab
                        var setPrefabMethod = typeof(ItemAgentUtilities).GetMethod("SetPrefab",
                            BindingFlags.NonPublic | BindingFlags.Instance,
                            null,
                            new Type[] { typeof(string), typeof(ItemAgent) },
                            null);

                        if (setPrefabMethod != null)
                        {
                            setPrefabMethod.Invoke(agentUtils, new object[] { "Handheld", handheldPrefab });
                            Debug.Log($"[微型虫洞] 已通过 SetPrefab 为 {item.DisplayName} 设置 Handheld");
                        }
                        else
                        {
                            // 尝试直接操作字段
                            var prefabField = typeof(ItemAgentUtilities).GetNestedType("AgentKeyPair", BindingFlags.NonPublic | BindingFlags.Instance)
                                ?.GetField("agentPrefab", BindingFlags.Public | BindingFlags.Instance);

                            // 由于无法创建 AgentKeyPair，我们尝试其他方法
                            Debug.Log($"[微型虫洞] 无法设置 Handheld，尝试修改 HashedAgentsCache...");

                            // 清除缓存强制重新计算
                            var cacheField = typeof(ItemAgentUtilities).GetField("hashedAgentsCache",
                                BindingFlags.NonPublic | BindingFlags.Instance);
                            cacheField?.SetValue(agentUtils, null);
                        }
                    }
                }

                return item.HasHandHeldAgent;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[微型虫洞] 修复 {item.DisplayName} 失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 修复物品的 UsageUtilities（对于可使用物品）
        /// </summary>
        private void FixItemUsageUtilities(Item item)
        {
            if (item == null) return;

            try
            {
                // 获取 UsageUtilities
                var usageUtils = item.UsageUtilities;

                // 如果已有 UsageUtilities 且 behaviors 不为空，直接返回
                if (usageUtils != null)
                {
                    var bf1 = typeof(UsageUtilities).GetField("behaviors",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var existingBehaviors = bf1?.GetValue(usageUtils) as System.Collections.IList;

                    if (existingBehaviors != null && existingBehaviors.Count > 0)
                    {
                        return; // 已有有效的 UsageUtilities
                    }
                }

                // 需要重新创建 UsageUtilities
                Debug.Log($"[微型虫洞] 正在为 {item.DisplayName} 重新创建 UsageUtilities...");

                // 创建新的 UsageUtilities
                var newUsageUtils = item.gameObject.AddComponent<UsageUtilities>();
                SetFieldValue(newUsageUtils, "useTime", 1.5f);
                SetFieldValue(newUsageUtils, "useDurability", false);

                // 获取 behaviors 列表
                var bf2 = typeof(UsageUtilities).GetField("behaviors",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var behaviorsList = bf2?.GetValue(newUsageUtils) as System.Collections.IList;

                if (behaviorsList == null)
                {
                    Debug.LogWarning($"[微型虫洞] 无法获取 {item.DisplayName} 的 behaviors 列表");
                    return;
                }

                // 添加对应的 UsageBehavior
                if (item.TypeID == WORMHOLE_TYPE_ID)
                {
                    var behavior = item.gameObject.GetComponent<MicroWormholeUse>();
                    if (behavior != null)
                    {
                        behaviorsList.Add(behavior);
                        Debug.Log($"[微型虫洞] 已添加 MicroWormholeUse 到 behaviors");
                    }
                }
                else if (item.TypeID == RECALL_TYPE_ID)
                {
                    var behavior = item.gameObject.GetComponent<WormholeRecallUse>();
                    if (behavior != null)
                    {
                        behaviorsList.Add(behavior);
                        Debug.Log($"[微型虫洞] 已添加 WormholeRecallUse 到 behaviors");
                    }
                }

                // 设置 usageUtilities 字段到 Item
                SetFieldValue(item, "usageUtilities", newUsageUtils);

                Debug.Log($"[微型虫洞] 已为 {item.DisplayName} 修复 UsageUtilities");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[微型虫洞] 修复 UsageUtilities 失败: {e.Message}");
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
            // 确定使用哪个位置数据
            Vector3 targetPosition;
            Quaternion targetRotation;

            if (pendingTeleport && !string.IsNullOrEmpty(pendingTeleportScene))
            {
                // 使用待传送数据
                targetPosition = pendingTeleportPosition;
                targetRotation = pendingTeleportRotation;
            }
            else if (savedWormholeData.IsValid)
            {
                // 使用保存的虫洞数据
                targetPosition = savedWormholeData.Position;
                targetRotation = savedWormholeData.Rotation;
            }
            else
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

            Debug.Log($"[微型虫洞] 正在传送到: {targetPosition}");

            // 传送角色
            mainCharacter.transform.position = targetPosition;
            mainCharacter.transform.rotation = targetRotation;

            // 播放特效
            PlayWormholeEffect();

            // 显示提示
            ShowMessage("虫洞回溯成功！");

            // 清除待传送标记
            pendingTeleport = false;
            pendingTeleportScene = null;

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
            badgeIcon = LoadIconFromBundle("WormholeBadgeIcon");

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

            // 虫洞徽章
            LocalizationManager.SetOverrideText("WormholeBadge_Name", "虫洞徽章");
            LocalizationManager.SetOverrideText("WormholeBadge_Desc", "蕴含虫洞能量的神秘徽章。放在物品栏中即可生效。\n\n<color=#87CEEB>被动效果：</color>\n• 被击中时有10%概率使伤害无效化\n• 多个徽章乘法叠加\n\n<color=#AAAAAA>叠加示例：</color>\n• 1个：10%闪避\n• 2个：19%闪避\n• 3个：27.1%闪避\n\n<color=#FFD700>「空间的裂缝，是最好的护盾」</color>");

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
                    LocalizationManager.SetOverrideText("WormholeBadge_Name", "Wormhole Badge");
                    LocalizationManager.SetOverrideText("WormholeBadge_Desc", "A mysterious badge infused with wormhole energy. Works passively in your inventory.\n\n<color=#87CEEB>Passive Effect:</color>\n• 10% chance to negate damage when hit\n• Multiple badges stack multiplicatively\n\n<color=#AAAAAA>Stacking Example:</color>\n• 1 badge: 10% dodge\n• 2 badges: 19% dodge\n• 3 badges: 27.1% dodge\n\n<color=#FFD700>\"Cracks in space make the best shields\"</color>");
                    break;
                default:
                    LocalizationManager.SetOverrideText("MicroWormhole_Name", "微型虫洞");
                    LocalizationManager.SetOverrideText("MicroWormhole_Desc", "高科技传送装置。使用后会记录当前位置并撤离回家。\n\n<color=#FFD700>配合「回溯虫洞」使用，可返回记录的位置</color>");
                    LocalizationManager.SetOverrideText("WormholeRecall_Name", "回溯虫洞");
                    LocalizationManager.SetOverrideText("WormholeRecall_Desc", "虫洞传送的配套装置。在家中使用，可以传送回「微型虫洞」记录的位置。\n\n<color=#FFD700>只能在家中使用</color>");
                    LocalizationManager.SetOverrideText("WormholeGrenade_Name", "虫洞手雷");
                    LocalizationManager.SetOverrideText("WormholeGrenade_Desc", "高科技空间扰乱装置。投掷后引爆，将范围内的所有生物随机传送到地图某处。\n\n<color=#87CEEB>特殊效果：</color>\n• 引信延迟：3秒\n• 传送范围：8米\n• 影响所有角色（包括自己）\n\n<color=#FFD700>「混乱是战场上最好的掩护」</color>");
                    LocalizationManager.SetOverrideText("WormholeBadge_Name", "虫洞徽章");
                    LocalizationManager.SetOverrideText("WormholeBadge_Desc", "蕴含虫洞能量的神秘徽章。放在物品栏中即可生效。\n\n<color=#87CEEB>被动效果：</color>\n• 被击中时有10%概率使伤害无效化\n• 多个徽章乘法叠加\n\n<color=#AAAAAA>叠加示例：</color>\n• 1个：10%闪避\n• 2个：19%闪避\n• 3个：27.1%闪避\n\n<color=#FFD700>「空间的裂缝，是最好的护盾」</color>");
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

            // 先添加 UsageBehavior 组件
            itemObj.AddComponent<MicroWormholeUse>();

            wormholePrefab = itemObj.AddComponent<Item>();
            ConfigureItemProperties(wormholePrefab, WORMHOLE_TYPE_ID, "MicroWormhole_Name", "MicroWormhole_Desc", wormholeIcon);

            // 添加 AgentUtilities 自动修复组件
            itemObj.AddComponent<AgentUtilitiesFixer>();

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

            // 先添加 UsageBehavior 组件
            itemObj.AddComponent<WormholeRecallUse>();

            recallPrefab = itemObj.AddComponent<Item>();
            ConfigureRecallItemProperties(recallPrefab, RECALL_TYPE_ID, "WormholeRecall_Name", "WormholeRecall_Desc", recallIcon);

            // 添加 AgentUtilities 自动修复组件
            itemObj.AddComponent<AgentUtilitiesFixer>();

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

            // 添加 AgentUtilities 自动修复组件
            itemObj.AddComponent<AgentUtilitiesFixer>();

            Debug.Log("[微型虫洞] 虫洞手雷Prefab创建完成");
        }

        /// <summary>
        /// 创建虫洞徽章物品
        /// </summary>
        private void CreateBadgeItem()
        {
            Debug.Log("[微型虫洞] 开始创建虫洞徽章Prefab...");

            GameObject itemObj = CreateBadgeGameObject("WormholeBadge", new Color(0.2f, 0.8f, 1f));

            DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            badgePrefab = itemObj.AddComponent<Item>();

            CreateBadgeEffect(itemObj, badgePrefab);

            ConfigureBadgeProperties(badgePrefab, BADGE_TYPE_ID, "WormholeBadge_Name", "WormholeBadge_Desc", badgeIcon);

            itemObj.AddComponent<AgentUtilitiesFixer>();

            Debug.Log("[微型虫洞] 虫洞徽章Prefab创建完成");
        }

        /// <summary>
        /// 为徽章物品添加 Effect 系统
        /// </summary>
        private void CreateBadgeEffect(GameObject itemObj, Item badgeItem)
        {
            // 创建 Effect 容器
            GameObject effectObj = new GameObject("BadgeDodgeEffect");
            effectObj.transform.SetParent(itemObj.transform);
            effectObj.transform.localPosition = Vector3.zero;

            // 添加 Effect 组件
            Effect effect = effectObj.AddComponent<Effect>();

            // 设置 Effect 属性
            SetFieldValue(effect, "display", true);
            SetFieldValue(effect, "description", "虫洞徽章被动：受到伤害时有概率闪避并恢复生命");

            // 添加闪避触发器
            WormholeDodgeTrigger trigger = effectObj.AddComponent<WormholeDodgeTrigger>();

            // 添加闪避动作
            WormholeDodgeAction action = effectObj.AddComponent<WormholeDodgeAction>();

            // 将 Effect 添加到物品的 Effects 列表
            badgeItem.Effects.Add(effect);

            Debug.Log("[微型虫洞] 已为徽章添加 Effect 系统（触发器+动作）");
        }

        /// <summary>
        /// 创建虫洞徽章GameObject
        /// </summary>
        private GameObject CreateBadgeGameObject(string name, Color color)
        {
            GameObject itemObj = new GameObject(name);

            // 创建徽章的视觉效果
            CreateBadgeVisual(itemObj, color);

            return itemObj;
        }

        /// <summary>
        /// 创建徽章的视觉效果
        /// </summary>
        private void CreateBadgeVisual(GameObject parent, Color color)
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
        /// 配置虫洞徽章物品属性
        /// </summary>
        private void ConfigureBadgeProperties(Item item, int typeId, string nameKey, string descKey, Sprite icon)
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
            SetFieldValue(item, "value", 15000);
            SetFieldValue(item, "weight", 0.05f);

            item.Initialize();

            ConfigureBadgeAvailability(item);

            Debug.Log($"[微型虫洞] 已配置虫洞徽章 {typeId}，Initialize 完成，Availability 已配置");
        }

        /// <summary>
        /// 配置虫洞徽章的 Availability
        /// </summary>
        private void ConfigureBadgeAvailability(Item item)
        {
            Availability availability = new Availability();
            availability.canSpawnInLoot = true;
            availability.canSpawnInShop = true;
            availability.canSpawnInCraft = false;
            availability.canDropFromEnemy = true;
            availability.canBeGivenAsQuestReward = true;
            availability.minPlayerLevel = 15;
            availability.randomDropWeight = 5f;

            SetFieldValue(item, "availability", availability);
            Debug.Log($"[微型虫洞] 虫洞徽章 Availability 配置完成");
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
        /// 配置虫洞手雷物品属性（使用技能系统）
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
            SetFieldValue(item, "quality", 5);          // 传说级
            SetFieldValue(item, "value", 25000);        // 更贵
            SetFieldValue(item, "weight", 0.3f);        // 比虫洞重
            SetFieldValue(item, "soundKey", "Grenade"); // 设置音效键

            // 在物品对象下创建技能子对象
            GameObject skillObj = new GameObject("WormholeGrenadeSkill");
            skillObj.transform.SetParent(item.gameObject.transform);
            skillObj.transform.localPosition = Vector3.zero;

            // 添加技能组件
            grenadeSkill = skillObj.AddComponent<WormholeGrenadeSkill>();
            grenadeSkill.staminaCost = 0f;              // 不消耗体力
            grenadeSkill.coolDownTime = 0.5f;            // 冷却时间
            grenadeSkill.damageRange = 5f;
            grenadeSkill.delayTime = 1f;                 // 引爆时间1秒
            grenadeSkill.throwForce = 15f;
            grenadeSkill.throwAngle = 30f;
            grenadeSkill.canControlCastDistance = true;
            grenadeSkill.grenadeVerticleSpeed = 10f;
            grenadeSkill.canHurtSelf = true;

            // 设置 SkillContext（关键：设置 castRange 以允许控制投掷距离）
            var skillContextField = typeof(SkillBase).GetField("skillContext",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (skillContextField != null)
            {
                var skillContext = new SkillContext
                {
                    castRange = 15f,              // 最大投掷距离15米
                    effectRange = 5f,             // 爆炸范围5米
                    isGrenade = true,              // 标记为手雷
                    grenageVerticleSpeed = 10f,    // 垂直速度
                    movableWhileAim = true,        // 瞄准时可以移动
                    skillReadyTime = 0f,           // 无准备时间
                    checkObsticle = true,          // 检查障碍物
                    releaseOnStartAim = false      // 不在开始瞄准时释放
                };
                skillContextField.SetValue(grenadeSkill, skillContext);
                Debug.Log($"[微型虫洞] 已设置虫洞手雷 skillContext，castRange: 15f, effectRange: 5f");
            }

            // 添加 ItemSetting_Skill 组件（必须在物品上）
            ItemSetting_Skill skillSetting = item.gameObject.AddComponent<ItemSetting_Skill>();
            SetFieldValue(skillSetting, "Skill", grenadeSkill);
            SetFieldValue(skillSetting, "onRelease", ItemSetting_Skill.OnReleaseAction.reduceCount);

            // 设置物品为技能物品
            item.SetBool("IsSkill", true, true);

            // 初始化物品（必须调用，否则物品无法正确注册）
            item.Initialize();

            Debug.Log($"[微型虫洞] 虫洞手雷 {typeId} 配置完成，使用技能系统，本地化键: {nameKey}");
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
            SetFieldValue(item, "usable", true);           // 可以主动使用
            SetFieldValue(item, "quality", 4);
            SetFieldValue(item, "value", 10000);
            SetFieldValue(item, "weight", 0.1f);

            // 手动添加 UsageUtilities 组件（在 Initialize 之前添加所有组件）
            UsageUtilities usageUtilities = item.gameObject.AddComponent<UsageUtilities>();
            SetFieldValue(usageUtilities, "useTime", 1.5f);  // 使用需要1.5秒
            SetFieldValue(usageUtilities, "useDurability", false);

            // 添加 UsageBehavior
            var wormholeUseBehavior = item.gameObject.GetComponent<MicroWormholeUse>();
            if (wormholeUseBehavior != null && usageUtilities.behaviors != null)
            {
                usageUtilities.behaviors.Add(wormholeUseBehavior);
                Debug.Log($"[微型虫洞] 已添加 MicroWormholeUse 到 behaviors 列表");
            }

            // 设置 usageUtilities 字段到 Item
            SetFieldValue(item, "usageUtilities", usageUtilities);

            // 初始化物品（所有组件添加完成后调用）
            item.Initialize();

            Debug.Log($"[微型虫洞] 物品 {typeId} 配置完成，UsageUtilities 已手动添加");
        }

        /// <summary>
        /// 配置虫洞回溯物品属性（可使用）
        /// </summary>
        private void ConfigureRecallItemProperties(Item item, int typeId, string nameKey, string descKey, Sprite icon)
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
            SetFieldValue(item, "usable", true);           // 可以主动使用
            SetFieldValue(item, "quality", 4);
            SetFieldValue(item, "value", 10000);
            SetFieldValue(item, "weight", 0.1f);

            // 手动添加 UsageUtilities 组件（在 Initialize 之前添加所有组件）
            UsageUtilities usageUtilities = item.gameObject.AddComponent<UsageUtilities>();
            SetFieldValue(usageUtilities, "useTime", 1.5f);  // 使用需要1.5秒
            SetFieldValue(usageUtilities, "useDurability", false);

            // 添加 UsageBehavior
            var recallUseBehavior = item.gameObject.GetComponent<WormholeRecallUse>();
            if (recallUseBehavior != null && usageUtilities.behaviors != null)
            {
                usageUtilities.behaviors.Add(recallUseBehavior);
                Debug.Log($"[微型虫洞] 已添加 WormholeRecallUse 到 behaviors 列表");
            }

            // 设置 usageUtilities 字段到 Item
            SetFieldValue(item, "usageUtilities", usageUtilities);

            // 初始化物品（所有组件添加完成后调用）
            item.Initialize();

            Debug.Log($"[微型虫洞] 虫洞回溯 {typeId} 配置完成，usable=true，本地化键: {nameKey}");
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
            UnityEngine.Object.Destroy(visual.GetComponent<Collider>());

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

            if (badgePrefab != null)
            {
                bool success = ItemAssetsCollection.AddDynamicEntry(badgePrefab);
                Debug.Log($"[微型虫洞] 虫洞徽章注册: {(success ? "成功" : "失败")}");
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

                    // 添加虫洞徽章（价格倍率1.0，商店本身会涨价）
                    AddItemToMerchant(profile, BADGE_TYPE_ID, 2, 1.0f, 1.0f);
                }

                Debug.Log("[微型虫洞] 物品已添加到商店");
            }
            catch (Exception e)
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
            catch (Exception e)
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
                catch (Exception e)
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
                            if (!fixedItems.Contains(WORMHOLE_TYPE_ID) && UnityEngine.Random.value < 0.15f)
                            {
                                fixedItems.Add(WORMHOLE_TYPE_ID);
                                modified = true;
                            }

                            if (!fixedItems.Contains(RECALL_TYPE_ID) && UnityEngine.Random.value < 0.15f)
                            {
                                fixedItems.Add(RECALL_TYPE_ID);
                                modified = true;
                            }

                            if (!fixedItems.Contains(GRENADE_TYPE_ID) && UnityEngine.Random.value < 0.10f)
                            {
                                fixedItems.Add(GRENADE_TYPE_ID);
                                modified = true;
                            }

                            if (!fixedItems.Contains(BADGE_TYPE_ID) && UnityEngine.Random.value < 0.01f)
                            {
                                fixedItems.Add(BADGE_TYPE_ID);
                                modified = true;
                            }

                            if (modified)
                            {
                                Debug.Log($"[微型虫洞] 已修改 LootBoxLoader: {loader.gameObject.name}");
                            }
                        }
                    }
                }
                catch (Exception e)
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
                            if (UnityEngine.Random.value < 0.18f)
                            {
                                if (TryAddItemToInventory(inventory, WORMHOLE_TYPE_ID))
                                {
                                    addedCount++;
                                    Debug.Log($"[微型虫洞] 向箱子添加了微型虫洞");
                                }
                            }

                            // 15%概率添加回溯虫洞
                            if (UnityEngine.Random.value < 0.15f)
                            {
                                if (TryAddItemToInventory(inventory, RECALL_TYPE_ID))
                                {
                                    addedCount++;
                                    Debug.Log($"[微型虫洞] 向箱子添加了回溯虫洞");
                                }
                            }

                            // 10%概率添加虫洞手雷（更稀有）
                            if (UnityEngine.Random.value < 0.10f)
                            {
                                if (TryAddItemToInventory(inventory, GRENADE_TYPE_ID))
                                {
                                    addedCount++;
                                    Debug.Log($"[微型虫洞] 向箱子添加了虫洞手雷");
                                }
                            }

                            // 1%概率添加虫洞徽章（稀有）
                            if (UnityEngine.Random.value < 0.01f)
                            {
                                if (TryAddItemToInventory(inventory, BADGE_TYPE_ID))
                                {
                                    addedCount++;
                                    Debug.Log($"[微型虫洞] 向箱子添加了虫洞徽章");
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
                catch (Exception e)
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
                else if (typeId == BADGE_TYPE_ID)
                {
                    prefab = badgePrefab;
                    itemName = "虫洞徽章";
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

                // 初始化物品（解决手持代理问题）
                InitializeModItem(newItem);

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
            catch (Exception e)
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

            if (badgePrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(badgePrefab);
                Destroy(badgePrefab.gameObject);
            }

            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                assetBundle = null;
            }

            Debug.Log("[微型虫洞] Mod卸载完成");
        }

        /// <summary>
        /// 初始化 Mod 创建的物品，解决手持代理问题
        /// </summary>
        private void InitializeModItem(Item item)
        {
            if (item == null) return;

            Debug.Log($"[微型虫洞] 开始初始化物品: {item.DisplayName}");

            // 初始化 AgentUtilities
            var agentUtils = item.AgentUtilities;
            if (agentUtils != null)
            {
                agentUtils.Initialize(item);
                Debug.Log($"[微型虫洞] AgentUtilities 初始化完成");
            }
            else
            {
                Debug.LogWarning($"[微型虫洞] AgentUtilities 为 null");
            }

            // 检查手持代理
            Debug.Log($"[微型虫洞] HasHandHeldAgent: {item.HasHandHeldAgent}");

            // 获取手持代理预制体
            var handheldPrefab = GameplayDataSettings.Prefabs.HandheldAgentPrefab;
            Debug.Log($"[微型虫洞] HandheldAgentPrefab: {handheldPrefab?.name ?? "null"}");

            Debug.Log($"[微型虫洞] 物品初始化完成: {item.DisplayName}");
        }

        /// <summary>
        /// 监听背包内容变化，自动修复 AgentUtilities
        /// </summary>
        private System.Collections.IEnumerator WatchInventoryChanges()
        {
            var processedItems = new System.Collections.Generic.HashSet<int>();
            var inventoryType = typeof(Inventory);

            while (true)
            {
                try
                {
                    // 查找所有背包
                    var inventories = FindObjectsOfType(inventoryType) as Inventory[];
                    if (inventories != null)
                    {
                        foreach (var inventory in inventories)
                        {
                            if (inventory == null) continue;

                            // 获取背包内容变化事件
                            var onContentChangedField = inventoryType.GetField("onContentChanged",
                                BindingFlags.NonPublic | BindingFlags.Instance);
                            var onContentChanged = onContentChangedField?.GetValue(inventory) as Action<Inventory, int>;

                            if (onContentChanged != null)
                            {
                                // 检查是否已经订阅了这个背包
                                if (!inventoryFieldHandlers.ContainsKey(inventory))
                                {
                                    // 创建并订阅事件处理器
                                    Action<Inventory, int> handler = (inv, pos) => OnInventoryContentChanged(inv, pos, processedItems);
                                    onContentChanged += handler;
                                    inventoryFieldHandlers[inventory] = handler;

                                    // 更新字段值
                                    onContentChangedField.SetValue(inventory, onContentChanged);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[微型虫洞] 监听背包变化时出错: {e.Message}");
                }

                yield return new WaitForSeconds(2f);
            }
        }

        private System.Collections.Generic.Dictionary<Inventory, Action<Inventory, int>> inventoryFieldHandlers =
            new System.Collections.Generic.Dictionary<Inventory, Action<Inventory, int>>();

        /// <summary>
        /// 背包内容变化时的回调
        /// </summary>
        private void OnInventoryContentChanged(Inventory inventory, int position,
            System.Collections.Generic.HashSet<int> processedItems)
        {
            if (inventory == null) return;

            try
            {
                var itemsField = typeof(Inventory).GetField("content",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var content = itemsField?.GetValue(inventory) as System.Collections.IList;

                if (content != null && position >= 0 && position < content.Count)
                {
                    var item = content[position] as Item;
                    if (item != null && !processedItems.Contains(item.GetInstanceID()))
                    {
                        // 检查是否是虫洞物品
                        if (item.TypeID == WORMHOLE_TYPE_ID || item.TypeID == RECALL_TYPE_ID ||
                            item.TypeID == GRENADE_TYPE_ID || item.TypeID == BADGE_TYPE_ID)
                        {
                            Debug.Log($"[微型虫洞] 检测到虫洞物品被添加到背包: {item.DisplayName}，正在修复 AgentUtilities...");

                            // 修复 AgentUtilities
                            FixItemAgentUtilities(item);

                            processedItems.Add(item.GetInstanceID());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[微型虫洞] 修复背包物品 AgentUtilities 时出错: {e.Message}");
            }
        }

        /// <summary>
        /// 保存虫洞数据（供 MicroWormholeUse 调用）
        /// </summary>
        public void SetWormholeData(WormholeData data)
        {
            savedWormholeData = data;
        }

        /// <summary>
        /// 检查是否有有效的虫洞数据（供 WormholeRecallUse 调用）
        /// </summary>
        public bool HasValidWormholeData()
        {
            return savedWormholeData.IsValid;
        }

        /// <summary>
        /// 获取保存的虫洞数据（供 WormholeRecallUse 调用）
        /// </summary>
        public WormholeData GetWormholeData()
        {
            return savedWormholeData;
        }

        /// <summary>
        /// 设置待传送数据（供 WormholeRecallUse 调用）
        /// </summary>
        public void SetPendingTeleport(string sceneName, Vector3 position, Quaternion rotation)
        {
            pendingTeleport = true;
            pendingTeleportScene = sceneName;
            pendingTeleportPosition = position;
            pendingTeleportRotation = rotation;
            Debug.Log($"[微型虫洞] 设置待传送: 场景={sceneName}, 位置={position}");
        }

        /// <summary>
        /// 执行虫洞回溯场景加载（供 WormholeRecallUse 调用）
        /// 使用游戏原生的 SceneLoader 加载场景
        /// </summary>
        private void ExecuteRecallScene(string targetScene, Vector3 targetPosition, Quaternion targetRotation)
        {
            if (string.IsNullOrEmpty(targetScene))
            {
                Debug.LogWarning("[微型虫洞] 目标场景为空");
                return;
            }

            Debug.Log($"[微型虫洞] 开始回溯: 场景={targetScene}, 位置={targetPosition}");

            try
            {
                var sceneLoaderType = Type.GetType("SceneLoader, TeamSoda.Duckov.Core");
                if (sceneLoaderType == null)
                {
                    Debug.LogWarning("[微型虫洞] 找不到 SceneLoader 类型");
                    return;
                }

                var instanceProp = sceneLoaderType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp == null)
                {
                    Debug.LogWarning("[微型虫洞] 找不到 SceneLoader.Instance");
                    return;
                }

                var sceneLoader = instanceProp.GetValue(null);
                if (sceneLoader == null)
                {
                    Debug.LogWarning("[微型虫洞] SceneLoader.Instance 为空");
                    return;
                }

                // 设置待传送
                pendingTeleport = true;
                pendingTeleportScene = targetScene;
                pendingTeleportPosition = targetPosition;
                pendingTeleportRotation = targetRotation;

                // 尝试使用 string 版本的 LoadScene 方法
                var methods = sceneLoaderType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    if (method.Name == "LoadScene" && method.GetParameters().Length >= 2)
                    {
                        var parameters = method.GetParameters();
                        // 检查第一个参数是否为 string
                        if (parameters.Length >= 1 && parameters[0].ParameterType == typeof(string))
                        {
                            // 调用这个方法
                            Debug.Log($"[微型虫洞] 找到 LoadScene 方法，参数数: {parameters.Length}");

                            // 构建参数
                            var args = new object[parameters.Length];
                            args[0] = targetScene; // sceneID

                            // 填充剩余参数
                            for (int i = 1; i < parameters.Length; i++)
                            {
                                var paramType = parameters[i].ParameterType;
                                if (paramType == typeof(bool))
                                {
                                    // 根据参数位置设置默认值
                                    if (i == 2) args[i] = false;  // clickToContinue
                                    else if (i == 3) args[i] = false; // notifyEvacuation
                                    else if (i == 4) args[i] = true;  // doCircleFade
                                    else if (i == 5) args[i] = false; // useLocation
                                    else if (i == 7) args[i] = true;  // saveToFile
                                    else if (i == 8) args[i] = false; // hideTips
                                    else args[i] = false;
                                }
                                else if (paramType == typeof(MultiSceneLocation))
                                {
                                    args[i] = default(MultiSceneLocation);
                                }
                                else
                                {
                                    args[i] = null;
                                }
                            }

                            method.Invoke(sceneLoader, args);
                            Debug.Log($"[微型虫洞] 已调用 SceneLoader.LoadScene: {targetScene}");

                            // 监听场景加载完成，在新场景中恢复玩家位置
                            SceneManager.sceneLoaded += OnRecallSceneLoaded;
                            Debug.Log($"[微型虫洞] 已订阅场景加载事件，等待恢复玩家位置");
                            return;
                        }
                    }
                }

                Debug.LogWarning("[微型虫洞] 找不到合适的 LoadScene 方法");
            }
            catch (Exception e)
            {
                Debug.LogError($"[微型虫洞] 场景加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 场景加载完成回调 - 恢复玩家位置
        /// </summary>
        private void OnRecallSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 移除事件订阅
            SceneManager.sceneLoaded -= OnRecallSceneLoaded;

            if (!pendingTeleport)
            {
                Debug.Log($"[微型虫洞] 场景加载完成，但 pendingTeleport 为 false，跳过传送");
                return;
            }

            Debug.Log($"[微型虫洞] 场景加载完成，正在恢复玩家位置... pendingTeleportPosition={pendingTeleportPosition}");

            // 查找玩家并传送
            CharacterMainControl character = CharacterMainControl.Main;
            if (character != null)
            {
                character.transform.position = pendingTeleportPosition;
                character.transform.rotation = pendingTeleportRotation;
                Debug.Log($"[微型虫洞] 玩家已传送到位置: {pendingTeleportPosition}");
            }
            else
            {
                Debug.LogWarning("[微型虫洞] 找不到玩家角色，无法传送");
            }

            // 清除待传送标记
            pendingTeleport = false;
            pendingTeleportScene = null;
            pendingTeleportPosition = Vector3.zero;
            pendingTeleportRotation = Quaternion.identity;
        }

        /// <summary>
        /// 执行虫洞回溯（供 WormholeRecallUse 调用）
        /// </summary>
        public void ExecuteRecall(CharacterMainControl character)
        {
            if (!savedWormholeData.IsValid)
            {
                Debug.LogWarning("[微型虫洞] 没有有效的虫洞数据");
                return;
            }

            string targetScene = savedWormholeData.SceneName;
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            Debug.Log($"[微型虫洞] 正在回溯: 当前场景={currentScene}, 目标场景={targetScene}, 位置={savedWormholeData.Position}");

            // 检查是否已经在目标场景
            if (currentScene == targetScene)
            {
                Debug.Log("[微型虫洞] 已在目标场景，直接传送...");
                PlayWormholeEffect();
                TeleportToSavedPosition();
                return;
            }

            // 播放特效
            PlayWormholeEffect();

            // 设置待传送标记
            pendingTeleport = true;

            // 加载目标场景
            try
            {
                Debug.Log($"[微型虫洞] 开始加载场景: {targetScene}");

                // 使用异步加载避免卡死
                SceneManager.LoadSceneAsync(targetScene, UnityEngine.SceneManagement.LoadSceneMode.Single);

                Debug.Log($"[微型虫洞] 场景加载请求已发送: {targetScene}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[微型虫洞] 场景加载失败: {e.Message}\n{e.StackTrace}");
                ShowMessage("虫洞通道开启失败！");
                pendingTeleport = false;
            }
        }
    }
}
