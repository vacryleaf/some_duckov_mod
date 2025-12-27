using UnityEngine;
using ItemStatsSystem;
using System;
using System.Collections;
using Duckov.Scenes;

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
                    display = true
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
        /// 注意：不能在 OnUse 中直接执行场景加载，否则会阻塞使用进度条导致卡死
        /// 必须使用协程延迟执行
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

            // 如果已经在目标场景，直接传送（不涉及场景加载，可以使用协程）
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene == targetScene)
            {
                character.PopText("正在打开虫洞通道...");
                // 使用协程延迟一帧执行，避免阻塞
                character.StartCoroutine(ExecuteSameSceneTeleport(character, targetPosition, wormholeData.Rotation));
                return;
            }

            // 不同场景：使用协程延迟执行场景加载
            character.PopText("正在打开虫洞通道...");
            character.StartCoroutine(ExecuteCrossSceneTeleport(targetScene, targetPosition, wormholeData.Rotation));
        }

        /// <summary>
        /// 同场景传送（协程执行）
        /// </summary>
        private IEnumerator ExecuteSameSceneTeleport(CharacterMainControl character, Vector3 targetPosition, Quaternion targetRotation)
        {
            yield return null; // 等待一帧，避免阻塞

            if (character != null)
            {
                character.SetPosition(targetPosition);
                character.transform.rotation = targetRotation;
            }
        }

        /// <summary>
        /// 跨场景传送（协程执行）
        /// 必须在协程中执行场景加载，否则会阻塞使用进度条
        /// </summary>
        private IEnumerator ExecuteCrossSceneTeleport(string targetScene, Vector3 targetPosition, Quaternion targetRotation)
        {
            ModLogger.Log($"[回溯虫洞] 开始跨场景传送: {targetScene} -> {targetPosition}");

            // 异步加载目标场景
            UnityEngine.AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(targetScene, UnityEngine.SceneManagement.LoadSceneMode.Single);

            // 等待场景加载完成
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // 场景加载完成后，等待几帧确保对象初始化完成
            yield return new WaitForSeconds(0.5f);

            // 使用 FindObjectOfType 获取角色（场景加载后 CharacterMainControl.Main 可能为 null）
            CharacterMainControl character = UnityEngine.Object.FindObjectOfType<CharacterMainControl>();
            if (character != null)
            {
                character.SetPosition(targetPosition);
                character.transform.rotation = targetRotation;

                ModLogger.Log($"[回溯虫洞] 传送成功: {targetPosition}");
                character.PopText("虫洞回溯成功！");
            }
            else
            {
                ModLogger.LogWarning("[回溯虫洞] 找不到玩家角色");
            }
        }
    }
}
