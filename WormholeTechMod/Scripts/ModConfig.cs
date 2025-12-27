using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace WormholeTechMod
{
    /// <summary>
    /// Mod配置管理器
    /// 负责加载和保存配置文件，支持按键绑定等设置
    /// </summary>
    public class ModConfig
    {
        // ========== 配置键名常量 ==========
        public const string KEY_SHOP_TERMINAL_HOTKEY = "ShopTerminalHotkey";
        public const string KEY_SHOP_TERMINAL_COOLDOWN = "ShopTerminalCooldown";
        public const string KEY_SHOP_TERMINAL_ONLY_IN_BASE = "ShopTerminalOnlyInBase";
        public const string KEY_BLACKHOLE_DURATION = "BlackHoleDuration";
        public const string KEY_BLACKHOLE_PULL_RANGE = "BlackHolePullRange";
        public const string KEY_BLACKHOLE_PULL_FORCE = "BlackHolePullForce";
        public const string KEY_BLACKHOLE_DAMAGE_PER_SECOND = "BlackHoleDamagePerSecond";
        public const string KEY_BLACKHOLE_CAN_HURT_SELF = "BlackHoleCanHurtSelf";
        public const string KEY_DEBUG_MODE = "DebugMode";

        // ========== 默认值 ==========
        private static readonly Dictionary<string, string> DefaultValues = new Dictionary<string, string>
        {
            { KEY_SHOP_TERMINAL_HOTKEY, "F8" },
            { KEY_SHOP_TERMINAL_COOLDOWN, "30" },
            { KEY_SHOP_TERMINAL_ONLY_IN_BASE, "false" },
            { KEY_BLACKHOLE_DURATION, "5" },
            { KEY_BLACKHOLE_PULL_RANGE, "5" },
            { KEY_BLACKHOLE_PULL_FORCE, "3" },
            { KEY_BLACKHOLE_DAMAGE_PER_SECOND, "10" },
            { KEY_BLACKHOLE_CAN_HURT_SELF, "false" },
            { KEY_DEBUG_MODE, "false" }
        };

        // ========== 内部状态 ==========
        private Dictionary<string, string> configValues = new Dictionary<string, string>();
        private string configFilePath;
        private bool isLoaded = false;

        // 单例模式
        private static ModConfig _instance;
        public static ModConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ModConfig();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 私有构造函数（单例模式）
        /// </summary>
        private ModConfig()
        {
            // 初始化默认值
            foreach (var kvp in DefaultValues)
            {
                configValues[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// 初始化配置（需要传入Mod路径）
        /// </summary>
        public void Initialize(string modFolderPath)
        {
            configFilePath = Path.Combine(modFolderPath, "config.ini");
            Load();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void Load()
        {
            try
            {
                if (string.IsNullOrEmpty(configFilePath))
                {
                    ModLogger.LogWarning("[ModConfig] 配置文件路径未设置");
                    return;
                }

                if (!File.Exists(configFilePath))
                {
                    ModLogger.Log("[ModConfig] 配置文件不存在，使用默认值并创建配置文件");
                    Save(); // 创建默认配置文件
                    isLoaded = true;
                    return;
                }

                string[] lines = File.ReadAllLines(configFilePath);
                foreach (string line in lines)
                {
                    // 跳过空行和注释
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#") ||
                        trimmedLine.StartsWith(";") || trimmedLine.StartsWith("//"))
                    {
                        continue;
                    }

                    // 解析键值对
                    int separatorIndex = trimmedLine.IndexOf('=');
                    if (separatorIndex > 0)
                    {
                        string key = trimmedLine.Substring(0, separatorIndex).Trim();
                        string value = trimmedLine.Substring(separatorIndex + 1).Trim();

                        // 移除值两端的引号
                        if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
                        {
                            value = value.Substring(1, value.Length - 2);
                        }

                        configValues[key] = value;
                    }
                }

                isLoaded = true;
                ModLogger.Log($"[ModConfig] 配置文件加载成功: {configFilePath}");
            }
            catch (Exception e)
            {
                ModLogger.LogError($"[ModConfig] 加载配置文件失败: {e.Message}");
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void Save()
        {
            try
            {
                if (string.IsNullOrEmpty(configFilePath))
                {
                    ModLogger.LogWarning("[ModConfig] 配置文件路径未设置");
                    return;
                }

                List<string> lines = new List<string>();

                // 添加文件头注释
                lines.Add("# 微型虫洞 Mod 配置文件");
                lines.Add("# MicroWormholeMod Configuration");
                lines.Add("#");
                lines.Add("# 按键名称参考 Unity KeyCode 枚举:");
                lines.Add("# F1-F12, A-Z, Alpha0-Alpha9, Space, Return, Escape, Tab, etc.");
                lines.Add("# 完整列表: https://docs.unity3d.com/ScriptReference/KeyCode.html");
                lines.Add("");

                // 虫洞商店终端设置
                lines.Add("# ========== 虫洞商店终端设置 ==========");
                lines.Add($"{KEY_SHOP_TERMINAL_HOTKEY}={GetString(KEY_SHOP_TERMINAL_HOTKEY)}");
                lines.Add($"{KEY_SHOP_TERMINAL_COOLDOWN}={GetString(KEY_SHOP_TERMINAL_COOLDOWN)}");
                lines.Add($"{KEY_SHOP_TERMINAL_ONLY_IN_BASE}={GetString(KEY_SHOP_TERMINAL_ONLY_IN_BASE)}");
                lines.Add("");

                // 黑洞手雷设置
                lines.Add("# ========== 黑洞手雷设置 ==========");
                lines.Add($"{KEY_BLACKHOLE_DURATION}={GetString(KEY_BLACKHOLE_DURATION)}");
                lines.Add($"{KEY_BLACKHOLE_PULL_RANGE}={GetString(KEY_BLACKHOLE_PULL_RANGE)}");
                lines.Add($"{KEY_BLACKHOLE_PULL_FORCE}={GetString(KEY_BLACKHOLE_PULL_FORCE)}");
                lines.Add($"{KEY_BLACKHOLE_DAMAGE_PER_SECOND}={GetString(KEY_BLACKHOLE_DAMAGE_PER_SECOND)}");
                lines.Add($"{KEY_BLACKHOLE_CAN_HURT_SELF}={GetString(KEY_BLACKHOLE_CAN_HURT_SELF)}");
                lines.Add("");

                // 调试设置
                lines.Add("# ========== 调试设置 ==========");
                lines.Add($"{KEY_DEBUG_MODE}={GetString(KEY_DEBUG_MODE)}");

                File.WriteAllLines(configFilePath, lines);
                ModLogger.Log($"[ModConfig] 配置文件保存成功: {configFilePath}");
            }
            catch (Exception e)
            {
                ModLogger.LogError($"[ModConfig] 保存配置文件失败: {e.Message}");
            }
        }

        /// <summary>
        /// 获取字符串配置值
        /// </summary>
        public string GetString(string key, string defaultValue = null)
        {
            if (configValues.TryGetValue(key, out string value))
            {
                return value;
            }

            if (defaultValue != null)
            {
                return defaultValue;
            }

            if (DefaultValues.TryGetValue(key, out string defValue))
            {
                return defValue;
            }

            return "";
        }

        /// <summary>
        /// 获取整数配置值
        /// </summary>
        public int GetInt(string key, int defaultValue = 0)
        {
            string value = GetString(key);
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取浮点数配置值
        /// </summary>
        public float GetFloat(string key, float defaultValue = 0f)
        {
            string value = GetString(key);
            if (float.TryParse(value, out float result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取布尔配置值
        /// </summary>
        public bool GetBool(string key, bool defaultValue = false)
        {
            string value = GetString(key).ToLower();
            if (value == "true" || value == "1" || value == "yes")
            {
                return true;
            }
            if (value == "false" || value == "0" || value == "no")
            {
                return false;
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取按键配置值
        /// </summary>
        public KeyCode GetKeyCode(string key, KeyCode defaultValue = KeyCode.None)
        {
            string value = GetString(key);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return (KeyCode)Enum.Parse(typeof(KeyCode), value, true);
            }
            catch
            {
                ModLogger.LogWarning($"[ModConfig] 无效的按键名称: {value}，使用默认值: {defaultValue}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 设置配置值
        /// </summary>
        public void Set(string key, object value)
        {
            configValues[key] = value.ToString();
        }

        /// <summary>
        /// 设置并保存配置值
        /// </summary>
        public void SetAndSave(string key, object value)
        {
            Set(key, value);
            Save();
        }

        /// <summary>
        /// 重置为默认值
        /// </summary>
        public void ResetToDefaults()
        {
            configValues.Clear();
            foreach (var kvp in DefaultValues)
            {
                configValues[kvp.Key] = kvp.Value;
            }
            Save();
            ModLogger.Log("[ModConfig] 配置已重置为默认值");
        }

        /// <summary>
        /// 检查配置是否已加载
        /// </summary>
        public bool IsLoaded => isLoaded;

        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        public string ConfigFilePath => configFilePath;

        // ========== 便捷属性 ==========

        /// <summary>
        /// 商店终端快捷键
        /// </summary>
        public KeyCode ShopTerminalHotkey => GetKeyCode(KEY_SHOP_TERMINAL_HOTKEY, KeyCode.F8);

        /// <summary>
        /// 商店终端冷却时间
        /// </summary>
        public float ShopTerminalCooldown => GetFloat(KEY_SHOP_TERMINAL_COOLDOWN, 30f);

        /// <summary>
        /// 商店终端是否只能在基地使用
        /// </summary>
        public bool ShopTerminalOnlyInBase => GetBool(KEY_SHOP_TERMINAL_ONLY_IN_BASE, false);

        /// <summary>
        /// 黑洞持续时间
        /// </summary>
        public float BlackHoleDuration => GetFloat(KEY_BLACKHOLE_DURATION, 5f);

        /// <summary>
        /// 黑洞吸引范围
        /// </summary>
        public float BlackHolePullRange => GetFloat(KEY_BLACKHOLE_PULL_RANGE, 5f);

        /// <summary>
        /// 黑洞吸引力强度
        /// </summary>
        public float BlackHolePullForce => GetFloat(KEY_BLACKHOLE_PULL_FORCE, 3f);

        /// <summary>
        /// 黑洞每秒伤害
        /// </summary>
        public float BlackHoleDamagePerSecond => GetFloat(KEY_BLACKHOLE_DAMAGE_PER_SECOND, 10f);

        /// <summary>
        /// 黑洞是否能伤害自己
        /// </summary>
        public bool BlackHoleCanHurtSelf => GetBool(KEY_BLACKHOLE_CAN_HURT_SELF, false);

        /// <summary>
        /// 调试模式
        /// </summary>
        public bool DebugMode => GetBool(KEY_DEBUG_MODE, false);
    }
}
