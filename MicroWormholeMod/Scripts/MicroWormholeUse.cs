using UnityEngine;
using ItemStatsSystem;

namespace MicroWormholeMod
{
    /// <summary>
    /// 微型虫洞物品使用组件
    /// 附加到物品上，处理物品的使用逻辑
    /// </summary>
    public class MicroWormholeUse : MonoBehaviour
    {
        // 关联的物品
        private Item item;

        // 是否可以使用
        private bool canUse = true;

        // 使用冷却时间（防止重复使用）
        private float useCooldown = 2f;
        private float lastUseTime = 0f;

        void Awake()
        {
            // 获取关联的物品组件
            item = GetComponent<Item>();

            if (item != null)
            {
                // 注册物品使用事件
                item.onUse += OnUse;
                Debug.Log("[微型虫洞] MicroWormholeUse组件初始化完成");
            }
        }

        /// <summary>
        /// 物品使用回调
        /// </summary>
        private void OnUse(Item usedItem, object user)
        {
            // 检查冷却
            if (Time.time - lastUseTime < useCooldown)
            {
                Debug.Log("[微型虫洞] 使用冷却中...");
                return;
            }

            if (!canUse)
            {
                Debug.Log("[微型虫洞] 当前无法使用");
                return;
            }

            lastUseTime = Time.time;

            Debug.Log($"[微型虫洞] 物品被使用，使用者: {user}");

            // 物品使用逻辑在 ModBehaviour.OnItemUsed 中处理
            // 这里只做基础检查和冷却控制
        }

        /// <summary>
        /// 设置是否可以使用
        /// </summary>
        public void SetCanUse(bool value)
        {
            canUse = value;
        }

        /// <summary>
        /// 获取物品
        /// </summary>
        public Item GetItem()
        {
            return item;
        }

        void OnDestroy()
        {
            // 取消事件注册
            if (item != null)
            {
                item.onUse -= OnUse;
            }
        }
    }
}
