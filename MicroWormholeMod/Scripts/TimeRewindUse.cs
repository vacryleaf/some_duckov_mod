using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace MicroWormholeMod
{
    /// <summary>
    /// 时空回溯使用行为
    /// 记录玩家状态，可回溯到之前的状态
    /// </summary>
    public class TimeRewindUse : MonoBehaviour
    {
        // ========== 配置参数 ==========
        [Header("回溯设置")]
        public float rewindDuration = 5f;
        public float healthCostRatio = 0.15f;
        public float cooldownTime = 30f;
        public int maxRecordCount = 50;

        // ========== 状态记录 ==========
        private struct PlayerState
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        private List<PlayerState> stateHistory = new List<PlayerState>();
        private float lastRewindTime = -999f;
        private bool isRewinding = false;

        // ========== 引用 ==========
        private CharacterMainControl character;

        void Start()
        {
            character = GetComponentInParent<CharacterMainControl>();
        }

        void Update()
        {
            if (isRewinding) return;
            if (character == null) return;
            RecordPlayerState();
        }

        private void RecordPlayerState()
        {
            if (stateHistory.Count >= maxRecordCount)
            {
                stateHistory.RemoveAt(0);
            }

            stateHistory.Add(new PlayerState
            {
                position = character.transform.position,
                rotation = character.transform.rotation
            });
        }

        public bool CanRewind()
        {
            if (isRewinding) return false;
            float timeSinceLastRewind = Time.time - lastRewindTime;
            if (timeSinceLastRewind < cooldownTime) return false;
            if (stateHistory.Count < 10) return false;
            return true;
        }

        public bool ExecuteRewind()
        {
            if (!CanRewind()) return false;

            if (stateHistory.Count == 0) return false;

            PlayerState targetState = stateHistory[0];

            StartCoroutine(RewindAnimation(targetState));
            lastRewindTime = Time.time;
            return true;
        }

        private IEnumerator RewindAnimation(PlayerState targetState)
        {
            isRewinding = true;

            // 特效
            PlayRewindEffect();

            // 延迟
            yield return new WaitForSeconds(0.5f);

            // 执行回溯
            if (character != null)
            {
                character.transform.position = targetState.position;
                character.transform.rotation = targetState.rotation;
            }

            stateHistory.Clear();
            isRewinding = false;

            ModLogger.Log(string.Format("[时空回溯] 已回溯到 {0} 秒前", rewindDuration));
        }

        private void PlayRewindEffect()
        {
            if (character == null) return;

            GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.transform.position = character.transform.position;
            effect.transform.localScale = Vector3.one * 3f;

            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0f, 0.8f, 1f, 0.5f);
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3000;
            effect.GetComponent<Renderer>().material = mat;

            Destroy(effect, 0.5f);
        }

        public float GetCooldownRemaining()
        {
            return Mathf.Max(0, cooldownTime - (Time.time - lastRewindTime));
        }

        public float GetMaxRewindTime()
        {
            return rewindDuration;
        }

        void OnDisable()
        {
            isRewinding = false;
        }
    }
}
