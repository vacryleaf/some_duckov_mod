using UnityEngine;
using ItemStatsSystem;
using System.Collections;

namespace MoonlightSwordMod
{
    /// <summary>
    /// 名刀月影攻击逻辑
    /// 处理普通攻击和特殊攻击的输入、动画和伤害判定
    ///
    /// 标准参数（在 Unity Prefab 的 Item.Stats 中配置）:
    /// - Damage: 基础伤害
    /// - CritRate: 暴击率
    /// - CritDamageFactor: 暴击伤害倍率
    /// - ArmorPiercing: 护甲穿透
    /// - AttackSpeed: 攻击速度
    /// - AttackRange: 攻击范围
    /// - StaminaCost: 体力消耗
    /// - BleedChance: 流血几率
    ///
    /// 自定义参数（特殊攻击专用）:
    /// - specialDamage: 剑气伤害
    /// - specialRange: 剑气飞行距离
    /// - specialCooldown: 特殊攻击冷却
    /// </summary>
    public class MoonlightSwordAttack : MonoBehaviour
    {
        [Header("特殊攻击配置（自定义）")]
        public GameObject swordAuraPrefab;           // 剑气Prefab
        public float specialDamage = 90f;            // 特殊攻击伤害
        public float specialRange = 10f;             // 特殊攻击范围(剑气飞行距离)
        public float specialCooldown = 8f;           // 特殊攻击冷却时间

        [Header("攻击状态")]
        private bool isAttacking = false;            // 是否正在攻击
        private int comboIndex = 0;                  // 连击索引: 0=正手, 1=反手
        private float lastAttackTime;                // 上次攻击时间
        private float lastSpecialTime;               // 上次特殊攻击时间
        private float comboResetTime = 1.5f;         // 连击重置时间

        [Header("组件引用")]
        private GameObject player;                   // 玩家对象
        private Animator playerAnimator;             // 玩家动画控制器
        private Item weaponItem;                     // 武器 Item 组件
        private CharacterMainControl character;      // 角色控制器

        // Stat Hash 缓存（性能优化）
        private static readonly int DamageHash = "Damage".GetHashCode();
        private static readonly int CritRateHash = "CritRate".GetHashCode();
        private static readonly int CritDamageFactorHash = "CritDamageFactor".GetHashCode();
        private static readonly int ArmorPiercingHash = "ArmorPiercing".GetHashCode();
        private static readonly int AttackSpeedHash = "AttackSpeed".GetHashCode();
        private static readonly int AttackRangeHash = "AttackRange".GetHashCode();
        private static readonly int StaminaCostHash = "StaminaCost".GetHashCode();
        private static readonly int BleedChanceHash = "BleedChance".GetHashCode();

        #region 标准 Stat 属性（从 Item.Stats 读取）

        /// <summary>基础伤害</summary>
        public float Damage => weaponItem != null ? weaponItem.GetStatValue(DamageHash) : 50f;

        /// <summary>暴击率</summary>
        public float CritRate => weaponItem != null ? weaponItem.GetStatValue(CritRateHash) : 0.05f;

        /// <summary>暴击伤害倍率</summary>
        public float CritDamageFactor => weaponItem != null ? weaponItem.GetStatValue(CritDamageFactorHash) : 1.5f;

        /// <summary>护甲穿透</summary>
        public float ArmorPiercing => weaponItem != null ? weaponItem.GetStatValue(ArmorPiercingHash) : 0f;

        /// <summary>攻击速度</summary>
        public float AttackSpeed => weaponItem != null ? Mathf.Max(0.1f, weaponItem.GetStatValue(AttackSpeedHash)) : 1f;

        /// <summary>攻击范围</summary>
        public float AttackRange => weaponItem != null ? weaponItem.GetStatValue(AttackRangeHash) : 2f;

        /// <summary>体力消耗</summary>
        public float StaminaCost => weaponItem != null ? weaponItem.GetStatValue(StaminaCostHash) : 5f;

