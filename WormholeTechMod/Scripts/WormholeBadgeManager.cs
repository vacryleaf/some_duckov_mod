using UnityEngine;
using ItemStatsSystem;
using System.Collections.Generic;

namespace WormholeTechMod
{
    /// <summary>
    /// 虫洞徽章管理器
    /// 直接监听玩家受伤事件，计算闪避概率
    /// 不依赖 Effect 系统
    /// </summary>
    public class WormholeBadgeManager : MonoBehaviour
    {
        // 单个徽章的不闪避概率 90%不闪避，10%闪避
        private const float SINGLE_BADGE_FAIL_RATE = 0.9f;

        // 最多生效的徽章数量
        private const int MAX_ACTIVE_BADGES = 5;

        // 徽章物品TypeID
        public const int BADGE_TYPE_ID = 990004;

        // 上次触发效果的时间
        private float lastTriggerTime = 0f;
        private const float TRIGGER_COOLDOWN = 0.5f;

        // 无敌帧持续时间
        private const float IFRAME_DURATION = 0.3f;

        // 单例
        private static WormholeBadgeManager _instance;
        public static WormholeBadgeManager Instance => _instance;

        void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            // 场景加载完成后注册事件
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            // 立即尝试注册（如果是加载完成后才添加的组件）
            StartCoroutine(RegisterEventsDelayed());
        }

