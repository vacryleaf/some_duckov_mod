using UnityEngine;
using ItemStatsSystem;
using System.Collections.Generic;
using Duckov;

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

        // Health 组件引用（用于注册受伤事件）
        private Health _targetHealth;

        void Awake()
        {
            _instance = this;
        }

        void Start()
        {
        }

        void OnDestroy()
        {
            UnregisterEvents();
            _instance = null;
        }

        /// <summary>
        /// 注册玩家受伤事件（使用 Health.OnHurtEvent，这是游戏标准方式）
        /// </summary>
        public void RegisterDamageEvent()
        {

            // 如果已经注册过
            if (_targetHealth != null)
            {
                return;
            }

            // 通过 CharacterMainControl.Main 获取
            CharacterMainControl character = CharacterMainControl.Main;

            ModLogger.Log($"[虫洞徽章] CharacterMainControl: {character.name}");

            // 获取 Health
            Health health = character.Health;

            ModLogger.Log($"[虫洞徽章] Health 组件，注册 OnHurtEvent");
            
            // 取消旧的事件注册
            if (_targetHealth != null)
            {
                try
                {
                    _targetHealth.OnHurtEvent.RemoveListener(OnPlayerTookDamage);
                }
                catch (System.Exception e)
                {
                    ModLogger.LogWarning($"[虫洞徽章] 移除旧事件失败: {e.Message}");
                }
            }

            // 注册新事件
            try
            {
                health.OnHurtEvent.AddListener(OnPlayerTookDamage);
                _targetHealth = health;
                ModLogger.Log("[虫洞徽章] 事件注册成功！");
            }
            catch (System.Exception e)
            {
                ModLogger.LogWarning($"[虫洞徽章] 注册 OnHurtEvent 失败: {e.Message}");
                StartCoroutine(RetryRegister());
            }
        }

        /// <summary>
        /// 延迟重试注册
        /// </summary>
        private System.Collections.IEnumerator RetryRegister()
        {
            int maxRetries = 10;
            for (int i = 0; i < maxRetries; i++)
            {
                yield return new WaitForSeconds(0.2f);
                
                CharacterMainControl character = CharacterMainControl.Main;
                if (character != null && character.Health != null)
                {
                    ModLogger.Log($"[徽章] 第 {i + 1} 次重试成功");
                    RegisterDamageEvent();
                    yield break;
                }
            }
            ModLogger.LogWarning("[徽章] 重试注册失败，达到最大次数");
        }

        /// <summary>
        /// 取消注册玩家受伤事件
        /// </summary>
        private void UnregisterEvents()
        {
            try
            {
                if (_targetHealth != null)
                {
                    _targetHealth.OnHurtEvent.RemoveListener(OnPlayerTookDamage);
                    _targetHealth = null;
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

