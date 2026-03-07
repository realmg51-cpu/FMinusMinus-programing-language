using System;
using System.Collections.Generic;
using System.IO;
using Fminusminus.Errors;
using Fminusminus.Utils.Security;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// IO package - file operations with security
    /// </summary>
    public class IOPackage : BasePackage
    {
        public override string Name => "io";
        public override string Version => "1.0.0";
        public override string Description => "Secure file I/O operations for F--";
        
        private string? _currentFile;
        private List<string> _fileContent = new();
        private bool _inFileBlock;
        private int _fileOperationCount;
        private const int MaxFileOperations = 1000;

        public override void Initialize()
        {
            _methods["CreateFile"] = args =>
            {
                if (args.Length > 0 && args[0] is string filename)
                {
                    string path = args.Length > 1 && args[1] is string p ? p : ".";
                    return CreateFile(filename, path);
                }
                return null;
            };
            
            _methods["BeginWrite"] = args =>
            {
                if (args.Length > 0 && args[0] is string filename)
                {
                    BeginWrite(filename);
                }
                return null;
            };
            
            _methods["WriteLine"] = args =>
            {
                if (args.Length > 0 && args[0] != null)
                {
                    WriteLine(args[0].ToString() ?? "");
                }
                return null;
            };
            
            _methods["Write"] = args =>
            {
                if (args.Length > 0 && args[0] != null)
                {
                    Write(args[0].ToString() ?? "");
                }
                return null;
            };
            
            _methods["EndWrite"] = args =>
            {
                EndWrite();
                return null;
            };
            
            _methods["ListFiles"] = args =>
            {
                string path = args.Length > 0 && args[0] is string p ? p : ".";
                ListFiles(path);
                return null;
            };
            
            _methods["FileExists"] = args =>
            {
                if (args.Length > 0 && args[0] is string filename)
                {
                    return FileExists(filename);
                }
                return false;
            };
            
            _methods["DeleteFile"] = args =>
            {
                if (args.Length > 0 && args[0] is string filename)
                {
                    DeleteFile(filename);
                }
                return null;
            };
            
            _methods["ReadFile"] = args =>
            {
                if (args.Length > 0 && args[0] is string filename)
                {
                    return ReadFile(filename);
                }
                return Array.Empty<string>();
            };
            
            _methods["GetFileInfo"] = args =>
            {
                if (args.Length > 0 && args[0] is string filename)
                {
                    GetFileInfo(filename);
                }
                return null;
            };
            
            _methods["CopyFile"] = args =>
            {
                if (args.Length >= 2 && args[0] is string source && args[1] is string dest)
                {
                    CopyFile(source, dest);
                }
                return null;
            };
            
            _methods["MoveFile"] = args =>
            {
                if (args.Length >= 2 && args[0] is string source && args[1] is string dest)
                {
                    MoveFile(source, dest);
                }
                return null;
            };
        }

        private bool CheckRateLimit()
        {
            if (_fileOperationCount++ > MaxFileOperations)
            {
                Console.WriteLine("⚠️ Too many file operations. Please slow down.");
                return false;
            }
            return true;
        }

        private string? CreateFile(string filename, string? path = null)
        {
            if (!CheckRateLimit()) return null;

            try
            {
                // Validate filename
                if (!SecurePath.IsValidFileName(filename))
                    throw FileError.InvalidCharacters(filename);

                string safePath = path == null ? "." : SecurePath.Sanitize(path);
                string fullPath = Path.Combine(safePath, filename);
                
                if (!fullPath.EndsWith(".txt"))
                    fullPath += ".txt";

                // Create empty file
                File.WriteAllText(fullPath, "");
                Console.WriteLine($"📁 Created file: {Path.GetFileName(fullPath)}");
                
                return fullPath;
            }
            catch (UnauthorizedAccessException)
            {
                throw FileError.AccessDenied(filename);
            }
            catch (Exception ex)
            {
                LogError($"Error creating file: {ex.Message}");
                return null;
            }
        }

        private void BeginWrite(string filename)
        {
            if (!CheckRateLimit()) return;

            try
            {
                if (!SecurePath.IsValidFileName(filename))
                    throw FileError.InvalidCharacters(filename);

                string safePath = SecurePath.Sanitize(filename);
                _currentFile = safePath;
                _inFileBlock = true;
                _fileContent.Clear();
            }
            catch (Exception ex)
            {
                LogError($"Error beginning write: {ex.Message}");
            }
        }

        private void WriteLine(string content)
        {
            if (!_inFileBlock || _currentFile == null)
            {
                Console.WriteLine("⚠️ No file opened for writing");
                return;
            }

            if (_fileContent.Count >= SecurePath.MaxLines)
            {
                Console.WriteLine("⚠️ Too many lines, stopping at 10000");
                return;
            }

            _fileContent.Add(content);
        }

        private void Write(string content)
        {
            if (!_inFileBlock || _currentFile == null)
            {
                Console.WriteLine("⚠️ No file opened for writing");
                return;
            }

            if (_fileContent.Count == 0)
                _fileContent.Add(content);
            else
                _fileContent[_fileContent.Count - 1] += content;
        }

        private void EndWrite()
        {
            if (_currentFile == null || _fileContent.Count == 0)
                return;

            try
            {
                // Check total size
                long totalSize = 0;
                foreach (var line in _fileContent)
                    totalSize += System.Text.Encoding.UTF8.GetByteCount(line) + 2; // +2 for \r\n

                if (totalSize > SecurePath.MaxFileSize)
                {
                    Console.WriteLine($"⚠️ File too large (max {SecurePath.MaxFileSize / 1024 / 1024} MB)");
                    return;
                }

                File.WriteAllLines(_currentFile, _fileContent);
                Console.WriteLine($"💾 Saved: {Path.GetFileName(_currentFile)}");
            }
            catch (Exception ex)
            {
                LogError($"Error saving file: {ex.Message}");
            }
            finally
            {
                _inFileBlock = false;
                _fileContent.Clear();
            }
        }

        private void ListFiles(string path)
        {
            try
            {
                string safePath = SecurePath.Sanitize(path);
                
                if (!Directory.Exists(safePath))
                {
                    Console.WriteLine($"❌ Directory not found: {path}");
                    return;
                }

                var files = Directory.GetFiles(safePath);
                var dirs = Directory.GetDirectories(safePath);

                Console.WriteLine($"\n📁 Contents of '{path}':");
                Console.WriteLine($"   Total: {dirs.Length} folders, {files.Length} files\n");

                foreach (var dir in dirs)
                    Console.WriteLine($"   📂 {Path.GetFileName(dir)}/");
                    
                foreach (var file in files)
                {
                    var info = SecurePath.GetSafeFileInfo(file);
                    Console.WriteLine($"   📄 {info.Name} ({info.Size} bytes)");
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                LogError($"Error listing files: {ex.Message}");
            }
        }

        private bool FileExists(string filename)
        {
            try
            {
                if (!SecurePath.IsValidFileName(filename))
                    return false;

                string safePath = SecurePath.Sanitize(filename);
                return File.Exists(safePath);
            }
            catch
            {
                return false;
            }
        }

        private void DeleteFile(string filename)
        {
            try
            {
                if (!SecurePath.IsValidFileName(filename))
                    throw FileError.InvalidCharacters(filename);

                string safePath = SecurePath.Sanitize(filename);
                
                if (File.Exists(safePath))
                {
                    File.Delete(safePath);
                    Console.WriteLine($"🗑️ Deleted: {Path.GetFileName(safePath)}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error deleting file: {ex.Message}");
            }
        }

        private string[] ReadFile(string filename)
        {
            try
            {
                if (!SecurePath.IsValidFileName(filename))
                    throw FileError.InvalidCharacters(filename);

                string safePath = SecurePath.Sanitize(filename);
                
                if (!File.Exists(safePath))
                {
                    Console.WriteLine($"❌ File not found: {filename}");
                    return Array.Empty<string>();
                }

                if (!SecurePath.IsFileSizeValid(safePath))
                {
                    Console.WriteLine($"⚠️ File too large (max {SecurePath.MaxFileSize / 1024 / 1024} MB)");
                    return Array.Empty<string>();
                }

                return SecurePath.SafeReadAllLines(safePath);
            }
            catch (Exception ex)
            {
                LogError($"Error reading file: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        private void GetFileInfo(string filename)
        {
            try
            {
                if (!SecurePath.IsValidFileName(filename))
                    throw FileError.InvalidCharacters(filename);

                string safePath = SecurePath.Sanitize(filename);
                
                if (File.Exists(safePath))
                {
                    var info = SecurePath.GetSafeFileInfo(safePath);
                    Console.WriteLine($"\n📄 File: {info.Name}");
                    Console.WriteLine($"   Size: {info.Size} bytes");
                    Console.WriteLine($"   Created: {info.CreationTime}");
                    Console.WriteLine($"   Modified: {info.LastWriteTime}");
                    Console.WriteLine($"   Read-only: {info.IsReadOnly}");
                }
                else
                {
                    Console.WriteLine($"❌ File not found: {filename}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error getting file info: {ex.Message}");
            }
        }

        private void CopyFile(string source, string dest)
        {
            try
            {
                if (!SecurePath.IsValidFileName(source) || !SecurePath.IsValidFileName(dest))
                    throw FileError.InvalidCharacters("Invalid filename");

                string safeSource = SecurePath.Sanitize(source);
                string safeDest = SecurePath.Sanitize(dest);

                if (!File.Exists(safeSource))
                {
                    Console.WriteLine($"❌ Source file not found: {source}");
                    return;
                }

                if (!SecurePath.IsFileSizeValid(safeSource))
                {
                    Console.WriteLine($"⚠️ Source file too large to copy");
                    return;
                }

                File.Copy(safeSource, safeDest, true);
                Console.WriteLine($"📋 Copied: {Path.GetFileName(safeSource)} → {Path.GetFileName(safeDest)}");
            }
            catch (Exception ex)
            {
                LogError($"Error copying file: {ex.Message}");
            }
        }

        private void MoveFile(string source, string dest)
        {
            try
            {
                if (!SecurePath.IsValidFileName(source) || !SecurePath.IsValidFileName(dest))
                    throw FileError.InvalidCharacters("Invalid filename");

                string safeSource = SecurePath.Sanitize(source);
                string safeDest = SecurePath.Sanitize(dest);

                if (!File.Exists(safeSource))
                {
                    Console.WriteLine($"❌ Source file not found: {source}");
                    return;
                }

                File.Move(safeSource, safeDest, true);
                Console.WriteLine($"📦 Moved: {Path.GetFileName(safeSource)} → {Path.GetFileName(safeDest)}");
            }
            catch (Exception ex)
            {
                LogError($"Error moving file: {ex.Message}");
            }
        }

        private void LogError(string message)
        {
            // Log to file for debugging (safe, no sensitive info)
            try
            {
                string logPath = Path.Combine(Environment.CurrentDirectory, "fminus-error.log");
                File.AppendAllText(logPath, $"{DateTime.Now}: {message}\n");
            }
            catch
            {
                // Can't log, ignore
            }
            
            // Show user-friendly message
            Console.WriteLine($"❌ An error occurred. Check fminus-error.log for details.");
        }
    }
}
