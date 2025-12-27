using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using Duckov;
using ItemStatsSystem;

namespace WormholeTechMod
{
    /// <summary>
    /// 虫洞科技独立商店
    /// 只售卖虫洞科技系列物品
    /// </summary>
    public class WormholeTechShop : MonoBehaviour
    {
        // ========== 物品 TypeID 常量 ==========
        public const int WORMHOLE_TYPE_ID = 990001;
        public const int RECALL_TYPE_ID = 990002;
        public const int GRENADE_TYPE_ID = 990003;
        public const int BADGE_TYPE_ID = 990004;
        public const int BLACKHOLE_TYPE_ID = 990005;

        // ========== 商店配置 ==========

        /// <summary>
        /// 商店名称
        /// </summary>
        public string shopName = "虫洞科技终端";

        /// <summary>
        /// 商店ID
        /// </summary>
        public string shopId = "WormholeTech";

        /// <summary>
        /// 价格倍率（商店标准配置：商品价格的两倍）
        /// </summary>
        public float priceMultiplier = 2.0f;

        // ========== 内部状态 ==========

        // 商店实例 (Duckov.Economy.StockShop)
        private object stockShopInstance;

        // 商品列表
        private List<ShopItem> shopItems = new List<ShopItem>();

        // 是否已初始化
        private bool isInitialized = false;

        // 单例
        private static WormholeTechShop _instance;
        public static WormholeTechShop Instance => _instance;

        /// <summary>
        /// 商品信息
        /// </summary>
        public class ShopItem
        {
            public int typeId;
            public string name;
            public int basePrice;
            public int maxStock;
            public int currentStock;
            public string description;
        }

        void Awake()
        {
            _instance = this;
        }

        /// <summary>
        /// 初始化商店
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            try
            {
                // 初始化商品列表
                InitializeShopItems();

                // 创建 StockShop 实例
                CreateStockShopInstance();

                isInitialized = true;
                ModLogger.Log($"初始化完成，共 {shopItems.Count} 种商品");
            }
            catch (Exception e)
            {
                ModLogger.LogError($"初始化失败: {e.Message}");
            }
        }

        /// <summary>
        /// 初始化商品列表
        /// </summary>
        private void InitializeShopItems()
        {
            shopItems.Clear();

            // 微型虫洞
            shopItems.Add(new ShopItem
            {
                typeId = WORMHOLE_TYPE_ID,
                name = "微型虫洞",
                basePrice = 500,
                maxStock = 5,
                currentStock = 5,
                description = "一次性传送装置，使用后立即返回基地"
            });

            // 回溯虫洞
            shopItems.Add(new ShopItem
            {
                typeId = RECALL_TYPE_ID,
                name = "回溯虫洞",
                basePrice = 800,
                maxStock = 3,
                currentStock = 3,
                description = "记录当前位置，再次使用返回记录点"
            });

            // 虫洞手雷
            shopItems.Add(new ShopItem
            {
                typeId = GRENADE_TYPE_ID,
                name = "虫洞手雷",
                basePrice = 1200,
                maxStock = 2,
                currentStock = 2,
                description = "投掷后将范围内敌人随机传送"
            });

            // 虫洞徽章
            shopItems.Add(new ShopItem
            {
                typeId = BADGE_TYPE_ID,
                name = "虫洞徽章",
                basePrice = 3000,
                maxStock = 1,
                currentStock = 1,
                description = "被动装备，受到致命伤害时自动闪避"
            });

            // 黑洞手雷
            shopItems.Add(new ShopItem
            {
                typeId = BLACKHOLE_TYPE_ID,
                name = "黑洞手雷",
                basePrice = 2000,
                maxStock = 2,
                currentStock = 2,
                description = "投掷后生成黑洞，吸引并伤害敌人"
            });

            ModLogger.Log($"已添加 {shopItems.Count} 种商品");
        }

