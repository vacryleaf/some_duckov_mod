using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 虫洞手雷图标生成器
/// 用于在 Unity 中生成投掷物图标
/// </summary>
public class WormholeGrenadeIconGenerator : EditorWindow
{
    // 图标颜色配置
    private Color bodyColor = new Color(1f, 0.5f, 0.2f, 1f);        // 橙色主体
    private Color glowColor = new Color(0.6f, 0.3f, 1f, 1f);        // 紫色虫洞发光
    private Color ringColor = new Color(0.3f, 0.3f, 0.4f, 1f);      // 金属环带
    private Color coreColor = new Color(0.4f, 0.7f, 1f, 1f);        // 蓝色能量核心

    [MenuItem("Tools/微型虫洞/虫洞手雷图标生成器")]
    public static void ShowWindow()
    {
        var window = GetWindow<WormholeGrenadeIconGenerator>("虫洞手雷图标生成器");
        window.minSize = new Vector2(400, 500);
    }

    void OnGUI()
    {
        GUILayout.Label("虫洞手雷图标生成器", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 颜色配置
        EditorGUILayout.LabelField("颜色配置", EditorStyles.boldLabel);
        bodyColor = EditorGUILayout.ColorField("手雷主体颜色", bodyColor);
        glowColor = EditorGUILayout.ColorField("虫洞发光色", glowColor);
        ringColor = EditorGUILayout.ColorField("金属环带颜色", ringColor);
        coreColor = EditorGUILayout.ColorField("能量核心颜色", coreColor);

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "图标规格：\n" +
            "• 尺寸：256x256 像素\n" +
            "• 格式：PNG（支持透明）\n" +
            "• 样式：虫洞手雷 + 空间扭曲效果",
            MessageType.Info
        );

        EditorGUILayout.Space();

        // 生成按钮
        if (GUILayout.Button("生成手雷图标", GUILayout.Height(40)))
        {
            GenerateGrenadeIcon();
        }

        EditorGUILayout.Space();

        // 一键生成并部署
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("一键生成图标到 Assets/Icons/", GUILayout.Height(40)))
        {
            GenerateIconToAssetsFolder();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // 生成所有图标
        EditorGUILayout.LabelField("批量操作", EditorStyles.boldLabel);
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("生成所有虫洞物品图标", GUILayout.Height(40)))
        {
            GenerateAllIcons();
        }
        GUI.backgroundColor = Color.white;
    }

    /// <summary>
    /// 生成手雷图标（选择保存位置）
    /// </summary>
    private void GenerateGrenadeIcon()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "保存手雷图标",
            "WormholeGrenadeIcon",
            "png",
            "请选择保存位置",
            "Assets/Icons"
        );

        if (!string.IsNullOrEmpty(path))
        {
            GenerateAndSaveGrenadeIcon(path);
            EditorUtility.DisplayDialog("成功", $"图标已保存到:\n{path}", "确定");
        }
    }

    /// <summary>
    /// 一键生成图标到 Assets/Icons/
    /// </summary>
    private void GenerateIconToAssetsFolder()
    {
        string iconFolder = "Assets/Icons";

        // 确保目录存在
        if (!Directory.Exists(iconFolder))
        {
            Directory.CreateDirectory(iconFolder);
        }

        string path = Path.Combine(iconFolder, "WormholeGrenadeIcon.png");
        GenerateAndSaveGrenadeIcon(path);

        Debug.Log($"[虫洞手雷] 图标已生成到: {path}");
        EditorUtility.DisplayDialog("成功", $"图标已生成到:\n{path}\n\n请重新构建 AssetBundle", "确定");
    }

    /// <summary>
    /// 生成所有虫洞物品图标
    /// </summary>
    private void GenerateAllIcons()
    {
        string iconFolder = "Assets/Icons";

        if (!Directory.Exists(iconFolder))
        {
            Directory.CreateDirectory(iconFolder);
        }

        // 生成虫洞手雷图标
        GenerateAndSaveGrenadeIcon(Path.Combine(iconFolder, "WormholeGrenadeIcon.png"));

        // 生成微型虫洞图标
        GenerateAndSaveWormholeIcon(Path.Combine(iconFolder, "MicroWormholeIcon.png"), new Color(0.5f, 0.2f, 0.9f));

        // 生成回溯虫洞图标
        GenerateAndSaveWormholeIcon(Path.Combine(iconFolder, "WormholeRecallIcon.png"), new Color(0.2f, 0.8f, 0.5f));

        AssetDatabase.Refresh();

        Debug.Log("[虫洞手雷] 所有图标生成完成");
        EditorUtility.DisplayDialog("成功", "所有图标已生成到 Assets/Icons/\n\n请重新构建 AssetBundle", "确定");
    }

    /// <summary>
    /// 生成并保存手雷图标
    /// </summary>
    private void GenerateAndSaveGrenadeIcon(string path)
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        // 清空为透明
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        Vector2 center = new Vector2(size / 2f, size / 2f);

        // 绘制空间扭曲背景
        DrawSpaceDistortion(pixels, size, center);

        // 绘制手雷主体
        DrawGrenadeBody(pixels, size, center);

        // 绘制金属环带
        DrawMetalRings(pixels, size, center);

        // 绘制能量核心
        DrawEnergyCore(pixels, size, center);

        // 绘制虫洞光晕
        DrawWormholeGlow(pixels, size, center);

        tex.SetPixels(pixels);
        tex.Apply();

        // 保存为 PNG
        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(path, pngData);

        DestroyImmediate(tex);

        // 如果是 Assets 目录下的文件，设置导入设置
        if (path.StartsWith("Assets/"))
        {
            AssetDatabase.Refresh();
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }
        }
    }

    /// <summary>
    /// 生成并保存虫洞图标（球形）
    /// </summary>
    private void GenerateAndSaveWormholeIcon(string path, Color mainColor)
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size * 0.35f;

        // 绘制发光背景
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float dist = Vector2.Distance(pos, center);

                if (dist < radius * 1.5f)
                {
                    float t = 1f - (dist / (radius * 1.5f));
                    float alpha = Mathf.Clamp01(t * t * 0.5f);

                    Color col = mainColor;
                    col.a = alpha;

                    int idx = y * size + x;
                    pixels[idx] = Color.Lerp(pixels[idx], col, col.a);
                }
            }
        }

        // 绘制球体
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float dist = Vector2.Distance(pos, center);

                if (dist < radius)
                {
                    float edgeFade = 1f - (dist / radius);
                    float brightness = 0.5f + edgeFade * 0.5f;

                    Color col = mainColor * brightness;
                    col.a = Mathf.Clamp01(edgeFade * 2f);

                    int idx = y * size + x;
                    pixels[idx] = Color.Lerp(pixels[idx], col, col.a);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(path, pngData);

        DestroyImmediate(tex);

        if (path.StartsWith("Assets/"))
        {
            AssetDatabase.Refresh();
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }
        }
    }

    /// <summary>
    /// 绘制空间扭曲背景
    /// </summary>
    private void DrawSpaceDistortion(Color[] pixels, int size, Vector2 center)
    {
        float outerRadius = size * 0.48f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float dist = Vector2.Distance(pos, center);

                if (dist < outerRadius)
                {
                    // 螺旋扭曲效果
                    float angle = Mathf.Atan2(pos.y - center.y, pos.x - center.x);
                    float spiral = Mathf.Sin(angle * 3f + dist * 0.1f) * 0.5f + 0.5f;

                    float t = 1f - (dist / outerRadius);
                    float alpha = Mathf.Clamp01(t * t * 0.3f * spiral);

                    Color col = Color.Lerp(glowColor, coreColor, spiral);
                    col.a = alpha;

                    int idx = y * size + x;
                    pixels[idx] = Color.Lerp(pixels[idx], col, col.a);
                }
            }
        }
    }

    /// <summary>
    /// 绘制手雷主体（椭圆形）
    /// </summary>
    private void DrawGrenadeBody(Color[] pixels, int size, Vector2 center)
    {
        float radiusX = size * 0.15f;
        float radiusY = size * 0.22f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                Vector2 diff = pos - center;

                // 椭圆方程
                float ellipse = (diff.x * diff.x) / (radiusX * radiusX) +
                               (diff.y * diff.y) / (radiusY * radiusY);

                if (ellipse < 1f)
                {
                    float edgeFade = 1f - ellipse;
                    float brightness = 0.6f + edgeFade * 0.4f;

                    // 添加高光
                    float highlight = Mathf.Max(0, diff.x / radiusX * 0.3f + diff.y / radiusY * 0.3f);

                    Color col = bodyColor * brightness;
                    col = Color.Lerp(col, Color.white, highlight * edgeFade);
                    col.a = Mathf.Clamp01(edgeFade * 3f);

                    int idx = y * size + x;
                    pixels[idx] = Color.Lerp(pixels[idx], col, col.a);
                }
            }
        }
    }

    /// <summary>
    /// 绘制金属环带
    /// </summary>
    private void DrawMetalRings(Color[] pixels, int size, Vector2 center)
    {
        float ringWidth = size * 0.18f;
        float ringHeight = size * 0.015f;

        // 上环
        DrawRing(pixels, size, center + new Vector2(0, size * 0.08f), ringWidth, ringHeight);

        // 中环
        DrawRing(pixels, size, center, ringWidth * 1.1f, ringHeight);

        // 下环
        DrawRing(pixels, size, center - new Vector2(0, size * 0.08f), ringWidth, ringHeight);
    }

    /// <summary>
    /// 绘制单个环带
    /// </summary>
    private void DrawRing(Color[] pixels, int size, Vector2 ringCenter, float width, float height)
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                Vector2 diff = pos - ringCenter;

                if (Mathf.Abs(diff.x) < width && Mathf.Abs(diff.y) < height)
                {
                    float edgeX = 1f - Mathf.Abs(diff.x) / width;
                    float edgeY = 1f - Mathf.Abs(diff.y) / height;

                    // 金属光泽
                    float metallic = 0.7f + edgeX * 0.3f;

                    Color col = ringColor * metallic;
                    col.a = Mathf.Clamp01(edgeY * 2f);

                    int idx = y * size + x;
                    pixels[idx] = Color.Lerp(pixels[idx], col, col.a);
                }
            }
        }
    }

    /// <summary>
    /// 绘制能量核心
    /// </summary>
    private void DrawEnergyCore(Color[] pixels, int size, Vector2 center)
    {
        float coreRadius = size * 0.06f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float dist = Vector2.Distance(pos, center);

                if (dist < coreRadius)
                {
                    float t = 1f - (dist / coreRadius);
                    float brightness = 0.8f + t * 0.2f;

                    Color col = coreColor * brightness;
                    col.a = Mathf.Clamp01(t * 2f);

                    int idx = y * size + x;
                    pixels[idx] = Color.Lerp(pixels[idx], col, col.a);
                }
            }
        }
    }

    /// <summary>
    /// 绘制虫洞光晕
    /// </summary>
    private void DrawWormholeGlow(Color[] pixels, int size, Vector2 center)
    {
        float glowRadius = size * 0.12f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float dist = Vector2.Distance(pos, center);

                if (dist < glowRadius && dist > glowRadius * 0.3f)
                {
                    float t = 1f - Mathf.Abs(dist - glowRadius * 0.65f) / (glowRadius * 0.35f);
                    float alpha = Mathf.Clamp01(t * 0.6f);

                    Color col = glowColor;
                    col.a = alpha;

                    int idx = y * size + x;
                    pixels[idx] = Color.Lerp(pixels[idx], col, col.a * 0.5f);
                }
            }
        }
    }
}
