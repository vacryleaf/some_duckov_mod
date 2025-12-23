using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 名刀月影图标生成器
/// 用于在 Unity 中生成武器图标
/// </summary>
public class MoonlightSwordIconGenerator : EditorWindow
{
    // 图标颜色配置
    private Color bladeColor = new Color(0.91f, 0.93f, 0.97f, 1f);      // 银白色刀身
    private Color glowColor = new Color(0.63f, 0.78f, 0.91f, 1f);       // 月光蓝发光
    private Color handleColor = new Color(0.23f, 0.18f, 0.35f, 1f);     // 深紫色刀柄
    private Color guardColor = new Color(0.1f, 0.17f, 0.29f, 1f);       // 深蓝色护手

    [MenuItem("Tools/名刀月影/图标生成器")]
    public static void ShowWindow()
    {
        var window = GetWindow<MoonlightSwordIconGenerator>("名刀月影图标生成器");
        window.minSize = new Vector2(400, 500);
    }

    void OnGUI()
    {
        GUILayout.Label("名刀月影图标生成器", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 颜色配置
        EditorGUILayout.LabelField("颜色配置", EditorStyles.boldLabel);
        bladeColor = EditorGUILayout.ColorField("刀身颜色", bladeColor);
        glowColor = EditorGUILayout.ColorField("月光发光色", glowColor);
        handleColor = EditorGUILayout.ColorField("刀柄颜色", handleColor);
        guardColor = EditorGUILayout.ColorField("护手颜色", guardColor);

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "图标规格：\n" +
            "• 尺寸：256x256 像素\n" +
            "• 格式：PNG（支持透明）\n" +
            "• 样式：月牙形刀剑 + 月光光晕",
            MessageType.Info
        );

        EditorGUILayout.Space();

        // 生成按钮
        if (GUILayout.Button("生成武器图标", GUILayout.Height(40)))
        {
            GenerateWeaponIcon();
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

        // 预览图生成
        EditorGUILayout.LabelField("预览图生成", EditorStyles.boldLabel);
        if (GUILayout.Button("生成 Mod 预览图 (256x256)", GUILayout.Height(30)))
        {
            GeneratePreviewImage();
        }
    }

    /// <summary>
    /// 生成武器图标（选择保存位置）
    /// </summary>
    private void GenerateWeaponIcon()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "保存武器图标",
            "MoonlightSwordIcon",
            "png",
            "请选择保存位置",
            "Assets/Icons"
        );

        if (!string.IsNullOrEmpty(path))
        {
            GenerateAndSaveIcon(path);
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

        string path = Path.Combine(iconFolder, "MoonlightSwordIcon.png");
        GenerateAndSaveIcon(path);

        Debug.Log($"[名刀月影] 图标已生成到: {path}");
        EditorUtility.DisplayDialog("成功", $"图标已生成到:\n{path}\n\n请重新构建 AssetBundle", "确定");
    }

    /// <summary>
    /// 生成预览图
    /// </summary>
    private void GeneratePreviewImage()
    {
        string path = EditorUtility.SaveFilePanel(
            "保存预览图",
            "",
            "preview",
            "png"
        );

        if (!string.IsNullOrEmpty(path))
        {
            GenerateAndSaveIcon(path);
            Debug.Log($"[名刀月影] 预览图已保存到: {path}");
            EditorUtility.DisplayDialog("成功", $"预览图已保存到:\n{path}", "确定");
        }
    }

    /// <summary>
    /// 生成并保存图标
    /// </summary>
    private void GenerateAndSaveIcon(string path)
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

        // 绘制月光光晕背景
        DrawMoonlightGlow(pixels, size, center);

        // 绘制刀身
        DrawBlade(pixels, size, center);

        // 绘制护手
        DrawGuard(pixels, size, center);

        // 绘制刀柄
        DrawHandle(pixels, size, center);

