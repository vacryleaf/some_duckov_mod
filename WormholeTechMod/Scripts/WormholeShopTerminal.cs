using UnityEngine;
using System;
using System.Collections;
using System.Reflection;
using Duckov;
using ItemStatsSystem;

namespace WormholeTechMod
{
    /// <summary>
    /// 虫洞商店终端
    /// 按键触发技能，打开虫洞科技独立商店
    /// </summary>
    public class WormholeShopTerminal : MonoBehaviour
    {
        // ========== 配置属性 ==========

        /// <summary>
        /// 打开商店的快捷键（默认 F8）
        /// </summary>
        public KeyCode openShopKey = KeyCode.F8;

        /// <summary>
        /// 技能冷却时间（秒）
        /// </summary>
        public float cooldownTime = 30f;

        /// <summary>
        /// 是否只能在基地使用
        /// </summary>
        public bool onlyInBase = false;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool isEnabled = true;

        /// <summary>
        /// 是否使用独立商店（虫洞科技专属商店）
        /// </summary>
        public bool useExclusiveShop = true;

        // ========== 内部状态 ==========

        private float lastUseTime = -999f;
        private bool isOnCooldown = false;

        // 独立商店引用
        private WormholeTechShop wormholeShop;

        /// <summary>
        /// 初始化
        /// </summary>
        void Awake()
        {
            ModLogger.Log($"初始化完成，快捷键: {openShopKey}，独立商店: {useExclusiveShop}");
        }

        /// <summary>
        /// 设置独立商店引用
        /// </summary>
        public void SetWormholeShop(WormholeTechShop shop)
        {
            wormholeShop = shop;
            ModLogger.Log("已绑定虫洞科技独立商店");
        }

        /// <summary>
        /// 每帧更新 - 检测按键输入
        /// </summary>
        void Update()
        {
            if (!isEnabled) return;

            // 检测快捷键
            if (Input.GetKeyDown(openShopKey))
            {
                TryOpenShop();
            }

            // 更新冷却状态
            UpdateCooldown();
        }

        /// <summary>
        /// 尝试打开商店
        /// </summary>
        public void TryOpenShop()
        {
            // 检查冷却
            if (isOnCooldown)
            {
                float remainingTime = cooldownTime - (Time.time - lastUseTime);
                ShowMessage($"虫洞商店终端冷却中 ({remainingTime:F1}秒)");
                return;
            }

            // 检查玩家状态
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter == null)
            {
                ModLogger.LogWarning("无法找到玩家角色");
                return;
            }

            // 检查是否只能在基地使用
            if (onlyInBase && !IsInBase())
            {
                ShowMessage("虫洞商店终端只能在基地使用！");
                return;
            }

            // 尝试打开商店
            if (OpenShop())
            {
                // 记录使用时间
                lastUseTime = Time.time;
                isOnCooldown = true;

                // 创建视觉特效
                CreateActivationEffect(mainCharacter.transform.position);

                ShowMessage("虫洞科技终端已激活！");
                ModLogger.Log("商店已打开");
            }
            else
            {
                ShowMessage("无法打开商店，请稍后再试");
                ModLogger.LogWarning("打开商店失败");
            }
        }

