using UnityEngine;
using ItemStatsSystem;
using System;
using System.Reflection;
using Duckov.Scenes;
using Cysharp.Threading.Tasks;

namespace WormholeTechMod
{
    /// <summary>
    /// 虫洞回溯使用行为
    /// 继承自 UsageBehavior，传送到记录的位置
    /// </summary>
    public class WormholeRecallUse : UsageBehavior
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
                    description = "传送到记录的位置"
                };
            }
        }

        void Awake()
        {
            item = GetComponent<Item>();
        }

        /// <summary>
        /// 检查物品是否可以使用
        /// </summary>
        public override bool CanBeUsed(Item item, object user)
        {
            // 基础检查：用户必须是角色
            if (!(user as CharacterMainControl))
            {
                return false;
            }

            // 检查 LevelManager
            if (LevelManager.Instance == null)
            {
                return false;
            }

            // 只能在基地使用
            if (!LevelManager.Instance.IsBaseLevel)
            {
                return false;
            }

            // 检查是否有有效的虫洞记录
            if (WormholeTeleportManager.Instance == null)
            {
                return false;
            }
            if (!WormholeTeleportManager.Instance.HasValidWormholeData())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 执行使用逻辑
        /// </summary>
        protected override void OnUse(Item item, object user)
        {
            CharacterMainControl character = user as CharacterMainControl;
            if (character == null) return;

            var teleportManager = WormholeTeleportManager.Instance;

            // 检查条件
            if (LevelManager.Instance == null)
            {
                character.PopText("无法使用虫洞！");
                return;
            }

            if (!LevelManager.Instance.IsBaseLevel)
            {
                character.PopText("只能在家中使用！");
                return;
            }

            if (teleportManager == null || !teleportManager.HasValidWormholeData())
            {
                character.PopText("没有可回溯的虫洞残留");
                return;
            }

            // 获取保存的虫洞数据
            var wormholeData = teleportManager.GetWormholeData();
            string targetScene = wormholeData.SceneName;
            Vector3 targetPosition = wormholeData.Position;

            // 如果已经在目标场景，直接传送
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene == targetScene)
            {
                character.PopText("正在打开虫洞通道...");
                character.transform.position = targetPosition;
                character.transform.rotation = wormholeData.Rotation;
                return;
            }

            // 使用 WormholeTeleportManager 的场景加载逻辑
            character.PopText("正在打开虫洞通道...");
            teleportManager.ExecuteRecallScene(targetScene, targetPosition, wormholeData.Rotation);
        }
    }
}
