using UnityEngine;
using UnityEditor;
using ItemStatsSystem;
using System.Reflection;
using System.Collections.Generic;

namespace MoonlightSwordMod.Editor
{
    /// <summary>
    /// 名刀月影 Stats 配置工具
    /// 用于在 Unity Editor 中快速配置武器的标准参数
    /// </summary>
    public class MoonlightSwordStatsSetup : EditorWindow
    {
        // 目标 Item
        private Item targetItem;

        // Stats 参数
        private float damage = 50f;
        private float critRate = 0.05f;
        private float critDamageFactor = 1.5f;
        private float armorPiercing = 0f;
        private float attackSpeed = 1f;
        private float attackRange = 2f;
        private float staminaCost = 5f;
        private float bleedChance = 0f;

        [MenuItem("Tools/名刀月影/配置武器 Stats")]
        public static void ShowWindow()
        {
            var window = GetWindow<MoonlightSwordStatsSetup>("名刀月影 Stats");
            window.minSize = new Vector2(350, 400);
        }

        private void OnGUI()
        {
            GUILayout.Label("名刀月影 - Stats 配置工具", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // 选择目标 Item
            EditorGUILayout.BeginHorizontal();
            targetItem = (Item)EditorGUILayout.ObjectField("武器 Prefab", targetItem, typeof(Item), true);
            EditorGUILayout.EndHorizontal();

            if (targetItem == null)
            {
                EditorGUILayout.HelpBox("请拖入武器 Prefab 或从场景中选择武器对象", MessageType.Info);

                // 尝试从选中对象获取
                if (Selection.activeGameObject != null)
                {
                    var item = Selection.activeGameObject.GetComponent<Item>();
                    if (item != null)
                    {
                        if (GUILayout.Button("使用选中对象: " + Selection.activeGameObject.name))
                        {
                            targetItem = item;
                        }
                    }
                }
                return;
            }

            GUILayout.Space(10);
            GUILayout.Label("标准近战武器参数", EditorStyles.boldLabel);

            // 参数输入
            damage = EditorGUILayout.FloatField("Damage (基础伤害)", damage);
            critRate = EditorGUILayout.FloatField("CritRate (暴击率)", critRate);
            critDamageFactor = EditorGUILayout.FloatField("CritDamageFactor (暴击倍率)", critDamageFactor);
            armorPiercing = EditorGUILayout.FloatField("ArmorPiercing (护甲穿透)", armorPiercing);
            attackSpeed = EditorGUILayout.FloatField("AttackSpeed (攻击速度)", attackSpeed);
            attackRange = EditorGUILayout.FloatField("AttackRange (攻击范围)", attackRange);
            staminaCost = EditorGUILayout.FloatField("StaminaCost (体力消耗)", staminaCost);
            bleedChance = EditorGUILayout.FloatField("BleedChance (流血几率)", bleedChance);

            GUILayout.Space(10);

            // 快速设置按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("普通刀剑"))
            {
                damage = 35f;
                critRate = 0.05f;
                critDamageFactor = 1.5f;
                attackRange = 1.8f;
                attackSpeed = 1.2f;
            }
            if (GUILayout.Button("重型武器"))
            {
                damage = 65f;
                critRate = 0.08f;
                critDamageFactor = 2f;
                attackRange = 2.2f;
                attackSpeed = 0.8f;
            }
            if (GUILayout.Button("名刀月影"))
            {
                damage = 50f;
                critRate = 0.1f;
                critDamageFactor = 1.8f;
                armorPiercing = 5f;
                attackRange = 2f;
                attackSpeed = 1f;
                staminaCost = 5f;
                bleedChance = 0.1f;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            // 应用按钮
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("应用 Stats 到武器", GUILayout.Height(40)))
            {
                ApplyStats();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // 显示当前 Stats
            if (GUILayout.Button("查看当前 Stats"))
            {
                ShowCurrentStats();
            }
        }

        /// <summary>
        /// 应用 Stats 到目标 Item
        /// </summary>
        private void ApplyStats()
        {
            if (targetItem == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择武器 Prefab", "确定");
                return;
            }

            Undo.RecordObject(targetItem.gameObject, "Setup Moonlight Sword Stats");

            // 获取或创建 StatCollection
            StatCollection stats = targetItem.GetComponent<StatCollection>();
            if (stats == null)
            {
                stats = targetItem.gameObject.AddComponent<StatCollection>();
                Debug.Log("[名刀月影] 已添加 StatCollection 组件");
            }

            // 通过反射访问私有的 list 字段
            var listField = typeof(StatCollection).GetField("list",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (listField == null)
            {
                EditorUtility.DisplayDialog("错误", "无法访问 StatCollection.list 字段", "确定");
                return;
            }

            // 获取或创建 list
            var statList = listField.GetValue(stats) as List<Stat>;
            if (statList == null)
            {
                statList = new List<Stat>();
                listField.SetValue(stats, statList);
            }

            // 清空现有 stats
            statList.Clear();

            // 添加新的 stats
            statList.Add(new Stat("Damage", damage, true));
            statList.Add(new Stat("CritRate", critRate, true));
            statList.Add(new Stat("CritDamageFactor", critDamageFactor, true));
            statList.Add(new Stat("ArmorPiercing", armorPiercing, true));
            statList.Add(new Stat("AttackSpeed", attackSpeed, true));
            statList.Add(new Stat("AttackRange", attackRange, true));
            statList.Add(new Stat("StaminaCost", staminaCost, true));
            statList.Add(new Stat("BleedChance", bleedChance, true));

            // 设置 Item 的 stats 引用
            var statsField = typeof(Item).GetField("stats",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (statsField != null)
            {
                statsField.SetValue(targetItem, stats);
            }

            // 标记为已修改
            EditorUtility.SetDirty(targetItem);
            EditorUtility.SetDirty(stats);

            // 如果是 Prefab，保存
            if (PrefabUtility.IsPartOfPrefabAsset(targetItem.gameObject))
            {
                PrefabUtility.SavePrefabAsset(targetItem.gameObject);
            }

            Debug.Log("[名刀月影] Stats 配置完成!");
            Debug.Log($"  Damage: {damage}");
            Debug.Log($"  CritRate: {critRate}");
            Debug.Log($"  CritDamageFactor: {critDamageFactor}");
            Debug.Log($"  ArmorPiercing: {armorPiercing}");
            Debug.Log($"  AttackSpeed: {attackSpeed}");
            Debug.Log($"  AttackRange: {attackRange}");
            Debug.Log($"  StaminaCost: {staminaCost}");
            Debug.Log($"  BleedChance: {bleedChance}");

            EditorUtility.DisplayDialog("成功", "Stats 已应用到武器!\n请重新打包 AssetBundle", "确定");
        }

        /// <summary>
        /// 显示当前 Stats
        /// </summary>
        private void ShowCurrentStats()
        {
            if (targetItem == null) return;

            StatCollection stats = targetItem.GetComponent<StatCollection>();
            if (stats == null)
            {
                EditorUtility.DisplayDialog("信息", "该武器没有 StatCollection 组件", "确定");
                return;
            }

            var listField = typeof(StatCollection).GetField("list",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var statList = listField?.GetValue(stats) as List<Stat>;

            if (statList == null || statList.Count == 0)
            {
                EditorUtility.DisplayDialog("信息", "Stats 列表为空", "确定");
                return;
            }

            string info = "当前 Stats:\n\n";
            foreach (var stat in statList)
            {
                info += $"{stat.Key}: {stat.BaseValue}\n";
            }

            EditorUtility.DisplayDialog("当前 Stats", info, "确定");
        }
    }
}
