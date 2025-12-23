using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 名刀月影模型生成器 - Unity编辑器工具
/// 用于在Unity编辑器中快速生成名刀月影的3D模型
/// </summary>
public class MoonlightSwordModelGenerator : EditorWindow
{
    // 模型参数
    private float bladeLength = 1.2f;
    private float bladeWidth = 0.08f;
    private float bladeThickness = 0.02f;
    private float handleLength = 0.2f;
    private float handleRadius = 0.025f;
    private float guardWidth = 0.15f;
    private float guardThickness = 0.02f;

    // 材质颜色
    private Color bladeColor = new Color(0.91f, 0.91f, 0.94f, 1f); // 银白色
    private Color bladeEmissionColor = new Color(0.63f, 0.78f, 0.91f, 1f); // 淡蓝色
    private Color guardColor = new Color(0.1f, 0.17f, 0.29f, 1f); // 深蓝色
    private Color handleColor = new Color(0.23f, 0.18f, 0.35f, 1f); // 深紫色

    // 材质属性
    private float bladeMetallic = 0.95f;
    private float bladeSmoothness = 0.9f;
    private float emissionIntensity = 0.5f;

    // 生成的对象引用
    private GameObject swordRoot;

    [MenuItem("Tools/名刀月影/模型生成器")]
    public static void ShowWindow()
    {
        GetWindow<MoonlightSwordModelGenerator>("名刀月影模型生成器");
    }

