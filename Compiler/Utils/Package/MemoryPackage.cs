using System;
using System.Collections.Generic;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Memory package - memory management with limits
    /// </summary>
    public class MemoryPackage : BasePackage
    {
        public override string Name => "memory";
        public override string Version => "1.0.0";
        public override string Description => "Secure memory management for F--";
        
        private readonly object _memoryLock = new object();
        private readonly object _rateLock = new object();
        
        private long _totalMemory = 1024; // MB
        private long _usedMemory = 256;
        private DateTime _rateLimitReset = DateTime.UtcNow.AddHours(1);
        private int _operationCount = 0;
        private int _allocCount;
        
        private Dictionary<string, List<int>> _allocations = new();
        
        private const int MaxAllocations = 1000;
        private const int MaxAllocationSize = 512; // MB
        private const int MinAllocationSize = 1; // MB
        private const int BaseMemory = 256; // MB
        private const int MaxOperations = 10000;

        public override void Initialize()
        {
            base.Initialize();
            
            RegisterMethod("GetTotal", args => _totalMemory, 0, 0, "Get total memory");
            RegisterMethod("GetUsed", args => GetUsedMemory(), 0, 0, "Get used memory");
            RegisterMethod("GetFree", args => GetMemoryLeft(), 0, 0, "Get free memory");
            
            RegisterMethod("PrintTotal", args =>
            {
                Console.WriteLine($"Total Memory: {_totalMemory} MB");
                return null;
            }, 0, 0, "Print total memory");
            
            RegisterMethod("PrintUsed", args =>
            {
                Console.WriteLine($"Used Memory: {GetUsedMemory()} MB");
                return null;
            }, 0, 0, "Print used memory");
            
            RegisterMethod("PrintFree", args =>
            {
                Console.WriteLine($"Free Memory: {GetMemoryLeft()} MB");
                return null;
            }, 0, 0, "Print free memory");
            
            RegisterMethod("Allocate", args => Allocate(args), 1, 1, "Allocate memory");
            RegisterMethod("Free", args => Free(args), 1, 1, "Free memory");
            RegisterMethod("GC", args => GarbageCollect(), 0, 0, "Run garbage collection");
            RegisterMethod("Reset", args => Reset(), 0, 0, "Reset memory to initial state");
            RegisterMethod("GetMemoryInfo", args => GetMemoryInfo(), 0, 0, "Get memory information");
            RegisterMethod("GetStats", args => GetStats(), 0, 0, "Get allocation statistics");
            
            LogInfo("Memory package initialized");
        }

        private bool CheckRateLimit()
        {
            lock (_rateLock)
            {
                if (DateTime.UtcNow > _rateLimitReset)
                {
                    _operationCount = 0;
                    _rateLimitReset = DateTime.UtcNow.AddHours(1);
                }
                
                if (_operationCount++ > MaxOperations)
                {
                    LogWarning("Too many memory operations. Please slow down.");
                    return false;
                }
                return true;
            }
        }

        private long GetUsedMemory()
        {
            lock (_memoryLock)
            {
                return _usedMemory;
            }
        }

        private long GetMemoryLeft()
        {
            lock (_memoryLock)
            {
                return _totalMemory - _usedMemory;
            }
        }

        private object Allocate(object?[] args)
        {
            if (!CheckRateLimit()) return false;
            
            if (args.Length == 0 || args[0] == null)
            {
                LogWarning("Allocation size required");
                return false;
            }

            if (!int.TryParse(args[0].ToString(), out int size))
            {
                LogWarning("Invalid allocation size");
                return false;
            }

            string? allocator = args.Length > 1 ? args[1]?.ToString() : null;
            
            return AllocateMemory(size, allocator);
        }

        private bool AllocateMemory(int size, string? allocator = null)
        {
            // Validate size
            if (size < MinAllocationSize || size > MaxAllocationSize)
            {
                LogWarning($"Allocation size must be between {MinAllocationSize} and {MaxAllocationSize} MB");
                return false;
            }

            lock (_memoryLock)
            {
                // Check for overflow
                if (_usedMemory > long.MaxValue - size)
                {
                    LogWarning("Memory allocation would cause overflow");
                    return false;
                }

                // Check allocation count
                if (_allocCount >= MaxAllocations)
                {
                    LogWarning("Too many allocations. Please free some memory first.");
                    return false;
                }

                // Check if enough memory
                if (_usedMemory + size > _totalMemory)
                {
                    LogWarning($"Not enough memory. Available: {GetMemoryLeft()} MB");
                    return false;
                }

                _usedMemory += size;
                _allocCount++;
                
                // Track allocation
                if (allocator != null)
                {
                    if (!_allocations.ContainsKey(allocator))
                        _allocations[allocator] = new List<int>();
                    _allocations[allocator].Add(size);
                }
                
                LogInfo($"Allocated {size} MB (Total used: {_usedMemory} MB)");
                return true;
            }
        }

        private object Free(object?[] args)
        {
            if (!CheckRateLimit()) return false;
            
            if (args.Length == 0 || args[0] == null)
            {
                LogWarning("Free size required");
                return false;
            }

            if (!int.TryParse(args[0].ToString(), out int size))
            {
                LogWarning("Invalid free size");
                return false;
            }

            string? allocator = args.Length > 1 ? args[1]?.ToString() : null;
            
            return FreeMemory(size, allocator);
        }

        private bool FreeMemory(int size, string? allocator = null)
        {
            if (size < MinAllocationSize || size > MaxAllocationSize)
            {
                LogWarning($"Free size must be between {MinAllocationSize} and {MaxAllocationSize} MB");
                return false;
            }

            lock (_memoryLock)
            {
                // Cannot free more than allocated (minus base memory)
                long maxFreeable = _usedMemory - BaseMemory;
                if (maxFreeable <= 0)
                {
                    LogWarning("No memory to free");
                    return false;
                }
                
                if (size > maxFreeable)
                {
                    LogWarning($"Cannot free {size} MB. Maximum freeable: {maxFreeable} MB");
                    return false;
                }

                _usedMemory -= size;
                _allocCount = Math.Max(0, _allocCount - 1);
                
                LogInfo($"Freed {size} MB (Total used: {_usedMemory} MB)");
                return true;
            }
        }

        private object GarbageCollect()
        {
            if (!CheckRateLimit()) return null;
            
            lock (_memoryLock)
            {
                long freedMemory = _usedMemory - BaseMemory;
                if (freedMemory > 0)
                {
                    LogInfo($"Garbage collecting {freedMemory} MB...");
                    _usedMemory = BaseMemory;
                    _allocCount = 0;
                    _allocations.Clear();
                    
                    // Only call real GC if we have significant memory
                    if (System.GC.GetTotalMemory(false) > 100 * 1024 * 1024) // >100MB
                    {
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        LogInfo("Real garbage collection completed");
                    }
                }
                else
                {
                    LogInfo("No memory to garbage collect");
                }
            }
            
            return null;
        }

        private void Reset()
        {
            lock (_memoryLock)
            {
                _usedMemory = BaseMemory;
                _allocCount = 0;
                _allocations.Clear();
                LogInfo("Memory reset to initial state");
            }
        }

        private string GetMemoryInfo()
        {
            lock (_memoryLock)
            {
                long free = _totalMemory - _usedMemory;
                int usagePercent = (int)(_usedMemory * 100 / _totalMemory);
                
                return $@"
╔══════════════════════════════════════╗
║         MEMORY INFORMATION           ║
╠══════════════════════════════════════╣
║ Total:  {_totalMemory,8} MB                    ║
║ Used:   {_usedMemory,8} MB                    ║
║ Free:   {free,8} MB                    ║
║ Usage:  {usagePercent,7}%                    ║
║ Allocs: {_allocCount,7} / {MaxAllocations,-7}               ║
╚══════════════════════════════════════╝";
            }
        }

        private string GetStats()
        {
            lock (_memoryLock)
            {
                var result = $"📊 Memory Statistics:\n";
                result += $"   Total Operations: {_operationCount}\n";
                result += $"   Current Allocations: {_allocCount}\n";
                
                if (_allocations.Count > 0)
                {
                    result += $"   Top Allocators:\n";
                    foreach (var kvp in _allocations)
                    {
                        int total = 0;
                        foreach (var size in kvp.Value)
                            total += size;
                        result += $"      • {kvp.Key}: {kvp.Value.Count} allocations, {total} MB\n";
                    }
                }
                
                return result;
            }
        }
    }
}
