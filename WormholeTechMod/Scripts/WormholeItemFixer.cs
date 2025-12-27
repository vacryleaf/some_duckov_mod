using UnityEngine;
using ItemStatsSystem;
using System.Reflection;

namespace WormholeTechMod
{
    /// <summary>
    /// 在物品启用时修复 AgentUtilities
    ///
    /// 问题根源：
    /// 1. prefab 创建时，代码设置 `initialized = true` 和 `master = item`
    /// 2. Instantiate 克隆时，复制了 `initialized = true` 和 `master = prefab.item`
    /// 3. 新实例的 `Awake()` → `Initialize()` 检查 `initialized == true`，跳过初始化
    /// 4. 所以新实例的 `master` 指向错误的 Item
    ///
    /// 解决方案：
    /// - 不依赖 Awake 执行顺序
    /// - 直接调用 agentUtilities.Initialize(this) 来正确设置 master
    /// </summary>
    public class WormholeItemFixer : MonoBehaviour
    {
        void Awake()
        {
            FixAgentUtils();
        }

        void OnEnable()
        {
            FixAgentUtils();
        }

        void FixAgentUtils()
        {
            var item = GetComponent<Item>();
            if (item == null) return;

            try
            {
                var agentUtils = item.AgentUtilities;
                if (agentUtils == null) return;

                // 直接调用 Initialize() 来正确设置 master
                // Initialize() 会设置 this.master = this（即当前 item）
                agentUtils.Initialize(item);

                Debug.Log($"[WormholeItemFixer] FIXED {item.DisplayName} (InstanceID={item.GetInstanceID()})");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[WormholeItemFixer] Error: {e.Message}");
            }
        }
    }
}
