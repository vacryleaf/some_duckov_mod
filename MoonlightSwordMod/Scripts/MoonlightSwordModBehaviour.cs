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
    /// </summary>
    public class MoonlightSwordModBehaviour : ModBehaviour
    {
        // 武器Prefab
        private Item moonlightSwordPrefab;

        // 剑气Prefab
        private GameObject swordAuraPrefab;

        // AssetBundle引用
        private AssetBundle weaponBundle;

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

                // 创建武器Prefab
                CreateWeaponPrefab();

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
            string bundlePath = Path.Combine(ModPath, "Assets", "moonlight_sword");

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
                        Debug.LogWarning("[名刀月影] 武器Prefab未找到");
                    }

                    if (swordAuraPrefab == null)
                    {
                        Debug.LogWarning("[名刀月影] 剑气Prefab未找到");
                    }
                }
                else
                {
                    Debug.LogError("[名刀月影] AssetBundle加载失败");
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
        /// </summary>
        private void CreateWeaponPrefab()
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

            // 配置攻击参数
            attackComponent.swordAuraPrefab = swordAuraPrefab;
            attackComponent.normalDamage = 52.5f;
            attackComponent.specialDamage = 90f;
            attackComponent.attackRange = 3f;
            attackComponent.specialRange = 10f;
            attackComponent.specialCooldown = 8f;

            // 配置武器属性
            ConfigureWeaponStats(moonlightSwordPrefab);

            Debug.Log("[名刀月影] 武器Prefab配置完成");
        }

        /// <summary>
        /// 配置武器统计属性
        /// </summary>
        private void ConfigureWeaponStats(Item weapon)
        {
            // 基础属性
            weapon.TypeID = 10001;
            weapon.DisplayName = "名刀月影";
            weapon.Description = "传说中以月光淬炼的神刀，挥舞时可释放银白色的剑气";
            weapon.Quality = ItemQuality.Purple;
            weapon.MaxStackCount = 1;
            weapon.Stackable = false;
            weapon.UnitSelfWeight = 3.5f;
            weapon.MaxDurability = 500;
            weapon.Durability = 500;

            // 添加标签
            if (!weapon.Tags.Contains("Weapon"))
                weapon.Tags.Add("Weapon");
            if (!weapon.Tags.Contains("MeleeWeapon"))
                weapon.Tags.Add("MeleeWeapon");
            if (!weapon.Tags.Contains("Sword"))
                weapon.Tags.Add("Sword");
            if (!weapon.Tags.Contains("Legendary"))
                weapon.Tags.Add("Legendary");

            // 配置统计数据
            var stats = weapon.GetComponent<ItemStats>();
            if (stats == null)
            {
                stats = weapon.gameObject.AddComponent<ItemStats>();
            }

            // 添加武器属性
            AddOrUpdateStat(stats, "Damage", 52.5f, 45f, 60f);
            AddOrUpdateStat(stats, "AttackSpeed", 1.2f);
            AddOrUpdateStat(stats, "Range", 3f);
            AddOrUpdateStat(stats, "CriticalChance", 0.15f);
            AddOrUpdateStat(stats, "CriticalDamage", 1.8f);
            AddOrUpdateStat(stats, "SpecialAttackDamage", 90f, 80f, 100f);
            AddOrUpdateStat(stats, "SpecialAttackCooldown", 8f);
            AddOrUpdateStat(stats, "SpecialAttackRange", 10f);

            Debug.Log("[名刀月影] 武器属性配置完成");
        }

        /// <summary>
        /// 添加或更新Stat属性
        /// </summary>
        private void AddOrUpdateStat(ItemStats stats, string statName, float value, float minValue = 0f, float maxValue = 0f)
        {
            // 这里需要根据实际的ItemStats API来实现
            // 以下是示例代码，可能需要调整
            try
            {
                if (maxValue > minValue && maxValue > 0)
                {
                    // 有范围的属性
                    stats.SetStat(statName, value, minValue, maxValue);
                }
                else
                {
                    // 固定值属性
                    stats.SetStat(statName, value);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[名刀月影] 添加Stat失败: {statName}, 错误: {e.Message}");
            }
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
                Debug.Log($"  - 品质: {moonlightSwordPrefab.Quality}");
            }
            else
            {
                Debug.LogError("[名刀月影] 武器注册失败！可能是TypeID冲突");
            }
        }

        /// <summary>
        /// 程序化生成武器（备用方案）
        /// 当AssetBundle不存在时使用
        /// </summary>
        private void CreateProceduralWeapon()
        {
            Debug.Log("[名刀月影] 开始程序化生成武器");

            // 创建基础GameObject
            GameObject weaponObj = new GameObject("MoonlightSword");

            // 添加Item组件
            moonlightSwordPrefab = weaponObj.AddComponent<Item>();

            // 创建简单的刀模型
            CreateSimpleSwordModel(weaponObj);

            // 创建剑气特效
            swordAuraPrefab = CreateSwordAuraEffect();

            Debug.Log("[名刀月影] 程序化武器生成完成");
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
            Destroy(blade.GetComponent<Collider>());

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
            Destroy(guard.GetComponent<Collider>());

            // 刀柄
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(parent.transform);
            handle.transform.localPosition = new Vector3(0, -0.05f, 0);
            handle.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);

            Material handleMaterial = new Material(Shader.Find("Standard"));
            handleMaterial.color = new Color(0.23f, 0.18f, 0.35f); // 深紫色
            handle.GetComponent<Renderer>().material = handleMaterial;
            Destroy(handle.GetComponent<Collider>());

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
            Destroy(crescent.GetComponent<Collider>());

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
