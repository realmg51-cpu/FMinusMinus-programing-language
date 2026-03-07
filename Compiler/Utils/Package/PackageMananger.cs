using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Manages all F-- packages with security
    /// </summary>
    public class PackageManager : IDisposable
    {
        private static readonly object _instanceLock = new object();
        private static PackageManager? _instance;
        
        private readonly Dictionary<string, BasePackage> _loadedPackages = new();
        private readonly Dictionary<string, Type> _availablePackages = new();
        private readonly Dictionary<string, HashSet<string>> _dependencyGraph = new();
        private readonly ThreadLocal<Stack<string>> _callStack = new(() => new Stack<string>());
        
        private readonly object _rateLock = new object();
        private readonly object _packageLock = new object();
        
        private DateTime _rateLimitReset = DateTime.UtcNow.AddHours(1);
        private int _totalPackageOperations = 0;
        
        private const int MaxPackageOperations = 10000;
        private const int MaxPackages = 20;
        private readonly string _logFile;
        
        private bool _disposed = false;

        private PackageManager()
        {
            _logFile = Path.Combine(Environment.CurrentDirectory, "fminus-package.log");
            DiscoverPackages();
        }

        public static PackageManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        _instance ??= new PackageManager();
                    }
                }
                return _instance;
            }
        }

        private void DiscoverPackages()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                if (assembly == null) return;
                
                var packageTypes = assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(BasePackage)) && !t.IsAbstract);
                
                foreach (var type in packageTypes)
                {
                    try
                    {
                        // Check if type has parameterless constructor
                        if (type.GetConstructor(Type.EmptyTypes) == null)
                        {
                            LogError($"Package {type.Name} missing parameterless constructor", null);
                            continue;
                        }
                        
                        var instance = Activator.CreateInstance(type) as BasePackage;
                        if (instance != null && !string.IsNullOrWhiteSpace(instance.Name))
                        {
                            lock (_availablePackages)
                            {
                                if (!_availablePackages.ContainsKey(instance.Name))
                                {
                                    _availablePackages[instance.Name] = type;
                                }
                                else
                                {
                                    LogError($"Duplicate package name: {instance.Name}", null);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Failed to load package {type.Name}", ex);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var loaderEx in ex.LoaderExceptions ?? Array.Empty<Exception>())
                    LogError("Type load error", loaderEx);
            }
            catch (Exception ex)
            {
                LogError("Error discovering packages", ex);
            }
        }

        private bool CheckRateLimit()
        {
            lock (_rateLock)
            {
                if (DateTime.UtcNow > _rateLimitReset)
                {
                    _totalPackageOperations = 0;
                    _rateLimitReset = DateTime.UtcNow.AddHours(1);
                }
                
                if (_totalPackageOperations++ > MaxPackageOperations)
                {
                    LogError("Rate limit exceeded. Too many package operations.", null);
                    return false;
                }
                return true;
            }
        }

        private bool CheckCyclicDependency(string packageName)
        {
            if (_callStack.Value!.Contains(packageName))
            {
                LogError($"Cyclic dependency detected: {string.Join(" -> ", _callStack.Value)} -> {packageName}", null);
                return false;
            }
            return true;
        }

        private void LogError(string message, Exception? ex)
        {
            try
            {
                // Rotate log if too large (>1MB)
                if (File.Exists(_logFile) && new FileInfo(_logFile).Length > 1024 * 1024)
                {
                    string backup = _logFile + ".old";
                    if (File.Exists(backup)) File.Delete(backup);
                    File.Move(_logFile, backup);
                }
                
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                if (ex != null)
                    logMessage += $" - {ex.GetType().Name}: {ex.Message}";
                
                File.AppendAllText(_logFile, logMessage + "\n");
            }
            catch
            {
                // Can't log, ignore
            }
        }

        public BasePackage? ImportPackage(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                LogError("Package name cannot be empty", null);
                return null;
            }
            
            if (!CheckRateLimit()) return null;

            // Validate package name
            if (packageName.Length > 100)
            {
                LogError($"Package name too long: {packageName}", null);
                return null;
            }

            lock (_packageLock)
            {
                // Check if already loaded
                if (_loadedPackages.TryGetValue(packageName, out var loadedPackage))
                {
                    return loadedPackage;
                }

                // Check max packages
                if (_loadedPackages.Count >= MaxPackages)
                {
                    LogError($"Maximum number of packages reached ({MaxPackages})", null);
                    return null;
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
                        // Subscribe to package events
                        if (package is BasePackage basePkg)
                        {
                            basePkg.LogEvent += (sender, args) =>
                            {
                                LogError($"Package {packageName}: {args.Level} - {args.Message}", args.Exception);
                            };
                        }
                        
                        package.Initialize();
                        _loadedPackages[packageName] = package;
                        
                        // Track dependency
                        if (_callStack.Value!.Count > 0)
                        {
                            var caller = _callStack.Value.Peek();
                            if (!_dependencyGraph.ContainsKey(caller))
                                _dependencyGraph[caller] = new HashSet<string>();
                            _dependencyGraph[caller].Add(packageName);
                        }
                        
                        return package;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to load package '{packageName}'", ex);
                }

                return null;
            }
        }

        public void ImportPackages(IEnumerable<string> packageNames, int timeoutMs = 5000)
        {
            if (packageNames == null) return;
            
            var packages = packageNames.Distinct().Take(10).ToList();
            
            foreach (var name in packages)
            {
                try
                {
                    var task = Task.Run(() => ImportPackage(name));
                    if (!task.Wait(timeoutMs))
                    {
                        LogError($"Package '{name}' import timed out", null);
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error importing package '{name}'", ex);
                }
            }
        }

        public bool IsPackageLoaded(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName)) return false;
            
            lock (_packageLock)
            {
                return _loadedPackages.ContainsKey(packageName);
            }
        }

        public BasePackage? GetPackage(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName)) return null;
            
            lock (_packageLock)
            {
                return _loadedPackages.TryGetValue(packageName, out var package) ? package : null;
            }
        }

        public object? CallMethod(string packageName, string methodName, object?[]? args)
        {
            if (string.IsNullOrWhiteSpace(packageName) || string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("Package and method names cannot be empty");
            
            if (!CheckRateLimit()) return null;

            if (!CheckCyclicDependency(packageName))
                throw new InvalidOperationException("Cyclic dependency detected");

            var package = GetPackage(packageName);
            if (package == null)
            {
                LogError($"Package '{packageName}' not loaded", null);
                throw new InvalidOperationException($"Package '{packageName}' not loaded. Did you forget to import it?");
            }

            if (!package.HasMethod(methodName))
            {
                string availableMethods = string.Join(", ", package.GetMethodNames().Take(5));
                LogError($"Method '{methodName}' not found in package '{packageName}'. Available: {availableMethods}", null);
                throw new InvalidOperationException($"Method '{methodName}' not found in package '{packageName}'");
            }

            args ??= Array.Empty<object?>();
            
            _callStack.Value!.Push(packageName);
            try
            {
                return package.CallMethod(methodName, args);
            }
            catch (Exception ex)
            {
                LogError($"Error calling {packageName}.{methodName}", ex);
                throw new InvalidOperationException($"Error calling {packageName}.{methodName}: {ex.Message}", ex);
            }
            finally
            {
                _callStack.Value!.Pop();
            }
        }

        public IEnumerable<string> GetLoadedPackages()
        {
            lock (_packageLock)
            {
                return _loadedPackages.Keys.ToList();
            }
        }

        public IEnumerable<string> GetAvailablePackages()
        {
            lock (_availablePackages)
            {
                return _availablePackages.Keys.ToList();
            }
        }

        public void UnloadPackage(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName)) return;
            
            lock (_packageLock)
            {
                if (_loadedPackages.TryGetValue(packageName, out var package))
                {
                    try
                    {
                        if (package is IDisposable disposable)
                            disposable.Dispose();
                        
                        _loadedPackages.Remove(packageName);
                        LogError($"Package '{packageName}' unloaded", null);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error unloading package '{packageName}'", ex);
                    }
                }
            }
        }

        public void ClearPackages()
        {
            lock (_packageLock)
            {
                foreach (var package in _loadedPackages.Values.ToList())
                {
                    try
                    {
                        if (package is IDisposable disposable)
                            disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error disposing package {package.Name}", ex);
                    }
                }
                _loadedPackages.Clear();
                _dependencyGraph.Clear();
            }
            
            lock (_rateLock)
            {
                _totalPackageOperations = 0;
                _rateLimitReset = DateTime.UtcNow.AddHours(1);
            }
            
            _callStack.Value?.Clear();
        }

        public string GetStats()
        {
            lock (_packageLock)
            {
                return $@"
📦 Package Manager Statistics:
   Loaded: {_loadedPackages.Count} packages
   Available: {_availablePackages.Count} packages
   Operations: {_totalPackageOperations}/{MaxPackageOperations}
   Dependencies: {_dependencyGraph.Sum(d => d.Value.Count)} edges";
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            ClearPackages();
            _callStack.Dispose();
            
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
