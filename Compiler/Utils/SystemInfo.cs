using System;
using System.Runtime.InteropServices;

namespace Fminusminus.Utils
{
    /// <summary>
    /// System information utility for F--
    /// </summary>
    public class SystemInfo
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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var gcMemoryInfo = GC.GetGCMemoryInfo();
                    return gcMemoryInfo.TotalAvailableMemoryBytes;
                }
                else
                {
                    // Fallback for Linux/macOS
                    return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
                }
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
                // Simple approximation - in real scenario, use platform-specific APIs
                var gcMemoryInfo = GC.GetGCMemoryInfo();
                return gcMemoryInfo.TotalAvailableMemoryBytes - GC.GetTotalMemory(false);
            }
            catch
            {
                return 8L * 1024 * 1024 * 1024; // 8GB default
            }
        }

        /// <summary>
        /// Get OS-specific path
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

        /// <summary>
        /// Format memory to human readable string
        /// </summary>
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
║         F-- SYSTEM INFORMATION           ║
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
}
