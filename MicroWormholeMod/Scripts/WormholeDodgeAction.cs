using UnityEngine;
using ItemStatsSystem;

namespace MicroWormholeMod
{
    /// <summary>
    /// 虫洞徽章闪避动作
    /// 继承 EffectAction，执行闪避效果（恢复伤害并显示特效）
    /// </summary>
    public class WormholeDodgeAction : EffectAction
    {
        // 缓存玩家的 Health 组件
        private Health cachedHealth;

        void OnEnable()
        {
            // 尝试缓存 Health 组件
            if (cachedHealth == null && base.Master?.Item != null)
            {
                var character = base.Master.Item.GetCharacterMainControl();
                if (character != null)
                {
                    cachedHealth = character.Health;
                }
            }
        }

        protected override void OnTriggeredPositive()
        {
            ExecuteDodge();
        }

        protected override void OnTriggeredNegative()
        {
            // 负面触发时可以在这里处理（如果需要）
        }

        /// <summary>
        /// 执行闪避效果
        /// </summary>
        private void ExecuteDodge()
        {
            if (base.Master == null || base.Master.Item == null)
            {
                return;
            }

            // 获取角色
            CharacterMainControl character = base.Master.Item.GetCharacterMainControl();
            if (character == null)
            {
                return;
            }

            // 获取 Health 组件（使用缓存）
            Health health = cachedHealth;
            if (health == null)
            {
                health = character.Health;
                cachedHealth = health;
            }
            
            if (health == null)
            {
                return;
            }

            // 闪避效果：恢复相当于玩家当前损失生命值的量（即完全抵消伤害）
            // 由于伤害已经扣除，我们通过 AddHealth 来恢复
            // 注意：这里无法直接获取刚才的伤害值，因为 Effect 系统不传递伤害值

            // 方案：恢复少量生命作为闪避奖励/补偿
            float healAmount = 5f; // 闪避时恢复 5 点生命

            health.AddHealth(healAmount);

            // 显示特效和提示
            ShowDodgeEffect(character);

            Debug.Log($"[虫洞徽章] 闪避效果生效，恢复 {healAmount} 点生命");
        }

        /// <summary>
        /// 显示闪避特效和提示
        /// </summary>
        private void ShowDodgeEffect(CharacterMainControl character)
        {
            if (character == null) return;

            // 显示文字提示
            character.PopText("虫洞徽章：闪避成功！");

            // 创建粒子特效
            CreateDodgeParticles(character.transform.position);
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
            Object.Destroy(effectObj, 2f);
        }
    }
}
