using UnityEngine;
using ItemStatsSystem;

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
            Debug.Log("[微型虫洞] MicroWormholeUse行为初始化完成");
        }

        /// <summary>
        /// 检查物品是否可以使用
        /// </summary>
        public override bool CanBeUsed(Item item, object user)
        {
            // 基础检查：用户必须是角色
            if (!(user as CharacterMainControl))
            {
                return false;
            }

            // 只能在突袭地图使用
            if (LevelManager.Instance == null || !LevelManager.Instance.IsRaidMap)
            {
                return false;
            }

            // 不能在基地使用
            if (LevelManager.Instance.IsBaseLevel)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 执行使用逻辑
        /// </summary>
        protected override void OnUse(Item item, object user)
        {
            CharacterMainControl character = user as CharacterMainControl;
            if (character == null) return;

            // 再次检查条件，不满足时显示消息
            if (LevelManager.Instance == null)
            {
                character.PopText("无法使用虫洞！");
                return;
            }

            if (LevelManager.Instance.IsBaseLevel)
            {
                character.PopText("你已经在家中了！");
                return;
            }

            if (!LevelManager.Instance.IsRaidMap)
            {
                character.PopText("只能在突袭任务中使用！");
                return;
            }

            // 记录当前位置和场景
            WormholeData savedData = new WormholeData();
            savedData.IsValid = true;
            savedData.Position = character.transform.position;
            savedData.Rotation = character.transform.rotation;
            savedData.SceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            Debug.Log($"[微型虫洞] 位置已记录: {savedData.Position}, 场景: {savedData.SceneName}");

            // 保存到 ModBehaviour
            var modBehaviour = FindObjectOfType<ModBehaviour>();
            if (modBehaviour != null)
            {
                modBehaviour.SetWormholeData(savedData);
            }

            character.PopText("位置已记录！正在撤离...");

            // 触发撤离
            EvacuationInfo evacuationInfo = new EvacuationInfo();
            LevelManager.Instance.NotifyEvacuated(evacuationInfo);
        }
    }
}
