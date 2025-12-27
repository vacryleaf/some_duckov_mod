using UnityEngine;
using ItemStatsSystem;

namespace WormholeTechMod
{
    /// <summary>
    /// 自动修复物品的 AgentUtilities 问题
    /// 当物品被 Instantiate 后会自动调用 OnEnable，确保手持代理正确初始化
    /// </summary>
    public class AgentUtilitiesFixer : MonoBehaviour
    {
        private bool initialized = false;

        void OnEnable()
        {
            if (initialized) return;

            var item = GetComponent<Item>();
            if (item == null) return;

            try
            {
                // 确保物品已初始化
                if (!item.GetBool("initialized"))
                {
                    item.Initialize();
                }

                // 初始化 AgentUtilities
                var agentUtils = item.AgentUtilities;
                if (agentUtils != null)
                {
                    // 使用反射设置 Master
                    var masterField = typeof(ItemAgentUtilities).GetField("master",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (masterField != null)
                    {
                        masterField.SetValue(agentUtils, item);
                    }

                    // 获取或创建 agents 列表
                    var agentsField = typeof(ItemAgentUtilities).GetField("agents",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var agents = agentsField?.GetValue(agentUtils) as System.Collections.IList;

                    if (agents == null)
                    {
                        var agentKeyPairType = typeof(ItemAgentUtilities).GetNestedType("AgentKeyPair",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (agentKeyPairType != null)
                        {
                            var listType = typeof(System.Collections.Generic.List<>).MakeGenericType(agentKeyPairType);
                            agents = (System.Collections.IList)System.Activator.CreateInstance(listType);
                            agentsField?.SetValue(agentUtils, agents);
                        }
                    }

                    // 获取手持代理预制体
                    var handheldPrefab = Duckov.Utilities.GameplayDataSettings.Prefabs.HandheldAgentPrefab;

                    // 检查是否已有 Handheld
                    bool hasHandheld = false;
                    if (agents != null)
                    {
                        foreach (var agent in agents)
                        {
                            var keyField = agent?.GetType().GetField("key",
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (keyField != null && keyField.GetValue(agent)?.ToString() == "Handheld")
                            {
                                hasHandheld = true;
                                break;
                            }
                        }
                    }

                    // 添加 Handheld 代理
                    if (!hasHandheld && handheldPrefab != null)
                    {
                        var agentKeyPairType = typeof(ItemAgentUtilities).GetNestedType("AgentKeyPair",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var agentKeyPair = System.Activator.CreateInstance(agentKeyPairType);

                        var keyField = agentKeyPairType.GetField("key",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        var prefabField = agentKeyPairType.GetField("agentPrefab",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        keyField.SetValue(agentKeyPair, "Handheld");
                        prefabField.SetValue(agentKeyPair, handheldPrefab);

                        agents?.Add(agentKeyPair);
                    }
                }

                initialized = true;
            }
            catch (System.Exception e)
            {
                // Debug.LogWarning($"[微型虫洞] AgentUtilitiesFixer 修复失败: {e.Message}");
            }
        }
    }
}
