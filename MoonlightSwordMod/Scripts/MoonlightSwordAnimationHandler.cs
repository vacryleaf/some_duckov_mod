using UnityEngine;

namespace MoonlightSwordMod
{
    /// <summary>
    /// 动画事件处理器
    /// 挂载在角色对象上，处理动画事件回调
    /// </summary>
    public class MoonlightSwordAnimationHandler : MonoBehaviour
    {
        [Header("音效")]
        public AudioClip swooshSound;           // 挥刀音效
        public AudioClip airCutSound;           // 破空音效
        public AudioClip chargeSound;           // 充能音效
        public AudioClip dashSound;             // 冲刺音效
        public AudioClip releaseSound;          // 释放音效

        [Header("特效Prefab")]
        public GameObject slashTrailPrefab;     // 挥击轨迹特效
        public GameObject chargeEffectPrefab;   // 充能特效
        public GameObject dashTrailPrefab;      // 冲刺轨迹特效

        [Header("引用")]
        private MoonlightSwordAttack attackScript;
        private AudioSource audioSource;
        private GameObject chargeEffect;        // 当前充能特效

        /// <summary>
        /// 初始化
        /// </summary>
        void Start()
        {
            attackScript = GetComponent<MoonlightSwordAttack>();
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            Debug.Log("[名刀月影] 动画事件处理器初始化完成");
        }

        /// <summary>
        /// 普通攻击伤害判定（动画事件调用）
        /// </summary>
        /// <param name="attackType">攻击类型: "ForwardSlash" 或 "BackhandSlash"</param>
        public void OnAttackDamageFrame(string attackType)
        {
            Debug.Log($"[名刀月影] 触发伤害判定: {attackType}");

            if (attackScript != null)
            {
                attackScript.PerformMeleeDamage(attackScript.normalDamage);
            }
        }

        /// <summary>
        /// 播放挥刀音效（动画事件调用）
        /// </summary>
        /// <param name="soundId">音效ID: 1=正手, 2=反手</param>
        public void PlaySlashSound(int soundId)
        {
            if (audioSource != null && swooshSound != null)
            {
                float pitch = soundId == 1 ? 1.0f : 1.1f;
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(swooshSound, 0.8f);
            }
        }

        /// <summary>
        /// 播放破空音效（动画事件调用）
        /// </summary>
        public void PlayAirCutSound()
        {
            if (audioSource != null && airCutSound != null)
            {
                audioSource.PlayOneShot(airCutSound, 0.6f);
            }
        }

        /// <summary>
        /// 显示挥击特效（动画事件调用）
        /// </summary>
        /// <param name="effectType">特效类型: "ForwardTrail" 或 "BackhandTrail"</param>
        public void ShowSlashEffect(string effectType)
        {
            Debug.Log($"[名刀月影] 显示特效: {effectType}");

            if (slashTrailPrefab != null)
            {
                // 在刀身位置生成轨迹特效
                Vector3 swordPosition = transform.position + transform.forward + Vector3.up;
                GameObject effect = Instantiate(slashTrailPrefab, swordPosition, transform.rotation, transform);

                // 自动销毁
                Destroy(effect, 1f);
            }
        }

        /// <summary>
        /// 开始充能特效（动画事件调用）
        /// </summary>
        public void StartChargeEffect()
        {
            Debug.Log("[名刀月影] 开始充能");

            // 播放充能音效
            if (audioSource != null && chargeSound != null)
            {
                audioSource.clip = chargeSound;
                audioSource.loop = true;
                audioSource.volume = 0.7f;
                audioSource.Play();
            }

            // 生成充能特效
            if (chargeEffectPrefab != null)
            {
                Vector3 effectPosition = transform.position + Vector3.up;
                chargeEffect = Instantiate(chargeEffectPrefab, effectPosition, Quaternion.identity, transform);
            }
        }

        /// <summary>
        /// 停止充能特效
        /// </summary>
        public void StopChargeEffect()
        {
            Debug.Log("[名刀月影] 停止充能");

            // 停止充能音效
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == chargeSound)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }

            // 销毁充能特效
            if (chargeEffect != null)
            {
                Destroy(chargeEffect);
                chargeEffect = null;
            }
        }

        /// <summary>
        /// 开始冲刺移动（动画事件调用）
        /// </summary>
        /// <param name="distance">冲刺距离</param>
        public void StartDashMovement(float distance)
        {
            Debug.Log($"[名刀月影] 开始冲刺: {distance}米");

            // 停止充能特效
            StopChargeEffect();

            // 播放冲刺音效
            if (audioSource != null && dashSound != null)
            {
                audioSource.PlayOneShot(dashSound, 0.9f);
            }

            // 生成冲刺轨迹特效
            if (dashTrailPrefab != null)
            {
                GameObject trail = Instantiate(dashTrailPrefab, transform.position, Quaternion.identity, transform);
                Destroy(trail, 1f);
            }

            // 执行冲刺移动
            if (attackScript != null)
            {
                StartCoroutine(attackScript.DashMovement(distance));
            }
        }

        /// <summary>
        /// 发射剑气（动画事件调用）
        /// </summary>
        public void LaunchSwordAura()
        {
            Debug.Log("[名刀月影] 发射剑气");

            // 播放释放音效
            if (audioSource != null && releaseSound != null)
            {
                audioSource.PlayOneShot(releaseSound, 1.0f);
            }

            // 相机震动
            CameraShake(0.3f);

            // 发射剑气
            if (attackScript != null)
            {
                attackScript.LaunchSwordAura();
            }
        }

        /// <summary>
        /// 播放特殊音效（动画事件调用）
        /// </summary>
        /// <param name="soundType">音效类型</param>
        public void PlaySpecialSound(string soundType)
        {
            Debug.Log($"[名刀月影] 播放音效: {soundType}");

            // 根据类型播放对应音效
            AudioClip clip = null;
            switch (soundType)
            {
                case "Charge":
                    clip = chargeSound;
                    break;
                case "Dash":
                    clip = dashSound;
                    break;
                case "AuraRelease":
                    clip = releaseSound;
                    break;
            }

            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// 相机震动
        /// </summary>
        /// <param name="intensity">震动强度</param>
        public void CameraShake(float intensity)
        {
            // 尝试触发相机震动效果
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                var shakeComponent = mainCamera.GetComponent<CameraShake>();
                if (shakeComponent != null)
                {
                    shakeComponent.Shake(intensity, 0.2f);
                }
                else
                {
                    // 如果没有相机震动组件，创建一个简单的震动效果
                    StartCoroutine(SimpleCameraShake(mainCamera.transform, intensity, 0.2f));
                }
            }
        }

        /// <summary>
        /// 简单的相机震动效果
        /// </summary>
        private System.Collections.IEnumerator SimpleCameraShake(Transform cameraTransform, float intensity, float duration)
        {
            Vector3 originalPosition = cameraTransform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;

                cameraTransform.localPosition = originalPosition + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            cameraTransform.localPosition = originalPosition;
        }
    }

    /// <summary>
    /// 简单的相机震动组件
    /// 可以挂载到主相机上
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        private bool isShaking = false;

        /// <summary>
        /// 触发震动
        /// </summary>
        /// <param name="intensity">强度</param>
        /// <param name="duration">持续时间</param>
        public void Shake(float intensity, float duration)
        {
            if (!isShaking)
            {
                StartCoroutine(ShakeCoroutine(intensity, duration));
            }
        }

        private System.Collections.IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            isShaking = true;
            Vector3 originalPosition = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;

                transform.localPosition = originalPosition + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalPosition;
            isShaking = false;
        }
    }
}
