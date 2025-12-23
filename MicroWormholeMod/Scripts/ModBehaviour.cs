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

        // 物品TypeID（使用较大的数值避免与游戏本体和其他Mod冲突）
        private const int WORMHOLE_TYPE_ID = 990001;  // 微型虫洞
        private const int RECALL_TYPE_ID = 990002;    // 虫洞回溯

        // AssetBundle
        private AssetBundle assetBundle;

        // 物品图标
        private Sprite wormholeIcon;
        private Sprite recallIcon;

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

                // 注册到游戏系统
                RegisterItems();

                // 监听物品使用事件
                RegisterEvents();

                // 检查是否需要传送（场景加载后）
                CheckPendingTeleport();

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
                    break;
                default:
                    LocalizationManager.SetOverrideText("MicroWormhole_Name", "微型虫洞");
                    LocalizationManager.SetOverrideText("MicroWormhole_Desc", "高科技传送装置。使用后会记录当前位置并撤离回家。\n\n<color=#FFD700>配合「回溯虫洞」使用，可返回记录的位置</color>");
                    LocalizationManager.SetOverrideText("WormholeRecall_Name", "回溯虫洞");
                    LocalizationManager.SetOverrideText("WormholeRecall_Desc", "虫洞传送的配套装置。在家中使用，可以传送回「微型虫洞」记录的位置。\n\n<color=#FFD700>只能在家中使用</color>");
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

            // 检查是否有有效的记录
            if (!savedWormholeData.IsValid)
            {
                ShowMessage("没有记录的位置！请先使用微型虫洞。");
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
        /// 显示消息提示
        /// </summary>
        private void ShowMessage(string message)
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter != null)
            {
                Duckov.UI.DialogueBubbles.DialogueBubblesManager.Show(
                    message,
                    mainCharacter.transform,
                    -1f,
                    false,
                    false,
                    -1f,
                    2f
                );
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

            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                assetBundle = null;
            }

            Debug.Log("[微型虫洞] Mod卸载完成");
        }
    }
}
