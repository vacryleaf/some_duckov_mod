using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Duckov.Utilities;
using ItemStatsSystem;

namespace WormholeTechMod
{
    /// <summary>
    /// 背包管理辅助类
    /// 负责背包物品的修复、添加和监控
    /// </summary>
    public class WormholeInventoryHelper : MonoBehaviour
    {
        // 已修复物品记录
        private HashSet<int> fixedItems = new HashSet<int>();

        // 背包事件处理器记录
        private Dictionary<Inventory, Action<Inventory, int>> inventoryFieldHandlers =
            new Dictionary<Inventory, Action<Inventory, int>>();

        // 物品 prefab 引用
        private Item wormholePrefab;
        private Item recallPrefab;
        private Item grenadePrefab;
        private Item badgePrefab;
        private Item blackHolePrefab;

        // 检查间隔
        private float inventoryCheckTimer = 0f;
        private float inventoryCheckInterval = 0.5f;

        // 协程引用
        private Coroutine watchInventoryCoroutine;

        // ========== 反射缓存 ==========
        private static readonly Type inventoryType = typeof(Inventory);
        private static readonly FieldInfo contentField;
        private static readonly FieldInfo onContentChangedField;

        static WormholeInventoryHelper()
        {
            // 缓存 Inventory 类型的反射信息
            contentField = inventoryType.GetField("content",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onContentChangedField = inventoryType.GetField("onContentChanged",
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

        void Update()
        {
            // 定期扫描背包，修复虫洞物品
            inventoryCheckTimer += Time.deltaTime;
            if (inventoryCheckTimer >= inventoryCheckInterval)
            {
                inventoryCheckTimer = 0f;
                ScanAndFixInventoryItems();
            }
        }

        /// <summary>
        /// 开始监听背包变化
        /// </summary>
        public void StartWatchInventoryChanges()
        {
            if (watchInventoryCoroutine == null)
            {
                watchInventoryCoroutine = StartCoroutine(WatchInventoryChanges());
            }
        }

        /// <summary>
        /// 停止监听背包变化
        /// </summary>
        public void StopWatchInventoryChanges()
        {
            if (watchInventoryCoroutine != null)
            {
                StopCoroutine(watchInventoryCoroutine);
                watchInventoryCoroutine = null;
            }
            // 清理所有事件监听
            UnsubscribeAllInventoryEvents();
        }

        /// <summary>
        /// 取消所有背包事件监听
        /// </summary>
        private void UnsubscribeAllInventoryEvents()
        {
            var inventoryType = typeof(Inventory);
            foreach (var kvp in inventoryFieldHandlers)
            {
                var inventory = kvp.Key;
                var handler = kvp.Value;
                if (inventory != null)
                {
                    try
                    {
                        var onContentChangedField = inventoryType.GetField("onContentChanged",
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        if (onContentChangedField != null)
                        {
                            var onContentChanged = onContentChangedField.GetValue(inventory) as Action<Inventory, int>;
                            if (onContentChanged != null)
                            {
                                onContentChanged -= handler;
                                onContentChangedField.SetValue(inventory, onContentChanged);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[微型虫洞] 清理背包事件监听失败: {e.Message}");
                    }
                }
            }
            inventoryFieldHandlers.Clear();
        }

        #region 物品修复

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
                        if (item.TypeID == WormholeItemFactory.WORMHOLE_TYPE_ID ||
                            item.TypeID == WormholeItemFactory.RECALL_TYPE_ID)
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
            catch (Exception)
            {
                // 静默失败，避免日志刷屏
            }
        }

        /// <summary>
        /// 检查是否是虫洞物品
        /// </summary>
        public static bool IsWormholeItem(int typeId)
        {
            return typeId == WormholeItemFactory.WORMHOLE_TYPE_ID ||
                   typeId == WormholeItemFactory.RECALL_TYPE_ID ||
                   typeId == WormholeItemFactory.GRENADE_TYPE_ID ||
                   typeId == WormholeItemFactory.BADGE_TYPE_ID ||
                   typeId == WormholeItemFactory.BLACKHOLE_TYPE_ID;
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
                                    // Debug.Log($"[微型虫洞] 已补全 {item.DisplayName} 的 Handheld prefab");
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
                            // Debug.Log($"[微型虫洞] 已通过 SetPrefab 为 {item.DisplayName} 设置 Handheld");
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
                Debug.LogWarning($"[微型虫洞] 修复 {item.DisplayName} 失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 修复物品的 UsageUtilities
        /// </summary>
        public void FixItemUsageUtilities(Item item)
        {
            if (item == null) return;

            try
            {
                var usageUtils = item.UsageUtilities;

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

                // Debug.Log($"[微型虫洞] 正在为 {item.DisplayName} 重新创建 UsageUtilities...");

                var newUsageUtils = item.gameObject.AddComponent<UsageUtilities>();
                WormholeItemFactory.SetFieldValue(newUsageUtils, "useTime", 1.5f);
                WormholeItemFactory.SetFieldValue(newUsageUtils, "useDurability", false);

                var bf2 = typeof(UsageUtilities).GetField("behaviors",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var behaviorsList = bf2?.GetValue(newUsageUtils) as System.Collections.IList;

                if (behaviorsList == null)
                {
                    Debug.LogWarning($"[微型虫洞] 无法获取 {item.DisplayName} 的 behaviors 列表");
                    return;
                }

                if (item.TypeID == WormholeItemFactory.WORMHOLE_TYPE_ID)
                {
                    var behavior = item.gameObject.GetComponent<MicroWormholeUse>();
                    if (behavior != null)
                    {
                        behaviorsList.Add(behavior);
                        // Debug.Log($"[微型虫洞] 已添加 MicroWormholeUse 到 behaviors");
                    }
                }
                else if (item.TypeID == WormholeItemFactory.RECALL_TYPE_ID)
                {
                    var behavior = item.gameObject.GetComponent<WormholeRecallUse>();
                    if (behavior != null)
                    {
                        behaviorsList.Add(behavior);
                        // Debug.Log($"[微型虫洞] 已添加 WormholeRecallUse 到 behaviors");
                    }
                }

                WormholeItemFactory.SetFieldValue(item, "usageUtilities", newUsageUtils);

                // Debug.Log($"[微型虫洞] 已为 {item.DisplayName} 修复 UsageUtilities");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[微型虫洞] 修复 UsageUtilities 失败: {e.Message}");
            }
        }

        #endregion

        #region 物品添加

        /// <summary>
        /// 尝试将物品添加到背包
        /// </summary>
        public bool TryAddItemToInventory(Inventory inventory, int typeId)
        {
            try
            {
                Item prefab = GetPrefabByTypeId(typeId);
                string itemName = GetItemNameByTypeId(typeId);

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
                    UnityEngine.Object.Destroy(newItem.gameObject);
                    Debug.LogWarning($"[微型虫洞] 添加 {itemName} 到背包失败");
                }
                else
                {
                    // Debug.Log($"[微型虫洞] 成功添加 {itemName} 到背包");
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
        /// 根据 TypeID 获取 prefab
        /// </summary>
        private Item GetPrefabByTypeId(int typeId)
        {
            switch (typeId)
            {
                case WormholeItemFactory.WORMHOLE_TYPE_ID:
                    return wormholePrefab;
                case WormholeItemFactory.RECALL_TYPE_ID:
                    return recallPrefab;
                case WormholeItemFactory.GRENADE_TYPE_ID:
                    return grenadePrefab;
                case WormholeItemFactory.BADGE_TYPE_ID:
                    return badgePrefab;
                case WormholeItemFactory.BLACKHOLE_TYPE_ID:
                    return blackHolePrefab;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 根据 TypeID 获取物品名称
        /// </summary>
        private string GetItemNameByTypeId(int typeId)
        {
            switch (typeId)
            {
                case WormholeItemFactory.WORMHOLE_TYPE_ID:
                    return "微型虫洞";
                case WormholeItemFactory.RECALL_TYPE_ID:
                    return "回溯虫洞";
                case WormholeItemFactory.GRENADE_TYPE_ID:
                    return "虫洞手雷";
                case WormholeItemFactory.BADGE_TYPE_ID:
                    return "虫洞徽章";
                case WormholeItemFactory.BLACKHOLE_TYPE_ID:
                    return "微型黑洞发生器";
                default:
                    return "未知物品";
            }
        }

        /// <summary>
        /// 初始化 Mod 创建的物品
        /// </summary>
        public void InitializeModItem(Item item)
        {
            if (item == null) return;

            // Debug.Log($"[微型虫洞] 开始初始化物品: {item.DisplayName}");

            var agentUtils = item.AgentUtilities;
            if (agentUtils != null)
            {
                agentUtils.Initialize(item);
                // Debug.Log($"[微型虫洞] AgentUtilities 初始化完成");
            }

            // Debug.Log($"[微型虫洞] HasHandHeldAgent: {item.HasHandHeldAgent}");
            // Debug.Log($"[微型虫洞] 物品初始化完成: {item.DisplayName}");
        }

        #endregion

        #region 背包监听

        /// <summary>
        /// 开始监听背包变化
        /// 优化：首次扫描注册事件，之后依赖事件回调
        /// </summary>
        public System.Collections.IEnumerator WatchInventoryChanges()
        {
            var processedItems = new HashSet<int>();

            // 首次扫描：注册所有背包的事件监听
            try
            {
                var inventories = FindObjectsOfType(inventoryType) as Inventory[];
                if (inventories != null)
                {
                    foreach (var inventory in inventories)
                    {
                        if (inventory == null) continue;

                        var onContentChanged = onContentChangedField?.GetValue(inventory) as Action<Inventory, int>;

                        if (onContentChanged != null)
                        {
                            if (!inventoryFieldHandlers.ContainsKey(inventory))
                            {
                                Action<Inventory, int> handler = (inv, pos) => OnInventoryContentChanged(inv, pos, processedItems);
                                onContentChanged += handler;
                                inventoryFieldHandlers[inventory] = handler;
                                onContentChangedField?.SetValue(inventory, onContentChanged);
                            }
                        }
                    }
                }
                // Debug.Log($"[微型虫洞] 已注册 {inventoryFieldHandlers.Count} 个背包的事件监听");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[微型虫洞] 首次扫描背包时出错: {e.Message}");
            }

            // 降低扫描频率：每 30 秒检查一次是否有新增背包
            while (this != null && this.gameObject != null)
            {
                try
                {
                    var allInventories = FindObjectsOfType(inventoryType) as Inventory[];
                    if (allInventories != null)
                    {
                        foreach (var inventory in allInventories)
                        {
                            if (inventory == null) continue;

                            // 只处理尚未注册事件的背包
                            if (!inventoryFieldHandlers.ContainsKey(inventory))
                            {
                                var onContentChanged = onContentChangedField?.GetValue(inventory) as Action<Inventory, int>;

                                if (onContentChanged != null)
                                {
                                    Action<Inventory, int> handler = (inv, pos) => OnInventoryContentChanged(inv, pos, processedItems);
                                    onContentChanged += handler;
                                    inventoryFieldHandlers[inventory] = handler;
                                    onContentChangedField?.SetValue(inventory, onContentChanged);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[微型虫洞] 扫描背包时出错: {e.Message}");
                }

                // 降低到 30 秒一次（从 2 秒优化）
                yield return new WaitForSeconds(30f);
            }
        }

        /// <summary>
        /// 背包内容变化回调
        /// </summary>
        private void OnInventoryContentChanged(Inventory inventory, int position, HashSet<int> processedItems)
        {
            // 修复：先检查 inventory 是否为 null
            if (inventory == null) return;

            try
            {
                var content = contentField?.GetValue(inventory) as System.Collections.IList;

                if (content != null && position >= 0 && position < content.Count)
                {
                    var item = content[position] as Item;
                    if (item != null && !processedItems.Contains(item.GetInstanceID()))
                    {
                        if (IsWormholeItem(item.TypeID))
                        {
                            // Debug.Log($"[微型虫洞] 检测到虫洞物品被添加到背包: {item.DisplayName}");
                            FixItemAgentUtilities(item);
                            processedItems.Add(item.GetInstanceID());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[微型虫洞] 修复背包物品时出错: {e.Message}");
            }
        }

        #endregion

        /// <summary>
        /// 清理资源
        /// </summary>
        void OnDestroy()
        {
            StopWatchInventoryChanges();
        }
    }
}
