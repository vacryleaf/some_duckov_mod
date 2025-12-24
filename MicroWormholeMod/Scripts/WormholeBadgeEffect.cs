using UnityEngine;
using ItemStatsSystem;
using System;

namespace MicroWormholeMod
{
    /// <summary>
    /// 虫洞徽章被动效果组件
    /// 直接订阅玩家 Health.OnHurt 事件，根据徽章数量计算闪避概率
    /// 多个徽章进行乘法叠加：1 - (0.9 ^ 徽章数量)
    /// </summary>
    public class WormholeBadgeEffect : MonoBehaviour
    {
        // 徽章物品TypeID
        private const int BADGE_TYPE_ID = 990004;

        // 单个徽章的不闪避概率（90%不闪避，即10%闪避）
        private const float SINGLE_BADGE_FAIL_RATE = 0.9f;

        // 玩家的Health组件引用
        private Health playerHealth;

        // 事件委托引用（用于取消订阅）
        private Action<Health, DamageInfo> onHurtHandler;

        // 上次触发效果的时间（用于防止短时间内多次触发）
        private float lastTriggerTime = 0f;
        private const float TRIGGER_COOLDOWN = 0.3f;

        // 是否已经订阅了事件
        private bool isSubscribed = false;

        void Start()
        {
            Debug.Log("[虫洞徽章] 效果组件初始化");
            // 创建事件处理委托
            onHurtHandler = OnPlayerHurt;
            // 延迟订阅，确保玩家角色已加载
            StartCoroutine(DelayedSubscribe());
        }

        /// <summary>
        /// 延迟订阅玩家受伤事件
        /// </summary>
        private System.Collections.IEnumerator DelayedSubscribe()
        {
            yield return new WaitForSeconds(1f);
            TrySubscribeToPlayerHurt();
        }

        void Update()
        {
            // 定期检查是否需要重新订阅（场景切换后）
            if (!isSubscribed || playerHealth == null)
            {
                TrySubscribeToPlayerHurt();
            }
        }

        /// <summary>
        /// 尝试订阅玩家的受伤事件
        /// </summary>
        private void TrySubscribeToPlayerHurt()
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter == null)
            {
                return;
            }

            // 直接获取 Health 组件
            Health health = mainCharacter.GetComponent<Health>();
            if (health == null)
            {
                // 尝试从子对象获取
                health = mainCharacter.GetComponentInChildren<Health>();
            }

            if (health == null)
            {
                return;
            }

            // 如果是新的 Health 组件，取消旧订阅并重新订阅
            if (playerHealth != health)
            {
                if (playerHealth != null && isSubscribed)
                {
                    // 取消旧订阅
                    Health.OnHurt -= onHurtHandler;
                }

                playerHealth = health;

                // 订阅静态事件 Health.OnHurt
                Health.OnHurt += onHurtHandler;
                isSubscribed = true;

                Debug.Log("[虫洞徽章] 已订阅玩家受伤事件（直接订阅）");
            }
        }

        /// <summary>
        /// 玩家受伤事件处理
        /// </summary>
        private void OnPlayerHurt(Health health, DamageInfo damageInfo)
        {
            // 检查是否是玩家的 Health
            if (health != playerHealth)
            {
                return;
            }

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

            // 获取伤害值（使用最终伤害值）
            float damage = damageInfo.finalDamage;
            if (damage <= 0)
            {
                return;
            }

            // 计算闪避概率
            float dodgeChance = CalculateDodgeChance(badgeCount);

            // 判定是否闪避
            if (UnityEngine.Random.value < dodgeChance)
            {
                lastTriggerTime = Time.time;

                // 恢复伤害（在受伤后立即恢复）
                HealDamage(damage);

                // 显示特效和提示
                ShowDodgeEffect();

                Debug.Log($"[虫洞徽章] 触发！闪避了 {damage:F1} 点伤害（徽章数: {badgeCount}, 闪避率: {dodgeChance * 100:F1}%）");
            }
        }

        /// <summary>
        /// 获取玩家背包中的徽章数量
        /// 背包访问路径：CharacterMainControl.CharacterItem.Inventory
        /// </summary>
        private int GetBadgeCount()
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter == null) return 0;

            try
            {
                // 获取角色物品
                Item characterItem = mainCharacter.CharacterItem;
                if (characterItem == null)
                {
                    return 0;
                }

                // 从 CharacterItem 获取 Inventory
                Inventory inventory = characterItem.Inventory;
                if (inventory == null) return 0;

                // 计算背包中徽章的数量
                int count = 0;
                foreach (Item item in inventory)
                {
                    if (item != null && item.TypeID == BADGE_TYPE_ID)
                    {
                        // 如果可堆叠，加上堆叠数
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
        /// 1个徽章: 10%
        /// 2个徽章: 19%
        /// 3个徽章: 27.1%
        /// 5个徽章: 40.95%
        /// </summary>
        private float CalculateDodgeChance(int badgeCount)
        {
            if (badgeCount <= 0) return 0f;
            return 1f - Mathf.Pow(SINGLE_BADGE_FAIL_RATE, badgeCount);
        }

        /// <summary>
        /// 恢复伤害
        /// </summary>
        private void HealDamage(float damage)
        {
            if (playerHealth == null) return;

            // 直接调用 Health.AddHealth 方法
            playerHealth.AddHealth(damage);
        }

        /// <summary>
        /// 显示闪避特效和提示
        /// </summary>
        private void ShowDodgeEffect()
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter == null) return;

            // 显示文字提示
            mainCharacter.PopText("虫洞徽章：伤害无效化！");

            // 创建粒子特效
            CreateDodgeParticles(mainCharacter.transform.position);
        }

        /// <summary>
        /// 创建闪避特效
        /// </summary>
        private void CreateDodgeParticles(Vector3 position)
        {
            GameObject effectObj = new GameObject("BadgeDodgeEffect");
            effectObj.transform.position = position + Vector3.up * 1f;

            ParticleSystem particles = effectObj.AddComponent<ParticleSystem>();

            var main = particles.main;
            main.startColor = new Color(0.2f, 0.8f, 1f, 0.8f); // 青色
            main.startSize = 0.3f;
            main.startLifetime = 0.8f;
            main.startSpeed = 3f;
            main.duration = 0.3f;
            main.loop = false;

            var emission = particles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 20)
            });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            // 添加颜色渐变
            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.2f, 0.8f, 1f), 0f),
                    new GradientColorKey(new Color(0.6f, 0.3f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            particles.Play();
            Destroy(effectObj, 2f);
        }

        void OnDestroy()
        {
            // 取消订阅事件
            if (isSubscribed && onHurtHandler != null)
            {
                Health.OnHurt -= onHurtHandler;
                isSubscribed = false;
            }
            Debug.Log("[虫洞徽章] 效果组件已销毁");
        }
    }
}
