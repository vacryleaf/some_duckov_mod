using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Duckov.Utilities;
using ItemStatsSystem;

namespace WormholeTechMod
{
    /// <summary>
    /// 背包管理辅助类
    /// 负责虫洞科技物品的注入（通过箱子打开事件）
    /// 以及修复物品的 AgentUtilities master 字段
    /// </summary>
    public class WormholeInventoryHelper : MonoBehaviour
    {
        // 单例
        public static WormholeInventoryHelper Instance { get; private set; }

        // 已注入的箱子记录（避免重复注入）
        private HashSet<int> injectedChests = new HashSet<int>();

        // 已修复的物品记录（避免重复修复）
        private HashSet<int> fixedItems = new HashSet<int>();

        // 物品 prefab 引用
        private Item wormholePrefab;
        private Item recallPrefab;
        private Item grenadePrefab;
        private Item badgePrefab;
        private Item blackHolePrefab;

        // ========== 反射缓存 ==========
        private static readonly Type inventoryType = typeof(Inventory);
        private static readonly FieldInfo contentField;

        static WormholeInventoryHelper()
        {
            contentField = inventoryType.GetField("content",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// 设置物品 prefab 引用
        /// </summary>
        public void SetPrefabs(Item wormhole, Item recall, Item grenade, Item badge, Item blackHole)
        {
            wormholePrefab = wormhole;
            recallPrefab = recall;
            grenadePrefab = grenade;
            badgePrefab = badge;
            blackHolePrefab = blackHole;
        }

        void Start()
        {
            // 设置单例
            Instance = this;

            // 订阅玩家加载完成事件
            PlayerStorage.OnLoadingFinished += ScanAndFixInventoryItems;
            ModLogger.Log("[虫洞科技] 已订阅 PlayerStorage.OnLoadingFinished");

            // 订阅箱子打开事件
            InteractableLootbox.OnStartLoot += OnStartLootChest;

            // 监听玩家背包变化（用于修复物品的 master）
            InvokeRepeating(nameof(ScanAndFixInventoryItems), 1f, 2f);
            ModLogger.Log("[虫洞科技] InvokeRepeating 已启动");
        }

        void OnDestroy()
        {
            // 清理单例
            if (Instance == this) Instance = null;

            // 取消订阅
            PlayerStorage.OnLoadingFinished -= ScanAndFixInventoryItems;
            InteractableLootbox.OnStartLoot -= OnStartLootChest;
        }

        /// <summary>
        /// 扫描并修复背包中的虫洞物品
        /// </summary>
        public void ScanAndFixInventoryItems()
        {
            try
            {
                var character = CharacterMainControl.Main;
                if (character == null || character.CharacterItem == null) return;

                var characterItem = character.CharacterItem;
                if (characterItem.Inventory == null) return;

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
                    if (IsWormholeItem(item.TypeID))
                    {
                        // 修复 UsageUtilities（对于可使用的物品）
                        bool usageFixed = false;
                        if (item.TypeID == WormholeItemFactory.WORMHOLE_TYPE_ID ||
                            item.TypeID == WormholeItemFactory.RECALL_TYPE_ID)
                        {
                            usageFixed = FixItemUsageUtilities(item);
                        }

                        // 修复 AgentUtilities
                        bool agentFixed = FixItemAgentUtilities(item);

                        // 任一修复成功就添加到 fixedItems
                        if (usageFixed || agentFixed)
                        {
                            fixedItems.Add(instanceId);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 静默失败，避免日志刷屏
            }
        }

        /// <summary>
        /// 检查是否是虫洞物品
        /// </summary>
        private static bool IsWormholeItem(int typeId)
        {
            return typeId >= 990001 && typeId <= 990005;
        }

        /// <summary>
        /// 玩家打开箱子时注入虫洞科技物品
        /// </summary>
        private void OnStartLootChest(InteractableLootbox lootbox)
        {
            if (lootbox == null) return;

            int chestId = lootbox.GetInstanceID();
            if (injectedChests.Contains(chestId)) return; // 已经注入过

            ModLogger.Log($"玩家打开箱子: {lootbox.name}");

            // 注入虫洞科技物品（30%概率）
            if (UnityEngine.Random.value < 0.3f)
            {
                InjectItemsToChest(lootbox);
                injectedChests.Add(chestId);
            }
        }

        /// <summary>
        /// 向箱子注入虫洞科技物品
        /// </summary>
        private void InjectItemsToChest(InteractableLootbox lootbox)
        {
            try
            {
                // 获取箱子的 Inventory
                var inventory = lootbox.Inventory;
                if (inventory == null)
                {
                    ModLogger.LogWarning($"箱子 {lootbox.name} 没有 Inventory");
                    return;
                }

                // 获取背包内容
                var content = contentField?.GetValue(inventory) as System.Collections.IList;
                if (content == null) return;

                // 随机选择要注入的物品
                var prefabs = new[] { wormholePrefab, recallPrefab, grenadePrefab, badgePrefab, blackHolePrefab }
                    .Where(p => p != null)
                    .ToList();

                if (prefabs.Count == 0)
                {
                    ModLogger.LogWarning("没有可用的物品 prefab");
                    return;
                }

                // 随机注入 1-2 个物品
                int itemCount = UnityEngine.Random.Range(1, 3);
                int injected = 0;

                for (int i = 0; i < itemCount && injected < 2; i++)
                {
                    var prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];

                    // 检查是否有空位
                    int emptySlot = inventory.GetFirstEmptyPosition(0);
                    if (emptySlot < 0)
                    {
                        ModLogger.Log($"箱子 {lootbox.name} 已满");
                        break;
                    }

                    // 创建物品实例
                    var newItem = prefab.CreateInstance();
                    if (newItem != null)
                    {
                        // 修复物品的 AgentUtilities
                        FixItemAgentUtilities(newItem);

                        // 添加到箱子
                        bool success = inventory.AddItem(newItem);
                        if (success)
                        {
                            ModLogger.Log($"已注入 {newItem.DisplayName} 到箱子");
                            injected++;
                        }
                        else
                        {
                            UnityEngine.Object.Destroy(newItem.gameObject);
                            ModLogger.LogWarning($"注入 {newItem.DisplayName} 失败");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"注入物品失败: {e.Message}");
            }
        }

        /// <summary>
        /// 修复物品的 AgentUtilities
        /// </summary>
        public bool FixItemAgentUtilities(Item item)
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
                                    ModLogger.Log($"[虫洞科技] 已补全 {item.DisplayName} 的 Handheld prefab");
                                }
                            }
                            break;
                        }
                    }
                }

