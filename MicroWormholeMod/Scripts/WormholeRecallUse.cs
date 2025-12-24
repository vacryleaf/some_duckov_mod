using UnityEngine;
using ItemStatsSystem;

namespace MicroWormholeMod
{
    /// <summary>
    /// 虫洞回溯使用行为
    /// 继承自 UsageBehavior，传送到记录的位置
    /// </summary>
    public class WormholeRecallUse : UsageBehavior
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
                    description = "传送到记录的位置"
                };
            }
        }

        void Awake()
        {
            item = GetComponent<Item>();
            Debug.Log("[虫洞回溯] WormholeRecallUse行为初始化完成");
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

            // 只能在基地使用
            if (LevelManager.Instance == null || !LevelManager.Instance.IsBaseLevel)
            {
                return false;
            }

            // 检查是否有有效的虫洞记录
            var modBehaviour = FindObjectOfType<ModBehaviour>();
            if (modBehaviour == null || !modBehaviour.HasValidWormholeData())
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

            var modBehaviour = FindObjectOfType<ModBehaviour>();

            // 检查条件
            if (LevelManager.Instance == null)
            {
                character.PopText("无法使用虫洞！");
                return;
            }

            if (!LevelManager.Instance.IsBaseLevel)
            {
                character.PopText("只能在家中使用！");
                return;
            }

            if (modBehaviour == null || !modBehaviour.HasValidWormholeData())
            {
                character.PopText("没有可回溯的虫洞残留");
                return;
            }

            // 传送到记录的位置
            modBehaviour.ExecuteRecall(character);

            character.PopText("正在打开虫洞通道...");
        }
    }
}
