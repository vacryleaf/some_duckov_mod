using UnityEngine;
using ItemStatsSystem;

namespace WormholeTechMod
{
    /// <summary>
    /// 虫洞徽章闪避触发器
    /// 继承 EffectTrigger，检测玩家受伤并触发闪避效果
    /// </summary>
    public class WormholeDodgeTrigger : EffectTrigger
    {
        // 单个徽章的不闪避概率（90%不闪避，即10%闪避）
        private const float SINGLE_BADGE_FAIL_RATE = 0.9f;

        // 徽章物品TypeID
        private const int BADGE_TYPE_ID = 990004;

        // 玩家的Health组件引用
        private Health targetHealth;

        // 上次触发效果的时间（用于防止短时间内多次触发）
        private float lastTriggerTime = 0f;
        private const float TRIGGER_COOLDOWN = 0.3f;

        /// <summary>
        /// 注册事件监听
        /// </summary>
        protected override void OnMasterSetTargetItem(Effect effect, Item item)
        {
            RegisterEvents();
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        protected override void OnDisable()
        {
            UnregisterEvents();
            base.OnDisable();
        }

        /// <summary>
        /// 注册玩家受伤事件
        /// </summary>
        private void RegisterEvents()
        {
            UnregisterEvents();

            if (base.Master == null || base.Master.Item == null)
            {
                return;
            }

            // 获取物品绑定的角色
            CharacterMainControl character = base.Master.Item.GetCharacterMainControl();
            if (character == null)
            {
                return;
            }

            targetHealth = character.Health;
            if (targetHealth != null)
            {
                targetHealth.OnHurtEvent.AddListener(OnTookDamage);
                ModLogger.Log("[虫洞徽章] 已注册玩家受伤事件");
            }
        }

        /// <summary>
        /// 取消注册玩家受伤事件
        /// </summary>
        private void UnregisterEvents()
        {
            if (targetHealth != null)
            {
                targetHealth.OnHurtEvent.RemoveListener(OnTookDamage);
                targetHealth = null;
            }
        }

        /// <summary>
        /// 玩家受伤回调
        /// </summary>
        private void OnTookDamage(DamageInfo damageInfo)
        {
            // 检查冷却
            if (Time.time - lastTriggerTime < TRIGGER_COOLDOWN)
            {
                return;
            }

            // 获取徽章数量
            int badgeCount = GetBadgeCount();
            if (badgeCount <= 0)
            {
                return;
            }

            // 获取伤害值
            float damage = damageInfo.finalDamage;
            if (damage <= 0)
            {
                return;
            }

            // 计算闪避概率并判定
            float dodgeChance = CalculateDodgeChance(badgeCount);

            if (UnityEngine.Random.value < dodgeChance)
            {
                lastTriggerTime = Time.time;

                // 触发闪避效果（positive = true 表示成功闪避）
                base.Trigger(true);

                // Debug.Log($"[虫洞徽章] 触发闪避！伤害: {damage:F1}, 徽章数: {badgeCount}, 闪避率: {dodgeChance * 100:F1}%");
            }
        }

        /// <summary>
        /// 获取玩家背包中的徽章数量
        /// </summary>
        private int GetBadgeCount()
        {
            if (base.Master == null || base.Master.Item == null)
            {
                return 0;
            }

            try
            {
                // 获取物品所在角色的背包
                CharacterMainControl character = base.Master.Item.GetCharacterMainControl();
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
            catch (System.Exception e)
            {
                Debug.LogWarning($"[虫洞徽章] 获取徽章数量失败: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 计算闪避概率
        /// 多个徽章乘法叠加：1 - (0.9 ^ 徽章数量)
        /// </summary>
        private float CalculateDodgeChance(int badgeCount)
        {
            if (badgeCount <= 0) return 0f;
            return 1f - Mathf.Pow(SINGLE_BADGE_FAIL_RATE, badgeCount);
        }
    }
}
