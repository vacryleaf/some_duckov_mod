using UnityEngine;

namespace WormholeTechMod
{
    /// <summary>
    /// 统一的日志管理类
    /// 支持通过开关控制日志输出
    /// </summary>
    internal static class ModLogger
    {
        // 日志开关（生产环境建议设为 false）
        public static bool EnableDebugLog = true;
        public static bool EnableWarningLog = true;
        public static bool EnableErrorLog = true;

        private static string GetPrefix()
        {
            return System.DateTime.Now.ToString("[HH:mm:ss.fff]") + " [虫洞科技]";
        }

        public static void Log(string message)
        {
            if (EnableDebugLog)
            {
                Debug.Log(GetPrefix() + " " + message);
            }
        }

        public static void LogFormat(string format, params object[] args)
        {
            if (EnableDebugLog)
            {
                Debug.LogFormat(GetPrefix() + " " + format, args);
            }
        }

        public static void LogWarning(string message)
        {
            if (EnableWarningLog)
            {
                Debug.LogWarning(GetPrefix() + " " + message);
            }
        }

        public static void LogWarningFormat(string format, params object[] args)
        {
            if (EnableWarningLog)
            {
                Debug.LogWarningFormat(GetPrefix() + " " + format, args);
            }
        }

        public static void LogError(string message)
        {
            if (EnableErrorLog)
            {
                Debug.LogError(GetPrefix() + " " + message);
            }
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            if (EnableErrorLog)
            {
                Debug.LogErrorFormat(GetPrefix() + " " + format, args);
            }
        }
    }
}