        /// <summary>
        /// 创建 StockShop 实例
        /// </summary>
        private void CreateStockShopInstance()
        {
            try
            {
                // 获取 StockShop 类型
                var stockShopType = Type.GetType("Duckov.Economy.StockShop, TeamSoda.Duckov.Core");
                if (stockShopType == null)
                {
                    ModLogger.LogWarning("无法找到 StockShop 类型");
                    return;
                }

                // 在当前 GameObject 上添加 StockShop 组件
                stockShopInstance = gameObject.AddComponent(stockShopType);

                if (stockShopInstance != null)
                {
                    // 设置 merchantID，防止 InitializeEntries 时因为找不到商人配置导致 entries 为空
                    var merchantIdField = stockShopType.GetField("merchantID",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    merchantIdField?.SetValue(stockShopInstance, shopId);

                    // 调用 InitializeEntries 初始化条目（StockShop.Awake 中会调用）
                    // 由于 merchantID 已设置，entries 会被正确初始化

                    // 设置商店属性
                    SetupStockShopEntries();
                    ModLogger.Log("StockShop 实例创建成功");
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"创建 StockShop 实例失败: {e.Message}");
            }
        }

        /// <summary>
        /// 设置商店商品条目
        /// </summary>
        private void SetupStockShopEntries()
        {
            if (stockShopInstance == null) return;

            try
            {
                var stockShopType = stockShopInstance.GetType();

                // 查找 entries 字段或属性
                var entriesField = stockShopType.GetField("entries",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (entriesField != null)
                {
                    // 获取 Entry 类型
                    var entryType = Type.GetType("Duckov.Economy.StockShop+Entry, TeamSoda.Duckov.Core");
                    if (entryType != null)
                    {
                        // 创建 entries 列表
                        var listType = typeof(List<>).MakeGenericType(entryType);
                        var entriesList = Activator.CreateInstance(listType);

                        // 为每个商品创建 Entry
                        foreach (var item in shopItems)
                        {
                            var entry = CreateShopEntry(entryType, item);
                            if (entry != null)
                            {
                                listType.GetMethod("Add").Invoke(entriesList, new[] { entry });
                            }
                        }

                        entriesField.SetValue(stockShopInstance, entriesList);
                        ModLogger.Log($"已设置 {shopItems.Count} 个商品条目");
                    }
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"设置商品条目失败: {e.Message}");
            }
        }

        /// <summary>
        /// 创建商店条目
        /// </summary>
        private object CreateShopEntry(Type entryType, ShopItem item)
        {
            try
            {
                // 获取 ItemEntry 从 StockShopDatabase
                var shopDatabase = StockShopDatabase.Instance;
                if (shopDatabase == null) return null;

                // 查找匹配的 ItemEntry
                var itemEntryField = shopDatabase.GetType().GetField("entries",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (itemEntryField != null)
                {
                    var dbEntries = itemEntryField.GetValue(shopDatabase) as System.Collections.IList;
                    if (dbEntries != null)
                    {
                        foreach (var dbEntry in dbEntries)
                        {
                            var typeIdField = dbEntry.GetType().GetField("typeID",
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (typeIdField != null)
                            {
                                int typeId = (int)typeIdField.GetValue(dbEntry);
                                if (typeId == item.typeId)
                                {
                                    // 找到匹配的条目，创建 Entry
                                    var entry = Activator.CreateInstance(entryType, dbEntry);
                                    return entry;
                                }
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"创建条目失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 打开商店
        /// </summary>
        public bool OpenShop()
        {
            if (!isInitialized)
            {
                Initialize();
            }

            try
            {
                // 方法1：使用 StockShopView.SetupAndShow
                if (TryOpenViaStockShopView())
                {
                    return true;
                }

                // 方法2：使用自定义 UI（如果 StockShopView 不可用）
                if (TryOpenCustomUI())
                {
                    return true;
                }

                ModLogger.LogWarning("无法打开商店");
                return false;
            }
            catch (Exception e)
            {
                ModLogger.LogError($"打开商店异常: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 通过 StockShopView 打开商店
        /// </summary>
        private bool TryOpenViaStockShopView()
        {
            try
            {
                // 获取 StockShopView 类型
                var viewType = Type.GetType("Duckov.Economy.UI.StockShopView, TeamSoda.Duckov.Core");
                if (viewType == null)
                {
                    ModLogger.LogWarning("无法找到 StockShopView 类型");
                    return false;
                }

                // 获取 Instance
                var instanceProp = viewType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static);
                if (instanceProp == null)
                {
                    ModLogger.LogWarning("无法找到 StockShopView.Instance");
                    return false;
                }

                var viewInstance = instanceProp.GetValue(null);
                if (viewInstance == null)
                {
                    ModLogger.LogWarning("StockShopView.Instance 为 null");
                    return false;
                }

                // 调用 SetupAndShow
                if (stockShopInstance != null)
                {
                    var setupMethod = viewType.GetMethod("SetupAndShow",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (setupMethod != null)
                    {
                        setupMethod.Invoke(viewInstance, new[] { stockShopInstance });
                        ModLogger.Log("通过 StockShopView.SetupAndShow 打开");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"StockShopView 方式失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 尝试打开自定义 UI（作为备选方案）
        /// </summary>
        private bool TryOpenCustomUI()
        {
            // 如果游戏原生 UI 不可用，显示简单的购买界面
            // 这里使用 IMGUI 作为临时方案
            var uiComponent = gameObject.GetComponent<WormholeTechShopUI>();
            if (uiComponent == null)
            {
                uiComponent = gameObject.AddComponent<WormholeTechShopUI>();
            }

            uiComponent.Show(shopItems);
            ModLogger.Log("使用自定义 UI 打开");
            return true;
        }

        /// <summary>
        /// 购买物品
        /// </summary>
        public bool PurchaseItem(int typeId)
        {
            var item = shopItems.Find(i => i.typeId == typeId);
            if (item == null)
            {
                ModLogger.LogWarning($"物品不存在: {typeId}");
                return false;
            }

            if (item.currentStock <= 0)
            {
                ShowMessage("库存不足！");
                return false;
            }

            // 检查玩家金币
            var mainCharacter = CharacterMainControl.Main;
            if (mainCharacter == null) return false;

            int price = (int)(item.basePrice * priceMultiplier);

            // 获取玩家金币
            var inventory = mainCharacter.GetComponent<Inventory>();
            if (inventory == null) return false;

            int playerGold = GetPlayerGold(mainCharacter);
            if (playerGold < price)
            {
                ShowMessage($"金币不足！需要 {price} 金币");
                return false;
            }

            // 扣除金币
            if (!DeductGold(mainCharacter, price))
            {
                ShowMessage("扣款失败！");
                return false;
            }

            // 给予物品
            if (!GiveItem(mainCharacter, typeId))
            {
                // 退款
                AddGold(mainCharacter, price);
                ShowMessage("物品添加失败！");
                return false;
            }

            // 减少库存
            item.currentStock--;

            ShowMessage($"购买成功！获得 {item.name}");
            ModLogger.Log($"玩家购买了 {item.name}，剩余库存: {item.currentStock}");
            return true;
        }

        /// <summary>
        /// 获取玩家金币
        /// </summary>
        private int GetPlayerGold(CharacterMainControl character)
        {
            try
            {
                var inventory = character.GetComponent<Inventory>();
                if (inventory != null)
                {
                    // 尝试获取金币数量
                    var goldProp = inventory.GetType().GetProperty("Gold",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (goldProp != null)
                    {
                        return (int)goldProp.GetValue(inventory);
                    }

                    // 尝试其他属性名
                    var moneyField = inventory.GetType().GetField("money",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (moneyField != null)
                    {
                        return (int)moneyField.GetValue(inventory);
                    }
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"获取金币失败: {e.Message}");
            }
            return 0;
        }

        /// <summary>
        /// 扣除金币
        /// </summary>
        private bool DeductGold(CharacterMainControl character, int amount)
        {
            try
            {
                var inventory = character.GetComponent<Inventory>();
                if (inventory != null)
                {
                    // 尝试调用扣款方法
                    var deductMethod = inventory.GetType().GetMethod("DeductGold",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (deductMethod != null)
                    {
                        deductMethod.Invoke(inventory, new object[] { amount });
                        return true;
                    }

                    // 直接设置金币
                    var goldProp = inventory.GetType().GetProperty("Gold",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (goldProp != null && goldProp.CanWrite)
                    {
                        int current = (int)goldProp.GetValue(inventory);
                        goldProp.SetValue(inventory, current - amount);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"扣除金币失败: {e.Message}");
            }
            return false;
        }

        /// <summary>
        /// 添加金币
        /// </summary>
        private bool AddGold(CharacterMainControl character, int amount)
        {
            try
            {
                var inventory = character.GetComponent<Inventory>();
                if (inventory != null)
                {
                    var addMethod = inventory.GetType().GetMethod("AddGold",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (addMethod != null)
                    {
                        addMethod.Invoke(inventory, new object[] { amount });
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"添加金币失败: {e.Message}");
            }
            return false;
        }

        /// <summary>
        /// 给予物品
        /// </summary>
        private bool GiveItem(CharacterMainControl character, int typeId)
        {
            try
            {
                // 虫洞物品都是动态注册的，使用 TryGetDynamicEntry 获取
                ItemAssetsCollection.DynamicEntry dynamicEntry;
                Item prefab = null;

                if (ItemAssetsCollection.TryGetDynamicEntry(typeId, out dynamicEntry))
                {
                    prefab = dynamicEntry.prefab;
                }

                if (prefab == null)
                {
                    ModLogger.LogWarning($"无法找到物品 prefab: {typeId}");
                    return false;
                }

                // 创建物品实例
                Item item = prefab.CreateInstance();
                if (item == null)
                {
                    ModLogger.LogWarning($"无法创建物品实例: {typeId}");
                    return false;
                }

                // 添加到背包 (使用 AddItem 方法)
                var inventory = character.GetComponent<Inventory>();
                if (inventory != null)
                {
                    return inventory.AddItem(item);
                }

                return false;
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"给予物品失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 补充库存
        /// </summary>
        public void RestockAll()
        {
            foreach (var item in shopItems)
            {
                item.currentStock = item.maxStock;
            }
            ModLogger.Log("库存已补充");
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        private void ShowMessage(string message)
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter != null)
            {
                mainCharacter.PopText(message);
            }
            ModLogger.Log($"{message}");
        }

        /// <summary>
        /// 获取商品列表
        /// </summary>
        public List<ShopItem> GetShopItems()
        {
            return shopItems;
        }

        /// <summary>
        /// 检查商店是否已初始化
        /// </summary>
        public bool IsInitialized => isInitialized;

        void OnDestroy()
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 虫洞科技商店自定义 UI（使用 IMGUI）
    /// 作为游戏原生 UI 不可用时的备选方案
    /// </summary>
    public class WormholeTechShopUI : MonoBehaviour
    {
        private bool isVisible = false;
        private List<WormholeTechShop.ShopItem> items;
        private Vector2 scrollPosition;
        private GUIStyle titleStyle;
        private GUIStyle itemStyle;
        private GUIStyle buttonStyle;
        private GUIStyle priceStyle;
        private bool stylesInitialized = false;

        // 窗口配置
        private Rect windowRect = new Rect(100, 100, 400, 500);

        public void Show(List<WormholeTechShop.ShopItem> shopItems)
        {
            items = shopItems;
            isVisible = true;

            // 暂停游戏（如果需要）
            // Time.timeScale = 0f;
        }

        public void Hide()
        {
            isVisible = false;
            // Time.timeScale = 1f;
        }

        void OnGUI()
        {
            if (!isVisible || items == null) return;

            // 初始化样式
            if (!stylesInitialized)
            {
                InitStyles();
            }

            // 绘制窗口
            windowRect = GUI.Window(9527, windowRect, DrawShopWindow, "");
        }

        private void InitStyles()
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(0.4f, 0.8f, 1f);

            itemStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 5, 5)
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            priceStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleRight
            };
            priceStyle.normal.textColor = Color.yellow;

            stylesInitialized = true;
        }

        private void DrawShopWindow(int windowId)
        {
            // 标题
            GUILayout.Space(5);
            GUILayout.Label("虫洞科技终端", titleStyle);
            GUILayout.Space(10);

            // 滚动区域
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(380));

            foreach (var item in items)
            {
                DrawShopItem(item);
                GUILayout.Space(5);
            }

            GUILayout.EndScrollView();

            GUILayout.Space(10);

            // 关闭按钮
            if (GUILayout.Button("关闭", buttonStyle, GUILayout.Height(30)))
            {
                Hide();
            }

            // 允许拖动窗口
            GUI.DragWindow(new Rect(0, 0, 400, 30));
        }

        private void DrawShopItem(WormholeTechShop.ShopItem item)
        {
            GUILayout.BeginVertical(itemStyle);

            // 计算实际售价（基础价格 * 倍率）
            int actualPrice = (int)(item.basePrice * (WormholeTechShop.Instance?.priceMultiplier ?? 2.0f));

            GUILayout.BeginHorizontal();
            GUILayout.Label(item.name, GUILayout.Width(150));
            GUILayout.Label($"库存: {item.currentStock}/{item.maxStock}", GUILayout.Width(80));
            GUILayout.Label($"{actualPrice} 金", priceStyle, GUILayout.Width(80));
            GUILayout.EndHorizontal();

            GUILayout.Label(item.description, GUILayout.Height(20));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.enabled = item.currentStock > 0;
            if (GUILayout.Button("购买", buttonStyle, GUILayout.Width(80)))
            {
                if (WormholeTechShop.Instance != null)
                {
                    WormholeTechShop.Instance.PurchaseItem(item.typeId);
                }
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        void Update()
        {
            // 按 ESC 关闭
            if (isVisible && Input.GetKeyDown(KeyCode.Escape))
            {
                Hide();
            }
        }
    }
}