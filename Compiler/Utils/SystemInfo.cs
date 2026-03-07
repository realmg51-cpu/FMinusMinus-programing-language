using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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
        public DateTime ObjectCreatedTime { get; }
        public TimeSpan SystemUptime => TimeSpan.FromMilliseconds(Environment.TickCount64);

        public SystemInfo()
        {
            OS = GetOS();
            OSVersion = GetFormattedOSVersion();
            MachineName = Environment.MachineName;
            ProcessorCount = Environment.ProcessorCount;
            TotalMemory = GetTotalMemory();
            AvailableMemory = GetAvailableMemory();
            DotNetVersion = Environment.Version.ToString();
            UserName = Environment.UserName;
            Is64Bit = Environment.Is64BitOperatingSystem;
            CurrentDirectory = Environment.CurrentDirectory;
            ObjectCreatedTime = DateTime.Now;
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

        private string GetFormattedOSVersion()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var version = Environment.OSVersion.Version;
                    return $"Windows {version.Major}.{version.Minor}.{version.Build}";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (File.Exists("/etc/os-release"))
                    {
                        var lines = File.ReadAllLines("/etc/os-release");
                        foreach (var line in lines)
                        {
                            if (line.StartsWith("PRETTY_NAME="))
                                return line.Substring(13).Trim('"');
                        }
                    }
                    return "Linux";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return $"macOS {Environment.OSVersion.Version}";
                }
            }
            catch { }
            return Environment.OSVersion.ToString();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryStatus
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MemoryStatus lpBuffer);

        private long GetTotalMemory()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var memoryStatus = new MemoryStatus { dwLength = (uint)Marshal.SizeOf<MemoryStatus>() };
                    if (GlobalMemoryStatusEx(ref memoryStatus))
                        return (long)memoryStatus.ullTotalPhys;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && File.Exists("/proc/meminfo"))
                {
                    string memInfo = File.ReadAllText("/proc/meminfo");
                    var match = Regex.Match(memInfo, @"MemTotal:\s+(\d+)");
                    if (match.Success && long.TryParse(match.Groups[1].Value, out long kb))
                        return kb * 1024;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return GetMacOSTotalMemory();
                }
                
                return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var memoryStatus = new MemoryStatus { dwLength = (uint)Marshal.SizeOf<MemoryStatus>() };
                    if (GlobalMemoryStatusEx(ref memoryStatus))
                        return (long)memoryStatus.ullAvailPhys;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && File.Exists("/proc/meminfo"))
                {
                    string memInfo = File.ReadAllText("/proc/meminfo");
                    
                    // Try MemAvailable first (Linux 3.14+)
                    var match = Regex.Match(memInfo, @"MemAvailable:\s+(\d+)");
                    if (match.Success && long.TryParse(match.Groups[1].Value, out long kb))
                        return kb * 1024;
                    
                    // Fallback to MemFree
                    match = Regex.Match(memInfo, @"MemFree:\s+(\d+)");
                    if (match.Success && long.TryParse(match.Groups[1].Value, out long freeKb))
                        return freeKb * 1024;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return GetMacOSAvailableMemory();
                }
                
                return 8L * 1024 * 1024 * 1024;
            }
            catch
            {
                return 8L * 1024 * 1024 * 1024;
            }
        }

        private long GetMacOSTotalMemory()
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "sysctl",
                        Arguments = "hw.memsize",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                if (long.TryParse(output.Replace("hw.memsize:", "").Trim(), out long bytes))
                    return bytes;
            }
            catch { }
            return 16L * 1024 * 1024 * 1024;
        }

        private long GetMacOSAvailableMemory()
        {
            try
            {
                long total = GetMacOSTotalMemory();
                // Rough estimate - in real app, use host_statistics64 or vm_stat
                return total / 4;
            }
            catch { }
            return 8L * 1024 * 1024 * 1024;
        }

        /// <summary>
        /// Get OS-specific user path
        /// </summary>
        public static string GetOSPath()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    return string.IsNullOrEmpty(userProfile) ? "C:\\Users\\Public" : userProfile;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string home = Environment.GetEnvironmentVariable("HOME");
                    return string.IsNullOrEmpty(home) ? "/home" : home;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    string home = Environment.GetEnvironmentVariable("HOME");
                    return string.IsNullOrEmpty(home) ? "/Users" : home;
                }
            }
            catch { }
            return Environment.CurrentDirectory;
        }

        /// <summary>
        /// Format memory to human readable string
        /// </summary>
        private string FormatMemory(long bytes)
        {
            if (bytes <= 0) return "0 B";
            
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
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
║ Uptime: {SystemUptime:hh\:mm\:ss,-28} ║
╚══════════════════════════════════════════╝";
        }
    }
}
