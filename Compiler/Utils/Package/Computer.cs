using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Computer system utilities for F--
    /// Handles printing, variables, data types, and system information
    /// </summary>
    public static class Computer
    {
        private static Dictionary<string, object> _variables = new();
        private static SystemInfo _systemInfo = new();

        #region Printing Methods

        /// <summary>
        /// Print with newline
        /// </summary>
        public static void PrintLn(string value)
        {
            Console.WriteLine(value);
        }

        /// <summary>
        /// Print without newline
        /// </summary>
        public static void Print(string value)
        {
            Console.Write(value);
        }

        /// <summary>
        /// Print with interpolation
        /// </summary>
        public static void PrintLn(string template, Dictionary<string, object> variables)
        {
            Console.WriteLine(Interpolate(template, variables));
        }

        /// <summary>
        /// Print without newline with interpolation
        /// </summary>
        public static void Print(string template, Dictionary<string, object> variables)
        {
            Console.Write(Interpolate(template, variables));
        }

        private static string Interpolate(string template, Dictionary<string, object> variables)
        {
            var result = template;
            foreach (var kvp in variables)
            {
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
            }
            return result;
        }

        #endregion

        #region Variable Management

        /// <summary>
        /// Set variable value
        /// </summary>
        public static void SetVariable(string name, object value)
        {
            _variables[name] = value;
        }

        /// <summary>
        /// Get variable value
        /// </summary>
        public static object? GetVariable(string name)
        {
            return _variables.TryGetValue(name, out var value) ? value : null;
        }

        /// <summary>
        /// Check if variable exists
        /// </summary>
        public static bool HasVariable(string name)
        {
            return _variables.ContainsKey(name);
        }

        /// <summary>
        /// Clear all variables
        /// </summary>
        public static void ClearVariables()
        {
            _variables.Clear();
        }

        #endregion

        #region Data Type Conversion

        /// <summary>
        /// Convert to integer
        /// </summary>
        public static int ToInt(object value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Convert to double
        /// </summary>
        public static double ToDouble(object value)
        {
            try
            {
                return Convert.ToDouble(value);
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        public static string ToString(object value)
        {
            return value?.ToString() ?? "";
        }

        /// <summary>
        /// Convert to boolean
        /// </summary>
        public static bool ToBool(object value)
        {
            if (value is bool b) return b;
            if (value is int i) return i != 0;
            if (value is string s) return !string.IsNullOrEmpty(s);
            return value != null;
        }

        #endregion

        #region System Information

        /// <summary>
        /// Get all system information
        /// </summary>
        public static string GetInfo()
        {
            return _systemInfo.ToString();
        }

        /// <summary>
        /// Get specific system information
        /// </summary>
        public static string GetInfo(string property)
        {
            return property.ToLower() switch
            {
                "os" => _systemInfo.OS,
                "version" => _systemInfo.OSVersion,
                "machine" => _systemInfo.MachineName,
                "cpu" => _systemInfo.ProcessorCount.ToString(),
                "memory" => _systemInfo.TotalMemory.ToString(),
                "free" => _systemInfo.AvailableMemory.ToString(),
                "dotnet" => _systemInfo.DotNetVersion,
                "user" => _systemInfo.UserName,
                "is64" => _systemInfo.Is64Bit.ToString(),
                "directory" => _systemInfo.CurrentDirectory,
                "time" => _systemInfo.StartupTime.ToString("HH:mm:ss"),
                _ => "Unknown property"
            };
        }

        /// <summary>
        /// Get OS path
        /// </summary>
        public static string GetOSPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "C:\\";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "/home";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "/Users";
            else
                return ".";
        }

        #endregion

        #region Helper Classes

        private class SystemInfo
        {
            public string OS { get; }
            public string OSVersion { get; }
            public string MachineName { get; }
            public int ProcessorCount { get; }
            public long TotalMemory { get; }
            public long AvailableMemory { get; }
            public string DotNetVersion { get; }
            public string UserName { get; }
            public bool Is64Bit { get; }
            public string CurrentDirectory { get; }
            public DateTime StartupTime { get; }

            public SystemInfo()
            {
                OS = GetOS();
                OSVersion = Environment.OSVersion.ToString();
                MachineName = Environment.MachineName;
                ProcessorCount = Environment.ProcessorCount;
                TotalMemory = GetTotalMemory();
                AvailableMemory = GetAvailableMemory();
                DotNetVersion = Environment.Version.ToString();
                UserName = Environment.UserName;
                Is64Bit = Environment.Is64BitOperatingSystem;
                CurrentDirectory = Environment.CurrentDirectory;
                StartupTime = DateTime.Now;
            }

            private string GetOS()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return "Windows";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return "Linux";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return "macOS";
                return "Unknown";
            }

            private long GetTotalMemory()
            {
                try
                {
                    var gcMemoryInfo = GC.GetGCMemoryInfo();
                    return gcMemoryInfo.TotalAvailableMemoryBytes;
                }
                catch
                {
                    return 16L * 1024 * 1024 * 1024; // 16GB default
                }
            }

            private long GetAvailableMemory()
            {
                try
                {
                    var gcMemoryInfo = GC.GetGCMemoryInfo();
                    return gcMemoryInfo.TotalAvailableMemoryBytes - GC.GetTotalMemory(false);
                }
                catch
                {
                    return 8L * 1024 * 1024 * 1024; // 8GB default
                }
            }

            private string FormatMemory(long bytes)
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = bytes;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }

            public override string ToString()
            {
                return $@"
╔══════════════════════════════════════════╗
║         COMPUTER SYSTEM INFORMATION      ║
╠══════════════════════════════════════════╣
║ OS: {OS,-32} ║
║ Version: {OSVersion,-30} ║
║ Machine: {MachineName,-30} ║
║ CPU Cores: {ProcessorCount,-27} ║
║ RAM Total: {FormatMemory(TotalMemory),-27} ║
║ RAM Free: {FormatMemory(AvailableMemory),-28} ║
║ .NET: {DotNetVersion,-32} ║
║ User: {UserName,-32} ║
║ 64-bit: {Is64Bit,-29} ║
║ Directory: {CurrentDirectory,-27} ║
║ Started: {StartupTime:HH:mm:ss,-28} ║
╚══════════════════════════════════════════╝";
            }
        }

        #endregion
    }
}
