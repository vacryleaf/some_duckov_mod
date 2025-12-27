using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 微型虫洞 AssetBundle 构建工具
/// 用于将物品 Prefab 打包为 AssetBundle
/// </summary>
public class MicroWormholeAssetBundleBuilder
{
    // Mod 输出路径（相对于 Unity 项目）
    private static string outputPath = "Assets/AssetBundles";

    // AssetBundle 名称
    private static string bundleName = "micro_wormhole";

    /// <summary>
    /// 构建 AssetBundle
    /// </summary>
    [MenuItem("Tools/微型虫洞/构建 AssetBundle")]
    public static void BuildAssetBundles()
    {
        // Debug.Log("[AssetBundle] 开始构建...");

        // 设置 Prefab 的 AssetBundle 名称
        SetAssetBundleNames();

        // 创建输出目录
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
            // Debug.Log($"[AssetBundle] 创建输出目录: {outputPath}");
        }

        // 构建 AssetBundle
        BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget
        );

        // 刷新资源数据库
        AssetDatabase.Refresh();

        // 显示完成信息
        string fullPath = Path.GetFullPath(outputPath);
        // Debug.Log($"[AssetBundle] 构建完成！输出位置: {fullPath}");

        EditorUtility.DisplayDialog(
            "AssetBundle 构建完成",
            $"文件已导出到:\n{fullPath}\n\n请将 '{bundleName}' 文件复制到 mod 的 Assets 文件夹中。",
            "确定"
        );

        // 在文件浏览器中打开输出目录
        EditorUtility.RevealInFinder(outputPath);
    }

    /// <summary>
    /// 设置资源的 AssetBundle 名称
    /// </summary>
    private static void SetAssetBundleNames()
    {
        // 物品 Prefab
        SetBundleName("Assets/Prefabs/MicroWormhole.prefab");

        // 物品图标
        SetBundleName("Assets/Icons/MicroWormholeIcon.png");
        SetBundleName("Assets/Icons/WormholeRecallIcon.png");
        SetBundleName("Assets/Icons/WormholeGrenadeIcon.png");

        AssetDatabase.SaveAssets();
        // Debug.Log("[AssetBundle] AssetBundle 名称设置完成");
    }

    /// <summary>
    /// 为单个资源设置 AssetBundle 名称
    /// </summary>
    private static void SetBundleName(string assetPath)
    {
        if (File.Exists(assetPath))
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
            {
                importer.assetBundleName = bundleName;
                // Debug.Log($"[AssetBundle] 设置 {assetPath} -> {bundleName}");
            }
        }
        else
        {
            Debug.LogWarning($"[AssetBundle] 文件不存在: {assetPath}");
        }
    }

    /// <summary>
    /// 清理 AssetBundle 名称
    /// </summary>
    [MenuItem("Tools/微型虫洞/清理 AssetBundle 名称")]
    public static void CleanAssetBundleNames()
    {
        string[] allBundleNames = AssetDatabase.GetAllAssetBundleNames();
        foreach (string name in allBundleNames)
        {
            AssetDatabase.RemoveAssetBundleName(name, true);
        }
        AssetDatabase.SaveAssets();
        // Debug.Log("[AssetBundle] 所有 AssetBundle 名称已清理");
    }

    /// <summary>
    /// 一键部署到 Mod 目录
    /// </summary>
    [MenuItem("Tools/微型虫洞/部署到 Mod 目录")]
    public static void DeployToModFolder()
    {
        // 先构建
        BuildAssetBundles();

        // 源文件路径
        string sourcePath = Path.Combine(outputPath, bundleName);

        if (!File.Exists(sourcePath))
        {
            EditorUtility.DisplayDialog("错误", "AssetBundle 文件不存在，请先构建。", "确定");
            return;
        }

        // 让用户选择 Mod 目录
        string modFolder = EditorUtility.OpenFolderPanel(
            "选择 Mod 的 Assets 文件夹",
            "",
            "Assets"
        );

        if (string.IsNullOrEmpty(modFolder))
        {
            return;
        }

        // 复制文件
        string destPath = Path.Combine(modFolder, bundleName);
        File.Copy(sourcePath, destPath, true);

        // Debug.Log($"[AssetBundle] 已部署到: {destPath}");
        EditorUtility.DisplayDialog(
            "部署完成",
            $"AssetBundle 已复制到:\n{destPath}",
            "确定"
        );
    }
}
