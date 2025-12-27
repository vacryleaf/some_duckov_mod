using UnityEngine;
using UnityEngine.SceneManagement;
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
    /// 箱子物品注入器
    /// 负责将虫洞物品注入到游戏中的箱子
    /// </summary>
    public class WormholeLootInjector : MonoBehaviour
    {
        // 已处理的箱子记录
        private HashSet<int> processedBoxes = new HashSet<int>();

        // 背包辅助引用
        private WormholeInventoryHelper inventoryHelper;

        // 协程引用
        private Coroutine injectionCoroutine;

        // 缓存 LootBoxLoader 引用
        private List<LootBoxLoader> cachedLoaders = new List<LootBoxLoader>();

        /// <summary>
        /// 设置背包辅助引用
        /// </summary>
        public void SetInventoryHelper(WormholeInventoryHelper helper)
        {
            inventoryHelper = helper;
        }

        /// <summary>
        /// 启动箱子注入协程
        /// </summary>
        public void StartInjection()
        {
            if (injectionCoroutine == null)
            {
                // 注册场景加载事件用于清理和刷新缓存
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
                injectionCoroutine = StartCoroutine(LootBoxInjectionRoutine());
            }
        }

        /// <summary>
        /// 停止箱子注入协程
        /// </summary>
        public void StopInjection()
        {
            if (injectionCoroutine != null)
            {
                StopCoroutine(injectionCoroutine);
                injectionCoroutine = null;
            }
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// 场景加载时刷新缓存
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshCache();
        }

        /// <summary>
        /// 刷新缓存（只在场景加载时调用一次）
        /// </summary>
        private void RefreshCache()
        {
            cachedLoaders = new List<LootBoxLoader>(FindObjectsOfType<LootBoxLoader>());
            processedBoxes.Clear();
            ModLogger.Log($"[虫洞科技] 刷新箱子缓存: {cachedLoaders.Count} 个Loader");
        }

        /// <summary>
        /// 箱子物品注入协程
        /// 定期扫描场景中的箱子，注入虫洞物品
        /// 优化：减少 FindObjectsOfType 调用频率
        /// </summary>
        private IEnumerator LootBoxInjectionRoutine()
        {
            // 等待场景完全加载
            yield return new WaitForSeconds(2f);

            ModLogger.Log("[虫洞科技] 开始箱子物品注入...");

            // 首次刷新缓存并执行注入
            RefreshCache();

            while (this != null && this.gameObject != null)
            {
                try
                {
                    // 只处理新增的箱子（性能优化）
                    InjectToNewLootboxes();

                    // 定时刷新 LootBoxLoader 列表（新场景可能有新增）
                    // 降低频率到 30 秒一次
                    InjectToLootBoxLoaders();
                }
                catch (Exception e)
                {
                    ModLogger.LogWarning($"[虫洞科技] 箱子注入时发生错误: {e.Message}");
                }

                // 每隔较长时间检查一次（性能优化）
                yield return new WaitForSeconds(30f);
            }
        }

        /// <summary>
        /// 只注入新出现的箱子（性能优化）
        /// </summary>
        private void InjectToNewLootboxes()
        {
            var allBoxes = FindObjectsOfType<InteractableLootbox>();
            var newBoxes = allBoxes.Where(box => box != null && !processedBoxes.Contains(box.GetInstanceID())).ToList();

            foreach (var lootbox in newBoxes)
            {
                InjectToSingleLootbox(lootbox);
            }
        }

        /// <summary>
        /// 注入到单个箱子背包
        /// </summary>
        private void InjectToSingleLootbox(InteractableLootbox lootbox)
        {
            try
            {
                var getInventoryMethod = typeof(InteractableLootbox).GetMethod("GetOrCreateInventory",
                    BindingFlags.Public | BindingFlags.Static);

                if (getInventoryMethod != null)
                {
                    var inventory = getInventoryMethod.Invoke(null, new object[] { lootbox }) as Inventory;

                    if (inventory != null && inventoryHelper != null)
                    {
                        int addedCount = 0;

                        // 18%概率添加微型虫洞
                        if (UnityEngine.Random.value < 0.18f)
                        {
                            if (inventoryHelper.TryAddItemToInventory(inventory, WormholeItemFactory.WORMHOLE_TYPE_ID))
                            {
                                addedCount++;
                            }
                        }

                        // 15%概率添加回溯虫洞
                        if (UnityEngine.Random.value < 0.15f)
                        {
                            if (inventoryHelper.TryAddItemToInventory(inventory, WormholeItemFactory.RECALL_TYPE_ID))
                            {
                                addedCount++;
                            }
                        }

                        // 10%概率添加虫洞手雷
                        if (UnityEngine.Random.value < 0.10f)
                        {
                            if (inventoryHelper.TryAddItemToInventory(inventory, WormholeItemFactory.GRENADE_TYPE_ID))
                            {
                                addedCount++;
                            }
                        }

                        // 1%概率添加虫洞徽章
                        if (UnityEngine.Random.value < 0.01f)
                        {
                            if (inventoryHelper.TryAddItemToInventory(inventory, WormholeItemFactory.BADGE_TYPE_ID))
                            {
                                addedCount++;
                            }
                        }

                        // 10%概率添加黑洞手雷
                        if (UnityEngine.Random.value < 0.10f)
                        {
                            if (inventoryHelper.TryAddItemToInventory(inventory, WormholeItemFactory.BLACKHOLE_TYPE_ID))
                            {
                                addedCount++;
                            }
                        }

                        if (addedCount > 0)
                        {
                            ModLogger.Log($"[虫洞科技] 已向箱子 {lootbox.gameObject.name} 注入 {addedCount} 个物品");
                        }
                    }
                }

                // 标记为已处理
                processedBoxes.Add(lootbox.GetInstanceID());
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"[虫洞科技] 注入到箱子失败: {e.Message}");
                processedBoxes.Add(lootbox.GetInstanceID());
            }
        }

        /// <summary>
        /// 注入到 LootBoxLoader（影响新生成的箱子）
        /// 优化：使用缓存的 loader 列表
        /// </summary>
        private void InjectToLootBoxLoaders()
        {
            // 使用缓存的列表，避免每次都调用 FindObjectsOfType
            var loadersToProcess = cachedLoaders.Where(l => l != null).ToList();

            if (loadersToProcess.Count == 0)
            {
                // 如果缓存为空，尝试重新获取
                loadersToProcess = new List<LootBoxLoader>(FindObjectsOfType<LootBoxLoader>());
                if (loadersToProcess.Count == 0)
                {
                    return;
                }
            }

            foreach (var loader in loadersToProcess)
            {
                if (loader == null) continue;

                try
                {
                    var fixedItemsField = loader.GetType().GetField("fixedItems",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    if (fixedItemsField != null)
                    {
                        var fixedItems = fixedItemsField.GetValue(loader) as List<int>;
                        if (fixedItems != null)
                        {
                            bool modified = false;

                            // 有一定概率添加虫洞物品
                            if (!fixedItems.Contains(WormholeItemFactory.WORMHOLE_TYPE_ID) && UnityEngine.Random.value < 0.15f)
                            {
                                fixedItems.Add(WormholeItemFactory.WORMHOLE_TYPE_ID);
                                modified = true;
                            }

                            if (!fixedItems.Contains(WormholeItemFactory.RECALL_TYPE_ID) && UnityEngine.Random.value < 0.15f)
                            {
                                fixedItems.Add(WormholeItemFactory.RECALL_TYPE_ID);
                                modified = true;
                            }

                            if (!fixedItems.Contains(WormholeItemFactory.GRENADE_TYPE_ID) && UnityEngine.Random.value < 0.10f)
                            {
                                fixedItems.Add(WormholeItemFactory.GRENADE_TYPE_ID);
                                modified = true;
                            }

                            if (!fixedItems.Contains(WormholeItemFactory.BADGE_TYPE_ID) && UnityEngine.Random.value < 0.01f)
                            {
                                fixedItems.Add(WormholeItemFactory.BADGE_TYPE_ID);
                                modified = true;
                            }

                            if (!fixedItems.Contains(WormholeItemFactory.BLACKHOLE_TYPE_ID) && UnityEngine.Random.value < 0.10f)
                            {
                                fixedItems.Add(WormholeItemFactory.BLACKHOLE_TYPE_ID);
                                modified = true;
                            }

                            if (modified)
                            {
                                ModLogger.Log($"[虫洞科技] 已修改 LootBoxLoader: {loader.gameObject.name}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ModLogger.LogWarning($"[虫洞科技] 修改 LootBoxLoader 失败: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 清除已处理记录（场景切换时调用）
        /// </summary>
        public void ClearProcessedBoxes()
        {
            processedBoxes.Clear();
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        void OnDestroy()
        {
            StopInjection();
        }
    }
}
