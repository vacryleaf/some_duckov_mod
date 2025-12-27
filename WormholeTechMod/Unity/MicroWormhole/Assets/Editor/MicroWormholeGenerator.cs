using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 微型虫洞物品生成器
/// 用于在 Unity 中创建物品 Prefab 并设置图标
/// </summary>
public class MicroWormholeGenerator : EditorWindow
{
    // 生成的物品
    private GameObject generatedItem;

    // 物品参数
    private float sphereRadius = 0.1f;
    private Color mainColor = new Color(0.5f, 0.2f, 0.9f, 1f);  // 紫色
    private Color glowColor = new Color(0.6f, 0.3f, 1f, 1f);    // 发光紫色
    private float glowIntensity = 2f;

    // 图标设置
    private Sprite itemIcon;
    private Texture2D iconTexture;

    [MenuItem("Tools/微型虫洞/物品生成器")]
    public static void ShowWindow()
    {
        var window = GetWindow<MicroWormholeGenerator>("微型虫洞生成器");
        window.minSize = new Vector2(400, 600);
    }

    void OnGUI()
    {
        GUILayout.Label("微型虫洞物品生成器", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 物品外观设置
        EditorGUILayout.LabelField("物品外观", EditorStyles.boldLabel);
        sphereRadius = EditorGUILayout.FloatField("球体半径", sphereRadius);
        mainColor = EditorGUILayout.ColorField("主颜色", mainColor);
        glowColor = EditorGUILayout.ColorField("发光颜色", glowColor);
        glowIntensity = EditorGUILayout.FloatField("发光强度", glowIntensity);

        EditorGUILayout.Space();

        // 图标设置
        EditorGUILayout.LabelField("物品图标", EditorStyles.boldLabel);
        iconTexture = (Texture2D)EditorGUILayout.ObjectField("图标贴图", iconTexture, typeof(Texture2D), false);

        EditorGUILayout.HelpBox(
            "图标要求：\n" +
            "• 建议尺寸：128x128 或 256x256\n" +
            "• 格式：PNG（支持透明）\n" +
            "• 导入设置：Texture Type = Sprite",
            MessageType.Info
        );

        EditorGUILayout.Space();

        // 生成按钮
        if (GUILayout.Button("生成物品模型", GUILayout.Height(40)))
        {
            GenerateWormholeItem();
        }

        EditorGUILayout.Space();

        // 如果有生成的物品，显示保存按钮
        if (generatedItem != null)
        {
            EditorGUILayout.HelpBox($"已生成: {generatedItem.name}", MessageType.Info);

            if (GUILayout.Button("保存为 Prefab", GUILayout.Height(30)))
            {
                SaveAsPrefab();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // 快速创建图标
        EditorGUILayout.LabelField("快速创建图标", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("生成微型虫洞图标（紫色）", GUILayout.Height(30)))
        {
            GenerateProceduralIcon(
                "MicroWormholeIcon",
                new Color(0.8f, 0.5f, 1f, 1f),      // 核心颜色 - 亮紫色
                new Color(0.6f, 0.3f, 1f, 1f),      // 内圈颜色 - 紫色
                new Color(0.3f, 0.1f, 0.5f, 0.5f)   // 外圈颜色 - 深紫色
            );
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("生成回溯虫洞图标（绿色）", GUILayout.Height(30)))
        {
            GenerateProceduralIcon(
                "WormholeRecallIcon",
                new Color(0.5f, 1f, 0.7f, 1f),      // 核心颜色 - 亮绿色
                new Color(0.2f, 0.9f, 0.5f, 1f),    // 内圈颜色 - 绿色
                new Color(0.1f, 0.4f, 0.2f, 0.5f)   // 外圈颜色 - 深绿色
            );
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 一键生成两个图标
        if (GUILayout.Button("一键生成两个图标", GUILayout.Height(40)))
        {
            GenerateBothIcons();
        }
    }

    /// <summary>
    /// 生成虫洞物品模型
    /// </summary>
    private void GenerateWormholeItem()
    {
        // 清理旧物品
        if (generatedItem != null)
        {
            DestroyImmediate(generatedItem);
        }

        // 创建主物体
        generatedItem = new GameObject("MicroWormhole");

        // 创建外层球体（半透明）
        GameObject outerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        outerSphere.name = "OuterSphere";
        outerSphere.transform.SetParent(generatedItem.transform);
        outerSphere.transform.localPosition = Vector3.zero;
        outerSphere.transform.localScale = Vector3.one * sphereRadius * 2f;

        // 外层材质（半透明发光）
        Material outerMat = new Material(Shader.Find("Standard"));
        outerMat.color = new Color(mainColor.r, mainColor.g, mainColor.b, 0.3f);
        outerMat.SetFloat("_Mode", 3); // Transparent
        outerMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        outerMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        outerMat.EnableKeyword("_EMISSION");
        outerMat.SetColor("_EmissionColor", glowColor * glowIntensity);
        outerMat.renderQueue = 3000;
        outerSphere.GetComponent<Renderer>().material = outerMat;
        DestroyImmediate(outerSphere.GetComponent<Collider>());

        // 创建内层球体（实心核心）
        GameObject innerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        innerSphere.name = "InnerCore";
        innerSphere.transform.SetParent(generatedItem.transform);
        innerSphere.transform.localPosition = Vector3.zero;
        innerSphere.transform.localScale = Vector3.one * sphereRadius * 1.2f;

        // 内层材质（实心发光）
        Material innerMat = new Material(Shader.Find("Standard"));
        innerMat.color = mainColor;
        innerMat.SetFloat("_Metallic", 0.9f);
        innerMat.SetFloat("_Glossiness", 1f);
        innerMat.EnableKeyword("_EMISSION");
        innerMat.SetColor("_EmissionColor", glowColor * (glowIntensity * 1.5f));
        innerSphere.GetComponent<Renderer>().material = innerMat;
        DestroyImmediate(innerSphere.GetComponent<Collider>());

        // 添加拾取碰撞体
        SphereCollider collider = generatedItem.AddComponent<SphereCollider>();
        collider.radius = sphereRadius;

        // 选中生成的物品
        Selection.activeGameObject = generatedItem;

        ModLogger.Log("[虫洞科技] 物品模型生成完成");
    }

    /// <summary>
    /// 保存为 Prefab
    /// </summary>
    private void SaveAsPrefab()
    {
        if (generatedItem == null) return;

        string path = EditorUtility.SaveFilePanelInProject(
            "保存物品 Prefab",
            "MicroWormhole",
            "prefab",
            "请选择保存位置"
        );

        if (string.IsNullOrEmpty(path)) return;

        // 保存 Prefab
        PrefabUtility.SaveAsPrefabAsset(generatedItem, path);

        ModLogger.Log($"[虫洞科技] Prefab 已保存到: {path}");
        EditorUtility.DisplayDialog("成功", $"Prefab 已保存到:\n{path}", "确定");
    }

    /// <summary>
    /// 一键生成两个图标
    /// </summary>
    private void GenerateBothIcons()
    {
        string iconFolder = "Assets/Icons";

        // 确保目录存在
        if (!Directory.Exists(iconFolder))
        {
            Directory.CreateDirectory(iconFolder);
        }

        // 生成微型虫洞图标（紫色）
        GenerateAndSaveIcon(
            Path.Combine(iconFolder, "MicroWormholeIcon.png"),
            new Color(0.8f, 0.5f, 1f, 1f),
            new Color(0.6f, 0.3f, 1f, 1f),
            new Color(0.3f, 0.1f, 0.5f, 0.5f)
        );

        // 生成回溯虫洞图标（绿色）
        GenerateAndSaveIcon(
            Path.Combine(iconFolder, "WormholeRecallIcon.png"),
            new Color(0.5f, 1f, 0.7f, 1f),
            new Color(0.2f, 0.9f, 0.5f, 1f),
            new Color(0.1f, 0.4f, 0.2f, 0.5f)
        );

        AssetDatabase.Refresh();

        ModLogger.Log("[虫洞科技] 两个图标已生成到 Assets/Icons/");
        EditorUtility.DisplayDialog("成功", "已生成两个图标：\n• MicroWormholeIcon.png（紫色）\n• WormholeRecallIcon.png（绿色）\n\n保存位置：Assets/Icons/", "确定");
    }

    /// <summary>
    /// 生成并保存图标
    /// </summary>
    private void GenerateAndSaveIcon(string path, Color coreColor, Color innerColor, Color outerColor)
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float outerRadius = size * 0.4f;
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
                    float alpha = Mathf.Clamp01(t * 2f);

                    Color col;
                    if (dist < innerRadius)
                    {
                        col = coreColor;
                    }
                    else
                    {
                        float blend = (dist - innerRadius) / (outerRadius - innerRadius);
                        col = Color.Lerp(innerColor, outerColor, blend);
                    }

                    col.a = alpha;
                    pixels[y * size + x] = col;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(path, pngData);

        DestroyImmediate(tex);

        // 设置导入设置为 Sprite
        AssetDatabase.Refresh();
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
    }

    /// <summary>
    /// 生成程序化图标（带对话框选择保存位置）
    /// </summary>
    private void GenerateProceduralIcon(string defaultName, Color coreColor, Color innerColor, Color outerColor)
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        // 清空为透明
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        // 绘制虫洞效果
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float outerRadius = size * 0.4f;
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
                    float alpha = Mathf.Clamp01(t * 2f);

                    Color col;
                    if (dist < innerRadius)
                    {
                        // 核心区域 - 更亮
                        col = coreColor;
                    }
                    else
                    {
                        // 外围 - 渐变
                        float blend = (dist - innerRadius) / (outerRadius - innerRadius);
                        col = Color.Lerp(innerColor, outerColor, blend);
                    }

                    col.a = alpha;
                    pixels[y * size + x] = col;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        // 保存为 PNG
        string path = EditorUtility.SaveFilePanelInProject(
            "保存物品图标",
            defaultName,
            "png",
            "请选择保存位置",
            "Assets/Icons"
        );

        if (!string.IsNullOrEmpty(path))
        {
            byte[] pngData = tex.EncodeToPNG();
            File.WriteAllBytes(path, pngData);
            AssetDatabase.Refresh();

            // 设置导入设置为 Sprite
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }

            ModLogger.Log($"[虫洞科技] 图标已保存到: {path}");
            EditorUtility.DisplayDialog("成功", $"图标已保存到:\n{path}", "确定");
        }

        DestroyImmediate(tex);
    }
}
