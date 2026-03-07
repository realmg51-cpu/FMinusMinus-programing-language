using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Package method metadata
    /// </summary>
    public class PackageMethodInfo
    {
        public string Name { get; set; } = string.Empty;
        public int MinParameters { get; set; }
        public int MaxParameters { get; set; }
        public string Description { get; set; } = string.Empty;
        public Func<object?[], object?> Method { get; set; } = null!;
        
        public override string ToString()
        {
            return $"{Name}({MinParameters}-{MaxParameters} params)";
        }
    }
    
    /// <summary>
    /// Package log levels
    /// </summary>
    public enum PackageLogLevel
    {
        Info,
        Warning,
        Error
    }
    
    /// <summary>
    /// Package log event arguments
    /// </summary>
    public class PackageLogEventArgs : EventArgs
    {
        public PackageLogLevel Level { get; }
        public string Message { get; }
        public Exception? Exception { get; }
        
        public PackageLogEventArgs(PackageLogLevel level, string message, Exception? ex = null)
        {
            Level = level;
            Message = message;
            Exception = ex;
        }
    }
    
    /// <summary>
    /// Exception thrown when package method is not found
    /// </summary>
    public class PackageMethodNotFoundException : InvalidOperationException
    {
        public string PackageName { get; }
        public string MethodName { get; }
        
        public PackageMethodNotFoundException(string methodName, string packageName)
            : base($"Method '{methodName}' not found in package '{packageName}'")
        {
            MethodName = methodName;
            PackageName = packageName;
        }
    }
    
    /// <summary>
    /// Base class for all F-- packages
    /// </summary>
    public abstract class BasePackage : IDisposable
    {
        /// <summary>
        /// Package name (used in import statement)
        /// </summary>
        public abstract string Name { get; }
        
        /// <summary>
        /// Package version
        /// </summary>
        public virtual string Version => "1.0.0";
        
        /// <summary>
        /// Package description
        /// </summary>
        public virtual string Description => "";
        
        /// <summary>
        /// Methods available in this package
        /// </summary>
        protected ConcurrentDictionary<string, PackageMethodInfo> _methods = new();
        
        /// <summary>
        /// Event for package logging
        /// </summary>
        public event EventHandler<PackageLogEventArgs>? LogEvent;
        
        private bool _disposed = false;
        
        /// <summary>
        /// Initialize the package
        /// </summary>
        public virtual void Initialize()
        {
            LogInfo($"Initializing package '{Name}' v{Version}");
            // Override in derived classes
        }
        
        /// <summary>
        /// Register a method in the package
        /// </summary>
        protected void RegisterMethod(string name, Func<object?[], object?> method, 
                                      int minParams = 0, int maxParams = int.MaxValue, 
                                      string description = "")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Method name cannot be null or empty", nameof(name));
            
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            
            var methodInfo = new PackageMethodInfo
            {
                Name = name,
                MinParameters = minParams,
                MaxParameters = maxParams,
                Description = description,
                Method = method
            };
            
            _methods[name] = methodInfo;
            LogInfo($"Registered method '{name}' (params: {minParams}-{maxParams})");
        }
        
        /// <summary>
        /// Check if method exists
        /// </summary>
        public bool HasMethod(string name) => !string.IsNullOrWhiteSpace(name) && _methods.ContainsKey(name);
        
        /// <summary>
        /// Call a method in this package
        /// </summary>
        public object? CallMethod(string name, object?[]? args)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Method name cannot be null or empty", nameof(name));
            
            args ??= Array.Empty<object?>();
            
            if (!_methods.TryGetValue(name, out var methodInfo))
            {
                LogError($"Method '{name}' not found");
                throw new PackageMethodNotFoundException(name, Name);
            }
            
            // Validate parameter count
            if (args.Length < methodInfo.MinParameters)
            {
                string error = $"Method '{name}' requires at least {methodInfo.MinParameters} parameters, got {args.Length}";
                LogError(error);
                throw new ArgumentException(error);
            }
            
            if (args.Length > methodInfo.MaxParameters)
            {
                string error = $"Method '{name}' accepts at most {methodInfo.MaxParameters} parameters, got {args.Length}";
                LogError(error);
                throw new ArgumentException(error);
            }
            
            LogInfo($"Calling method '{name}' with {args.Length} parameters");
            
            try
            {
                var result = methodInfo.Method(args);
                LogInfo($"Method '{name}' completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                LogError($"Error calling method '{name}'", ex);
                throw new InvalidOperationException($"Error calling method '{name}' in package '{Name}': {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Get all method names
        /// </summary>
        public IEnumerable<string> GetMethodNames() => _methods.Keys;
        
        /// <summary>
        /// Get method info
        /// </summary>
        public PackageMethodInfo? GetMethodInfo(string name)
        {
            _methods.TryGetValue(name, out var info);
            return info;
        }
        
        /// <summary>
        /// Log information
        /// </summary>
        protected void LogInfo(string message)
        {
            LogEvent?.Invoke(this, new PackageLogEventArgs(PackageLogLevel.Info, message));
        }
        
        /// <summary>
        /// Log warning
        /// </summary>
        protected void LogWarning(string message)
        {
            LogEvent?.Invoke(this, new PackageLogEventArgs(PackageLogLevel.Warning, message));
        }
        
        /// <summary>
        /// Log error
        /// </summary>
        protected void LogError(string message, Exception? ex = null)
        {
            LogEvent?.Invoke(this, new PackageLogEventArgs(PackageLogLevel.Error, message, ex));
        }
        
        /// <summary>
        /// Dispose resources
        /// </summary>
        public virtual void Dispose()
        {
            if (_disposed) return;
            
            LogInfo($"Disposing package '{Name}'");
            
            // Clear methods
            _methods.Clear();
            
            _disposed = true;
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Finalizer
        /// </summary>
        ~BasePackage()
        {
            Dispose();
        }
    }
}
