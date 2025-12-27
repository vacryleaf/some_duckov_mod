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
                ModLogger.LogWarning("[虫洞科技] 没有有效的虫洞数据");
                return;
            }

            string targetScene = savedWormholeData.SceneName;
            string currentScene = SceneManager.GetActiveScene().name;

            // 检查是否已经在目标场景
            if (currentScene == targetScene)
            {
                ModLogger.Log("[虫洞科技] 已在目标场景，直接传送...");
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
                ModLogger.Log("[虫洞科技] 正在传送中，忽略重复请求");
                return;
            }

            if (string.IsNullOrEmpty(targetScene))
            {
                ModLogger.LogWarning("[虫洞科技] 目标场景为空");
                return;
            }

            // 冷却检查（与游戏传送仪一致）
            if (!CanTeleport)
            {
                ModLogger.Log("[虫洞科技] 传送冷却中...");
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
            ModLogger.LogWarning($"[虫洞科技] 开始传送: 场景={targetScene}, 位置={targetPosition}");

            // 播放虫洞特效
            PlayWormholeEffect();

            // 异步加载目标场景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);

            // 等待场景加载完成
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // 场景加载完成，设置角色位置（使用记录的坐标）
            CharacterMainControl character = CharacterMainControl.Main;
            if (character != null)
            {
                // 使用 SetPosition 而不是直接设置 transform.position
                // SetPosition 会处理地面约束暂停和速度清零（与游戏传送仪一致）
                character.SetPosition(targetPosition);
                character.transform.rotation = targetRotation;

                ModLogger.Log($"[虫洞科技] 传送成功: {targetPosition}");
                ShowMessage("虫洞回溯成功！");
            }
            else
            {
                ModLogger.LogWarning("[虫洞科技] 找不到玩家角色");
            }

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
                ModLogger.Log("[虫洞科技] 传送冷却中...");
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
                ModLogger.LogWarning("[虫洞科技] 没有有效的传送数据");
                return;
            }

            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter == null)
            {
                ModLogger.LogWarning("[虫洞科技] 找不到主角");
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

            ModLogger.Log("[虫洞科技] 传送完成");
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
