using UnityEngine;
using ItemStatsSystem;
using System;
using System.Reflection;
using Duckov.Scenes;

namespace MicroWormholeMod
{
    /// <summary>
    /// 微型虫洞使用行为
    /// 继承自 UsageBehavior，记录位置并触发撤离
    /// </summary>
    public class MicroWormholeUse : UsageBehavior
    {
        // 关联的物品
        private Item item;

        // 重写 DisplaySettings - 让UI显示使用信息
        public override DisplaySettingsData DisplaySettings
        {
            get
            {
                return new DisplaySettingsData
                {
                    display = true,
                    description = "记录位置并撤离"
                };
            }
        }

        void Awake()
        {
            item = GetComponent<Item>();
            ModLogger.Log("[微型虫洞] MicroWormholeUse行为初始化完成");
        }

        /// <summary>
        /// 检查物品是否可以使用
        /// </summary>
        public override bool CanBeUsed(Item item, object user)
        {
            // 基础检查：用户必须是角色
            if (!(user as CharacterMainControl))
            {
                ModLogger.Log("[微型虫洞] CanBeUsed失败：用户不是角色");
                return false;
            }

            // 检查 LevelManager
            if (LevelManager.Instance == null)
            {
                ModLogger.Log("[微型虫洞] CanBeUsed失败：LevelManager为空");
                return false;
            }

            // 只能在突袭地图使用
            if (!LevelManager.Instance.IsRaidMap)
            {
                Debug.Log($"[微型虫洞] CanBeUsed失败：不是突袭地图，IsRaidMap={LevelManager.Instance.IsRaidMap}");
                return false;
            }

            // 不能在基地使用
            if (LevelManager.Instance.IsBaseLevel)
            {
                Debug.Log($"[微型虫洞] CanBeUsed失败：在基地中，IsBaseLevel={LevelManager.Instance.IsBaseLevel}");
                return false;
            }

            ModLogger.Log("[微型虫洞] CanBeUsed成功");
            return true;
        }

        /// <summary>
        /// 执行使用逻辑
        /// </summary>
        protected override void OnUse(Item item, object user)
        {
            CharacterMainControl character = user as CharacterMainControl;
            if (character == null) return;

            // 检查条件
            if (LevelManager.Instance == null || LevelManager.Instance.IsBaseLevel || !LevelManager.Instance.IsRaidMap)
            {
                character.PopText("无法使用虫洞！");
                return;
            }

            // 记录当前位置和场景
            WormholeData savedData = new WormholeData();
            savedData.IsValid = true;
            savedData.Position = character.transform.position;
            savedData.Rotation = character.transform.rotation;
            savedData.SceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            Debug.Log($"[微型虫洞] 位置已记录: {savedData.Position}, 场景: {savedData.SceneName}");

            // 保存到 TeleportManager
            if (WormholeTeleportManager.Instance != null)
            {
                WormholeTeleportManager.Instance.SetWormholeData(savedData);
            }

            character.PopText("位置已记录！正在撤离...");

            // 获取基地场景ID
            string baseSceneID = "Base_01"; // 默认基地场景

            // 使用游戏原生的场景加载系统撤离
            try
            {
                var sceneLoaderType = Type.GetType("SceneLoader, TeamSoda.Duckov.Core");
                if (sceneLoaderType != null)
                {
                    var instanceProp = sceneLoaderType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    if (instanceProp != null)
                    {
                        var sceneLoader = instanceProp.GetValue(null);
                        if (sceneLoader != null)
                        {
                            // 查找合适的 LoadScene 方法
                            var methods = sceneLoaderType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                            foreach (var method in methods)
                            {
                                if (method.Name == "LoadScene" && method.GetParameters().Length >= 2)
                                {
                                    var parameters = method.GetParameters();
                                    if (parameters.Length >= 1 && parameters[0].ParameterType == typeof(string))
                                    {
                                        // 构建参数
                                        var args = new object[parameters.Length];
                                        args[0] = baseSceneID; // sceneID

                                        for (int i = 1; i < parameters.Length; i++)
                                        {
                                            var paramType = parameters[i].ParameterType;
                                            if (paramType == typeof(bool))
                                            {
                                                if (i == 2) args[i] = false;  // clickToContinue
                                                else if (i == 3) args[i] = true;  // notifyEvacuation - 撤离！
                                                else if (i == 4) args[i] = true;  // doCircleFade
                                                else if (i == 5) args[i] = false; // useLocation
                                                else if (i == 7) args[i] = true;  // saveToFile
                                                else if (i == 8) args[i] = false; // hideTips
                                                else args[i] = false;
                                            }
                                            else if (paramType == typeof(MultiSceneLocation))
                                            {
                                                args[i] = default(MultiSceneLocation);
                                            }
                                            else
                                            {
                                                args[i] = null;
                                            }
                                        }

                                        method.Invoke(sceneLoader, args);
                                        Debug.Log($"[微型虫洞] 已调用场景加载撤离到: {baseSceneID}");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[微型虫洞] 使用SceneLoader撤离失败: {e.Message}");
            }

            // 备用方案：直接使用 EvacuationInfo
            try
            {
                string subsceneID = MultiSceneCore.ActiveSubSceneID;
                EvacuationInfo evacuationInfo = new EvacuationInfo(subsceneID, character.transform.position);
                LevelManager.Instance.NotifyEvacuated(evacuationInfo);
                Debug.Log($"[微型虫洞] 已触发撤离，subsceneID: {subsceneID}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[微型虫洞] 撤离失败: {e.Message}");
            }
        }
    }
}
