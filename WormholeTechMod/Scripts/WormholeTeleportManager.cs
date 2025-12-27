using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Reflection;
using Duckov.Scenes;

namespace WormholeTechMod
{
    /// <summary>
    /// 虫洞传送管理器
    /// 负责处理虫洞传送相关的所有逻辑
    /// </summary>
    public class WormholeTeleportManager : MonoBehaviour
    {
        // 单例
        private static WormholeTeleportManager _instance;
        public static WormholeTeleportManager Instance => _instance;

        // 虫洞记录数据
        private WormholeData savedWormholeData = new WormholeData();

        // 待传送状态
        private bool pendingTeleport = false;
        private string pendingTeleportScene = null;
        private Vector3 pendingTeleportPosition = Vector3.zero;
        private Quaternion pendingTeleportRotation = Quaternion.identity;

        // 协程引用
        private Coroutine delayedTeleportCoroutine;

        void Awake()
        {
            _instance = this;
        }

        void OnDestroy()
        {
            _instance = null;
            // 停止协程
            if (delayedTeleportCoroutine != null)
            {
                StopCoroutine(delayedTeleportCoroutine);
                delayedTeleportCoroutine = null;
            }
        }

        #region 公开接口

        /// <summary>
        /// 保存虫洞数据
        /// </summary>
        public void SetWormholeData(WormholeData data)
        {
            savedWormholeData = data;
        }

        /// <summary>
        /// 获取虫洞数据
        /// </summary>
        public WormholeData GetWormholeData()
        {
            return savedWormholeData;
        }

        /// <summary>
        /// 检查是否有有效的虫洞数据
        /// </summary>
        public bool HasValidWormholeData()
        {
            return savedWormholeData.IsValid;
        }

        /// <summary>
        /// 设置待传送数据
        /// </summary>
        public void SetPendingTeleport(string sceneName, Vector3 position, Quaternion rotation)
        {
            pendingTeleport = true;
            pendingTeleportScene = sceneName;
            pendingTeleportPosition = position;
            pendingTeleportRotation = rotation;
            // Debug.Log($"[微型虫洞] 设置待传送: 场景={sceneName}, 位置={position}");
        }

        /// <summary>
        /// 检查是否有待处理的传送
        /// </summary>
        public void CheckPendingTeleport()
        {
            if (pendingTeleport && savedWormholeData.IsValid)
            {
                if (delayedTeleportCoroutine == null)
                {
                    delayedTeleportCoroutine = StartCoroutine(DelayedTeleport());
                }
            }
        }

        /// <summary>
        /// 执行虫洞回溯
        /// </summary>
        public void ExecuteRecall(CharacterMainControl character)
        {
            if (!savedWormholeData.IsValid)
            {
                ModLogger.LogWarning("[微型虫洞] 没有有效的虫洞数据");
                return;
            }

            string targetScene = savedWormholeData.SceneName;
            string currentScene = SceneManager.GetActiveScene().name;

            // Debug.Log($"[微型虫洞] 正在回溯: 当前场景={currentScene}, 目标场景={targetScene}, 位置={savedWormholeData.Position}");

            // 检查是否已经在目标场景
            if (currentScene == targetScene)
            {
                ModLogger.Log("[微型虫洞] 已在目标场景，直接传送...");
                PlayWormholeEffect();
                TeleportToSavedPosition();
                return;
            }

            // 播放特效
            PlayWormholeEffect();

            // 设置待传送标记
            pendingTeleport = true;

            // 加载目标场景
            try
            {
                // Debug.Log($"[微型虫洞] 开始加载场景: {targetScene}");
                SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);
                // Debug.Log($"[微型虫洞] 场景加载请求已发送: {targetScene}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[微型虫洞] 场景加载失败: {e.Message}\n{e.StackTrace}");
                ShowMessage("虫洞通道开启失败！");
                pendingTeleport = false;
            }
        }

        #endregion

        #region 传送逻辑

        /// <summary>
        /// 延迟传送协程
        /// </summary>
        private IEnumerator DelayedTeleport()
        {
            yield return new WaitForSeconds(1f);
            TeleportToSavedPosition();
            pendingTeleport = false;
            delayedTeleportCoroutine = null;
        }

        /// <summary>
        /// 传送到保存的位置
        /// </summary>
        public void TeleportToSavedPosition()
        {
            Vector3 targetPosition;
            Quaternion targetRotation;

            if (pendingTeleport && !string.IsNullOrEmpty(pendingTeleportScene))
            {
                targetPosition = pendingTeleportPosition;
                targetRotation = pendingTeleportRotation;
            }
            else if (savedWormholeData.IsValid)
            {
                targetPosition = savedWormholeData.Position;
                targetRotation = savedWormholeData.Rotation;
            }
            else
            {
                ModLogger.LogWarning("[微型虫洞] 没有有效的传送数据");
                return;
            }

            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter == null)
            {
                ModLogger.LogWarning("[微型虫洞] 找不到主角");
                return;
            }

            // Debug.Log($"[微型虫洞] 正在传送到: {targetPosition}");

            mainCharacter.transform.position = targetPosition;
            mainCharacter.transform.rotation = targetRotation;

            PlayWormholeEffect();
            ShowMessage("虫洞回溯成功！");

            // 清除待传送标记
            pendingTeleport = false;
            pendingTeleportScene = null;

            // 清除保存的数据
            savedWormholeData.Clear();

