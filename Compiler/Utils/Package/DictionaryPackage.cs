using System;
using System.Collections.Generic;
using System.Linq;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Dictionary package - key-value store operations
    /// </summary>
    public class DictionaryPackage : BasePackage
    {
        public override string Name => "dictionary";
        public override string Version => "1.0.0";
        public override string Description => "Dictionary/Map operations for F--";
        
        private Dictionary<string, Dictionary<string, object>> _dictionaries = new();
        
        public override void Initialize()
        {
            _methods["Create"] = args =>
            {
                if (args.Length > 0 && args[0] is string name)
                {
                    CreateDictionary(name);
                }
                return null;
            };
            
            _methods["Set"] = args =>
            {
                if (args.Length >= 3 && args[0] is string dictName && 
                    args[1] is string key)
                {
                    Set(dictName, key, args[2]);
                }
                return null;
            };
            
            _methods["Get"] = args =>
            {
                if (args.Length >= 2 && args[0] is string dictName && 
                    args[1] is string key)
                {
                    return Get(dictName, key);
                }
                return null;
            };
            
            _methods["Has"] = args =>
            {
                if (args.Length >= 2 && args[0] is string dictName && 
                    args[1] is string key)
                {
                    return Has(dictName, key);
                }
                return false;
            };
            
            _methods["Remove"] = args =>
            {
                if (args.Length >= 2 && args[0] is string dictName && 
                    args[1] is string key)
                {
                    return Remove(dictName, key);
                }
                return false;
            };
            
            _methods["Clear"] = args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    Clear(dictName);
                }
                return null;
            };
            
            _methods["Keys"] = args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return GetKeys(dictName);
                }
                return Array.Empty<string>();
            };
            
            _methods["Values"] = args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return GetValues(dictName);
                }
                return Array.Empty<object>();
            };
            
            _methods["Count"] = args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return GetCount(dictName);
                }
                return 0;
            };
            
            _methods["Print"] = args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    PrintDictionary(dictName);
                }
                return null;
            };
            
            _methods["Exists"] = args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return DictionaryExists(dictName);
                }
                return false;
            };
        }
        
        private void CreateDictionary(string name)
        {
            if (!_dictionaries.ContainsKey(name))
            {
                _dictionaries[name] = new Dictionary<string, object>();
                Console.WriteLine($"📚 Created dictionary: '{name}'");
            }
            else
            {
                Console.WriteLine($"⚠️ Dictionary '{name}' already exists");
            }
        }
        
        private void Set(string dictName, string key, object? value)
        {
            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                dict[key] = value!;
            }
            else
            {
                Console.WriteLine($"❌ Dictionary '{dictName}' not found");
            }
        }
        
        private object? Get(string dictName, string key)
        {
            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                return dict.TryGetValue(key, out var value) ? value : null;
            }
            Console.WriteLine($"❌ Dictionary '{dictName}' not found");
            return null;
        }
        
        private bool Has(string dictName, string key)
        {
            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                return dict.ContainsKey(key);
            }
            return false;
        }
        
        private bool Remove(string dictName, string key)
        {
            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                return dict.Remove(key);
            }
            return false;
        }
        
        private void Clear(string dictName)
        {
            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                dict.Clear();
                Console.WriteLine($"🧹 Cleared dictionary: '{dictName}'");
            }
        }
        
        private string[] GetKeys(string dictName)
        {
            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                return dict.Keys.ToArray();
            }
            return Array.Empty<string>();
        }
        
        private object[] GetValues(string dictName)
        {
            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                return dict.Values.ToArray();
            }
            return Array.Empty<object>();
        }
        
        private int GetCount(string dictName)
        {
            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                return dict.Count;
            }
            return 0;
        }
        
        private void PrintDictionary(string dictName)
        {
            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                Console.WriteLine($"\n📚 Dictionary: '{dictName}' ({dict.Count} items)");
                Console.WriteLine("========================================");
                
                if (dict.Count == 0)
                {
                    Console.WriteLine("   (empty)");
                }
                else
                {
                    foreach (var kvp in dict)
                    {
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
                Console.WriteLine($"❌ Dictionary '{dictName}' not found");
            }
        }
        
        private bool DictionaryExists(string dictName)
        {
            return _dictionaries.ContainsKey(dictName);
        }
    }
}
