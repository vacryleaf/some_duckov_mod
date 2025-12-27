using UnityEngine;
using ItemStatsSystem;
using System.Reflection;

namespace WormholeTechMod
{
    /// <summary>
    /// 在物品 Awake 时修复 AgentUtilities
    /// 确保每个实例的 master 字段都正确设置
    /// </summary>
    public class WormholeItemFixer : MonoBehaviour
    {
        private bool fixedAgentUtils = false;

        void Awake()
        {
            if (fixedAgentUtils) return;

            var item = GetComponent<Item>();
            if (item == null) return;

            try
            {
                var agentUtils = item.AgentUtilities;
                if (agentUtils == null) return;

                var masterField = typeof(ItemAgentUtilities).GetField("master",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (masterField != null)
                {
                    masterField.SetValue(agentUtils, item);
                }

                fixedAgentUtils = true;
            }
            catch { }
        }
    }
}
