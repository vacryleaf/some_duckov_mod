using UnityEngine;
using UnityEditor;

/// <summary>
/// 剑气特效生成器 - Unity编辑器工具
/// 用于生成名刀月影的剑气特效Prefab
/// </summary>
public class SwordAuraEffectGenerator : EditorWindow
{
    // 剑气参数
    private float auraWidth = 2f;
    private float auraHeight = 1f;
    private Color auraColor = new Color(0.44f, 0.69f, 0.88f, 0.7f);
    private float emissionIntensity = 2f;

    // 粒子系统参数
    private int particleCount = 100;
    private float particleSize = 0.1f;
    private float particleLifetime = 0.5f;
    private float particleSpeed = 2f;

    // 生成的对象
    private GameObject auraRoot;

    [MenuItem("Tools/名刀月影/剑气特效生成器")]
    public static void ShowWindow()
    {
        GetWindow<SwordAuraEffectGenerator>("剑气特效生成器");
    }

    void OnGUI()
    {
        GUILayout.Label("剑气特效生成器", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 剑气尺寸
        GUILayout.Label("剑气尺寸", EditorStyles.boldLabel);
        auraWidth = EditorGUILayout.Slider("宽度", auraWidth, 0.5f, 5f);
        auraHeight = EditorGUILayout.Slider("高度", auraHeight, 0.5f, 3f);

        EditorGUILayout.Space();

        // 剑气颜色
        GUILayout.Label("剑气外观", EditorStyles.boldLabel);
        auraColor = EditorGUILayout.ColorField("剑气颜色", auraColor);
        emissionIntensity = EditorGUILayout.Slider("发光强度", emissionIntensity, 0.5f, 5f);

        EditorGUILayout.Space();

        // 粒子参数
        GUILayout.Label("粒子效果", EditorStyles.boldLabel);
        particleCount = EditorGUILayout.IntSlider("粒子数量", particleCount, 20, 500);
        particleSize = EditorGUILayout.Slider("粒子大小", particleSize, 0.02f, 0.3f);
        particleLifetime = EditorGUILayout.Slider("粒子生命周期", particleLifetime, 0.2f, 2f);
        particleSpeed = EditorGUILayout.Slider("粒子速度", particleSpeed, 0.5f, 10f);

        EditorGUILayout.Space();

        // 生成按钮
        if (GUILayout.Button("生成剑气特效", GUILayout.Height(40)))
        {
            GenerateSwordAura();
        }

        EditorGUILayout.Space();

        // 信息显示
        if (auraRoot != null)
        {
            EditorGUILayout.HelpBox($"剑气特效已生成: {auraRoot.name}", MessageType.Info);

            if (GUILayout.Button("选中特效"))
            {
                Selection.activeGameObject = auraRoot;
                SceneView.FrameLastActiveSceneView();
            }

            if (GUILayout.Button("保存为Prefab"))
            {
                SaveAsPrefab();
            }

            if (GUILayout.Button("预览效果"))
            {
                PreviewEffect();
            }
        }
    }

    /// <summary>
    /// 生成剑气特效
    /// </summary>
    private void GenerateSwordAura()
    {
        Debug.Log("[剑气特效] 开始生成...");

        // 创建根对象
        auraRoot = new GameObject("SwordAuraPrefab");
        auraRoot.transform.position = Vector3.zero;
        auraRoot.transform.rotation = Quaternion.identity;

        // 创建月牙形状
        CreateCrescentShape(auraRoot.transform);

        // 创建粒子系统
        CreateParticleSystem(auraRoot.transform);

        // 创建光晕效果
        CreateGlowEffect(auraRoot.transform);

        // 添加投射物组件占位(需要实际脚本)
        // auraRoot.AddComponent<SwordAuraProjectile>();

        // 选中生成的对象
        Selection.activeGameObject = auraRoot;
        SceneView.FrameLastActiveSceneView();

        Debug.Log("[剑气特效] 生成完成!");
    }

    /// <summary>
    /// 创建月牙形状
    /// </summary>
    private void CreateCrescentShape(Transform parent)
    {
        GameObject crescent = GameObject.CreatePrimitive(PrimitiveType.Quad);
        crescent.name = "CrescentShape";
        crescent.transform.SetParent(parent);
        crescent.transform.localPosition = Vector3.zero;
        crescent.transform.localRotation = Quaternion.identity;
        crescent.transform.localScale = new Vector3(auraWidth, auraHeight, 1f);

        // 创建发光材质
        Material auraMaterial = CreateAuraMaterial();
        crescent.GetComponent<Renderer>().material = auraMaterial;

        // 移除碰撞体
        DestroyImmediate(crescent.GetComponent<Collider>());

        // 添加网格变形(创建月牙形状)
        CreateCrescentMesh(crescent);

        Debug.Log("[剑气特效] 月牙形状创建完成");
    }

    /// <summary>
    /// 创建月牙形网格
    /// </summary>
    private void CreateCrescentMesh(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        Mesh mesh = new Mesh();
        mesh.name = "CrescentMesh";

        // 顶点数据(月牙形)
        Vector3[] vertices = new Vector3[12];
        int segments = 10;

        // 外弧(右侧凸起)
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.PI * (i / (float)segments - 0.5f);
            float x = Mathf.Cos(angle) * 0.5f;
            float y = Mathf.Sin(angle) * 0.5f;

            if (i < vertices.Length)
            {
                vertices[i] = new Vector3(x + 0.15f, y, 0);
            }
        }

        // UV坐标
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x + 0.5f, vertices[i].y + 0.5f);
        }

        // 三角形索引
        int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    /// <summary>
    /// 创建粒子系统
    /// </summary>
    private void CreateParticleSystem(Transform parent)
    {
        GameObject particleObj = new GameObject("Particles");
        particleObj.transform.SetParent(parent);
        particleObj.transform.localPosition = Vector3.zero;

        ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();

        // 主模块
        var main = particles.main;
        main.startColor = new Color(1f, 1f, 1f, 0.8f);
        main.startSize = particleSize;
        main.startLifetime = particleLifetime;
        main.startSpeed = particleSpeed;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        // 发射模块
        var emission = particles.emission;
        emission.rateOverTime = particleCount / particleLifetime;

        // 形状模块
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(auraWidth * 0.8f, auraHeight * 0.8f, 0.1f);

        // 速度模块
        var velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.z = new ParticleSystem.MinMaxCurve(particleSpeed * 0.5f);

        // 颜色模块
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(auraColor, 0f),
                new GradientColorKey(Color.white, 0.5f),
                new GradientColorKey(auraColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        // 大小模块
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(1f, 0.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // 渲染模块
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateParticleMaterial();

        Debug.Log("[剑气特效] 粒子系统创建完成");
    }

    /// <summary>
    /// 创建光晕效果
    /// </summary>
    private void CreateGlowEffect(Transform parent)
    {
        GameObject glow = new GameObject("GlowEffect");
        glow.transform.SetParent(parent);
        glow.transform.localPosition = Vector3.zero;

        // 添加光源
        Light light = glow.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = auraColor;
        light.intensity = emissionIntensity * 2f;
        light.range = auraWidth * 2f;
        light.shadows = LightShadows.None;

        Debug.Log("[剑气特效] 光晕效果创建完成");
    }

    /// <summary>
    /// 创建剑气材质
    /// </summary>
    private Material CreateAuraMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "SwordAura_Material";

        mat.color = auraColor;

        // 启用透明模式
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        // 启用发光
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", auraColor * emissionIntensity);

        return mat;
    }

    /// <summary>
    /// 创建粒子材质
    /// </summary>
    private Material CreateParticleMaterial()
    {
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.name = "SwordAura_Particle";

        mat.SetColor("_Color", Color.white);
        mat.SetFloat("_Mode", 2); // Fade
        mat.EnableKeyword("_ALPHABLEND_ON");

        return mat;
    }

    /// <summary>
    /// 保存为Prefab
    /// </summary>
    private void SaveAsPrefab()
    {
        if (auraRoot == null)
        {
            EditorUtility.DisplayDialog("错误", "没有可保存的特效", "确定");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "保存剑气特效Prefab",
            "SwordAuraPrefab",
            "prefab",
            "选择保存位置"
        );

        if (!string.IsNullOrEmpty(path))
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(auraRoot, path);

            if (prefab != null)
            {
                Debug.Log($"[剑气特效] Prefab已保存: {path}");
                EditorUtility.DisplayDialog("成功", $"Prefab已保存到:\n{path}", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "Prefab保存失败", "确定");
            }
        }
    }

    /// <summary>
    /// 预览效果
    /// </summary>
    private void PreviewEffect()
    {
        if (auraRoot == null) return;

        // 播放粒子系统
        ParticleSystem[] particles = auraRoot.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Stop();
            ps.Play();
        }

        Debug.Log("[剑气特效] 正在预览...");
    }
}
