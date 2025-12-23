using UnityEngine;
using Duckov.Modding;
using ItemStatsSystem;
using SodaCraft.Localizations;

namespace MicroWormholeMod
{
    /// <summary>
    /// 微型虫洞Mod主类
    /// 负责注册一次性使用的传送物品，使用后可以立即撤离回家
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // 物品Prefab
        private Item wormholePrefab;

        // 物品TypeID（使用较大的数值避免与游戏本体和其他Mod冲突）
        private const int WORMHOLE_TYPE_ID = 990001;

        /// <summary>
        /// Mod启动入口
        /// </summary>
        void Start()
        {
            Debug.Log("[微型虫洞] 开始加载Mod...");

            try
            {
                // 设置本地化文本
                SetupLocalization();

                // 创建物品Prefab
                CreateWormholeItem();

                // 注册到游戏系统
                RegisterItem();

                // 监听物品使用事件
                RegisterEvents();

                Debug.Log("[微型虫洞] Mod加载完成!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[微型虫洞] Mod加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 设置本地化文本
        /// </summary>
        private void SetupLocalization()
        {
            // 设置物品名称和描述的本地化文本
            LocalizationManager.SetOverrideText("MicroWormhole_Name", "微型虫洞");
            LocalizationManager.SetOverrideText("MicroWormhole_Desc", "一次性使用的高科技传送装置。激活后会打开一个通往家中的虫洞，让你立即撤离。\n\n<color=#FFD700>注意：使用后物品会被消耗</color>");

            // 监听语言切换事件
            LocalizationManager.OnSetLanguage += OnLanguageChanged;

            Debug.Log("[微型虫洞] 本地化设置完成");
        }

        /// <summary>
        /// 语言切换时更新本地化文本
        /// </summary>
        private void OnLanguageChanged(SystemLanguage language)
        {
            // 根据语言设置不同的文本
            switch (language)
            {
                case SystemLanguage.English:
                    LocalizationManager.SetOverrideText("MicroWormhole_Name", "Micro Wormhole");
                    LocalizationManager.SetOverrideText("MicroWormhole_Desc", "A one-time use high-tech teleportation device. When activated, it opens a wormhole leading home for immediate evacuation.\n\n<color=#FFD700>Note: Item will be consumed after use</color>");
                    break;
                case SystemLanguage.Korean:
                    LocalizationManager.SetOverrideText("MicroWormhole_Name", "마이크로 웜홀");
                    LocalizationManager.SetOverrideText("MicroWormhole_Desc", "일회용 고급 텔레포트 장치입니다. 활성화하면 집으로 통하는 웜홀이 열려 즉시 탈출할 수 있습니다.\n\n<color=#FFD700>주의: 사용 후 아이템이 소비됩니다</color>");
                    break;
                default:
                    // 默认中文
                    LocalizationManager.SetOverrideText("MicroWormhole_Name", "微型虫洞");
                    LocalizationManager.SetOverrideText("MicroWormhole_Desc", "一次性使用的高科技传送装置。激活后会打开一个通往家中的虫洞，让你立即撤离。\n\n<color=#FFD700>注意：使用后物品会被消耗</color>");
                    break;
            }
        }

        /// <summary>
        /// 创建虫洞物品Prefab
        /// </summary>
        private void CreateWormholeItem()
        {
            Debug.Log("[微型虫洞] 开始创建物品Prefab...");

            // 创建基础GameObject
            GameObject itemObj = new GameObject("MicroWormhole");

            // 设置为不销毁
            DontDestroyOnLoad(itemObj);
            itemObj.SetActive(false);

            // 添加Item组件
            wormholePrefab = itemObj.AddComponent<Item>();

            // 添加自定义使用组件
            itemObj.AddComponent<MicroWormholeUse>();

            // 创建简单的视觉效果
            CreateWormholeVisual(itemObj);

            Debug.Log("[微型虫洞] 物品Prefab创建完成");
        }

        /// <summary>
        /// 创建虫洞的视觉效果
        /// </summary>
        private void CreateWormholeVisual(GameObject parent)
        {
            // 创建一个简单的球体作为物品模型
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(parent.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

            // 创建发光材质
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.4f, 0.2f, 0.8f, 0.9f); // 紫色
            material.SetFloat("_Metallic", 0.8f);
            material.SetFloat("_Glossiness", 0.9f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.6f, 0.3f, 1f) * 2f); // 紫色发光

            visual.GetComponent<Renderer>().material = material;

            // 移除碰撞体（使用父对象的碰撞体）
            Object.Destroy(visual.GetComponent<Collider>());

            // 添加拾取碰撞体
            SphereCollider collider = parent.AddComponent<SphereCollider>();
            collider.radius = 0.1f;

            Debug.Log("[微型虫洞] 视觉效果创建完成");
        }

        /// <summary>
        /// 注册物品到游戏系统
        /// </summary>
        private void RegisterItem()
        {
            if (wormholePrefab == null)
            {
                Debug.LogError("[微型虫洞] 无法注册空物品");
                return;
            }

            Debug.Log($"[微型虫洞] 正在注册物品，TypeID: {WORMHOLE_TYPE_ID}");

            // 注册到动态物品系统
            bool success = ItemAssetsCollection.AddDynamicEntry(wormholePrefab);

            if (success)
            {
                Debug.Log($"[微型虫洞] 物品注册成功！");
            }
            else
            {
                Debug.LogError("[微型虫洞] 物品注册失败！可能是TypeID冲突");
            }
        }

        /// <summary>
        /// 注册事件监听
        /// </summary>
        private void RegisterEvents()
        {
            // 监听物品使用事件
            Item.onUseStatic += OnItemUsed;

            Debug.Log("[微型虫洞] 事件监听注册完成");
        }

        /// <summary>
        /// 物品使用事件处理
        /// </summary>
        private void OnItemUsed(Item item, object user)
        {
            // 检查是否为微型虫洞物品
            if (item == null || item.TypeID != WORMHOLE_TYPE_ID)
            {
                return;
            }

            Debug.Log("[微型虫洞] 检测到微型虫洞被使用！");

            // 触发撤离效果
            TriggerEvacuation(item);
        }

        /// <summary>
        /// 触发撤离效果
        /// </summary>
        private void TriggerEvacuation(Item item)
        {
            // 检查是否在突袭地图中
            if (LevelManager.Instance == null)
            {
                Debug.LogWarning("[微型虫洞] LevelManager未初始化");
                return;
            }

            if (LevelManager.Instance.IsBaseLevel)
            {
                Debug.Log("[微型虫洞] 已在基地中，无需撤离");
                // 显示提示
                ShowMessage("你已经在家中了！");
                return;
            }

            if (!LevelManager.Instance.IsRaidMap)
            {
                Debug.Log("[微型虫洞] 不在突袭地图中");
                ShowMessage("只能在突袭任务中使用！");
                return;
            }

            Debug.Log("[微型虫洞] 正在触发撤离...");

            // 播放特效
            PlayWormholeEffect();

            // 消耗物品（一次性使用）
            ConsumeItem(item);

            // 触发撤离
            EvacuationInfo evacuationInfo = new EvacuationInfo();
            LevelManager.Instance.NotifyEvacuated(evacuationInfo);

            Debug.Log("[微型虫洞] 撤离成功！");
        }

        /// <summary>
        /// 播放虫洞特效
        /// </summary>
        private void PlayWormholeEffect()
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter == null) return;

            // 在角色位置创建特效
            Vector3 position = mainCharacter.transform.position;

            // 创建简单的粒子效果
            GameObject effectObj = new GameObject("WormholeEffect");
            effectObj.transform.position = position;

            ParticleSystem particles = effectObj.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(0.6f, 0.3f, 1f, 0.8f); // 紫色
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

            // 播放后自动销毁
            particles.Play();
            Destroy(effectObj, 2f);

            Debug.Log("[微型虫洞] 特效播放完成");
        }

        /// <summary>
        /// 消耗物品（一次性使用）
        /// </summary>
        private void ConsumeItem(Item item)
        {
            if (item == null) return;

            // 如果物品可堆叠，减少堆叠数量
            if (item.Stackable && item.StackCount > 1)
            {
                // 减少堆叠数量（需要通过游戏API实现）
                Debug.Log("[微型虫洞] 减少堆叠数量");
            }
            else
            {
                // 销毁物品
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
            // 使用气泡对话显示提示
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

            // 取消事件监听
            Item.onUseStatic -= OnItemUsed;
            LocalizationManager.OnSetLanguage -= OnLanguageChanged;

            // 移除动态物品
            if (wormholePrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(wormholePrefab);
                Destroy(wormholePrefab.gameObject);
                Debug.Log("[微型虫洞] 物品已从游戏中移除");
            }

            Debug.Log("[微型虫洞] Mod卸载完成");
        }
    }
}
