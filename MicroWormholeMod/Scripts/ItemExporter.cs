using UnityEngine;
using ItemStatsSystem;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace MicroWormholeMod
{
    /// <summary>
    /// 物品导出工具 - 将游戏所有物品导出到CSV文件
    /// CSV文件可以直接用Excel打开
    /// </summary>
    public class ItemExporter : MonoBehaviour
    {
        /// <summary>
        /// 导出所有物品到CSV文件
        /// </summary>
        public static void ExportAllItemsToCSV(string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = Path.Combine(Application.dataPath, "../AllItems.csv");
            }

            try
            {
                var collection = ItemAssetsCollection.Instance;
                if (collection == null || collection.entries == null)
                {
                    ModLogger.LogError("[物品导出] 无法获取ItemAssetsCollection实例");
                    return;
                }

                StringBuilder sb = new StringBuilder();

                // 写入CSV表头（使用中文）
                sb.AppendLine("物品ID,物品名称,显示名称,品质,分类,标签,描述,口径,最大堆叠,默认堆叠,单价,可堆叠,可使用,重量,耐久度");

                // 遍历所有物品条目
                int count = 0;
                foreach (var entry in collection.entries)
                {
                    if (entry == null || entry.prefab == null)
                        continue;

                    try
                    {
                        Item item = entry.prefab;
                        ItemMetaData meta = entry.metaData;

                        // 收集数据
                        string id = item.TypeID.ToString();
                        string name = EscapeCSV(item.name);
                        string displayName = EscapeCSV(meta.DisplayName);
                        string quality = item.Quality.ToString();
                        string category = EscapeCSV(meta.Catagory);
                        
                        // 标签
                        string tags = "";
                        if (item.Tags != null && item.Tags.Count > 0)
                        {
                            tags = string.Join(";", item.Tags.Select(t => t.name));
                        }
                        tags = EscapeCSV(tags);

                        string description = EscapeCSV(meta.Description);
                        string caliber = EscapeCSV(meta.caliber);
                        string maxStack = meta.maxStackCount.ToString();
                        string defaultStack = meta.defaultStackCount.ToString();
                        string price = meta.priceEach.ToString();
                        string stackable = item.Stackable ? "是" : "否";
                        string usable = item.IsUsable(null) ? "是" : "否";  // 使用 IsUsable 方法
                        string weight = item.UnitSelfWeight.ToString("F2");  // 使用 UnitSelfWeight
                        string durability = item.MaxDurability.ToString("F0");

                        // 写入CSV行
                        sb.AppendLine($"{id},{name},{displayName},{quality},{category},{tags},{description},{caliber},{maxStack},{defaultStack},{price},{stackable},{usable},{weight},{durability}");
                        count++;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[物品导出] 导出物品 {entry.typeID} 时出错: {ex.Message}");
                    }
                }

                // 写入文件（使用UTF-8 BOM，确保Excel能正确识别中文）
                File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));

                Debug.Log($"[物品导出] 成功导出 {count} 个物品到: {filePath}");
                Debug.Log($"[物品导出] 文件大小: {new FileInfo(filePath).Length / 1024} KB");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[物品导出] 导出失败: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 转义CSV字段（处理逗号、引号、换行符）
        /// </summary>
        private static string EscapeCSV(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // 如果包含逗号、引号或换行符，需要用引号包裹
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                // 引号需要双写转义
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }

        /// <summary>
        /// 导出所有物品的详细统计信息
        /// </summary>
        public static void ExportItemStatistics(string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = Path.Combine(Application.dataPath, "../ItemStatistics.csv");
            }

            try
            {
                var collection = ItemAssetsCollection.Instance;
                if (collection == null || collection.entries == null)
                {
                    ModLogger.LogError("[物品统计] 无法获取ItemAssetsCollection实例");
                    return;
                }

                StringBuilder sb = new StringBuilder();

                // 统计数据
                Dictionary<string, int> categoryCount = new Dictionary<string, int>();
                Dictionary<int, int> qualityCount = new Dictionary<int, int>();
                int totalItems = 0;
                int usableItems = 0;
                int stackableItems = 0;

                foreach (var entry in collection.entries)
                {
                    if (entry == null || entry.prefab == null)
                        continue;

                    totalItems++;
                    Item item = entry.prefab;
                    ItemMetaData meta = entry.metaData;

                    // 分类统计
                    string category = meta.Catagory;
                    if (!categoryCount.ContainsKey(category))
                        categoryCount[category] = 0;
                    categoryCount[category]++;

                    // 品质统计
                    int quality = item.Quality;
                    if (!qualityCount.ContainsKey(quality))
                        qualityCount[quality] = 0;
                    qualityCount[quality]++;

                    // 特性统计
                    if (item.IsUsable(null)) usableItems++;  // 使用 IsUsable 方法
                    if (item.Stackable) stackableItems++;
                }

                // 写入统计报告
                sb.AppendLine("=== 物品统计报告 ===");
                sb.AppendLine($"总物品数,{totalItems}");
                sb.AppendLine($"可使用物品,{usableItems}");
                sb.AppendLine($"可堆叠物品,{stackableItems}");
                sb.AppendLine("");
                sb.AppendLine("=== 按分类统计 ===");
                sb.AppendLine("分类,数量");
                foreach (var kvp in categoryCount.OrderByDescending(x => x.Value))
                {
                    sb.AppendLine($"{EscapeCSV(kvp.Key)},{kvp.Value}");
                }
                sb.AppendLine("");
                sb.AppendLine("=== 按品质统计 ===");
                sb.AppendLine("品质,数量");
                foreach (var kvp in qualityCount.OrderBy(x => x.Key))
                {
                    sb.AppendLine($"{kvp.Key},{kvp.Value}");
                }

                File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));

                Debug.Log($"[物品统计] 统计报告已保存到: {filePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[物品统计] 导出失败: {ex.Message}");
            }
        }
    }
}