        /// <summary>
        /// 打开商店
        /// </summary>
        private bool OpenShop()
        {
            try
            {
                // 优先使用独立的虫洞科技商店
                if (useExclusiveShop)
                {
                    if (TryOpenWormholeTechShop())
                    {
                        return true;
                    }
                }

                // 备选方案：使用游戏原有商店系统
                // 方法1：尝试通过 StockShop 打开
                if (TryOpenViaStockShop())
                {
                    return true;
                }

                // 方法2：尝试通过 UIManager 打开
                if (TryOpenViaUIManager())
                {
                    return true;
                }

                // 方法3：尝试通过场景中的商店对象打开
                if (TryOpenViaSceneShop())
                {
                    return true;
                }

                // 方法4：尝试模拟商店交互
                if (TrySimulateShopInteraction())
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                ModLogger.LogError($"打开商店异常: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 尝试打开虫洞科技独立商店
        /// </summary>
        private bool TryOpenWormholeTechShop()
        {
            try
            {
                // 如果没有设置商店引用，尝试查找
                if (wormholeShop == null)
                {
                    wormholeShop = WormholeTechShop.Instance;
                }

                if (wormholeShop == null)
                {
                    wormholeShop = FindObjectOfType<WormholeTechShop>();
                }

                if (wormholeShop != null)
                {
                    bool success = wormholeShop.OpenShop();
                    if (success)
                    {
                        ModLogger.Log("通过虫洞科技独立商店打开");
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"虫洞科技商店方式失败: {e.Message}");
            }
            return false;
        }

        /// <summary>
        /// 通过 StockShop 打开商店
        /// </summary>
        private bool TryOpenViaStockShop()
        {
            try
            {
                // 查找场景中的 StockShop
                var stockShops = FindObjectsOfType<MonoBehaviour>();
                foreach (var obj in stockShops)
                {
                    if (obj.GetType().Name == "StockShop")
                    {
                        // 尝试调用 OpenShop 方法
                        var openMethod = obj.GetType().GetMethod("OpenShop",
                            BindingFlags.Public | BindingFlags.Instance);
                        if (openMethod != null)
                        {
                            openMethod.Invoke(obj, null);
                            ModLogger.Log("通过 StockShop.OpenShop() 打开");
                            return true;
                        }

                        // 尝试调用 Open 方法
                        var altOpenMethod = obj.GetType().GetMethod("Open",
                            BindingFlags.Public | BindingFlags.Instance);
                        if (altOpenMethod != null)
                        {
                            altOpenMethod.Invoke(obj, null);
                            ModLogger.Log("通过 StockShop.Open() 打开");
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"StockShop方式失败: {e.Message}");
            }
            return false;
        }

        /// <summary>
        /// 通过 UIManager 打开商店
        /// </summary>
        private bool TryOpenViaUIManager()
        {
            try
            {
                // 查找 UIManager
                var uiManagerType = Type.GetType("Duckov.UIManager, Assembly-CSharp") ??
                                    Type.GetType("UIManager, Assembly-CSharp");

                if (uiManagerType != null)
                {
                    // 获取单例实例
                    var instanceProp = uiManagerType.GetProperty("Instance",
                        BindingFlags.Public | BindingFlags.Static);
                    if (instanceProp != null)
                    {
                        var uiManager = instanceProp.GetValue(null);
                        if (uiManager != null)
                        {
                            // 尝试打开商店UI
                            var showShopMethod = uiManagerType.GetMethod("ShowShopUI",
                                BindingFlags.Public | BindingFlags.Instance);
                            if (showShopMethod != null)
                            {
                                showShopMethod.Invoke(uiManager, null);
                                ModLogger.Log("通过 UIManager.ShowShopUI() 打开");
                                return true;
                            }

                            // 尝试其他方法名
                            var openShopUIMethod = uiManagerType.GetMethod("OpenShopUI",
                                BindingFlags.Public | BindingFlags.Instance);
                            if (openShopUIMethod != null)
                            {
                                openShopUIMethod.Invoke(uiManager, null);
                                ModLogger.Log("通过 UIManager.OpenShopUI() 打开");
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"UIManager方式失败: {e.Message}");
            }
            return false;
        }

        /// <summary>
        /// 通过场景中的商店对象打开
        /// </summary>
        private bool TryOpenViaSceneShop()
        {
            try
            {
                // 查找场景中带有 "Shop" 名称的对象
                var allObjects = FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name.Contains("Shop") || obj.name.Contains("Merchant") ||
                        obj.name.Contains("Vendor") || obj.name.Contains("Store"))
                    {
                        // 尝试获取交互组件
                        var interactable = obj.GetComponent<MonoBehaviour>();
                        if (interactable != null)
                        {
                            var interactMethod = interactable.GetType().GetMethod("Interact",
                                BindingFlags.Public | BindingFlags.Instance);
                            if (interactMethod != null)
                            {
                                interactMethod.Invoke(interactable, null);
                                ModLogger.Log($"通过场景对象 {obj.name} 打开");
                                return true;
                            }

                            var onInteractMethod = interactable.GetType().GetMethod("OnInteract",
                                BindingFlags.Public | BindingFlags.Instance);
                            if (onInteractMethod != null)
                            {
                                onInteractMethod.Invoke(interactable, null);
                                ModLogger.Log($"通过场景对象 {obj.name} 打开");
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"场景商店方式失败: {e.Message}");
            }
            return false;
        }

        /// <summary>
        /// 尝试模拟商店交互
        /// </summary>
        private bool TrySimulateShopInteraction()
        {
            try
            {
                // 获取商店数据库
                var shopDatabase = StockShopDatabase.Instance;
                if (shopDatabase == null)
                {
                    ModLogger.LogWarning("无法获取商店数据库");
                    return false;
                }

                // 查找第一个商人配置
                if (shopDatabase.merchantProfiles != null && shopDatabase.merchantProfiles.Count > 0)
                {
                    var profile = shopDatabase.merchantProfiles[0];
                    ModLogger.Log($"找到商人配置: {profile.merchantID}");

                    // 尝试创建商店UI
                    // 这里需要游戏内部的UI系统支持
                    // 如果游戏有专门的商店UI类，可以尝试实例化

                    // 暂时返回false，因为无法确定具体的UI打开方式
                    return false;
                }
            }
            catch (Exception e)
            {
                ModLogger.LogWarning($"模拟交互失败: {e.Message}");
            }
            return false;
        }

        /// <summary>
        /// 检查是否在基地
        /// </summary>
        private bool IsInBase()
        {
            if (LevelManager.Instance != null)
            {
                return LevelManager.Instance.IsBaseLevel;
            }
            return false;
        }

        /// <summary>
        /// 更新冷却状态
        /// </summary>
        private void UpdateCooldown()
        {
            if (isOnCooldown)
            {
                if (Time.time - lastUseTime >= cooldownTime)
                {
                    isOnCooldown = false;
                    ModLogger.Log("冷却结束");
                }
            }
        }

        /// <summary>
        /// 获取剩余冷却时间
        /// </summary>
        public float GetRemainingCooldown()
        {
            if (!isOnCooldown) return 0f;
            return Mathf.Max(0f, cooldownTime - (Time.time - lastUseTime));
        }

        /// <summary>
        /// 创建激活特效
        /// </summary>
        private void CreateActivationEffect(Vector3 position)
        {
            GameObject effectObj = new GameObject("ShopTerminalEffect");
            effectObj.transform.position = position + Vector3.up * 1f;

            ParticleSystem particles = effectObj.AddComponent<ParticleSystem>();

            var main = particles.main;
            main.startColor = new Color(0.2f, 0.8f, 1f, 0.8f);  // 青色
            main.startSize = 0.3f;
            main.startLifetime = 1.5f;
            main.startSpeed = 2f;
            main.duration = 1f;
            main.loop = false;

            var emission = particles.emission;
            emission.rateOverTime = 50f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.y = 2f;

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.2f, 0.8f, 1f), 0f),
                    new GradientColorKey(new Color(0.6f, 0.2f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            particles.Play();

            // 添加光源
            Light light = effectObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.2f, 0.8f, 1f);
            light.intensity = 3f;
            light.range = 5f;

            // 启动淡出协程
            StartCoroutine(FadeOutLight(light, 1.5f));

            Destroy(effectObj, 3f);
        }

        /// <summary>
        /// 光源淡出协程
        /// </summary>
        private IEnumerator FadeOutLight(Light light, float duration)
        {
            float startIntensity = light.intensity;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                if (light != null)
                {
                    light.intensity = Mathf.Lerp(startIntensity, 0f, timer / duration);
                }
                yield return null;
            }
        }

        /// <summary>
        /// 显示消息提示
        /// </summary>
        private void ShowMessage(string message)
        {
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter != null)
            {
                mainCharacter.PopText(message);
            }
            ModLogger.Log($"{message}");
        }

        /// <summary>
        /// 设置快捷键
        /// </summary>
        public void SetHotkey(KeyCode key)
        {
            openShopKey = key;
            ModLogger.Log($"快捷键已更改为: {key}");
        }

        /// <summary>
        /// 设置快捷键（通过字符串）
        /// </summary>
        public bool SetHotkey(string keyName)
        {
            try
            {
                KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), keyName, true);
                SetHotkey(key);
                return true;
            }
            catch
            {
                ModLogger.LogWarning($"无效的按键名称: {keyName}");
                return false;
            }
        }
    }
}
