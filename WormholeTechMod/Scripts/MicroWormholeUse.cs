using UnityEngine;
using ItemStatsSystem;
using System;
using System.Reflection;
using Duckov.Scenes;

namespace WormholeTechMod
{
    /// <summary>
    /// 微型虫洞使用行为
    /// 继承自 UsageBehavior，记录位置并触发撤离
    /// </summary>
    public class MicroWormholeUse : UsageBehavior
    {
        // 关联的物品
        private Item item;

        // 重写 DisplaySettings - 让UI显示使用信息
        public override DisplaySettingsData DisplaySettings
        {
            get
            {
                return new DisplaySettingsData
                {
                    display = true,
                    description = "记录位置并撤离"
                };
            }
        }

        void Awake()
        {
            item = GetComponent<Item>();
            ModLogger.Log("[虫洞科技] MicroWormholeUse行为初始化完成");
        }

        /// <summary>
        /// 检查物品是否可以使用
        /// </summary>
        public override bool CanBeUsed(Item item, object user)
        {
            // 基础检查：用户必须是角色
            if (!(user as CharacterMainControl))
            {
                ModLogger.Log("[虫洞科技] CanBeUsed失败：用户不是角色");
                return false;
            }

            // 检查 LevelManager
            if (LevelManager.Instance == null)
            {
                ModLogger.Log("[虫洞科技] CanBeUsed失败：LevelManager为空");
                return false;
            }

            // 只能在突袭地图使用
            if (!LevelManager.Instance.IsRaidMap)
            {
                // Debug.Log($"[虫洞科技] CanBeUsed失败：不是突袭地图，IsRaidMap={LevelManager.Instance.IsRaidMap}");
                return false;
            }

            // 不能在基地使用
            if (LevelManager.Instance.IsBaseLevel)
            {
                // Debug.Log($"[虫洞科技] CanBeUsed失败：在基地中，IsBaseLevel={LevelManager.Instance.IsBaseLevel}");
                return false;
            }

            // ModLogger.Log("[虫洞科技] CanBeUsed成功");
            return true;
        }

        /// <summary>
        /// 执行使用逻辑
        /// </summary>
        protected override void OnUse(Item item, object user)
        {
            CharacterMainControl character = user as CharacterMainControl;
            if (character == null) return;

            // 检查条件
            if (LevelManager.Instance == null || LevelManager.Instance.IsBaseLevel || !LevelManager.Instance.IsRaidMap)
            {
                character.PopText("无法使用虫洞！");
                return;
            }

            // 记录当前位置和场景
            WormholeData savedData = new WormholeData();
            savedData.IsValid = true;
            savedData.Position = character.transform.position;
            savedData.Rotation = character.transform.rotation;
            savedData.SceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // Debug.Log($"[虫洞科技] 位置已记录: {savedData.Position}, 场景: {savedData.SceneName}");

            // 保存到 TeleportManager
            if (WormholeTeleportManager.Instance != null)
            {
                WormholeTeleportManager.Instance.SetWormholeData(savedData);
            }

            character.PopText("位置已记录！正在撤离...");

            // 使用 NotifyEvacuated 触发撤离
            try
            {
                string subsceneID = MultiSceneCore.ActiveSubSceneID;
                EvacuationInfo evacuationInfo = new EvacuationInfo(subsceneID, character.transform.position);

                // 通知撤离（会设置无敌、保存数据、触发事件）
                LevelManager.Instance.NotifyEvacuated(evacuationInfo);

                // 使用 SceneLoader 加载基地场景
                var sceneLoader = GameManager.SceneLoader;
                if (sceneLoader != null)
                {
                    // 使用反射调用 LoadBaseScene
                    var loadBaseMethod = sceneLoader.GetType().GetMethod("LoadBaseScene",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (loadBaseMethod != null)
                    {
                        loadBaseMethod.Invoke(sceneLoader, new object[] { null, true });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[虫洞科技] 撤离失败: {e.Message}");
            }
        }
    }
}
