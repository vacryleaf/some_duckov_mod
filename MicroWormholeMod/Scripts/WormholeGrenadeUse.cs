using UnityEngine;
using ItemStatsSystem;

namespace MicroWormholeMod
{
    /// <summary>
    /// 虫洞手雷使用组件
    /// 处理手雷的投掷逻辑
    /// </summary>
    public class WormholeGrenadeUse : MonoBehaviour
    {
        // 关联的物品
        private Item item;

        // 使用冷却时间
        private float useCooldown = 1f;
        private float lastUseTime = 0f;

        // 投掷物预制体（动态创建）
        private GameObject projectilePrefab;

        void Awake()
        {
            item = GetComponent<Item>();

            if (item != null)
            {
                item.onUse += OnUse;
                Debug.Log("[虫洞手雷] WormholeGrenadeUse组件初始化完成");
            }

            // 创建投掷物预制体
            CreateProjectilePrefab();
        }

        /// <summary>
        /// 创建投掷物预制体
        /// </summary>
        private void CreateProjectilePrefab()
        {
            projectilePrefab = new GameObject("WormholeGrenadeProjectile");
            projectilePrefab.SetActive(false);
            DontDestroyOnLoad(projectilePrefab);

            // 添加视觉效果
            CreateProjectileVisual(projectilePrefab);

            // 添加投掷物组件
            var projectile = projectilePrefab.AddComponent<WormholeGrenadeProjectile>();
            projectile.fuseTime = 3f;
            projectile.teleportRadius = 8f;
            projectile.throwForce = 15f;
            projectile.throwAngle = 30f;

            Debug.Log("[虫洞手雷] 投掷物预制体创建完成");
        }

        /// <summary>
        /// 创建投掷物的视觉效果
        /// </summary>
        private void CreateProjectileVisual(GameObject parent)
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
        }

        /// <summary>
        /// 物品使用回调
        /// </summary>
        private void OnUse(Item usedItem, object user)
        {
            // 检查冷却
            if (Time.time - lastUseTime < useCooldown)
            {
                Debug.Log("[虫洞手雷] 使用冷却中...");
                return;
            }

            lastUseTime = Time.time;

            Debug.Log($"[虫洞手雷] 手雷被使用，使用者: {user}");

            // 获取投掷者
            CharacterMainControl thrower = CharacterMainControl.Main;
            if (thrower == null)
            {
                Debug.LogWarning("[虫洞手雷] 找不到投掷者");
                return;
            }

            // 投掷手雷
            ThrowGrenade(thrower);

            // 消耗物品
            ConsumeItem(usedItem);
        }

        /// <summary>
        /// 投掷手雷
        /// </summary>
        private void ThrowGrenade(CharacterMainControl thrower)
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("[虫洞手雷] 投掷物预制体为空");
                return;
            }

            // 计算投掷起始位置（角色前方稍高处）
            Vector3 throwPosition = thrower.transform.position +
                                    thrower.transform.forward * 0.5f +
                                    Vector3.up * 1.5f;

            // 计算投掷方向（角色面向方向）
            Vector3 throwDirection = thrower.transform.forward;

            // 创建投掷物实例
            GameObject grenadeInstance = Instantiate(projectilePrefab, throwPosition, Quaternion.identity);
            grenadeInstance.SetActive(true);

            // 获取投掷物组件并投掷
            var projectile = grenadeInstance.GetComponent<WormholeGrenadeProjectile>();
            if (projectile != null)
            {
                projectile.Throw(thrower, throwDirection);
            }

            // 显示提示
            ShowMessage("虫洞手雷已投掷！");

            Debug.Log($"[虫洞手雷] 手雷投掷成功，位置: {throwPosition}, 方向: {throwDirection}");
        }

        /// <summary>
        /// 消耗物品
        /// </summary>
        private void ConsumeItem(Item usedItem)
        {
            if (usedItem == null) return;

            if (usedItem.Stackable && usedItem.StackCount > 1)
            {
                Debug.Log("[虫洞手雷] 减少堆叠数量");
            }
            else
            {
                usedItem.Detach();
                Destroy(usedItem.gameObject);
                Debug.Log("[虫洞手雷] 物品已消耗");
            }
        }

        /// <summary>
        /// 显示消息提示
        /// </summary>
        private void ShowMessage(string message)
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter != null)
            {
                Duckov.UI.DialogueBubbles.DialogueBubblesManager.Show(
                    message,
                    mainCharacter.transform,
                    -1f,
                    false,
                    false,
                    -1f,
                    2f
                );
            }
        }

        void OnDestroy()
        {
            if (item != null)
            {
                item.onUse -= OnUse;
            }

            if (projectilePrefab != null)
            {
                Destroy(projectilePrefab);
            }
        }
    }
}
