using UnityEngine;
using ItemStatsSystem;
using System.Collections.Generic;

namespace WormholeTechMod
{
    /// <summary>
    /// 虫洞徽章管理�?
    /// 直接监听玩家受伤事件，计算闪避概�?
    /// 不依�?Effect 系统
    /// </summary>
    public class WormholeBadgeManager : MonoBehaviour
    {
        // 单个徽章的不闪避概率�?0%不闪避，�?0%闪避�?
        private const float SINGLE_BADGE_FAIL_RATE = 0.9f;

        // 最多生效的徽章数量
        private const int MAX_ACTIVE_BADGES = 5;

        // 徽章物品TypeID
        public const int BADGE_TYPE_ID = 990004;

        // 上次触发效果的时�?
        private float lastTriggerTime = 0f;
        private const float TRIGGER_COOLDOWN = 0.5f;

        // 是否已注册事�?
        private bool eventsRegistered = false;

        // 单例
        private static WormholeBadgeManager _instance;
        public static WormholeBadgeManager Instance => _instance;

        void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnregisterEvents();
            _instance = null;
        }

        /// <summary>
        /// 场景加载完成时触�?
        /// </summary>
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // 场景加载后立即尝试注�?
            RegisterEventsImmediate();
        }

        /// <summary>
        /// 立即注册玩家受伤事件
        /// </summary>
        private bool RegisterEventsImmediate()
        {
            if (eventsRegistered)
            {
                return true;
            }

            UnregisterEvents();

            CharacterMainControl character = CharacterMainControl.Main;
            if (character == null)
            {
                return false;
            }

            // 获取 DamageReceiver 组件
            DamageReceiver damageReceiver = character.GetComponentInChildren<DamageReceiver>();
            if (damageReceiver == null)
            {
                return false;
            }

            // 注册 DamageReceiver.OnHurtEvent 事件
            damageReceiver.OnHurtEvent.AddListener(OnPlayerTookDamage);
            eventsRegistered = true;
            return true;
        }

        /// <summary>
        /// 取消注册玩家受伤事件
        /// </summary>
        private void UnregisterEvents()
        {
            CharacterMainControl character = CharacterMainControl.Main;
            if (character != null)
            {
                DamageReceiver damageReceiver = character.GetComponentInChildren<DamageReceiver>();
                if (damageReceiver != null)
                {
                    damageReceiver.OnHurtEvent.RemoveListener(OnPlayerTookDamage);
                }
            }
            eventsRegistered = false;
        }

        /// <summary>
        /// 玩家受伤回调（DamageReceiver.OnHurtEvent�?
        /// </summary>
        private void OnPlayerTookDamage(DamageInfo damageInfo)
        {
            // 检查冷�?
            if (Time.time - lastTriggerTime < TRIGGER_COOLDOWN)
            {
                return;
            }

            // 使用 damageValue 作为原始伤害�?
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

            // 计算闪避概率并判定（最�?0%�?
            float dodgeChance = CalculateDodgeChance(badgeCount);

            if (UnityEngine.Random.value < dodgeChance)
            {
                lastTriggerTime = Time.time;

                // 闪避成功 - 设置无敌
                CharacterMainControl character = CharacterMainControl.Main;
                if (character != null && character.Health != null)
                {
                    character.Health.SetInvincible(true);
                    // 下一帧恢复无敌状�?
                    StartCoroutine(ResetInvincible());
                }

                // 显示闪避文字
                character?.PopText("虫洞闪避!");
            }
        }

        /// <summary>
        /// 恢复无敌状�?
        /// </summary>
        private System.Collections.IEnumerator ResetInvincible()
        {
            yield return null; // 等待一�?
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
                if (character == null || character.CharacterItem == null)
                {
                    return 0;
                }

                Inventory inventory = character.CharacterItem.Inventory;
                if (inventory == null) return 0;

                int count = 0;
                foreach (Item item in inventory)
                {
                    if (item != null && item.TypeID == BADGE_TYPE_ID)
                    {
                        count += item.Stackable ? item.StackCount : 1;
                    }
                }

                return count;
            }
            catch (System.Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 计算闪避概率
        /// 多个徽章乘法叠加�? - (0.9 ^ 徽章数量)
        /// 最�?个徽章生�?
        /// </summary>
        private float CalculateDodgeChance(int badgeCount)
        {
            if (badgeCount <= 0) return 0f;
            // 限制最�?MAX_ACTIVE_BADGES 个徽章生�?
            int effectiveCount = Mathf.Min(badgeCount, MAX_ACTIVE_BADGES);
            return 1f - Mathf.Pow(SINGLE_BADGE_FAIL_RATE, effectiveCount);
        }
    }
}