            ModLogger.Log("[微型虫洞] 传送完成");
        }

        /// <summary>
        /// 执行虫洞回溯场景加载
        /// 使用 MultiSceneCore.LoadAndTeleport 自动传送
        /// </summary>
        public void ExecuteRecallScene(string targetScene, Vector3 targetPosition, Quaternion targetRotation)
        {
            if (string.IsNullOrEmpty(targetScene))
            {
                ModLogger.LogWarning("[微型虫洞] 目标场景为空");
                return;
            }

            // 设置待传送（用于回退）
            pendingTeleport = true;
            pendingTeleportScene = targetScene;
            pendingTeleportPosition = targetPosition;
            pendingTeleportRotation = targetRotation;

            try
            {
                // 使用 MultiSceneCore.LoadAndTeleport
                // 使用完全限定名称：Duckov.Scenes.MultiSceneCore
                var multiSceneCoreType = Type.GetType("Duckov.Scenes.MultiSceneCore, TeamSoda.Duckov.Core");
                if (multiSceneCoreType == null)
                {
                    // 尝试不带命名空间的名称
                    multiSceneCoreType = Type.GetType("MultiSceneCore, TeamSoda.Duckov.Core");
                }

                if (multiSceneCoreType == null)
                {
                    ModLogger.LogWarning("[微型虫洞] 找不到 MultiSceneCore 类型");
                    UseSceneLoaderFallback(targetScene, targetPosition, targetRotation);
                    return;
                }

                var instanceProp = multiSceneCoreType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp == null)
                {
                    ModLogger.LogWarning("[微型虫洞] 找不到 MultiSceneCore.Instance");
                    UseSceneLoaderFallback(targetScene, targetPosition, targetRotation);
                    return;
                }

                var multiSceneCore = instanceProp.GetValue(null);
                if (multiSceneCore == null)
                {
                    ModLogger.LogWarning("[微型虫洞] MultiSceneCore.Instance 为空");
                    UseSceneLoaderFallback(targetScene, targetPosition, targetRotation);
                    return;
                }

                // 调用 LoadAndTeleport(sceneID, position)
                var loadAndTeleportMethod = multiSceneCoreType.GetMethod("LoadAndTeleport",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(string), typeof(Vector3), typeof(bool) },
                    null);

                if (loadAndTeleportMethod != null)
                {
                    // 调用 LoadAndTeleport(targetScene, targetPosition, false)
                    loadAndTeleportMethod.Invoke(multiSceneCore, new object[] { targetScene, targetPosition, false });
                }
                else
                {
                    // 备用：使用 SceneLoader.LoadScene 并在场景加载后设置位置
                    ModLogger.LogWarning("[微型虫洞] 找不到 LoadAndTeleport，使用备用方案");
                    UseSceneLoaderFallback(targetScene, targetPosition, targetRotation);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[微型虫洞] 场景加载失败: {e.Message}");
                UseSceneLoaderFallback(targetScene, targetPosition, targetRotation);
            }
        }

        /// <summary>
        /// 备用方案：使用 SceneLoader.LoadScene
        /// </summary>
        private void UseSceneLoaderFallback(string targetScene, Vector3 targetPosition, Quaternion targetRotation)
        {
            try
            {
                var sceneLoaderType = Type.GetType("SceneLoader, TeamSoda.Duckov.Core");
                if (sceneLoaderType == null) return;

                var instanceProp = sceneLoaderType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp == null) return;

                var sceneLoader = instanceProp.GetValue(null);
                if (sceneLoader == null) return;

                // 订阅场景加载事件
                SceneManager.sceneLoaded -= OnRecallSceneLoaded;
                SceneManager.sceneLoaded += OnRecallSceneLoaded;

                // 调用 LoadScene - 使用反射invoke，不指定具体类型
                var methods = sceneLoaderType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    if (method.Name == "LoadScene" && method.GetParameters().Length == 9)
                    {
                        // 调用方法
                        method.Invoke(sceneLoader, new object[] {
                            targetScene,
                            null,   // overrideCurtainScene
                            false,  // clickToContinue
                            false,  // notifyEvacuation
                            true,   // doCircleFade
                            false,  // useLocation
                            default(MultiSceneLocation),
                            true,   // saveToFile
                            false   // hideTips
                        });
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[微型虫洞] 备用方案失败: {e.Message}");
                SceneManager.sceneLoaded -= OnRecallSceneLoaded;
            }
        }

        /// <summary>
        /// 场景加载完成回调
        /// </summary>
        private void OnRecallSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnRecallSceneLoaded;

            if (!pendingTeleport)
            {
                // Debug.Log($"[微型虫洞] 场景加载完成，但 pendingTeleport 为 false，跳过传送");
                return;
            }

            // Debug.Log($"[微型虫洞] 场景加载完成，正在恢复玩家位置...");

            CharacterMainControl character = CharacterMainControl.Main;
            if (character != null)
            {
                character.transform.position = pendingTeleportPosition;
                character.transform.rotation = pendingTeleportRotation;
                // Debug.Log($"[微型虫洞] 玩家已传送到位置: {pendingTeleportPosition}");
            }
            else
            {
                ModLogger.LogWarning("[微型虫洞] 找不到玩家角色，无法传送");
            }

            pendingTeleport = false;
            pendingTeleportScene = null;
            pendingTeleportPosition = Vector3.zero;
            pendingTeleportRotation = Quaternion.identity;
        }

        #endregion

        #region 特效和消息

        /// <summary>
        /// 播放虫洞特效
        /// </summary>
        public void PlayWormholeEffect()
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
        /// 显示消息
        /// </summary>
        public void ShowMessage(string message)
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter != null)
            {
                mainCharacter.PopText(message);
            }
        }

        #endregion
    }
}
