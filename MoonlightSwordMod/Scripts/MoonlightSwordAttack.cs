using UnityEngine;
using ItemStatsSystem;
using System.Collections;

namespace MoonlightSwordMod
{
    /// <summary>
    /// 名刀月影攻击逻辑
    /// 处理普通攻击和特殊攻击的输入、动画和伤害判定
    /// </summary>
    public class MoonlightSwordAttack : MonoBehaviour
    {
        [Header("武器配置")]
        public GameObject swordAuraPrefab;           // 剑气Prefab
        public float normalDamage = 52.5f;           // 普通攻击伤害
        public float specialDamage = 90f;            // 特殊攻击伤害
        public float attackRange = 3f;               // 普通攻击范围
        public float specialRange = 10f;             // 特殊攻击范围(剑气飞行距离)
        public float specialCooldown = 8f;           // 特殊攻击冷却时间

        [Header("攻击状态")]
        private bool isAttacking = false;            // 是否正在攻击
        private int comboIndex = 0;                  // 连击索引: 0=正手, 1=反手
        private float lastAttackTime;                // 上次攻击时间
        private float lastSpecialTime;               // 上次特殊攻击时间
        private float comboResetTime = 1.5f;         // 连击重置时间

        [Header("玩家引用")]
        private GameObject player;                   // 玩家对象
        private Animator playerAnimator;             // 玩家动画控制器
        private Transform swordTransform;            // 刀的Transform

        /// <summary>
        /// 初始化
        /// </summary>
        void Start()
        {
            // 获取玩家引用
            // 注意: 需要根据实际游戏API调整获取方式
            player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerAnimator = player.GetComponent<Animator>();
                Debug.Log("[名刀月影] 玩家引用获取成功");
            }
            else
            {
                Debug.LogWarning("[名刀月影] 未找到玩家对象");
            }

            // 获取刀的Transform
            swordTransform = transform;
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        void Update()
        {
            // 检测玩家输入
            CheckInput();

            // 自动重置连击
            if (Time.time - lastAttackTime > comboResetTime)
            {
                ResetCombo();
            }
        }

        /// <summary>
        /// 检测玩家输入
        /// </summary>
        private void CheckInput()
        {
            if (player == null) return;

            // 检测攻击输入
            bool attackPressed = Input.GetMouseButtonDown(0);  // 左键攻击
            bool aimPressed = Input.GetMouseButton(1);         // 右键瞄准

            if (attackPressed && !isAttacking)
            {
                if (aimPressed)
                {
                    // 特殊攻击: 瞄准 + 攻击
                    TrySpecialAttack();
                }
                else
                {
                    // 普通攻击
                    PerformNormalAttack();
                }
            }
        }

        /// <summary>
        /// 执行普通攻击
        /// </summary>
        private void PerformNormalAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;

            // 播放攻击动画
            if (playerAnimator != null)
            {
                if (comboIndex == 0)
                {
                    playerAnimator.SetTrigger("ForwardSlash");
                    Debug.Log("[名刀月影] 执行正手挥击");
                }
                else
                {
                    playerAnimator.SetTrigger("BackhandSlash");
                    Debug.Log("[名刀月影] 执行反手挥击");
                }
            }
            else
            {
                Debug.LogWarning("[名刀月影] Animator未找到，使用简化攻击");
            }

            // 启动攻击检测协程
            StartCoroutine(NormalAttackCoroutine());

            // 切换连击索引
            comboIndex = (comboIndex + 1) % 2;
        }

        /// <summary>
        /// 普通攻击协程
        /// 控制攻击时序和伤害判定
        /// </summary>
        private IEnumerator NormalAttackCoroutine()
        {
            // 等待攻击动画到达判定点 (动画的50%处)
            yield return new WaitForSeconds(0.3f);

            // 执行伤害判定
            PerformMeleeDamage(normalDamage);

            // 等待攻击动画结束
            yield return new WaitForSeconds(0.3f);

            isAttacking = false;
        }

        /// <summary>
        /// 近战伤害判定
        /// 扇形范围检测并造成伤害
        /// </summary>
        public void PerformMeleeDamage(float damage)
        {
            if (player == null) return;

            // 计算攻击原点和方向
            Vector3 attackOrigin = player.transform.position + player.transform.forward;
            Vector3 attackDirection = player.transform.forward;

            // 扇形范围检测
            Collider[] hits = Physics.OverlapSphere(attackOrigin, attackRange);

            int hitCount = 0;

            foreach (Collider hit in hits)
            {
                // 检查是否是敌人
                if (hit.CompareTag("Enemy"))
                {
                    // 检查是否在扇形范围内 (120度扇形的一半 = 60度)
                    Vector3 directionToTarget = (hit.transform.position - player.transform.position).normalized;
                    float angle = Vector3.Angle(attackDirection, directionToTarget);

                    if (angle <= 60f)
                    {
                        // 造成伤害
                        ApplyDamage(hit.gameObject, damage);
                        hitCount++;
                    }
                }
            }

            if (hitCount > 0)
            {
                Debug.Log($"[名刀月影] 普通攻击命中 {hitCount} 个敌人");
            }
        }

