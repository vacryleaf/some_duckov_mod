using UnityEngine;
using ItemStatsSystem;
using System;
using System.Reflection;
using Duckov.Scenes;

namespace MicroWormholeMod
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
            Debug.Log("[虫洞回溯] WormholeRecallUse行为初始化完成");
        }

        /// <summary>
        /// 检查物品是否可以使用
        /// </summary>
        public override bool CanBeUsed(Item item, object user)
        {
            // 基础检查：用户必须是角色
            if (!(user as CharacterMainControl))
            {
                Debug.Log("[虫洞回溯] CanBeUsed失败：用户不是角色");
                return false;
            }

            // 检查 LevelManager
            if (LevelManager.Instance == null)
            {
                Debug.Log("[虫洞回溯] CanBeUsed失败：LevelManager为空");
                return false;
            }

            // 只能在基地使用
            if (!LevelManager.Instance.IsBaseLevel)
            {
                Debug.Log($"[虫洞回溯] CanBeUsed失败：不在基地，IsBaseLevel={LevelManager.Instance.IsBaseLevel}");
                return false;
            }

            // 检查是否有有效的虫洞记录
            var modBehaviour = FindObjectOfType<ModBehaviour>();
            if (modBehaviour == null)
            {
                Debug.Log("[虫洞回溯] CanBeUsed失败：找不到ModBehaviour");
                return false;
            }
            if (!modBehaviour.HasValidWormholeData())
            {
                Debug.Log("[虫洞回溯] CanBeUsed失败：没有有效的虫洞记录");
                return false;
            }

            Debug.Log("[虫洞回溯] CanBeUsed成功");
            return true;
        }

        /// <summary>
        /// 执行使用逻辑
        /// </summary>
        protected override void OnUse(Item item, object user)
        {
            CharacterMainControl character = user as CharacterMainControl;
            if (character == null) return;

            var modBehaviour = FindObjectOfType<ModBehaviour>();

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

            if (modBehaviour == null || !modBehaviour.HasValidWormholeData())
            {
                character.PopText("没有可回溯的虫洞残留");
                return;
            }

            // 获取保存的虫洞数据
            var wormholeData = modBehaviour.GetWormholeData();
            string targetScene = wormholeData.SceneName;
            Vector3 targetPosition = wormholeData.Position;
            Quaternion targetRotation = wormholeData.Rotation;

            Debug.Log($"[虫洞回溯] 目标场景: {targetScene}, 位置: {targetPosition}");

            // 获取当前场景
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // 如果已经在目标场景，直接传送
            if (currentScene == targetScene)
            {
                character.PopText("正在打开虫洞通道...");
                character.transform.position = targetPosition;
                character.transform.rotation = targetRotation;
                Debug.Log($"[虫洞回溯] 直接传送到位置: {targetPosition}");
                return;
            }

            // 如果在不同场景，使用场景加载
            character.PopText("正在打开虫洞通道...");

            try
            {
                // 获取 ModBehaviour 中的场景加载方法
                var executeMethod = modBehaviour.GetType().GetMethod("ExecuteRecallScene",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (executeMethod != null)
                {
                    // 调用新的场景加载方法
                    executeMethod.Invoke(modBehaviour, new object[] { targetScene, targetPosition, targetRotation });
                    Debug.Log($"[虫洞回溯] 已调用场景加载: {targetScene}");
                }
                else
                {
                    Debug.LogError($"[虫洞回溯] 找不到 ExecuteRecallScene 方法");
                    character.PopText("虫洞通道开启失败！");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[虫洞回溯] 场景加载失败: {e.Message}\n{e.StackTrace}");
                character.PopText("虫洞通道开启失败！");
            }
        }
    }
}