                // 如果没有 Handheld，尝试设置
                if (!hasHandheld)
                {
                    var existingPrefab = agentUtils.GetPrefab("Handheld");
                    if (existingPrefab == null)
                    {
                        var setPrefabMethod = typeof(ItemAgentUtilities).GetMethod("SetPrefab",
                            BindingFlags.NonPublic | BindingFlags.Instance,
                            null,
                            new Type[] { typeof(string), typeof(ItemAgent) },
                            null);

                        if (setPrefabMethod != null)
                        {
                            setPrefabMethod.Invoke(agentUtils, new object[] { "Handheld", handheldPrefab });
                            ModLogger.Log($"[虫洞科技] 已通过 SetPrefab 为 {item.DisplayName} 设置 Handheld");
                        }
                        else
                        {
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
                ModLogger.LogWarning($"[虫洞科技] 修复 {item.DisplayName} 失败: {e.Message}");
                return false;
            }
        }

        #region 兼容旧方法（供其他脚本调用）

        /// <summary>
        /// 开始监听背包变化（不再需要，保留兼容）
        /// </summary>
        public void StartWatchInventoryChanges() { }

        /// <summary>
        /// 停止监听背包变化（不再需要，保留兼容）
        /// </summary>
        public void StopWatchInventoryChanges() { }

        /// <summary>
        /// 尝试将物品添加到背包（兼容旧方法）
        /// </summary>
        public bool TryAddItemToInventory(Inventory inventory, int typeId)
        {
            try
            {
                Item prefab = GetPrefabByTypeId(typeId);
                string itemName = GetItemNameByTypeId(typeId);

                if (prefab == null)
                {
                    ModLogger.LogWarning($"{itemName} prefab 为空");
                    return false;
                }

                int emptySlot = inventory.GetFirstEmptyPosition(0);
                if (emptySlot < 0)
                {
                    ModLogger.LogWarning($"背包已满");
                    return false;
                }

                ModLogger.Log($"开始创建 {itemName} 实例...");
                var newItem = prefab.CreateInstance();
                if (newItem == null)
                {
                    ModLogger.LogWarning($"CreateInstance 返回 null");
                    return false;
                }
                ModLogger.Log($"{itemName} 实例创建完成, InstanceID={newItem.GetInstanceID()}");

                // 检查创建后的 master 状态
                var agentUtils = newItem.AgentUtilities;
                if (agentUtils != null)
                {
                    var masterField = typeof(ItemAgentUtilities).GetField("master",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var currentMaster = masterField?.GetValue(agentUtils);
                    ModLogger.Log($"创建后 master = {(currentMaster == null ? "null" : "已设置")}");
                }

                // 修复物品的 AgentUtilities
                ModLogger.Log($"调用 FixItemAgentUtilities...");
                FixItemAgentUtilities(newItem);
                ModLogger.Log($"FixItemAgentUtilities 完成");

                bool success = inventory.AddItem(newItem);
                ModLogger.Log($"AddItem 结果: {success}");

                if (success)
                {
                    ModLogger.Log($"成功添加 {itemName} 到背包");
                }
                else
                {
                    UnityEngine.Object.Destroy(newItem.gameObject);
                    ModLogger.LogWarning($"添加 {itemName} 失败");
                }

                return success;
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"添加物品失败: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        private Item GetPrefabByTypeId(int typeId)
        {
            switch (typeId)
            {
                case WormholeItemFactory.WORMHOLE_TYPE_ID: return wormholePrefab;
                case WormholeItemFactory.RECALL_TYPE_ID: return recallPrefab;
                case WormholeItemFactory.GRENADE_TYPE_ID: return grenadePrefab;
                case WormholeItemFactory.BADGE_TYPE_ID: return badgePrefab;
                case WormholeItemFactory.BLACKHOLE_TYPE_ID: return blackHolePrefab;
                default: return null;
            }
        }

        private string GetItemNameByTypeId(int typeId)
        {
            switch (typeId)
            {
                case WormholeItemFactory.WORMHOLE_TYPE_ID: return "微型虫洞";
                case WormholeItemFactory.RECALL_TYPE_ID: return "回溯虫洞";
                case WormholeItemFactory.GRENADE_TYPE_ID: return "虫洞手雷";
                case WormholeItemFactory.BADGE_TYPE_ID: return "虫洞徽章";
                case WormholeItemFactory.BLACKHOLE_TYPE_ID: return "黑洞手雷";
                default: return "未知物品";
            }
        }

        /// <summary>
        /// 修复物品的 UsageUtilities
        /// </summary>
        public bool FixItemUsageUtilities(Item item)
        {
            if (item == null) return false;

            try
            {
                var usageUtils = item.UsageUtilities;
                var itemName = item.DisplayName;
                var typeId = item.TypeID;

                ModLogger.Log($"[虫洞科技] [调试] FixItemUsageUtilities: {itemName} (TypeID:{typeId}) usageUtils={(usageUtils != null ? "非空" : "null")}");

                if (usageUtils != null)
                {
                    var bf1 = typeof(UsageUtilities).GetField("behaviors",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    // 先检查字段是否存在
                    if (bf1 != null)
                    {
                        var existingBehaviors = bf1.GetValue(usageUtils) as System.Collections.IList;
                        ModLogger.Log($"[虫洞科技] [调试] existingBehaviors={(existingBehaviors != null ? $"Count={existingBehaviors.Count}" : "null")}");

                        if (existingBehaviors != null && existingBehaviors.Count > 0)
                        {
                            ModLogger.Log($"[虫洞科技] [调试] {itemName} behaviors 已存在，跳过");
                            return true; // 已有有效的 UsageUtilities
                        }
                    }
                }

                ModLogger.Log($"[虫洞科技] [调试] 正在为 {itemName} 重新创建 UsageUtilities...");

                var newUsageUtils = item.gameObject.AddComponent<UsageUtilities>();
                WormholeItemFactory.SetFieldValue(newUsageUtils, "useTime", 1.5f);
                WormholeItemFactory.SetFieldValue(newUsageUtils, "useDurability", false);

                var behaviorsList = newUsageUtils.behaviors;
                ModLogger.Log($"[虫洞科技] [调试] behaviorsList={(behaviorsList != null ? $"Count={behaviorsList.Count}" : "null")}");

                if (behaviorsList == null)
                {
                    ModLogger.LogWarning($"[虫洞科技] 无法获取 {itemName} 的 behaviors 列表，主动创建");
                    behaviorsList = new System.Collections.Generic.List<UsageBehavior>();
                    var bf = typeof(UsageUtilities).GetField("behaviors",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    bf?.SetValue(newUsageUtils, behaviorsList);
                }

                if (typeId == WormholeItemFactory.WORMHOLE_TYPE_ID)
                {
                    var behavior = item.gameObject.GetComponent<MicroWormholeUse>();
                    ModLogger.Log($"[虫洞科技] [调试] MicroWormholeUse={(behavior != null ? "非空" : "null")}");
                    if (behavior != null)
                    {
                        behaviorsList.Add(behavior);
                        ModLogger.Log($"[虫洞科技] 已添加 MicroWormholeUse 到 behaviors");
                    }
                }
                else if (typeId == WormholeItemFactory.RECALL_TYPE_ID)
                {
                    var behavior = item.gameObject.GetComponent<WormholeRecallUse>();
                    ModLogger.Log($"[虫洞科技] [调试] WormholeRecallUse={(behavior != null ? "非空" : "null")}");
                    if (behavior != null)
                    {
                        behaviorsList.Add(behavior);
                        ModLogger.Log($"[虫洞科技] 已添加 WormholeRecallUse 到 behaviors");
                    }
                }

                WormholeItemFactory.SetFieldValue(item, "usageUtilities", newUsageUtils);

                ModLogger.Log($"[虫洞科技] [调试] 已为 {itemName} 修复 UsageUtilities, behaviorsCount={behaviorsList.Count}");
                return true;
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"[虫洞科技] 修复 UsageUtilities 失败: {e.Message}");
                return false;
            }
        }

        #endregion
    }
}