        /// <summary>
        /// 应用伤害到目标
        /// </summary>
        private void ApplyDamage(GameObject target, float damage)
        {
            // 尝试使用游戏的伤害接口
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"[名刀月影] 对 {target.name} 造成 {damage} 伤害");
            }
            else
            {
                // 备用方案: 直接调用可能的伤害方法
                target.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// 尝试执行特殊攻击
        /// </summary>
        private void TrySpecialAttack()
        {
            // 检查冷却时间
            float timeSinceLastSpecial = Time.time - lastSpecialTime;
            if (timeSinceLastSpecial < specialCooldown)
            {
                float remainingCooldown = specialCooldown - timeSinceLastSpecial;
                Debug.Log($"[名刀月影] 特殊攻击冷却中，剩余: {remainingCooldown:F1}秒");
                return;
            }

            // 检查耐久度
            Item weapon = GetComponent<Item>();
            if (weapon != null && weapon.Durability < 10)
            {
                Debug.Log("[名刀月影] 耐久度不足，无法使用特殊攻击");
                return;
            }

            // 执行特殊攻击
            PerformSpecialAttack();
        }

        /// <summary>
        /// 执行特殊攻击
        /// 包括冲刺和释放剑气
        /// </summary>
        private void PerformSpecialAttack()
        {
            isAttacking = true;
            lastSpecialTime = Time.time;
            lastAttackTime = Time.time;

            Debug.Log("[名刀月影] 执行特殊攻击: 月影剑气!");

            // 消耗耐久度
            Item weapon = GetComponent<Item>();
            if (weapon != null)
            {
                weapon.Durability -= 10;
            }

            // 播放特殊攻击动画
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("SpecialAttack");
            }

            // 启动特殊攻击协程
            StartCoroutine(SpecialAttackCoroutine());
        }

        /// <summary>
        /// 特殊攻击协程
        /// 分三个阶段: 蓄力、冲刺、释放剑气
        /// </summary>
        private IEnumerator SpecialAttackCoroutine()
        {
            // 阶段1: 蓄力 (0.3秒)
            Debug.Log("[名刀月影] 阶段1: 蓄力");
            yield return new WaitForSeconds(0.3f);

            // 阶段2: 冲刺 (0.3秒)
            Debug.Log("[名刀月影] 阶段2: 冲刺");
            yield return StartCoroutine(DashMovement(3f));

            // 阶段3: 释放剑气
            Debug.Log("[名刀月影] 阶段3: 释放剑气");
            LaunchSwordAura();

            // 等待动画结束
            yield return new WaitForSeconds(0.6f);

            isAttacking = false;
            Debug.Log("[名刀月影] 特殊攻击完成");
        }

        /// <summary>
        /// 冲刺移动
        /// </summary>
        public IEnumerator DashMovement(float distance)
        {
            if (player == null) yield break;

            Vector3 dashDirection = player.transform.forward;
            float dashDuration = 0.3f;
            float dashSpeed = distance / dashDuration;

            Vector3 startPos = player.transform.position;
            Vector3 endPos = startPos + dashDirection * distance;

            // 检查终点是否有障碍物
            RaycastHit hit;
            if (Physics.Raycast(startPos, dashDirection, out hit, distance))
            {
                endPos = hit.point - dashDirection * 0.5f; // 在障碍物前停下
            }

            float elapsed = 0f;
            while (elapsed < dashDuration)
            {
                player.transform.position = Vector3.Lerp(startPos, endPos, elapsed / dashDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            player.transform.position = endPos;
            Debug.Log($"[名刀月影] 冲刺完成，移动了 {Vector3.Distance(startPos, endPos):F2}米");
        }

        /// <summary>
        /// 发射剑气
        /// </summary>
        public void LaunchSwordAura()
        {
            if (swordAuraPrefab == null)
            {
                Debug.LogError("[名刀月影] 剑气Prefab未设置");
                return;
            }

            if (player == null)
            {
                Debug.LogError("[名刀月影] 玩家引用丢失");
                return;
            }

            // 在玩家前方生成剑气
            Vector3 spawnPosition = player.transform.position + Vector3.up + player.transform.forward * 1.5f;
            Quaternion spawnRotation = Quaternion.LookRotation(player.transform.forward);

            GameObject aura = Instantiate(swordAuraPrefab, spawnPosition, spawnRotation);

            // 配置剑气投射物
            SwordAuraProjectile projectile = aura.GetComponent<SwordAuraProjectile>();
            if (projectile != null)
            {
                projectile.damage = specialDamage;
                projectile.speed = 15f;
                projectile.maxDistance = specialRange;
                projectile.owner = player;
                projectile.Launch(player.transform.forward);

                Debug.Log("[名刀月影] 剑气已发射!");
            }
            else
            {
                Debug.LogError("[名刀月影] 剑气Prefab缺少SwordAuraProjectile组件");
            }
        }

        /// <summary>
        /// 重置连击
        /// </summary>
        private void ResetCombo()
        {
            if (comboIndex != 0)
            {
                comboIndex = 0;
                Debug.Log("[名刀月影] 连击已重置");
            }
        }

        /// <summary>
        /// 调试可视化
        /// </summary>
        void OnDrawGizmos()
        {
            if (player == null) return;

            // 绘制普通攻击范围
            Gizmos.color = Color.cyan;
            Vector3 origin = player.transform.position + player.transform.forward;
            Gizmos.DrawWireSphere(origin, attackRange);

            // 绘制攻击扇形
            Vector3 forward = player.transform.forward;
            Vector3 left = Quaternion.Euler(0, -60, 0) * forward * attackRange;
            Vector3 right = Quaternion.Euler(0, 60, 0) * forward * attackRange;

            Gizmos.DrawLine(player.transform.position, player.transform.position + left);
            Gizmos.DrawLine(player.transform.position, player.transform.position + right);

            // 绘制特殊攻击范围
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(player.transform.position, forward * specialRange);
        }
    }

    /// <summary>
    /// 伤害接口
    /// 游戏对象需要实现此接口才能接受伤害
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}