        // 添加月牙形剑气效果
        DrawSwordAura(pixels, size, center);

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
    /// 绘制月光光晕
    /// </summary>
    private void DrawMoonlightGlow(Color[] pixels, int size, Vector2 center)
    {
        float outerRadius = size * 0.45f;
        float innerRadius = size * 0.2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float dist = Vector2.Distance(pos, center);

                if (dist < outerRadius)
                {
                    float t = 1f - (dist / outerRadius);
                    float alpha = Mathf.Clamp01(t * t * 0.6f);

                    Color col = glowColor;
                    col.a = alpha;

                    // 混合颜色
                    int idx = y * size + x;
                    pixels[idx] = Color.Lerp(pixels[idx], col, col.a);
                }
            }
        }
    }

    /// <summary>
    /// 绘制刀身
    /// </summary>
    private void DrawBlade(Color[] pixels, int size, Vector2 center)
    {
        // 刀身参数 - 斜向放置
        float bladeLength = size * 0.7f;
        float bladeWidth = size * 0.08f;
        float angle = 45f * Mathf.Deg2Rad; // 45度角

        Vector2 bladeStart = center - new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * bladeLength * 0.3f;
        Vector2 bladeEnd = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * bladeLength * 0.7f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);

                // 计算点到刀身线段的距离
                float dist = DistanceToLineSegment(pos, bladeStart, bladeEnd);

                // 刀身宽度随位置变化（刀尖更窄）
                float t = ProjectionT(pos, bladeStart, bladeEnd);
                float currentWidth = bladeWidth * (1f - t * 0.5f);

                if (dist < currentWidth && t >= 0 && t <= 1)
                {
                    float edgeFade = 1f - (dist / currentWidth);
                    Color col = Color.Lerp(bladeColor, glowColor, 0.3f);
                    col.a = Mathf.Clamp01(edgeFade * 2f);

                    int idx = y * size + x;
                    pixels[idx] = Color.Lerp(pixels[idx], col, col.a);
                }
            }
        }
    }

    /// <summary>
    /// 绘制护手
    /// </summary>
    private void DrawGuard(Color[] pixels, int size, Vector2 center)
    {
        float guardWidth = size * 0.15f;
        float guardHeight = size * 0.03f;

        // 护手位置（刀身下方）
        Vector2 guardCenter = center - new Vector2(size * 0.1f, size * 0.1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                Vector2 diff = pos - guardCenter;

                // 旋转到刀身角度
                float angle = -45f * Mathf.Deg2Rad;
                float rotX = diff.x * Mathf.Cos(angle) - diff.y * Mathf.Sin(angle);
                float rotY = diff.x * Mathf.Sin(angle) + diff.y * Mathf.Cos(angle);

                if (Mathf.Abs(rotX) < guardWidth && Mathf.Abs(rotY) < guardHeight)
                {
                    int idx = y * size + x;
                    pixels[idx] = guardColor;
                }
            }
        }
    }

    /// <summary>
    /// 绘制刀柄
    /// </summary>
    private void DrawHandle(Color[] pixels, int size, Vector2 center)
    {
        float handleLength = size * 0.2f;
        float handleWidth = size * 0.04f;
        float angle = 45f * Mathf.Deg2Rad;

        Vector2 handleStart = center - new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (size * 0.15f);
        Vector2 handleEnd = center - new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (size * 0.15f + handleLength);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float dist = DistanceToLineSegment(pos, handleStart, handleEnd);
                float t = ProjectionT(pos, handleStart, handleEnd);

                if (dist < handleWidth && t >= 0 && t <= 1)
                {
                    int idx = y * size + x;
                    pixels[idx] = handleColor;
                }
            }
        }
    }

    /// <summary>
    /// 绘制剑气效果
    /// </summary>
    private void DrawSwordAura(Color[] pixels, int size, Vector2 center)
    {
        // 月牙形剑气
        float auraRadius = size * 0.35f;
        float auraWidth = size * 0.08f;
        Vector2 auraCenter = center + new Vector2(size * 0.15f, size * 0.15f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float dist = Vector2.Distance(pos, auraCenter);

                // 只绘制弧形的一部分（月牙形）
                Vector2 dir = (pos - auraCenter).normalized;
                float angleToPoint = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // 限制角度范围形成月牙
                if (angleToPoint > -30f && angleToPoint < 120f)
                {
                    float distFromArc = Mathf.Abs(dist - auraRadius);
                    if (distFromArc < auraWidth)
                    {
                        float fade = 1f - (distFromArc / auraWidth);
                        Color col = glowColor;
                        col.a = fade * 0.7f;

                        int idx = y * size + x;
                        pixels[idx] = Color.Lerp(pixels[idx], col, col.a);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 计算点到线段的距离
    /// </summary>
    private float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart;
        float len = line.magnitude;
        line.Normalize();

        Vector2 v = point - lineStart;
        float d = Vector2.Dot(v, line);
        d = Mathf.Clamp(d, 0, len);

        Vector2 closest = lineStart + line * d;
        return Vector2.Distance(point, closest);
    }

    /// <summary>
    /// 计算点在线段上的投影参数 t (0-1)
    /// </summary>
    private float ProjectionT(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart;
        float len = line.magnitude;
        if (len < 0.001f) return 0;

        Vector2 v = point - lineStart;
        return Vector2.Dot(v, line.normalized) / len;
    }
}
