using UnityEngine;
using ItemStatsSystem;
using System;
using System.Collections;
using Duckov.Scenes;
using Cysharp.Threading.Tasks;

namespace WormholeTechMod
{
    /// <summary>
    /// 虫洞回溯使用行为
    /// 继承自 UsageBehavior，传送到记录的位置
    /// 使用 UniTask 异步模式，与游戏原生传送器保持一致
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
        /// 使用 UniTask 异步执行，与原生传送器模式一致
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
                ExecuteSameSceneTeleport(character, targetPosition, wormholeData.Rotation).Forget();
                return;
            }

            // 不同场景：使用 UniTask 异步执行场景加载
            character.PopText("正在打开虫洞通道...");
            ExecuteCrossSceneTeleport(targetScene, targetPosition, wormholeData.Rotation).Forget();
        }

        /// <summary>
        /// 同场景传送（UniTask）
        /// </summary>
        private async UniTask ExecuteSameSceneTeleport(CharacterMainControl character, Vector3 targetPosition, Quaternion targetRotation)
        {
            await UniTask.Yield(); // 等待一帧，避免阻塞

            if (character != null)
            {
                character.SetPosition(targetPosition);
                character.transform.rotation = targetRotation;
                character.PopText("虫洞回溯成功！");
                ModLogger.Log($"[回溯虫洞] 同场景传送成功: {targetPosition}");
            }
        }

        /// <summary>
        /// 跨场景传送（UniTask）
        /// 使用与原生传送器一致的异步模式
        /// </summary>
        private async UniTask ExecuteCrossSceneTeleport(string targetScene, Vector3 targetPosition, Quaternion targetRotation)
        {
            ModLogger.Log($"[回溯虫洞] 开始跨场景传送: {targetScene} -> {targetPosition}");

            // 开始异步场景加载
            UnityEngine.AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                targetScene, 
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );

            // 允许场景激活
            asyncLoad.allowSceneActivation = true;

            // 等待场景加载完成
            await UniTask.WaitUntil(() => asyncLoad.isDone);

            ModLogger.Log($"[回溯虫洞] 场景加载完成: {targetScene}");

            // 等待场景完全初始化
            await UniTask.Delay(1000, DelayType.DeltaTime);

            // 查找角色（尝试多次）
            CharacterMainControl character = null;
            int maxRetries = 5;
            for (int retry = 0; retry < maxRetries; retry++)
            {
                character = UnityEngine.Object.FindObjectOfType<CharacterMainControl>();
                if (character != null)
                {
                    ModLogger.Log($"[回溯虫洞] 第 {retry + 1} 次尝试找到角色成功");
                    break;
                }
                await UniTask.Delay(500, DelayType.DeltaTime);
            }

            if (character != null)
            {
                character.SetPosition(targetPosition);
                character.transform.rotation = targetRotation;

                ModLogger.Log($"[回溯虫洞] 传送成功: {targetPosition}");
                character.PopText("虫洞回溯成功！");
            }
            else
            {
                ModLogger.LogWarning("[回溯虫洞] 多次尝试仍找不到玩家角色，传送失败");
            }
        }
    }
}