using UnityEngine;
using Duckov.Modding;
using ItemStatsSystem;
using System.Threading.Tasks;
using System.IO;

namespace MoonlightSwordMod
{
    /// <summary>
    /// 名刀月影Mod主类
    /// 负责加载资源、创建武器并注册到游戏系统
    /// 注意：武器属性（TypeID、DisplayName、Stats等）必须在Unity Prefab中预设
    /// </summary>
    public class MoonlightSwordModBehaviour : ModBehaviour
    {
        // 武器Prefab
        private Item moonlightSwordPrefab;

        // 剑气Prefab
        private GameObject swordAuraPrefab;

        // AssetBundle引用
        private AssetBundle weaponBundle;

        // 获取Mod所在目录路径
        private string GetModFolderPath()
        {
            // 通过info.dllPath获取DLL路径，然后取目录
            string dllPath = info.dllPath;
            if (!string.IsNullOrEmpty(dllPath))
            {
                return Path.GetDirectoryName(dllPath);
            }
            return string.Empty;
        }

        /// <summary>
        /// Mod启动入口
        /// </summary>
        async void Start()
        {
            Debug.Log("[名刀月影] 开始加载Mod...");

            try
            {
                // 加载AssetBundle资源
                await LoadAssets();

                // 配置武器Prefab组件
                ConfigureWeaponPrefab();

                // 注册到游戏系统
                RegisterWeapon();

                Debug.Log("[名刀月影] Mod加载完成!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[名刀月影] Mod加载失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 加载AssetBundle资源文件
        /// </summary>
        private async Task LoadAssets()
        {
            // 构建AssetBundle路径
            string modFolder = GetModFolderPath();
            string bundlePath = Path.Combine(modFolder, "Assets", "moonlight_sword");

            Debug.Log($"[名刀月影] 尝试加载AssetBundle: {bundlePath}");

            if (File.Exists(bundlePath))
            {
                // 异步加载AssetBundle
                weaponBundle = await Task.Run(() => AssetBundle.LoadFromFile(bundlePath));

                if (weaponBundle != null)
                {
                    // 加载武器Prefab
                    moonlightSwordPrefab = weaponBundle.LoadAsset<Item>("MoonlightSwordPrefab");

                    // 加载剑气Prefab
                    swordAuraPrefab = weaponBundle.LoadAsset<GameObject>("SwordAuraPrefab");

                    Debug.Log("[名刀月影] AssetBundle加载成功");

                    if (moonlightSwordPrefab == null)
                    {
                        Debug.LogWarning("[名刀月影] 武器Prefab未找到，使用程序化生成");
                        CreateProceduralWeapon();
                    }

                    if (swordAuraPrefab == null)
                    {
                        Debug.LogWarning("[名刀月影] 剑气Prefab未找到，使用程序化生成");
                        swordAuraPrefab = CreateSwordAuraEffect();
                    }
                }
                else
                {
                    Debug.LogError("[名刀月影] AssetBundle加载失败");
                    CreateProceduralWeapon();
                }
            }
            else
            {
                Debug.LogWarning($"[名刀月影] AssetBundle文件不存在: {bundlePath}");
                Debug.LogWarning("[名刀月影] 使用程序化生成武器");
                CreateProceduralWeapon();
            }
        }

        /// <summary>
        /// 配置武器Prefab组件
        /// 注意：基础参数（Damage, CritRate, AttackRange等）应在 Unity Prefab 的 Item.Stats 中配置
        /// 这里只配置特殊攻击和格挡的自定义参数
        /// </summary>
        private void ConfigureWeaponPrefab()
        {
            if (moonlightSwordPrefab == null)
            {
                Debug.LogError("[名刀月影] 武器Prefab为空，无法配置");
                return;
            }

            Debug.Log("[名刀月影] 开始配置武器Prefab");

            // 添加自定义攻击组件
            var attackComponent = moonlightSwordPrefab.gameObject.GetComponent<MoonlightSwordAttack>();
            if (attackComponent == null)
            {
                attackComponent = moonlightSwordPrefab.gameObject.AddComponent<MoonlightSwordAttack>();
            }

            // 配置特殊攻击参数（自定义参数，不从Stats读取）
            attackComponent.swordAuraPrefab = swordAuraPrefab;
            attackComponent.specialDamage = 90f;       // 剑气伤害
            attackComponent.specialRange = 10f;        // 剑气飞行距离
            attackComponent.specialCooldown = 8f;      // 特殊攻击冷却

            // 添加格挡组件
            var blockComponent = moonlightSwordPrefab.gameObject.GetComponent<MoonlightSwordBlock>();
            if (blockComponent == null)
            {
                blockComponent = moonlightSwordPrefab.gameObject.AddComponent<MoonlightSwordBlock>();
            }

            // 配置格挡参数（自定义参数）
            blockComponent.staminaCost = 5f;           // 格挡消耗体力
            blockComponent.deflectSpeed = 30f;         // 反弹速度
            blockComponent.deflectDamageRatio = 0.5f;  // 反弹伤害比例
            blockComponent.deflectMaxDistance = 20f;   // 反弹最大距离

            Debug.Log("[名刀月影] 武器Prefab配置完成（含格挡系统）");
            Debug.Log("[名刀月影] 注意：基础参数（Damage, CritRate等）需在 Unity Prefab 的 Item.Stats 中配置");
        }

        /// <summary>
        /// 注册武器到游戏系统
        /// </summary>
        private void RegisterWeapon()
        {
            if (moonlightSwordPrefab == null)
            {
                Debug.LogError("[名刀月影] 无法注册空武器");
                return;
            }

            Debug.Log($"[名刀月影] 正在注册武器，TypeID: {moonlightSwordPrefab.TypeID}");

            // 注册到动态物品系统
            bool success = ItemAssetsCollection.AddDynamicEntry(moonlightSwordPrefab);

            if (success)
            {
                Debug.Log($"[名刀月影] 武器注册成功！");
                Debug.Log($"  - 名称: {moonlightSwordPrefab.DisplayName}");
                Debug.Log($"  - ID: {moonlightSwordPrefab.TypeID}");
            }
            else
            {
                Debug.LogError("[名刀月影] 武器注册失败！可能是TypeID冲突");
            }
        }

        /// <summary>
        /// 程序化生成武器（备用方案）
        /// 当AssetBundle不存在时使用
        /// 注意：程序化生成的武器功能有限，建议使用AssetBundle
        /// </summary>
        private void CreateProceduralWeapon()
        {
            Debug.Log("[名刀月影] 开始程序化生成武器");

            // 创建基础GameObject
            GameObject weaponObj = new GameObject("MoonlightSword");

            // 添加Item组件（注意：属性在Prefab中是只读的，这里只能使用默认值）
            moonlightSwordPrefab = weaponObj.AddComponent<Item>();

            // 创建简单的刀模型
            CreateSimpleSwordModel(weaponObj);

            // 创建剑气特效
            if (swordAuraPrefab == null)
            {
                swordAuraPrefab = CreateSwordAuraEffect();
            }

            Debug.Log("[名刀月影] 程序化武器生成完成");
            Debug.LogWarning("[名刀月影] 注意：程序化生成的武器属性使用默认值，建议提供AssetBundle");
        }

        /// <summary>
        /// 创建简单的刀模型（备用）
        /// </summary>
        private void CreateSimpleSwordModel(GameObject parent)
        {
            // 刀身
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade";
            blade.transform.SetParent(parent.transform);
            blade.transform.localPosition = new Vector3(0, 0.75f, 0);
            blade.transform.localScale = new Vector3(0.08f, 1.2f, 0.02f);

            // 创建刀身材质
            Material bladeMaterial = new Material(Shader.Find("Standard"));
            bladeMaterial.color = new Color(0.91f, 0.91f, 0.94f); // 银白色
            bladeMaterial.SetFloat("_Metallic", 0.95f);
            bladeMaterial.SetFloat("_Glossiness", 0.9f);
            bladeMaterial.EnableKeyword("_EMISSION");
            bladeMaterial.SetColor("_EmissionColor", new Color(0.63f, 0.78f, 0.91f) * 0.5f);
            blade.GetComponent<Renderer>().material = bladeMaterial;

            // 移除默认碰撞体
            Object.Destroy(blade.GetComponent<Collider>());

            // 护手
            GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            guard.name = "Guard";
            guard.transform.SetParent(parent.transform);
            guard.transform.localPosition = new Vector3(0, 0.1f, 0);
            guard.transform.localScale = new Vector3(0.15f, 0.02f, 0.05f);

            Material guardMaterial = new Material(Shader.Find("Standard"));
            guardMaterial.color = new Color(0.1f, 0.17f, 0.29f); // 深蓝色
            guardMaterial.SetFloat("_Metallic", 0.8f);
            guard.GetComponent<Renderer>().material = guardMaterial;
            Object.Destroy(guard.GetComponent<Collider>());

            // 刀柄
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(parent.transform);
            handle.transform.localPosition = new Vector3(0, -0.05f, 0);
            handle.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);

            Material handleMaterial = new Material(Shader.Find("Standard"));
            handleMaterial.color = new Color(0.23f, 0.18f, 0.35f); // 深紫色
            handle.GetComponent<Renderer>().material = handleMaterial;
            Object.Destroy(handle.GetComponent<Collider>());

            // 添加拾取碰撞体
            BoxCollider collider = parent.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.2f, 1.5f, 0.2f);
            collider.center = new Vector3(0, 0.75f, 0);

            Debug.Log("[名刀月影] 简易刀模型创建完成");
        }

        /// <summary>
        /// 创建剑气特效（备用）
        /// </summary>
        private GameObject CreateSwordAuraEffect()
        {
            GameObject aura = new GameObject("SwordAura");

            // 创建月牙形状
            GameObject crescent = GameObject.CreatePrimitive(PrimitiveType.Quad);
            crescent.name = "CrescentShape";
            crescent.transform.SetParent(aura.transform);
            crescent.transform.localScale = new Vector3(2f, 1f, 1f);

            // 创建发光材质
            Material auraMaterial = new Material(Shader.Find("Standard"));
            auraMaterial.color = new Color(0.44f, 0.69f, 0.88f, 0.7f);
            auraMaterial.EnableKeyword("_EMISSION");
            auraMaterial.SetColor("_EmissionColor", new Color(0.44f, 0.69f, 0.88f) * 2f);

            // 设置透明模式
            auraMaterial.SetFloat("_Mode", 3);
            auraMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            auraMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            auraMaterial.renderQueue = 3000;

            crescent.GetComponent<Renderer>().material = auraMaterial;
            Object.Destroy(crescent.GetComponent<Collider>());

            // 添加粒子系统
            ParticleSystem particles = aura.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(1f, 1f, 1f, 0.8f);
            main.startSize = 0.1f;
            main.startLifetime = 0.5f;
            main.startSpeed = 2f;

            // 添加剑气投射物组件
            SwordAuraProjectile projectile = aura.AddComponent<SwordAuraProjectile>();
            projectile.damage = 90f;
            projectile.speed = 15f;
            projectile.maxDistance = 10f;
            projectile.pierceCount = 3;

            Debug.Log("[名刀月影] 简易剑气特效创建完成");

            return aura;
        }

        /// <summary>
        /// Mod卸载时清理资源
        /// </summary>
        void OnDestroy()
        {
            Debug.Log("[名刀月影] 开始卸载Mod");

            // 移除动态物品
            if (moonlightSwordPrefab != null)
            {
                ItemAssetsCollection.RemoveDynamicEntry(moonlightSwordPrefab);
                Debug.Log("[名刀月影] 武器已从游戏中移除");
            }

            // 卸载AssetBundle
            if (weaponBundle != null)
            {
                weaponBundle.Unload(true);
                Debug.Log("[名刀月影] AssetBundle已卸载");
            }

            Debug.Log("[名刀月影] Mod卸载完成");
        }
    }
}
