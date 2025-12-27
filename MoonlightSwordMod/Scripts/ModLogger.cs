using UnityEngine;

namespace MoonlightSwordMod
{
    /// <summary>
    /// 统一的日志管理类
    /// 支持通过开关控制日志输出
    /// 支持带时间戳的日志（用于性能分析）
    /// </summary>
    internal static class ModLogger
    {
        // 日志开关（生产环境建议设为 false）
        public static bool EnableDebugLog = false;
        public static bool EnableWarningLog = true;
        public static bool EnableErrorLog = true;
        // 是否显示时间戳
        public static bool ShowTimestamp = true;

        // 缓存上次记录的时间（用于计算耗时）
        private static float _lastTimestamp = 0f;
        private static string _lastLabel = "";

        private static string FormatMessage(string message)
        {
            if (ShowTimestamp)
            {
                float currentTime = Time.realtimeSinceStartup;
                float elapsed = _lastTimestamp > 0 ? currentTime - _lastTimestamp : 0;
                _lastTimestamp = currentTime;
                return $"[{(int)currentTime / 60:D2}:{(int)currentTime % 60:D2}.{(int)(currentTime * 1000) % 1000:D3} (+{elapsed * 1000:F1}ms)] {message}";
            }
            return message;
        }

        private static string FormatMessage(string message, string label)
        {
            if (ShowTimestamp)
            {
                float currentTime = Time.realtimeSinceStartup;
                float elapsed = _lastTimestamp > 0 ? currentTime - _lastTimestamp : 0;
                _lastTimestamp = currentTime;
                _lastLabel = label;
                return $"[{(int)currentTime / 60:D2}:{(int)currentTime % 60:D2}.{(int)(currentTime * 1000) % 1000:D3} (+{elapsed * 1000:F1}ms)] [{label}] {message}";
            }
            return $"[{label}] {message}";
        }

        public static void Log(string message)
        {
            if (EnableDebugLog)
            {
                Debug.Log(FormatMessage(message));
            }
        }

        public static void Log(string message, string label)
        {
            if (EnableDebugLog)
            {
                Debug.Log(FormatMessage(message, label));
            }
        }

        public static void LogFormat(string format, params object[] args)
        {
            if (EnableDebugLog)
            {
                Debug.LogFormat(FormatMessage(format), args);
            }
        }

        public static void LogWarning(string message)
        {
            if (EnableWarningLog)
            {
                Debug.LogWarning(FormatMessage(message));
            }
        }

        public static void LogWarning(string message, string label)
        {
            if (EnableWarningLog)
            {
                Debug.LogWarning(FormatMessage(message, label));
            }
        }

        public static void LogWarningFormat(string format, params object[] args)
        {
            if (EnableWarningLog)
            {
                Debug.LogWarningFormat(FormatMessage(format), args);
            }
        }

        public static void LogError(string message)
        {
            if (EnableErrorLog)
            {
                Debug.LogError(FormatMessage(message));
            }
        }

        public static void LogError(string message, string label)
        {
            if (EnableErrorLog)
            {
                Debug.LogError(FormatMessage(message, label));
            }
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            if (EnableErrorLog)
            {
                Debug.LogErrorFormat(FormatMessage(format), args);
            }
        }

        /// <summary>
        /// 开始计时（记录时间点）
        /// </summary>
        public static void StartTimer(string label)
        {
            _lastTimestamp = Time.realtimeSinceStartup;
            _lastLabel = label;
        }

        /// <summary>
        /// 结束计时并输出耗时
        /// </summary>
        public static void EndTimer(string message)
        {
            float elapsed = Time.realtimeSinceStartup - _lastTimestamp;
            Log($"{message} 耗时: {elapsed * 1000:F2}ms", _lastLabel);
        }

        /// <summary>
        /// 输出耗时标记（不改变计时基准）
        /// </summary>
        public static void MarkTime(string label)
        {
            float currentTime = Time.realtimeSinceStartup;
            float elapsed = _lastTimestamp > 0 ? currentTime - _lastTimestamp : 0;
            Debug.Log($"[{(int)currentTime / 60:D2}:{(int)currentTime % 60:D2}.{(int)(currentTime * 1000) % 1000:D3} (+{elapsed * 1000:F1}ms)] [{label}]");
            _lastTimestamp = currentTime;
        }
    }
}
