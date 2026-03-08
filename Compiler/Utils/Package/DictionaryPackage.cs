using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Dictionary package - key-value store with limits
    /// </summary>
    public class DictionaryPackage : BasePackage
    {
        public override string Name => "dictionary";
        public override string Version => "1.0.0";
        public override string Description => "Secure dictionary operations for F--";
        
        private readonly object _dictLock = new object();
        private Dictionary<string, Dictionary<string, object>> _dictionaries = new();
        
        private const int MaxDictCount = 100;
        private const int MaxDictSize = 10000;
        private const int MaxKeyLength = 256;
        private const int MaxValueLength = 10000;
        private const int MaxDisplayItems = 20;
        
        // Rate limiting
        private readonly object _rateLock = new object();
        private DateTime _rateLimitReset = DateTime.UtcNow.AddHours(1);
        private int _operationsThisPeriod = 0;
        private const int MaxOperationsPerPeriod = 100000;

        public override void Initialize()
        {
            base.Initialize();
            
            RegisterMethod("Create", args => 
            {
                if (args.Length > 0 && args[0] is string name)
                    return CreateDictionary(name);
                return false;
            }, 1, 1, "Create a new dictionary");
            
            RegisterMethod("Set", args =>
            {
                if (args.Length >= 3 && args[0] is string dictName && 
                    args[1] is string key)
                {
                    return Set(dictName, key, args[2]);
                }
                return false;
            }, 3, 3, "Set a key-value pair");
            
            RegisterMethod("Get", args =>
            {
                if (args.Length >= 2 && args[0] is string dictName && 
                    args[1] is string key)
                {
                    return Get(dictName, key);
                }
                return null;
            }, 2, 2, "Get value by key");
            
            RegisterMethod("Has", args =>
            {
                if (args.Length >= 2 && args[0] is string dictName && 
                    args[1] is string key)
                {
                    return Has(dictName, key);
                }
                return false;
            }, 2, 2, "Check if key exists");
            
            RegisterMethod("Remove", args =>
            {
                if (args.Length >= 2 && args[0] is string dictName && 
                    args[1] is string key)
                {
                    return Remove(dictName, key);
                }
                return false;
            }, 2, 2, "Remove a key-value pair");
            
            RegisterMethod("Clear", args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    Clear(dictName);
                }
                return null;
            }, 1, 1, "Clear all items");
            
            RegisterMethod("Keys", args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return GetKeys(dictName);
                }
                return Array.Empty<string>();
            }, 1, 1, "Get all keys");
            
            RegisterMethod("Values", args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return GetValues(dictName);
                }
                return Array.Empty<object>();
            }, 1, 1, "Get all values");
            
            RegisterMethod("Count", args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return GetCount(dictName);
                }
                return 0;
            }, 1, 1, "Get item count");
            
            RegisterMethod("Print", args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    PrintDictionary(dictName);
                }
                return null;
            }, 1, 1, "Print dictionary contents");
            
            RegisterMethod("Exists", args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return DictionaryExists(dictName);
                }
                return false;
            }, 1, 1, "Check if dictionary exists");
            
            RegisterMethod("Delete", args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return DeleteDictionary(dictName);
                }
                return false;
            }, 1, 1, "Delete entire dictionary");
            
            LogInfo("Dictionary package initialized");
        }

        private bool CheckRateLimit()
        {
            lock (_rateLock)
            {
                // Reset counter every hour
                if (DateTime.UtcNow > _rateLimitReset)
                {
                    _operationsThisPeriod = 0;
                    _rateLimitReset = DateTime.UtcNow.AddHours(1);
                }
                
                if (_operationsThisPeriod++ > MaxOperationsPerPeriod)
                {
                    LogWarning("Too many dictionary operations. Please wait.");
                    return false;
                }
                
                return true;
            }
        }

        private bool ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                LogWarning("Key cannot be empty");
                return false;
            }
                
            if (key.Length > MaxKeyLength)
            {
                LogWarning($"Key too long (max {MaxKeyLength} characters)");
                return false;
            }
            
            // Block keys that could be used for injection
            if (key.Contains("..") || key.Contains("/") || key.Contains("\\") || 
                key.Contains(":") || key.Contains("*") || key.Contains("?") ||
                key.Contains("<") || key.Contains(">") || key.Contains("|"))
            {
                LogWarning("Invalid key characters");
                return false;
            }
            
            return true;
        }

        private bool ValidateValue(object? value)
        {
            if (value == null) return true;
            
            string strValue = value.ToString() ?? "";
            if (strValue.Length > MaxValueLength)
            {
                LogWarning($"Value too long (max {MaxValueLength} characters)");
                return false;
            }
            
            return true;
        }

        private object CloneValue(object? value)
        {
            if (value == null) return null!;
            
            // Handle primitive types and strings
            if (value is string str)
                return str;
                
            if (value.GetType().IsPrimitive)
                return value;
            
            // For complex types, serialize to string to avoid reference leaks
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(value);
            }
            catch
            {
                return value.ToString() ?? "";
            }
        }

        private bool CreateDictionary(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                LogWarning("Dictionary name cannot be empty");
                return false;
            }
            
            if (!CheckRateLimit()) return false;
            if (!ValidateKey(name)) return false;

            lock (_dictLock)
            {
                if (_dictionaries.Count >= MaxDictCount)
                {
                    LogWarning($"Maximum number of dictionaries reached ({MaxDictCount})");
                    return false;
                }

                if (!_dictionaries.ContainsKey(name))
                {
                    _dictionaries[name] = new Dictionary<string, object>();
                    LogInfo($"Created dictionary: '{name}'");
                    return true;
                }
            }
            
            LogWarning($"Dictionary '{name}' already exists");
            return false;
        }

        private bool Set(string dictName, string key, object? value)
        {
            if (string.IsNullOrWhiteSpace(dictName))
            {
                LogWarning("Dictionary name cannot be empty");
                return false;
            }
            
            if (!CheckRateLimit()) return false;
            if (!ValidateKey(key)) return false;
            if (!ValidateValue(value)) return false;

            lock (_dictLock)
            {
                if (_dictionaries.TryGetValue(dictName, out var dict))
                {
                    if (dict.Count >= MaxDictSize)
                    {
                        LogWarning($"Dictionary '{dictName}' is full (max {MaxDictSize} items)");
                        return false;
                    }

                    dict[key] = CloneValue(value);
                    return true;
                }
            }
            
            LogWarning($"Dictionary '{dictName}' not found");
            return false;
        }

        private object? Get(string dictName, string key)
        {
            if (string.IsNullOrWhiteSpace(dictName) || string.IsNullOrWhiteSpace(key))
                return null;
                
            if (!CheckRateLimit()) return null;
            if (!ValidateKey(key)) return null;

            lock (_dictLock)
            {
                if (_dictionaries.TryGetValue(dictName, out var dict))
                {
                    return dict.TryGetValue(key, out var value) ? value : null;
                }
            }
            return null;
        }

        private bool Has(string dictName, string key)
        {
            if (string.IsNullOrWhiteSpace(dictName) || string.IsNullOrWhiteSpace(key))
                return false;
                
            if (!CheckRateLimit()) return false;
            if (!ValidateKey(key)) return false;

            lock (_dictLock)
            {
                return _dictionaries.TryGetValue(dictName, out var dict) && dict.ContainsKey(key);
            }
        }

        private bool Remove(string dictName, string key)
        {
            if (string.IsNullOrWhiteSpace(dictName) || string.IsNullOrWhiteSpace(key))
                return false;
                
            if (!CheckRateLimit()) return false;
            if (!ValidateKey(key)) return false;

            lock (_dictLock)
            {
                if (_dictionaries.TryGetValue(dictName, out var dict))
                {
                    return dict.Remove(key);
                }
            }
            return false;
        }

        private void Clear(string dictName)
        {
            if (string.IsNullOrWhiteSpace(dictName))
                return;
                
            if (!CheckRateLimit()) return;

            lock (_dictLock)
            {
                if (_dictionaries.TryGetValue(dictName, out var dict))
                {
                    dict.Clear();
                    LogInfo($"Cleared dictionary: '{dictName}'");
                }
            }
        }

        private bool DeleteDictionary(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
                
            lock (_dictLock)
            {
                if (_dictionaries.Remove(name))
                {
                    LogInfo($"Deleted dictionary: '{name}'");
                    return true;
                }
            }
            return false;
        }

        private string[] GetKeys(string dictName)
        {
            if (string.IsNullOrWhiteSpace(dictName))
                return Array.Empty<string>();
                
            if (!CheckRateLimit()) return Array.Empty<string>();

            lock (_dictLock)
            {
                if (_dictionaries.TryGetValue(dictName, out var dict))
                {
                    return dict.Keys.Take(MaxDisplayItems).ToArray();
                }
            }
            return Array.Empty<string>();
        }

        private object[] GetValues(string dictName)
        {
            if (string.IsNullOrWhiteSpace(dictName))
                return Array.Empty<object>();
                
            if (!CheckRateLimit()) return Array.Empty<object>();

            lock (_dictLock)
            {
                if (_dictionaries.TryGetValue(dictName, out var dict))
                {
                    return dict.Values.Take(MaxDisplayItems).ToArray();
                }
            }
            return Array.Empty<object>();
        }

        private int GetCount(string dictName)
        {
            if (string.IsNullOrWhiteSpace(dictName))
                return 0;
                
            if (!CheckRateLimit()) return 0;

            lock (_dictLock)
            {
                return _dictionaries.TryGetValue(dictName, out var dict) ? dict.Count : 0;
            }
        }

        private void PrintDictionary(string dictName)
        {
            if (string.IsNullOrWhiteSpace(dictName))
                return;
                
            if (!CheckRateLimit()) return;

            lock (_dictLock)
            {
                if (_dictionaries.TryGetValue(dictName, out var dict))
                {
                    LogInfo($"Dictionary: '{dictName}' ({dict.Count} items)");
                    Console.WriteLine("========================================");
                    
                    if (dict.Count == 0)
                    {
                        Console.WriteLine("   (empty)");
                    }
                    else
                    {
                        int count = 0;
                        foreach (var kvp in dict)
                        {
                            if (count++ >= MaxDisplayItems)
                            {
                                Console.WriteLine($"   ... and {dict.Count - MaxDisplayItems} more items");
                                break;
                            }

                            string valueStr = kvp.Value?.ToString() ?? "null";
                            if (valueStr.Length > 50)
                                valueStr = valueStr.Substring(0, 47) + "...";
                            
                            Console.WriteLine($"   🔑 {kvp.Key}: {valueStr}");
                        }
                    }
                    Console.WriteLine();
                }
                else
                {
                    LogWarning($"Dictionary '{dictName}' not found");
                }
            }
        }

        private bool DictionaryExists(string dictName)
        {
            if (string.IsNullOrWhiteSpace(dictName))
                return false;
                
            lock (_dictLock)
            {
                return _dictionaries.ContainsKey(dictName);
            }
        }
        
        public override void Dispose()
        {
            lock (_dictLock)
            {
                _dictionaries.Clear();
            }
            base.Dispose();
        }
    }
}
