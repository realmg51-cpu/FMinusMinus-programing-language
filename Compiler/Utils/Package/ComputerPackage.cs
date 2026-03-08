using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Computer package - core system functions with security
    /// </summary>
    public class ComputerPackage : BasePackage
    {
        public override string Name => "computer";
        public override string Version => "2.0.0";
        public override string Description => "Core system functions for F--";
        
        private Dictionary<string, object> _variables = new();
        private int _varCount;
        private const int MaxVariables = 1000;

        public override void Initialize()
        {
            // Print methods
            _methods["Print"] = args =>
            {
                if (args.Length > 0 && args[0] != null)
                    Console.Write(SafeString(args[0].ToString() ?? ""));
                return null;
            };
            
            _methods["PrintLn"] = args =>
            {
                if (args.Length > 0 && args[0] != null)
                    Console.WriteLine(SafeString(args[0].ToString() ?? ""));
                else
                    Console.WriteLine();
                return null;
            };
            
            // Variable methods with limits
            _methods["SetVar"] = args =>
            {
                if (args.Length >= 2 && args[0] is string name)
                {
                    return SetVariable(name, args[1]);
                }
                return false;
            };
            
            _methods["GetVar"] = args =>
            {
                if (args.Length > 0 && args[0] is string name)
                {
                    return GetVariable(name);
                }
                return null;
            };
            
            _methods["HasVar"] = args =>
            {
                if (args.Length > 0 && args[0] is string name)
                {
                    return _variables.ContainsKey(name);
                }
                return false;
            };
            
            _methods["ClearVars"] = args =>
            {
                _variables.Clear();
                _varCount = 0;
                return null;
            };
            
            // Type conversion methods
            _methods["ToInt"] = args =>
            {
                if (args.Length > 0 && args[0] != null)
                {
                    try { return Convert.ToInt32(args[0]); } catch { return 0; }
                }
                return 0;
            };
            
            _methods["ToDouble"] = args =>
            {
                if (args.Length > 0 && args[0] != null)
                {
                    try { return Convert.ToDouble(args[0]); } catch { return 0.0; }
                }
                return 0.0;
            };
            
            _methods["ToString"] = args =>
            {
                return args.Length > 0 ? SafeString(args[0]?.ToString() ?? "") : "";
            };
            
            _methods["ToBool"] = args =>
            {
                if (args.Length > 0 && args[0] != null)
                {
                    if (args[0] is bool b) return b;
                    if (args[0] is int i) return i != 0;
                    if (args[0] is string s) return !string.IsNullOrEmpty(s);
                    return true;
                }
                return false;
            };
            
            // System info methods (safe, no sensitive data)
            _methods["GetOS"] = args => GetOS();
            _methods["GetMachineName"] = args => Environment.MachineName;
            _methods["GetProcessorCount"] = args => Environment.ProcessorCount;
            _methods["GetOSVersion"] = args => "Protected"; // Hide full version
            _methods["GetUserName"] = args => Environment.UserName;
            _methods["Is64Bit"] = args => Environment.Is64BitOperatingSystem;
            _methods["GetCurrentDirectory"] = args => Path.GetFileName(Environment.CurrentDirectory); // Only show folder name
            
            _methods["GetInfo"] = args => GetSafeInfo();
            _methods["GetOSPath"] = args => GetOSPath();
        }

        private string SafeString(string input)
        {
            // Remove any potentially dangerous characters
            return input.Replace("\0", "").Replace("\b", "").Replace("\v", "");
        }

        private bool SetVariable(string name, object? value)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (name.Length > 256)
            {
                Console.WriteLine("⚠️ Variable name too long");
                return false;
            }

            if (!_variables.ContainsKey(name) && _variables.Count >= MaxVariables)
            {
                Console.WriteLine($"⚠️ Too many variables (max {MaxVariables})");
                return false;
            }

            _variables[name] = value!;
            return true;
        }

        private object? GetVariable(string name)
        {
            return _variables.TryGetValue(name, out var value) ? value : null;
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

        private string GetOSPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "C:\\Users";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "/home";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "/Users";
            return ".";
        }

        private string GetSafeInfo()
        {
            return $@"
╔══════════════════════════════════════════╗
║         F-- SYSTEM INFORMATION           ║
╠══════════════════════════════════════════╣
║ OS: {GetOS(),-32} ║
║ Machine: {Environment.MachineName,-30} ║
║ CPU Cores: {Environment.ProcessorCount,-27} ║
║ User: {Environment.UserName,-32} ║
║ 64-bit: {Environment.Is64BitOperatingSystem,-29} ║
║ Folder: {Path.GetFileName(Environment.CurrentDirectory),-29} ║
╚══════════════════════════════════════════╝";
        }
    }
}
