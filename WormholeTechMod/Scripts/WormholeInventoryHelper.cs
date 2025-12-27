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
    /// </summary>
    public class WormholeInventoryHelper : MonoBehaviour
    {
        // 单例
        public static WormholeInventoryHelper Instance { get; private set; }

        // 已注入的箱子记录（避免重复注入）
        private HashSet<int> injectedChests = new HashSet<int>();

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

            // 订阅箱子打开事件
            InteractableLootbox.OnStartLoot += OnStartLootChest;
        }

        void OnDestroy()
        {
            // 清理单例
            if (Instance == this) Instance = null;

            // 取消订阅
            InteractableLootbox.OnStartLoot -= OnStartLootChest;
        }

        /// <summary>
        /// 玩家打开箱子时注入虫洞科技物品
        /// </summary>
        private void OnStartLootChest(InteractableLootbox lootbox)
        {
            if (lootbox == null) return;

            int chestId = lootbox.GetInstanceID();
            if (injectedChests.Contains(chestId)) return; // 已经注入过

            ModLogger.Log($"[虫洞科技] 玩家打开箱子: {lootbox.name}");

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
                    ModLogger.LogWarning($"[虫洞科技] 箱子 {lootbox.name} 没有 Inventory");
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
                    ModLogger.LogWarning("[虫洞科技] 没有可用的物品 prefab");
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
                        ModLogger.Log($"[虫洞科技] 箱子 {lootbox.name} 已满");
                        break;
                    }

                    // 创建物品实例
                    var newItem = prefab.CreateInstance();
                    if (newItem != null)
                    {
                        // 初始化物品
                        InitializeModItem(newItem);

                        // 添加到箱子
                        bool success = inventory.AddItem(newItem);
                        if (success)
                        {
                            ModLogger.Log($"[虫洞科技] 已注入 {newItem.DisplayName} 到箱子");
                            injected++;
                        }
                        else
                        {
                            UnityEngine.Object.Destroy(newItem.gameObject);
                            ModLogger.LogWarning($"[虫洞科技] 注入 {newItem.DisplayName} 失败");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"[虫洞科技] 注入物品失败: {e.Message}");
            }
        }

        /// <summary>
        /// 初始化 Mod 创建的物品（箱子注入时调用）
        /// 注意：不在此时修复 AgentUtilities，留待使用时修复
        /// </summary>
        private void InitializeModItem(Item item)
        {
            if (item == null) return;
            // 只做最基本的初始化，不修复 AgentUtilities
        }

        /// <summary>
        /// 修复物品的 AgentUtilities（在物品使用时调用）
        /// </summary>
        public void FixItemAgentUtilities(Item item)
        {
            if (item == null) return;

            try
            {
                var agentUtils = item.AgentUtilities;
                if (agentUtils == null) return;

                // 设置 master 字段
                var masterField = typeof(ItemAgentUtilities).GetField("master",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (masterField != null)
                {
                    masterField.SetValue(agentUtils, item);
                }

                agentUtils.Initialize(item);
                ModLogger.Log($"[虫洞科技] 已修复 {item.DisplayName} 的 AgentUtilities");
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"[虫洞科技] 修复 {item.DisplayName} 失败: {e.Message}");
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
                    ModLogger.LogWarning($"[虫洞科技] {itemName} prefab 为空");
                    return false;
                }

                int emptySlot = inventory.GetFirstEmptyPosition(0);
                if (emptySlot < 0)
                {
                    return false;
                }

                var newItem = prefab.CreateInstance();
                if (newItem == null) return false;

                InitializeModItem(newItem);
                bool success = inventory.AddItem(newItem);

                if (success)
                {
                    ModLogger.Log($"[虫洞科技] 成功添加 {itemName} 到背包");
                }
                else
                {
                    UnityEngine.Object.Destroy(newItem.gameObject);
                }

                return success;
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"[虫洞科技] 添加物品失败: {e.Message}");
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

        #endregion
    }
}