        void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnregisterEvents();
            _instance = null;
        }

        /// <summary>
        /// 场景加载完成时触发
        /// </summary>
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // 重置注册状态，确保重新注册
            // 场景加载后 CharacterMainControl 已准备好
            ModLogger.Log($"[徽章] 场景加载完成: {scene.name}，开始注册事件...");
            StartCoroutine(RegisterEventsDelayed());
        }

        /// <summary>
        /// 延迟注册玩家受伤事件（确保 CharacterMainControl 已准备好）
        /// </summary>
        private System.Collections.IEnumerator RegisterEventsDelayed()
        {
            // 等待一帧，确保 CharacterMainControl 初始化完成
            yield return null;

            // 多次尝试注册，直到成功
            int maxAttempts = 10;
            for (int i = 0; i < maxAttempts; i++)
            {
                if (RegisterEventsImmediate())
                {
                    ModLogger.Log($"[徽章] 事件注册成功 (尝试 {i + 1} 次)");
                    yield break;
                }
                yield return new UnityEngine.WaitForSeconds(0.1f);
            }
            ModLogger.LogWarning("[徽章] 事件注册失败，已达最大重试次数");
        }

        /// <summary>
        /// 立即注册玩家受伤事件
        /// </summary>
        private bool RegisterEventsImmediate()
        {
            try
            {
                // 使用 CharacterMainControl.Main 获取角色
                CharacterMainControl character = CharacterMainControl.Main;
                if (character == null)
                {
                    // 尝试从场景中查找
                    character = UnityEngine.Object.FindObjectOfType<CharacterMainControl>();
                }

                if (character == null)
                {
                    return false;
                }

                // 获取 DamageReceiver 组件（优先使用 mainDamageReceiver）
                DamageReceiver damageReceiver = character.mainDamageReceiver;
                if (damageReceiver == null)
                {
                    damageReceiver = character.GetComponentInChildren<DamageReceiver>();
                }

                if (damageReceiver == null)
                {
                    return false;
                }

                // 注册 DamageReceiver.OnHurtEvent 事件
                damageReceiver.OnHurtEvent.AddListener(OnPlayerTookDamage);
                return true;
            }
            catch (System.Exception e)
            {
                ModLogger.LogWarning($"[徽章] 注册事件失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 取消注册玩家受伤事件
        /// </summary>
        private void UnregisterEvents()
        {
            try
            {
                CharacterMainControl character = CharacterMainControl.Main;
                if (character == null)
                {
                    character = UnityEngine.Object.FindObjectOfType<CharacterMainControl>();
                }

                if (character != null)
                {
                    DamageReceiver damageReceiver = character.mainDamageReceiver;
                    if (damageReceiver == null)
                    {
                        damageReceiver = character.GetComponentInChildren<DamageReceiver>();
                    }
                    if (damageReceiver != null)
                    {
                        damageReceiver.OnHurtEvent.RemoveListener(OnPlayerTookDamage);
                    }
                }
            }
            catch (System.Exception e)
            {
                ModLogger.LogWarning($"[徽章] 取消注册事件失败: {e.Message}");
            }
        }

        /// <summary>
        /// 玩家受伤回调（DamageReceiver.OnHurtEvent）
        /// </summary>
        private void OnPlayerTookDamage(DamageInfo damageInfo)
        {
            // 检查冷却
            if (Time.time - lastTriggerTime < TRIGGER_COOLDOWN)
            {
                return;
            }

            // 使用 damageValue 作为原始伤害值
            float originalDamage = damageInfo.damageValue;
            if (originalDamage <= 0)
            {
                return;
            }

            // 获取徽章数量
            int badgeCount = GetBadgeCount();
            if (badgeCount <= 0)
            {
                return;
            }

            // 计算闪避概率并判定
            float dodgeChance = CalculateDodgeChance(badgeCount);

            if (UnityEngine.Random.value < dodgeChance)
            {
                lastTriggerTime = Time.time;

                // 闪避成功 - 设置无敌帧
                CharacterMainControl character = CharacterMainControl.Main;
                if (character != null && character.Health != null)
                {
                    character.Health.SetInvincible(true);
                    // 持续 IFRAME_DURATION 秒无敌帧
                    StartCoroutine(ResetInvincible());

                    ModLogger.Log($"[徽章] 闪避成功！概率={dodgeChance:P1}，无敌帧={IFRAME_DURATION}秒");
                }

                // 显示闪避文字
                character?.PopText("虫洞闪避!");
            }
        }

        /// <summary>
        /// 恢复无敌状态
        /// </summary>
        private System.Collections.IEnumerator ResetInvincible()
        {
            yield return new WaitForSeconds(IFRAME_DURATION);
            CharacterMainControl character = CharacterMainControl.Main;
            if (character != null && character.Health != null)
            {
                character.Health.SetInvincible(false);
            }
        }

        /// <summary>
        /// 获取玩家背包中的徽章数量
        /// </summary>
        private int GetBadgeCount()
        {
            try
            {
                CharacterMainControl character = CharacterMainControl.Main;
                if (character == null)
                {
                    return 0;
                }

                // 使用反射获取 CharacterItem
                var characterItemField = typeof(CharacterMainControl).GetField("characterItem",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var characterItem = characterItemField?.GetValue(character) as object;

                if (characterItem == null)
                {
                    return 0;
                }

                var inventoryField = characterItem.GetType().GetField("Inventory",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var inventory = inventoryField?.GetValue(characterItem) as object;

                if (inventory == null)
                {
                    return 0;
                }

                // 遍历背包
                int count = 0;
                foreach (var item in inventory as System.Collections.IEnumerable)
                {
                    if (item == null) continue;

                    var typeIdField = item.GetType().GetField("TypeID",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (typeIdField == null) continue;

                    int typeId = (int)typeIdField.GetValue(item);
                    if (typeId == BADGE_TYPE_ID)
                    {
                        // 检查是否可堆叠
                        var stackableField = item.GetType().GetField("Stackable",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        bool stackable = stackableField != null && (bool)stackableField.GetValue(item);

                        if (stackable)
                        {
                            var stackCountField = item.GetType().GetField("StackCount",
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            int stackCount = (int)(stackCountField?.GetValue(item) ?? 1);
                            count += stackCount;
                        }
                        else
                        {
                            count += 1;
                        }
                    }
                }

                return count;
            }
            catch (System.Exception e)
            {
                ModLogger.LogWarning($"[徽章] 获取徽章数量失败: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 计算闪避概率
        /// 多个徽章乘法叠加：1 - (0.9 ^ 徽章数量)
        /// 最多 MAX_ACTIVE_BADGES 个徽章生效
        /// </summary>
        private float CalculateDodgeChance(int badgeCount)
        {
            if (badgeCount <= 0) return 0f;
            // 限制最多 MAX_ACTIVE_BADGES 个徽章生效
            int effectiveCount = Mathf.Min(badgeCount, MAX_ACTIVE_BADGES);
            return 1f - Mathf.Pow(SINGLE_BADGE_FAIL_RATE, effectiveCount);
        }
    }
}

