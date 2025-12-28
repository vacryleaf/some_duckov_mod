using UnityEngine;
using ItemStatsSystem;
using System.Collections.Generic;
using Duckov;
using Duckov.Utilities;

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

        // 无敌帧持续时间
        private const float IFRAME_DURATION = 0.1f;

        // 上次触发效果的时间
        private float lastTriggerTime = 0f;
        private const float TRIGGER_COOLDOWN = 0.5f;

        // 单例
        private static WormholeBadgeManager _instance;
        public static WormholeBadgeManager Instance => _instance;

        // DamageReceiver 组件引用（在扣血前触发）
        private DamageReceiver _targetDamageReceiver;

        // 真正的 Health 组件引用（用于设置 invincible）
        private Health _targetHealth;

        void Start()
        {
            _instance = this;

            ModLogger.Log("[虫洞徽章] 开始初始化...");

            // 使用 OnAfterLevelInitialized，CharacterMainControl 此时一定可用
            LevelManager.OnAfterLevelInitialized += RegisterDamageEvent;
            ModLogger.Log("[虫洞徽章] 已订阅 LevelManager.OnAfterLevelInitialized");
        }

        void OnDestroy()
        {
            LevelManager.OnAfterLevelInitialized -= RegisterDamageEvent;
            UnregisterEvents();
            _instance = null;
        }

        /// <summary>
        /// 注册玩家受伤事件（使用 DamageReceiver.OnHurtEvent，在扣血前触发）
        /// </summary>
        public void RegisterDamageEvent()
        {
            try
            {
                ModLogger.Log("[虫洞徽章] PlayerStorage.OnLoadingFinished. 开始注册受伤事件");

                // 如果已经注册过
                if (_targetDamageReceiver != null)
                {
                    ModLogger.Log("[虫洞徽章] 已注册受伤事件，无需重复注册");
                    return;
                }

                // 通过 CharacterMainControl.Main 获取
                CharacterMainControl character = CharacterMainControl.Main;
                ModLogger.Log($"[虫洞徽章] CharacterMainControl: {character.name}");

                // 获取 DamageReceiver（在 Health.Hurt 之前触发）
                var mainDamageReceiverField = typeof(CharacterMainControl).GetField("mainDamageReceiver",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                DamageReceiver damageReceiver = mainDamageReceiverField?.GetValue(character) as DamageReceiver;
                ModLogger.Log($"[虫洞徽章] DamageReceiver 组件，注册 OnHurtEvent");

                // 注册新事件（在扣血前触发，可以设置 invincible）
                damageReceiver.OnHurtEvent.AddListener(OnPlayerTookDamage);
                _targetDamageReceiver = damageReceiver;
                ModLogger.Log("[虫洞徽章] 事件注册成功！");
            }
            catch (System.Exception e)
            {
                ModLogger.LogWarning($"[虫洞徽章] 注册事件失败: {e.Message}");
            }
        }

        /// <summary>
        /// 取消注册玩家受伤事件
        /// </summary>
        private void UnregisterEvents()
        {
            try
            {
                if (_targetDamageReceiver != null)
                {
                    _targetDamageReceiver.OnHurtEvent.RemoveListener(OnPlayerTookDamage);
                    _targetDamageReceiver = null;
                }
                _targetHealth = null;
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
            ModLogger.Log($"玩家受伤，当前徽章数量={badgeCount}");
            if (badgeCount <= 0)
            {
                return;
            }

            // 计算闪避概率并判定
            float dodgeChance = CalculateDodgeChance(badgeCount);
            ModLogger.Log($"玩家受伤，闪避概率={dodgeChance:P1}");

            if (UnityEngine.Random.value < dodgeChance)
            {
                lastTriggerTime = Time.time;

                // 闪避成功 - 使用 DamageReceiver 持有的 Health 设置无敌帧
                if (_targetHealth != null)
                {
                    _targetHealth.SetInvincible(true);
                    // 持续 IFRAME_DURATION 秒无敌帧
                    StartCoroutine(ResetInvincible());

                    ModLogger.Log($"[徽章] 闪避成功！无敌帧={IFRAME_DURATION}秒");
                }

                // 显示闪避文字
                var character = CharacterMainControl.Main;
                character?.PopText("虫洞闪避!");
            }
        }

        /// <summary>
        /// 恢复无敌状态
        /// </summary>
        private System.Collections.IEnumerator ResetInvincible()
        {
            yield return new WaitForSeconds(IFRAME_DURATION);
            if (_targetHealth != null)
            {
                _targetHealth.SetInvincible(false);
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
                    ModLogger.Log("[徽章] GetBadgeCount: character == null");
                    return 0;
                }

                // 使用反射获取 CharacterItem
                var characterItemField = typeof(CharacterMainControl).GetField("characterItem",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var characterItem = characterItemField?.GetValue(character) as Item;

                if (characterItem == null)
                {
                    ModLogger.Log("[徽章] GetBadgeCount: characterItem == null");
                    return 0;
                }

                var inventory = characterItem.Inventory;

                if (inventory == null)
                {
                    ModLogger.Log("[徽章] GetBadgeCount: inventory == null");
                    return 0;
                }

                // 遍历背包
                int count = 0;
                int totalItems = 0;
                foreach (var item in inventory)
                {
                    totalItems++;
                    if (item.TypeID == BADGE_TYPE_ID)
                    {
                        count += item.StackCount;
                    }
                }

                ModLogger.Log($"[徽章] GetBadgeCount: totalItems={totalItems}, badgeCount={count}");
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

