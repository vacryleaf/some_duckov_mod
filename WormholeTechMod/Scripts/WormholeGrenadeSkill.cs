using UnityEngine;
using Duckov;
using ItemStatsSystem;

namespace WormholeTechMod
{
    /// <summary>
    /// 虫洞手雷技能
    /// 继承 SkillBase，通过技能系统释放
    /// </summary>
    public class WormholeGrenadeSkill : SkillBase
    {
        // 投掷参数
        public float damageRange = 16f;
        public float delayTime = 3f;
        public float throwForce = 15f;
        public float throwAngle = 30f;
        public bool canControlCastDistance = true;
        public float grenadeVerticleSpeed = 10f;
        public bool canHurtSelf = true;

        /// <summary>
        /// 释放技能（投掷手雷）
        /// </summary>
        public override void OnRelease()
        {
            if (fromCharacter == null)
            {
                ModLogger.LogWarning("[虫洞手雷] 没有投掷者");
                return;
            }

            // ModLogger.Log($"[虫洞手雷] 技能释放，使用者: {fromCharacter.name}");

            // 获取投掷位置和方向
            Vector3 position = fromCharacter.CurrentUsingAimSocket.position;
            Vector3 releasePoint = skillReleaseContext.releasePoint;

            // 计算投掷方向
            Vector3 point = releasePoint - fromCharacter.transform.position;
            point.y = 0;
            float castDistance = point.magnitude;

            if (!canControlCastDistance)
            {
                castDistance = skillContext.castRange;
            }

            point.Normalize();

            // 投掷手雷
            ThrowGrenade(position, point, castDistance, releasePoint.y);
        }

        /// <summary>
        /// 投掷手雷
        /// </summary>
        private void ThrowGrenade(Vector3 position, Vector3 direction, float distance, float height)
        {
            // 计算目标位置
            Vector3 target = position + direction * distance;
            target.y = height;

            // 创建投掷物
            GameObject grenadeObj = new GameObject("WormholeGrenade");
            grenadeObj.transform.position = position;
            grenadeObj.transform.rotation = fromCharacter.CurrentUsingAimSocket.rotation;

            // 添加视觉效果
            CreateGrenadeVisual(grenadeObj);

            // 添加投掷物组件
            WormholeGrenadeProjectile projectile = grenadeObj.AddComponent<WormholeGrenadeProjectile>();
            projectile.delayTime = delayTime;
            projectile.damageRange = damageRange;
            projectile.throwForce = throwForce;
            projectile.throwAngle = throwAngle;
            projectile.hasCollideSound = true;
            projectile.collideSound = "GrenadeCollide";
            projectile.isDangerForAi = true;

            // 设置伤害信息
            DamageInfo dmgInfo = new DamageInfo(fromCharacter);
            dmgInfo.damageValue = 0f;
            dmgInfo.damageType = DamageTypes.normal;
            projectile.SetDamageInfo(dmgInfo);

            // 计算投掷速度
            Vector3 velocity = CalculateVelocity(position, target, grenadeVerticleSpeed);

            // 投掷
            projectile.Launch(position, velocity, fromCharacter, canHurtSelf);

            // ModLogger.Log($"[虫洞手雷] 手雷投掷成功，位置: {position}, 目标: {target}");
        }

        /// <summary>
        /// 计算投掷速度（抛物线）
        /// </summary>
        private Vector3 CalculateVelocity(Vector3 start, Vector3 target, float verticleSpeed)
        {
            float g = Physics.gravity.magnitude;
            float y = target.y - start.y;
            Vector3 vector = target - start;
            vector.y = 0f;
            float num = vector.magnitude;
            float num2 = Mathf.Sqrt(y * y + num * num);
            float num3 = num2 + y;
            float num4 = num2 - y;
            float t = 2f * num3 / (g * num4);
            float num5 = num / Mathf.Sqrt(t);
            float num6 = g * t / 2f;
            Vector3 result = vector.normalized * num5;
            result.y = num6;
            return result;
        }

        /// <summary>
        /// 创建手雷视觉效果
        /// </summary>
        private void CreateGrenadeVisual(GameObject parent)
        {
            Color color = new Color(1f, 0.5f, 0.2f); // 橙色

            // 手雷主体
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(parent.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);

            Material bodyMaterial = new Material(Shader.Find("Standard"));
            bodyMaterial.color = color;
            bodyMaterial.SetFloat("_Metallic", 0.6f);
            bodyMaterial.SetFloat("_Glossiness", 0.7f);
            bodyMaterial.EnableKeyword("_EMISSION");
            bodyMaterial.SetColor("_EmissionColor", color * 1.5f);

            body.GetComponent<Renderer>().material = bodyMaterial;
            Object.Destroy(body.GetComponent<Collider>());

            // 环带
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Ring";
            ring.transform.SetParent(parent.transform);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(0.1f, 0.01f, 0.1f);

            Material ringMaterial = new Material(Shader.Find("Standard"));
            ringMaterial.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            ringMaterial.SetFloat("_Metallic", 0.9f);
            ring.GetComponent<Renderer>().material = ringMaterial;
            Object.Destroy(ring.GetComponent<Collider>());

            // 添加发光效果
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(parent.transform);
            glow.transform.localPosition = Vector3.zero;

            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = 0.5f;
            light.range = 1f;

            // 添加碰撞体
            SphereCollider collider = parent.AddComponent<SphereCollider>();
            collider.radius = 0.08f;
        }
    }
}
