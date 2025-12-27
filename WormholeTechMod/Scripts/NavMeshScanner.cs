using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace WormholeTechMod
{
    /// <summary>
    /// NavMesh 扫描�?
    /// 按需计算 NavMesh 位置，不再预扫描
    /// </summary>
    public class NavMeshScanner : MonoBehaviour
    {
        private static NavMeshScanner _instance;
        public static NavMeshScanner Instance => _instance;

        private string currentSceneName = string.Empty;

        void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            _instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #region 公开接口

        /// <summary>
        /// 初始化扫描器并开始监听场景加�?
        /// </summary>
        public void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            }

        /// <summary>
        /// 获取当前场景名称
        /// </summary>
        public string GetCurrentSceneName()
        {
            string activeSceneName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(currentSceneName))
            {
                currentSceneName = activeSceneName;
            }
            else if (activeSceneName != currentSceneName)
            {
                currentSceneName = activeSceneName;
            }
            return currentSceneName;
        }

        /// <summary>
        /// 获取当前场景所有可行走位置（空列表，不再预扫描�?
        /// </summary>
        public List<Vector3> GetCurrentScenePositions()
        {
            return new List<Vector3>();
        }

        /// <summary>
        /// 获取当前场景的随机可行走位置
        /// </summary>
        public Vector3 GetRandomPosition(Vector3 originPosition, float minDistance = 10f, float maxDistance = 200f)
        {
            return Vector3.zero;
        }

        /// <summary>
        /// 获取场景中心�?
        /// </summary>
        public Vector3 GetSceneCenterPoint(string sceneName = null)
        {
            return Vector3.zero;
        }

        /// <summary>
        /// 检查场景是否已扫描
        /// </summary>
        public bool IsSceneScanned(string sceneName = null)
        {
            return false;
        }

        /// <summary>
        /// 清除指定场景的扫描数�?
        /// </summary>
        public void ClearSceneData(string sceneName = null)
        {
        }

        /// <summary>
        /// 获取所有已扫描场景的名�?
        /// </summary>
        public List<string> GetScannedSceneNames()
        {
            return new List<string>();
        }

        /// <summary>
        /// 获取指定场景的可行走位置数量
        /// </summary>
        public int GetPositionCount(string sceneName = null)
        {
            return 0;
        }

        #endregion

        #region 场景事件

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentSceneName = scene.name;
            }

        #endregion
    }
}

