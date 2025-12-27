using UnityEngine;
using UnityEngine.SceneManagement;
using ItemStatsSystem;
using System;
using Duckov.Scenes;
using Duckov.Utilities;
using Cysharp.Threading.Tasks;

namespace WormholeTechMod
{
    /// <summary>
    /// 虫洞回溯使用行为 - 场景转场
    /// 使用 SceneLoader.LoadScene + 等待场景初始化完成
    /// </summary>
    public class WormholeRecallUse : UsageBehavior
    {
        private Item item;

        public override DisplaySettingsData DisplaySettings
        {
            get
            {
                return new DisplaySettingsData { display = true };
            }
        }

        void Awake()
        {
            item = GetComponent<Item>();
        }

        public override bool CanBeUsed(Item item, object user)
        {
            return user as CharacterMainControl != null &&
                   LevelManager.Instance != null &&
                   LevelManager.Instance.IsBaseLevel &&
                   WormholeTeleportManager.Instance != null &&
                   WormholeTeleportManager.Instance.HasValidWormholeData();
        }

        protected override void OnUse(Item item, object user)
        {
            CharacterMainControl character = user as CharacterMainControl;
            if (character == null) return;

            if (LevelManager.Instance == null ||
                !LevelManager.Instance.IsBaseLevel ||
                WormholeTeleportManager.Instance == null ||
                !WormholeTeleportManager.Instance.HasValidWormholeData())
            {
                character.PopText("条件不满足！");
                return;
            }

            var wormholeData = WormholeTeleportManager.Instance.GetWormholeData();
            string targetScene = wormholeData.SceneName;
            Vector3 targetPosition = wormholeData.Position;

            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == targetScene)
            {
                character.PopText("同场景直接传送...");
                character.SetPosition(targetPosition);
                character.PopText("虫洞回溯成功！");
                return;
            }

            character.PopText("正在打开虫洞通道...");
            ModLogger.Log($"[回溯虫洞] 当前场景: {currentScene}, 目标: {targetScene}, 位置: {targetPosition}");

            // 按顺序测试不同的转场方法
            TestAllMethods(targetScene, targetPosition, wormholeData.Rotation);
        }

        /// <summary>
        /// 按顺序测试所有转场方法
        /// </summary>
        private async void TestAllMethods(string targetScene, Vector3 targetPosition, Quaternion targetRotation)
        {
            // 方法4: SceneLoader.Instance.LoadScene（已注释，直接跳过）
            // ModLogger.Log("=== 测试方法4: SceneLoader.Instance.LoadScene ===");
            // ... 已注释

            // 方法5: SceneManager.LoadSceneAsync (Additive)
            ModLogger.Log("=== 测试方法5: SceneManager.LoadSceneAsync (Additive) ===");
            try
            {
                ModLogger.Log($"[回溯虫洞] 开始加载场景: {targetScene}");
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);

                if (asyncLoad != null)
                {
                    while (!asyncLoad.isDone)
                    {
                        await UniTask.Yield();
                    }

                    Scene newScene = SceneManager.GetSceneByName(targetScene);
                    if (newScene.IsValid())
                    {
                        SceneManager.SetActiveScene(newScene);
                        ModLogger.Log($"[回溯虫洞] 场景已激活: {newScene.name}");
                        await UniTask.Yield();
                        await UniTask.Yield();
                        await FinishTeleport(targetPosition, targetRotation);
                        return;
                    }
                }
                else
                {
                    ModLogger.LogWarning("[回溯虫洞] AsyncOperation 为 null");
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"[回溯虫洞] 异常: {e.Message}");
            }

            // 方法6: SceneManager.LoadScene (Single)
            ModLogger.Log("=== 使用方法6: SceneManager.LoadScene (Single) ===");
            try
            {
                ModLogger.Log($"[回溯虫洞] 加载场景: {targetScene}");
                SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
                ModLogger.Log("[回溯虫洞] LoadScene 完成");
                await UniTask.Yield();
                await UniTask.Yield();
                await FinishTeleport(targetPosition, targetRotation);
                return;
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"[回溯虫洞] 异常: {e.Message}");
            }

            ModLogger.LogError("[回溯虫洞] 所有方法都失败了！");
        }

        /// <summary>
        /// 等待玩家角色创建并初始化完成
        /// </summary>
        private async UniTask WaitForCharacterMainControl()
        {
            // 等待最多 10 秒
            float timeout = 10f;
            float elapsed = 0f;

            while (elapsed < timeout)
            {
                // 使用 FindObjectOfType 直接查找
                CharacterMainControl character = FindObjectOfType<CharacterMainControl>();
                if (character != null)
                {
                    // 额外等待一帧确保完全初始化
                    await UniTask.Yield();
                    ModLogger.Log("[回溯虫洞] 玩家角色已找到");
                    return;
                }

                await UniTask.Yield();
                elapsed += Time.deltaTime;
            }

            ModLogger.LogWarning("[回溯虫洞] 等待玩家角色超时");
        }

        private async UniTask FinishTeleport(Vector3 targetPosition, Quaternion targetRotation)
        {
            // 等待一帧让场景完全初始化
            await UniTask.Yield();
            await UniTask.Yield();

            // 使用 FindObjectOfType 直接查找（绕过 LevelManager）
            CharacterMainControl character = FindObjectOfType<CharacterMainControl>();
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