    void OnGUI()
    {
        GUILayout.Label("名刀月影 - 3D模型生成器", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 模型尺寸参数
        GUILayout.Label("模型尺寸 (米)", EditorStyles.boldLabel);
        bladeLength = EditorGUILayout.Slider("刀身长度", bladeLength, 0.5f, 2f);
        bladeWidth = EditorGUILayout.Slider("刀身宽度", bladeWidth, 0.03f, 0.15f);
        bladeThickness = EditorGUILayout.Slider("刀身厚度", bladeThickness, 0.01f, 0.05f);
        handleLength = EditorGUILayout.Slider("刀柄长度", handleLength, 0.1f, 0.4f);
        handleRadius = EditorGUILayout.Slider("刀柄半径", handleRadius, 0.015f, 0.04f);
        guardWidth = EditorGUILayout.Slider("护手宽度", guardWidth, 0.08f, 0.25f);
        guardThickness = EditorGUILayout.Slider("护手厚度", guardThickness, 0.01f, 0.04f);

        EditorGUILayout.Space();

        // 材质颜色
        GUILayout.Label("材质颜色", EditorStyles.boldLabel);
        bladeColor = EditorGUILayout.ColorField("刀身颜色", bladeColor);
        bladeEmissionColor = EditorGUILayout.ColorField("刀身发光颜色", bladeEmissionColor);
        guardColor = EditorGUILayout.ColorField("护手颜色", guardColor);
        handleColor = EditorGUILayout.ColorField("刀柄颜色", handleColor);

        EditorGUILayout.Space();

        // 材质属性
        GUILayout.Label("材质属性", EditorStyles.boldLabel);
        bladeMetallic = EditorGUILayout.Slider("刀身金属度", bladeMetallic, 0f, 1f);
        bladeSmoothness = EditorGUILayout.Slider("刀身光滑度", bladeSmoothness, 0f, 1f);
        emissionIntensity = EditorGUILayout.Slider("发光强度", emissionIntensity, 0f, 2f);

        EditorGUILayout.Space();

        // 生成按钮
        if (GUILayout.Button("生成刀模型", GUILayout.Height(40)))
        {
            GenerateSword();
        }

        EditorGUILayout.Space();

        // 信息显示
        if (swordRoot != null)
        {
            EditorGUILayout.HelpBox($"刀模型已生成: {swordRoot.name}\n总长度: {bladeLength + handleLength:F2}米", MessageType.Info);

            if (GUILayout.Button("选中模型"))
            {
                Selection.activeGameObject = swordRoot;
                SceneView.FrameLastActiveSceneView();
            }

            if (GUILayout.Button("保存为Prefab"))
            {
                SaveAsPrefab();
            }
        }
    }

    /// <summary>
    /// 生成刀模型
    /// </summary>
    private void GenerateSword()
    {
        Debug.Log("[名刀月影] 开始生成模型...");

        // 创建根对象
        swordRoot = new GameObject("MoonlightSword");
        swordRoot.transform.position = Vector3.zero;
        swordRoot.transform.rotation = Quaternion.identity;

        // 生成各部分
        CreateBlade(swordRoot.transform);
        CreateGuard(swordRoot.transform);
        CreateHandle(swordRoot.transform);
        CreateColliders(swordRoot);
        CreateEffectsNode(swordRoot.transform);

        // 添加Item组件占位
        // swordRoot.AddComponent<Item>(); // 需要游戏的Item组件

        // 选中生成的对象
        Selection.activeGameObject = swordRoot;
        SceneView.FrameLastActiveSceneView();

        Debug.Log($"[名刀月影] 模型生成完成! 总长度: {bladeLength + handleLength:F2}米");
    }

    /// <summary>
    /// 创建刀身
    /// </summary>
    private void CreateBlade(Transform parent)
    {
        // 主刀身
        GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blade.name = "Blade";
        blade.transform.SetParent(parent);

        // 位置: 刀身中心在刀柄上方
        float bladeCenter = handleLength + bladeLength * 0.5f;
        blade.transform.localPosition = new Vector3(0, bladeCenter, 0);
        blade.transform.localRotation = Quaternion.identity;
        blade.transform.localScale = new Vector3(bladeWidth, bladeLength, bladeThickness);

        // 创建刀身材质
        Material bladeMaterial = CreateBladeMaterial();
        blade.GetComponent<Renderer>().material = bladeMaterial;

        // 移除默认碰撞体(我们会单独添加)
        DestroyImmediate(blade.GetComponent<Collider>());

        // 创建刀尖
        CreateBladeTip(blade.transform);

        Debug.Log($"[名刀月影] 刀身创建完成 - 长度: {bladeLength}m, 宽度: {bladeWidth}m");
    }

    /// <summary>
    /// 创建刀尖
    /// </summary>
    private void CreateBladeTip(Transform bladeParent)
    {
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tip.name = "BladeTip";
        tip.transform.SetParent(bladeParent);

        // 位置: 刀身顶端
        tip.transform.localPosition = new Vector3(0, bladeLength * 0.5f + 0.05f, 0);
        tip.transform.localRotation = Quaternion.Euler(0, 0, 45f); // 旋转45度形成尖端
        tip.transform.localScale = new Vector3(0.06f, 0.1f, bladeThickness / bladeLength);

        // 使用相同材质
        Material bladeMaterial = bladeParent.GetComponent<Renderer>().sharedMaterial;
        tip.GetComponent<Renderer>().material = bladeMaterial;

        // 移除碰撞体
        DestroyImmediate(tip.GetComponent<Collider>());
    }

    /// <summary>
    /// 创建护手
    /// </summary>
    private void CreateGuard(Transform parent)
    {
        GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        guard.name = "Guard";
        guard.transform.SetParent(parent);

        // 位置: 刀柄顶部
        guard.transform.localPosition = new Vector3(0, handleLength, 0);
        guard.transform.localRotation = Quaternion.identity;
        guard.transform.localScale = new Vector3(guardWidth, guardThickness, bladeThickness * 2);

        // 创建护手材质
        Material guardMaterial = CreateGuardMaterial();
        guard.GetComponent<Renderer>().material = guardMaterial;

        // 移除碰撞体
        DestroyImmediate(guard.GetComponent<Collider>());

        // 添加月牙装饰
        CreateMoonDecoration(guard.transform);

        Debug.Log($"[名刀月影] 护手创建完成 - 宽度: {guardWidth}m");
    }

    /// <summary>
    /// 创建月牙装饰
    /// </summary>
    private void CreateMoonDecoration(Transform guardParent)
    {
        // 左侧月牙
        GameObject leftMoon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftMoon.name = "MoonDecoration_Left";
        leftMoon.transform.SetParent(guardParent);
        leftMoon.transform.localPosition = new Vector3(-guardWidth * 0.3f, 0, 0);
        leftMoon.transform.localScale = new Vector3(0.03f, 0.03f, 0.01f);
        leftMoon.GetComponent<Renderer>().material = guardParent.GetComponent<Renderer>().sharedMaterial;
        DestroyImmediate(leftMoon.GetComponent<Collider>());

        // 右侧月牙
        GameObject rightMoon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightMoon.name = "MoonDecoration_Right";
        rightMoon.transform.SetParent(guardParent);
        rightMoon.transform.localPosition = new Vector3(guardWidth * 0.3f, 0, 0);
        rightMoon.transform.localScale = new Vector3(0.03f, 0.03f, 0.01f);
        rightMoon.GetComponent<Renderer>().material = guardParent.GetComponent<Renderer>().sharedMaterial;
        DestroyImmediate(rightMoon.GetComponent<Collider>());
    }

    /// <summary>
    /// 创建刀柄
    /// </summary>
    private void CreateHandle(Transform parent)
    {
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "Handle";
        handle.transform.SetParent(parent);

        // 位置: 底部
        handle.transform.localPosition = new Vector3(0, handleLength * 0.5f, 0);
        handle.transform.localRotation = Quaternion.identity;
        handle.transform.localScale = new Vector3(handleRadius * 2, handleLength * 0.5f, handleRadius * 2);

        // 创建刀柄材质
        Material handleMaterial = CreateHandleMaterial();
        handle.GetComponent<Renderer>().material = handleMaterial;

        // 移除碰撞体
        DestroyImmediate(handle.GetComponent<Collider>());

        // 添加缠绕装饰
        CreateHandleWrapping(handle.transform);

        Debug.Log($"[名刀月影] 刀柄创建完成 - 长度: {handleLength}m");
    }

    /// <summary>
    /// 创建刀柄缠绕装饰
    /// </summary>
    private void CreateHandleWrapping(Transform handleParent)
    {
        // 创建3个缠绕环
        for (int i = 0; i < 3; i++)
        {
            GameObject wrap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wrap.name = $"Wrapping_{i}";
            wrap.transform.SetParent(handleParent);

            float yPos = -0.6f + i * 0.6f; // 均匀分布
            wrap.transform.localPosition = new Vector3(0, yPos, 0);
            wrap.transform.localScale = new Vector3(1.1f, 0.05f, 1.1f);

            // 金色材质
            Material wrapMaterial = new Material(Shader.Find("Standard"));
            wrapMaterial.color = new Color(0.83f, 0.69f, 0.22f, 1f); // 金色
            wrapMaterial.SetFloat("_Metallic", 0.9f);
            wrapMaterial.SetFloat("_Glossiness", 0.8f);
            wrap.GetComponent<Renderer>().material = wrapMaterial;

            DestroyImmediate(wrap.GetComponent<Collider>());
        }
    }

    /// <summary>
    /// 创建碰撞体
    /// </summary>
    private void CreateColliders(GameObject swordRoot)
    {
        // 拾取碰撞体
        BoxCollider pickupCollider = swordRoot.AddComponent<BoxCollider>();
        pickupCollider.size = new Vector3(0.2f, bladeLength + handleLength, 0.2f);
        pickupCollider.center = new Vector3(0, (bladeLength + handleLength) * 0.5f, 0);
        pickupCollider.isTrigger = false;

        // 攻击碰撞体(子对象)
        GameObject attackColliderObj = new GameObject("Collider_Attack");
        attackColliderObj.transform.SetParent(swordRoot.transform);
        attackColliderObj.transform.localPosition = new Vector3(0, handleLength + bladeLength * 0.5f, 0);

        BoxCollider attackCollider = attackColliderObj.AddComponent<BoxCollider>();
        attackCollider.size = new Vector3(bladeWidth * 1.5f, bladeLength, bladeThickness * 2);
        attackCollider.isTrigger = true;

        Debug.Log("[名刀月影] 碰撞体创建完成");
    }

    /// <summary>
    /// 创建特效节点
    /// </summary>
    private void CreateEffectsNode(Transform parent)
    {
        GameObject effectsRoot = new GameObject("Effects");
        effectsRoot.transform.SetParent(parent);
        effectsRoot.transform.localPosition = Vector3.zero;

        // 挥击轨迹节点
        GameObject slashEffect = new GameObject("SlashEffect");
        slashEffect.transform.SetParent(effectsRoot.transform);
        slashEffect.transform.localPosition = new Vector3(0, handleLength + bladeLength * 0.5f, 0);

        // 添加轨迹渲染器
        TrailRenderer trail = slashEffect.AddComponent<TrailRenderer>();
        trail.time = 0.3f;
        trail.startWidth = bladeWidth;
        trail.endWidth = 0.02f;
        trail.startColor = new Color(0.63f, 0.78f, 0.91f, 0.8f);
        trail.endColor = new Color(1f, 1f, 1f, 0f);
        trail.material = new Material(Shader.Find("Sprites/Default"));

        // 剑气光环节点
        GameObject auraEffect = new GameObject("AuraEffect");
        auraEffect.transform.SetParent(effectsRoot.transform);
        auraEffect.transform.localPosition = new Vector3(0, handleLength + bladeLength * 0.5f, 0);

        // 添加粒子系统
        ParticleSystem particles = auraEffect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = new Color(0.63f, 0.78f, 0.91f, 0.5f);
        main.startSize = 0.05f;
        main.startLifetime = 1f;
        main.startSpeed = 0.5f;
        main.maxParticles = 50;

        var emission = particles.emission;
        emission.rateOverTime = 10f;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(bladeWidth, bladeLength, 0.1f);

        Debug.Log("[名刀月影] 特效节点创建完成");
    }

    /// <summary>
    /// 创建刀身材质
    /// </summary>
    private Material CreateBladeMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "MoonlightSword_Blade";

        mat.color = bladeColor;
        mat.SetFloat("_Metallic", bladeMetallic);
        mat.SetFloat("_Glossiness", bladeSmoothness);

        // 启用发光
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", bladeEmissionColor * emissionIntensity);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        return mat;
    }

