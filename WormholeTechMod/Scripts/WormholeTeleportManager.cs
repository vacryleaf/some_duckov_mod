using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Reflection;
using Duckov.Scenes;
using Duckov.UI.Animations;
using Cysharp.Threading.Tasks;

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
        private Coroutine teleportCoroutine;

        // 冷却状态（与游戏传送仪一致）
        private static float lastTeleportTime = 0f;
        private const float TELEPORT_COOLDOWN = 1f;

        // 是否正在传送中
        private bool isTeleporting = false;

        void Awake()
        {
            _instance = this;
        }

        void OnDestroy()
        {
            _instance = null;
            // 停止协程
            if (teleportCoroutine != null)
            {
                StopCoroutine(teleportCoroutine);
                teleportCoroutine = null;
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
        }

        /// <summary>
        /// 检查是否有待处理的传送（同场景）
        /// </summary>
        public void CheckPendingTeleport()
        {
            if (pendingTeleport && !string.IsNullOrEmpty(pendingTeleportScene))
            {
                TeleportToSavedPosition();
            }
        }

        /// <summary>
        /// 执行虫洞回溯（同场景）
        /// </summary>
        public void ExecuteRecall(CharacterMainControl character)
        {
            if (!savedWormholeData.IsValid)
            {
                ModLogger.LogWarning("没有有效的虫洞数据");
                return;
            }

            string targetScene = savedWormholeData.SceneName;
            string currentScene = SceneManager.GetActiveScene().name;

            // 检查是否已经在目标场景
            if (currentScene == targetScene)
            {
                ModLogger.Log("已在目标场景，直接传送...");
                PlayWormholeEffect();
                TeleportToSavedPosition();
                return;
            }

            // 不同场景，使用 ExecuteRecallScene
            ExecuteRecallScene(targetScene, savedWormholeData.Position, savedWormholeData.Rotation);
        }

        /// <summary>
        /// 检查是否可传送（冷却中）
        /// </summary>
        public static bool CanTeleport
        {
            get { return Time.time - lastTeleportTime > TELEPORT_COOLDOWN; }
        }

        /// <summary>
        /// 执行虫洞回溯场景加载
        /// 完全按照游戏传送仪 MultiSceneTeleporter 的逻辑实现
        /// </summary>
        public void ExecuteRecallScene(string targetScene, Vector3 targetPosition, Quaternion targetRotation)
        {
            if (isTeleporting)
            {
                ModLogger.Log("正在传送中，忽略重复请求");
                return;
            }

            if (string.IsNullOrEmpty(targetScene))
            {
                ModLogger.LogWarning("目标场景为空");
                return;
            }

            // 冷却检查（与游戏传送仪一致）
            if (!CanTeleport)
            {
                ModLogger.Log("传送冷却中...");
                return;
            }

            // 设置待传送数据
            pendingTeleport = true;
            pendingTeleportScene = targetScene;
            pendingTeleportPosition = targetPosition;
            pendingTeleportRotation = targetRotation;

            // 启动协程执行传送
            teleportCoroutine = StartCoroutine(TeleportCoroutine(targetScene, targetPosition, targetRotation));
        }

        #endregion

        #region 传送逻辑（完全按照游戏传送仪逻辑）

        /// <summary>
        /// 传送协程 - 完全按照游戏传送仪 TeleportTask 逻辑
        /// 1. 异步加载场景
        /// 2. 场景加载完成后设置位置（使用记录的坐标）
        /// 3. 更新冷却时间
        /// </summary>
        private IEnumerator TeleportCoroutine(string targetScene, Vector3 targetPosition, Quaternion targetRotation)
        {
            isTeleporting = true;
            ModLogger.Log($"开始传送: 场景={targetScene}, 位置={targetPosition}");

            // 播放虫洞特效
            PlayWormholeEffect();

            if (GameManager.SceneLoader != null)
            {
                ModLogger.Log("使用 SceneLoader 加载场景...");
                var task = GameManager.SceneLoader.LoadScene(targetScene, null, false, false, true, false, default(MultiSceneLocation), true, false);
                while (!task.GetAwaiter().IsCompleted)
                {
                    yield return null;
                }
                ModLogger.Log("场景加载完成");
            }
            else
            {
                ModLogger.LogWarning("SceneLoader 为空，使用 Unity SceneManager");
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }

            // 场景加载完成，设置角色位置（使用记录的坐标）
            ModLogger.Log("等待关卡初始化...");
            float waitTime = 0f;
            float maxWaitTime = 15f;
            
            while (!LevelManager.LevelInited && waitTime < maxWaitTime)
            {
                yield return null;
                waitTime += Time.deltaTime;
            }
            
            if (!LevelManager.LevelInited)
            {
                ModLogger.LogWarning("等待关卡初始化超时");
            }
            else
            {
                ModLogger.Log($"关卡初始化完成，等待时间: {waitTime:F2}秒");
            }
            
            // 等待玩家初始化
            ModLogger.Log("等待玩家初始化...");
            CharacterMainControl character = null;
            waitTime = 0f;
            maxWaitTime = 10f;
            
            while (character == null && waitTime < maxWaitTime)
            {
                character = CharacterMainControl.Main;
                if (character == null)
                {
                    yield return null;
                    waitTime += Time.deltaTime;
                }
            }
            
            if (character == null)
            {
                ModLogger.LogWarning("等待玩家初始化超时");
            }
            else
            {
                ModLogger.Log($"玩家初始化完成，等待时间: {waitTime:F2}秒");
                character.SetPosition(targetPosition);
                character.transform.rotation = targetRotation;
                ModLogger.Log($"传送成功: {targetPosition}");
                ShowMessage("虫洞回溯成功！");
            }

            // 确保加载画面被隐藏（解决黑屏问题）
            HideLoadingScreens();

            // 更新冷却时间（与游戏传送仪一致）
            lastTeleportTime = Time.time;

            // 清除状态
            pendingTeleport = false;
            pendingTeleportScene = null;
            pendingTeleportPosition = Vector3.zero;
            pendingTeleportRotation = Quaternion.identity;
            teleportCoroutine = null;
            isTeleporting = false;
        }

        /// <summary>
        /// 传送到保存的位置（同场景传送）
        /// </summary>
        public void TeleportToSavedPosition()
        {
            // 冷却检查
            if (!CanTeleport)
            {
                ModLogger.Log("传送冷却中...");
                return;
            }

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
                ModLogger.LogWarning("没有有效的传送数据");
                return;
            }

            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter == null)
            {
                ModLogger.LogWarning("找不到主角");
                return;
            }

            // 播放特效
            PlayWormholeEffect();

            // 使用 SetPosition（与游戏传送仪一致）
            mainCharacter.SetPosition(targetPosition);
            mainCharacter.transform.rotation = targetRotation;

            ShowMessage("虫洞回溯成功！");

            // 更新冷却时间
            lastTeleportTime = Time.time;

            // 清除状态
            pendingTeleport = false;
            pendingTeleportScene = null;

            // 清除保存的数据
            savedWormholeData.Clear();

            ModLogger.Log("传送完成");
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

        #region 加载画面管理

        /// <summary>
        /// 隐藏所有加载画面，解决黑屏问题
        /// 当 SceneLoader.onAfterSceneInitialize 事件没有正确触发时，手动隐藏
        /// </summary>
        private void HideLoadingScreens()
        {
            try
            {
                ModLogger.Log("尝试隐藏加载画面...");
                
                // 隐藏 LevelInitializingIndicator 的 FadeGroup
                LevelInitializingIndicator indicator = UnityEngine.Object.FindFirstObjectByType<LevelInitializingIndicator>();
                if (indicator != null)
                {
                    ModLogger.Log("找到 LevelInitializingIndicator，隐藏 FadeGroup");
                    // 使用反射调用私有方法或访问私有字段
                    var fadeGroupField = typeof(LevelInitializingIndicator).GetField("fadeGroup", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fadeGroupField != null)
                    {
                        FadeGroup fadeGroup = fadeGroupField.GetValue(indicator) as FadeGroup;
                        if (fadeGroup != null)
                        {
                            fadeGroup.SkipHide();
                            ModLogger.Log("已隐藏 LevelInitializingIndicator 的 FadeGroup");
                        }
                    }
                }
                else
                {
                    ModLogger.Log("未找到 LevelInitializingIndicator");
                }
                
                // 隐藏 SceneLoader 的 content
                if (GameManager.SceneLoader != null)
                {
                    var contentField = typeof(SceneLoader).GetField("content", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (contentField != null)
                    {
                        FadeGroup content = contentField.GetValue(GameManager.SceneLoader) as FadeGroup;
                        if (content != null)
                        {
                            content.SkipHide();
                            ModLogger.Log("已隐藏 SceneLoader 的 content");
                        }
                    }
                }
                
                ModLogger.Log("加载画面隐藏完成");
            }
            catch (Exception ex)
            {
                ModLogger.LogWarning($"隐藏加载画面时出错: {ex.Message}");
            }
        }

        #endregion
    }
}
