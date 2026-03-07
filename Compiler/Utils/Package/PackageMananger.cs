using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Manages all F-- packages with security
    /// </summary>
    public class PackageManager
    {
        private static PackageManager? _instance;
        private readonly Dictionary<string, BasePackage> _loadedPackages = new();
        private readonly Dictionary<string, Type> _availablePackages = new();
        
        private int _totalPackageOperations;
        private const int MaxPackageOperations = 10000;
        private readonly string _logFile;

        private PackageManager()
        {
            _logFile = Path.Combine(Environment.CurrentDirectory, "fminus-package.log");
            DiscoverPackages();
        }

        public static PackageManager Instance => _instance ??= new PackageManager();

        private void DiscoverPackages()
        {
            var packageTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BasePackage)) && !t.IsAbstract);
            
            foreach (var type in packageTypes)
            {
                try
                {
                    var instance = Activator.CreateInstance(type) as BasePackage;
                    if (instance != null)
                    {
                        _availablePackages[instance.Name] = type;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to load package {type.Name}", ex);
                }
            }
        }

        private bool CheckRateLimit()
        {
            if (_totalPackageOperations++ > MaxPackageOperations)
            {
                LogError("Rate limit exceeded", null);
                return false;
            }
            return true;
        }

        private void LogError(string message, Exception? ex)
        {
            try
            {
                string logMessage = $"{DateTime.Now}: {message}";
                if (ex != null)
                    logMessage += $" - {ex.Message}";
                
                File.AppendAllText(_logFile, logMessage + "\n");
            }
            catch
            {
                // Can't log, ignore
            }
        }

        public BasePackage? ImportPackage(string packageName)
        {
            if (!CheckRateLimit()) return null;

            // Validate package name
            if (string.IsNullOrEmpty(packageName) || packageName.Length > 100)
            {
                LogError($"Invalid package name: {packageName}", null);
                return null;
            }

            // Check if already loaded
            if (_loadedPackages.TryGetValue(packageName, out var loadedPackage))
            {
                return loadedPackage;
            }

            // Check if available
            if (!_availablePackages.TryGetValue(packageName, out var packageType))
            {
                LogError($"Package '{packageName}' not found", null);
                return null;
            }

            // Load package
            try
            {
                var package = Activator.CreateInstance(packageType) as BasePackage;
                if (package != null)
                {
                    package.Initialize();
                    _loadedPackages[packageName] = package;
                    return package;
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to load package '{packageName}'", ex);
            }

            return null;
        }

        public void ImportPackages(IEnumerable<string> packageNames)
        {
            foreach (var name in packageNames.Distinct().Take(10)) // Max 10 packages
            {
                ImportPackage(name);
            }
        }

        public bool IsPackageLoaded(string packageName)
        {
            return _loadedPackages.ContainsKey(packageName);
        }

        public BasePackage? GetPackage(string packageName)
        {
            return _loadedPackages.TryGetValue(packageName, out var package) ? package : null;
        }

        public object? CallMethod(string packageName, string methodName, object?[] args)
        {
            if (!CheckRateLimit()) return null;

            var package = GetPackage(packageName);
            if (package == null)
            {
                LogError($"Package '{packageName}' not loaded", null);
                throw new Exception($"Package '{packageName}' not loaded. Did you forget to import it?");
            }

            try
            {
                return package.CallMethod(methodName, args);
            }
            catch (Exception ex)
            {
                LogError($"Error calling {packageName}.{methodName}", ex);
                throw;
            }
        }

        public IEnumerable<string> GetLoadedPackages() => _loadedPackages.Keys;
        public IEnumerable<string> GetAvailablePackages() => _availablePackages.Keys;

        public void ClearPackages()
        {
            _loadedPackages.Clear();
            _totalPackageOperations = 0;
        }
    }
}