    /// <summary>
    /// 创建护手材质
    /// </summary>
    private Material CreateGuardMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "MoonlightSword_Guard";

        mat.color = guardColor;
        mat.SetFloat("_Metallic", 0.8f);
        mat.SetFloat("_Glossiness", 0.7f);

        return mat;
    }

    /// <summary>
    /// 创建刀柄材质
    /// </summary>
    private Material CreateHandleMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "MoonlightSword_Handle";

        mat.color = handleColor;
        mat.SetFloat("_Metallic", 0.3f);
        mat.SetFloat("_Glossiness", 0.4f);

        return mat;
    }

    /// <summary>
    /// 保存为Prefab
    /// </summary>
    private void SaveAsPrefab()
    {
        if (swordRoot == null)
        {
            EditorUtility.DisplayDialog("错误", "没有可保存的模型", "确定");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "保存名刀月影Prefab",
            "MoonlightSword",
            "prefab",
            "选择保存位置"
        );

        if (!string.IsNullOrEmpty(path))
        {
            // 保存材质
            SaveMaterials(Path.GetDirectoryName(path));

            // 创建Prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(swordRoot, path);

            if (prefab != null)
            {
                Debug.Log($"[名刀月影] Prefab已保存: {path}");
                EditorUtility.DisplayDialog("成功", $"Prefab已保存到:\n{path}", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "Prefab保存失败", "确定");
            }
        }
    }

    /// <summary>
    /// 保存材质文件
    /// </summary>
    private void SaveMaterials(string directory)
    {
        string materialDir = Path.Combine(directory, "Materials");
        if (!Directory.Exists(materialDir))
        {
            Directory.CreateDirectory(materialDir);
            AssetDatabase.Refresh();
        }

        // 保存刀身材质
        Material bladeMat = CreateBladeMaterial();
        AssetDatabase.CreateAsset(bladeMat, Path.Combine(materialDir, "MoonlightSword_Blade.mat"));

        // 保存护手材质
        Material guardMat = CreateGuardMaterial();
        AssetDatabase.CreateAsset(guardMat, Path.Combine(materialDir, "MoonlightSword_Guard.mat"));

        // 保存刀柄材质
        Material handleMat = CreateHandleMaterial();
        AssetDatabase.CreateAsset(handleMat, Path.Combine(materialDir, "MoonlightSword_Handle.mat"));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[名刀月影] 材质已保存到: {materialDir}");
    }
}