        /// <summary>流血几率</summary>
        public float BleedChance => weaponItem != null ? weaponItem.GetStatValue(BleedChanceHash) : 0f;

        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        void Start()
        {
            // 获取武器 Item 组件
            weaponItem = GetComponent<Item>();
            if (weaponItem == null)
            {
                Debug.LogWarning("[名刀月影] 未找到 Item 组件，将使用默认参数");
            }
            else
            {
                Debug.Log($"[名刀月影] 武器参数已加载 - 伤害:{Damage} 暴击率:{CritRate} 攻击范围:{AttackRange}");
            }

            // 获取角色引用
            character = GetComponentInParent<CharacterMainControl>();
            if (character == null)
            {
                character = CharacterMainControl.Main;
            }

            if (character != null)
            {
                player = character.gameObject;
                playerAnimator = player.GetComponent<Animator>();
                Debug.Log("[名刀月影] 角色引用获取成功");
            }
            else
            {
                // 备用方案：通过 Tag 查找
                player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerAnimator = player.GetComponent<Animator>();
                    character = player.GetComponent<CharacterMainControl>();
                }
                else
                {
                    Debug.LogWarning("[名刀月影] 未找到玩家对象");
                }
            }
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
            // 等待攻击动画到达判定点 (根据攻击速度调整)
            float attackDelay = 0.3f / AttackSpeed;
            yield return new WaitForSeconds(attackDelay);

            // 执行伤害判定（使用标准参数）
            PerformMeleeDamage();

            // 等待攻击动画结束
            yield return new WaitForSeconds(attackDelay);

            isAttacking = false;
        }

        /// <summary>
        /// 近战伤害判定
        /// 扇形范围检测并造成伤害（使用游戏标准 DamageInfo）
        /// </summary>
        public void PerformMeleeDamage()
        {
            if (player == null) return;

            // 消耗体力
            if (character != null && StaminaCost > 0)
            {
                if (character.CurrentStamina < StaminaCost)
                {
                    Debug.Log("[名刀月影] 体力不足");
                    return;
                }
                character.UseStamina(StaminaCost);
            }

            // 计算攻击原点和方向
            Vector3 attackOrigin = player.transform.position + player.transform.forward;
            Vector3 attackDirection = player.transform.forward;

            // 扇形范围检测（使用标准 AttackRange）
            Collider[] hits = Physics.OverlapSphere(attackOrigin, AttackRange);

            int hitCount = 0;

            foreach (Collider hit in hits)
            {
                // 获取 DamageReceiver 组件
                DamageReceiver receiver = hit.GetComponent<DamageReceiver>();
                if (receiver == null) continue;

                // 检查是否是敌人
                if (character != null && !Team.IsEnemy(receiver.Team, character.Team))
                {
                    continue;
                }

                // 检查是否在扇形范围内 (90度，与游戏标准一致)
                Vector3 directionToTarget = (hit.transform.position - player.transform.position).normalized;
                directionToTarget.y = 0;
                float angle = Vector3.Angle(attackDirection, directionToTarget);

                if (angle <= 90f)
                {
                    // 使用游戏标准 DamageInfo 造成伤害
                    DamageInfo damageInfo = new DamageInfo(character);
                    damageInfo.damageValue = Damage;
                    damageInfo.critRate = CritRate;
                    damageInfo.critDamageFactor = CritDamageFactor;
                    damageInfo.armorPiercing = ArmorPiercing;
                    damageInfo.bleedChance = BleedChance;
                    damageInfo.crit = -1; // 让游戏自动计算是否暴击
                    damageInfo.damageNormal = -attackDirection;
                    damageInfo.damagePoint = hit.transform.position;
                    damageInfo.fromWeaponItemID = weaponItem != null ? weaponItem.TypeID : 0;

                    receiver.Hurt(damageInfo);
                    hitCount++;

                    Debug.Log($"[名刀月影] 命中 {hit.name}，伤害: {Damage}");
                }
            }

            if (hitCount > 0)
            {
                Debug.Log($"[名刀月影] 普通攻击命中 {hitCount} 个敌人");
            }
        }

        /// <summary>
        /// 应用伤害到目标（备用方法，用于没有 DamageReceiver 的目标）
        /// </summary>
        private void ApplyDamage(GameObject target, float damage)
        {
            // 尝试使用 DamageReceiver
            var receiver = target.GetComponent<DamageReceiver>();
            if (receiver != null)
            {
                DamageInfo damageInfo = new DamageInfo(character);
                damageInfo.damageValue = damage;
                receiver.Hurt(damageInfo);
                return;
            }

            // 备用方案: 直接调用可能的伤害方法
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"[名刀月影] 对 {target.name} 造成 {damage} 伤害");
            }
            else
            {
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

            // 绘制普通攻击范围（使用标准 AttackRange）
            Gizmos.color = Color.cyan;
            Vector3 origin = player.transform.position + player.transform.forward;
            Gizmos.DrawWireSphere(origin, AttackRange);

            // 绘制攻击扇形（90度，与游戏标准一致）
            Vector3 forward = player.transform.forward;
            Vector3 left = Quaternion.Euler(0, -90, 0) * forward * AttackRange;
            Vector3 right = Quaternion.Euler(0, 90, 0) * forward * AttackRange;

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
