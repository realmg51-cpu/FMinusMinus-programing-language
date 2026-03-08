using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fminusminus.Errors;
using Fminusminus.Utils.Security;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// IO package - file operations with security
    /// </summary>
    public class IOPackage : BasePackage, IDisposable
    {
        public override string Name => "io";
        public override string Version => "1.0.0";
        public override string Description => "Secure file I/O operations for F--";
        
        private string? _currentFile;
        private List<string> _fileContent = new();
        private bool _inFileBlock;
        
        // Thread safety
        private readonly object _fileLock = new object();
        
        // Rate limiting
        private readonly object _rateLock = new object();
        private DateTime _rateLimitReset = DateTime.UtcNow.AddHours(1);
        private int _fileOperationCount = 0;
        private const int MaxFileOperations = 1000;
        
        // Log file path
        private readonly string _logPath;

        public IOPackage()
        {
            _logPath = Path.Combine(Environment.CurrentDirectory, "fminus-io.log");
        }

        public override void Initialize()
        {
            base.Initialize();
            
            RegisterMethod("CreateFile", args =>
            {
                if (args.Length > 0 && args[0] is string filename)
                {
                    string path = args.Length > 1 && args[1] is string p ? p : ".";
                    return CreateFile(filename, path);
                }
                return null;
            }, 1, 2, "Create a new file");
            
            RegisterMethod("BeginWrite", args =>
            {
                if (args.Length > 0 && args[0] is string filename)
                {
                    BeginWrite(filename);
                }
                return null;
            }, 1, 1, "Begin writing to a file");
            
            RegisterMethod("WriteLine", args =>
            {
                if (args.Length > 0 && args[0] != null)
                {
                    WriteLine(args[0].ToString() ?? "");
                }
                return null;
            }, 1, 1, "Write a line to the current file");
            
            RegisterMethod("Write", args =>
            {
                if (args.Length > 0 && args[0] != null)
                {
                    Write(args[0].ToString() ?? "");
                }
                return null;
            }, 1, 1, "Write to current line");
            
            RegisterMethod("EndWrite", args =>
            {
                EndWrite();
                return null;
            }, 0, 0, "End writing and save file");
            
            RegisterMethod("ListFiles", args =>
            {
                string path = args.Length > 0 && args[0] is string p ? p : ".";
                ListFiles(path);
                return null;
            }, 0, 1, "List files in directory");
            
            RegisterMethod("FileExists", args =>
            {
                if (args.Length > 0 && args[0] is string filename)
                {
                    return FileExists(filename);
                }
                return false;
            }, 1, 1, "Check if file exists");
            
            RegisterMethod("DeleteFile", args =>
            {
                if (args.Length > 0 && args[0] is string filename)
                {
                    DeleteFile(filename);
                }
                return null;
            }, 1, 1, "Delete a file");
            
            RegisterMethod("ReadFile", args =>
            {
                if (args.Length > 0 && args[0] is string filename)
                {
                    return ReadFile(filename);
                }
                return Array.Empty<string>();
            }, 1, 1, "Read file contents");
            
            RegisterMethod("GetFileInfo", args =>
            {
                if (args.Length > 0 && args[0] is string filename)
                {
                    GetFileInfo(filename);
                }
                return null;
            }, 1, 1, "Get file information");
            
            RegisterMethod("CopyFile", args =>
            {
                if (args.Length >= 2 && args[0] is string source && args[1] is string dest)
                {
                    CopyFile(source, dest);
                }
                return null;
            }, 2, 2, "Copy a file");
            
            RegisterMethod("MoveFile", args =>
            {
                if (args.Length >= 2 && args[0] is string source && args[1] is string dest)
                {
                    MoveFile(source, dest);
                }
                return null;
            }, 2, 2, "Move/Rename a file");
            
            RegisterMethod("Close", args =>
            {
                Close();
                return null;
            }, 0, 0, "Close any open file");
            
            LogInfo("IO Package initialized");
        }

        private bool CheckRateLimit()
        {
            lock (_rateLock)
            {
                if (DateTime.UtcNow > _rateLimitReset)
                {
                    _fileOperationCount = 0;
                    _rateLimitReset = DateTime.UtcNow.AddHours(1);
                }
                
                if (_fileOperationCount++ > MaxFileOperations)
                {
                    LogWarning("Too many file operations. Please slow down.");
                    return false;
                }
                return true;
            }
        }

        private bool CheckDiskSpace(string path)
        {
            try
            {
                string root = Path.GetPathRoot(path) ?? ".";
                var drive = new DriveInfo(root);
                
                // Need at least 1MB free
                if (drive.AvailableFreeSpace < 1024 * 1024)
                {
                    LogWarning($"Low disk space on {root}. Operation cancelled.");
                    return false;
                }
                return true;
            }
            catch
            {
                // If we can't check disk space, assume it's ok
                return true;
            }
        }

        private void LogError(string message, Exception? ex = null, bool showUserMessage = true)
        {
            string fullMessage = ex != null ? $"{message}: {ex.Message}" : message;
            
            // Rotate log if too large
            try
            {
                if (File.Exists(_logPath) && new FileInfo(_logPath).Length > 1024 * 1024)
                {
                    string backupPath = _logPath + ".old";
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                    File.Move(_logPath, backupPath);
                }
                
                File.AppendAllText(_logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {fullMessage}\n");
            }
            catch
            {
                // Can't log, ignore
            }
            
            LogError(fullMessage); // Call base class LogError
            
            if (showUserMessage)
                Console.WriteLine($"❌ {message}");
        }

        private string? CreateFile(string filename, string? path = null)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                LogWarning("Filename cannot be empty");
                return null;
            }
            
            if (!CheckRateLimit()) return null;

            lock (_fileLock)
            {
                try
                {
                    if (!SecurePath.IsValidFileName(filename))
                        throw FileError.InvalidCharacters(filename);

                    string safePath = path == null ? "." : SecurePath.Sanitize(path);
                    
                    if (!CheckDiskSpace(safePath))
                        return null;
                    
                    string fullPath = Path.Combine(safePath, filename);
                    
                    // Don't auto-add .txt extension
                    
                    // Check if file already exists
                    if (File.Exists(fullPath))
                    {
                        LogWarning($"File '{filename}' already exists");
                        return null;
                    }

                    // Create empty file
                    File.WriteAllText(fullPath, "");
                    LogInfo($"Created file: {Path.GetFileName(fullPath)}");
                    
                    return fullPath;
                }
                catch (UnauthorizedAccessException)
                {
                    throw FileError.AccessDenied(filename);
                }
                catch (Exception ex)
                {
                    LogError("Error creating file", ex);
                    return null;
                }
            }
        }

        private void BeginWrite(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                LogWarning("Filename cannot be empty");
                return;
            }
            
            if (!CheckRateLimit()) return;

            lock (_fileLock)
            {
                if (_inFileBlock)
                {
                    LogWarning("Already in a write block. Call EndWrite first.");
                    return;
                }

                try
                {
                    if (!SecurePath.IsValidFileName(filename))
                        throw FileError.InvalidCharacters(filename);

                    string safePath = SecurePath.Sanitize(filename);
                    
                    if (!CheckDiskSpace(safePath))
                        return;

                    _currentFile = safePath;
                    _inFileBlock = true;
                    _fileContent.Clear();
                    LogInfo($"Started writing to: {Path.GetFileName(safePath)}");
                }
                catch (Exception ex)
                {
                    LogError("Error beginning write", ex);
                }
            }
        }

        private void WriteLine(string content)
        {
            lock (_fileLock)
            {
                if (!_inFileBlock || _currentFile == null)
                {
                    LogWarning("No file opened for writing");
                    return;
                }

                if (!CheckDiskSpace(_currentFile))
                {
                    _inFileBlock = false;
                    _fileContent.Clear();
                    _currentFile = null;
                    return;
                }

                if (_fileContent.Count >= SecurePath.MaxLines)
                {
                    LogWarning($"Too many lines, stopping at {SecurePath.MaxLines}");
                    return;
                }

                _fileContent.Add(content);
            }
        }

        private void Write(string content)
        {
            lock (_fileLock)
            {
                if (!_inFileBlock || _currentFile == null)
                {
                    LogWarning("No file opened for writing");
                    return;
                }

                if (!CheckDiskSpace(_currentFile))
                {
                    _inFileBlock = false;
                    _fileContent.Clear();
                    _currentFile = null;
                    return;
                }

                if (_fileContent.Count == 0)
                    _fileContent.Add(content);
                else
                    _fileContent[_fileContent.Count - 1] += content;
            }
        }

        private void EndWrite()
        {
            lock (_fileLock)
            {
                if (_currentFile == null || _fileContent.Count == 0)
                {
                    _inFileBlock = false;
                    _currentFile = null;
                    return;
                }

                try
                {
                    // Check total size
                    long totalSize = 0;
                    foreach (var line in _fileContent)
                        totalSize += System.Text.Encoding.UTF8.GetByteCount(line) + 2;

                    if (totalSize > SecurePath.MaxFileSize)
                    {
                        LogWarning($"File too large (max {SecurePath.MaxFileSize / 1024 / 1024} MB)");
                        return;
                    }

                    if (!CheckDiskSpace(_currentFile))
                        return;

                    File.WriteAllLines(_currentFile, _fileContent);
                    LogInfo($"Saved: {Path.GetFileName(_currentFile)}");
                }
                catch (Exception ex)
                {
                    LogError("Error saving file", ex);
                }
                finally
                {
                    _inFileBlock = false;
                    _fileContent.Clear();
                    _currentFile = null;
                }
            }
        }

        private void Close()
        {
            lock (_fileLock)
            {
                if (_inFileBlock)
                {
                    LogWarning("Force closing file without saving");
                    _inFileBlock = false;
                    _fileContent.Clear();
                    _currentFile = null;
                }
            }
        }

        private void ListFiles(string path)
        {
            try
            {
                string safePath = SecurePath.Sanitize(path);
                
                if (!Directory.Exists(safePath))
                {
                    LogWarning($"Directory not found: {path}");
                    return;
                }

                var files = Directory.GetFiles(safePath);
                var dirs = Directory.GetDirectories(safePath);

                LogInfo($"Contents of '{path}':");
                Console.WriteLine($"   Total: {dirs.Length} folders, {files.Length} files\n");

                foreach (var dir in dirs.OrderBy(d => d))
                    Console.WriteLine($"   📂 {Path.GetFileName(dir)}/");
                    
                foreach (var file in files.OrderBy(f => f))
                {
                    var info = SecurePath.GetSafeFileInfo(file);
                    Console.WriteLine($"   📄 {info.Name} ({FormatFileSize(info.Size)})");
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                LogError("Error listing files", ex);
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private bool FileExists(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;
                
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
            if (string.IsNullOrWhiteSpace(filename))
            {
                LogWarning("Filename cannot be empty");
                return;
            }
            
            try
            {
                if (!SecurePath.IsValidFileName(filename))
                    throw FileError.InvalidCharacters(filename);

                string safePath = SecurePath.Sanitize(filename);
                
                if (File.Exists(safePath))
                {
                    File.Delete(safePath);
                    LogInfo($"Deleted: {Path.GetFileName(safePath)}");
                }
            }
            catch (Exception ex)
            {
                LogError("Error deleting file", ex);
            }
        }

        private string[] ReadFile(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                LogWarning("Filename cannot be empty");
                return Array.Empty<string>();
            }
            
            try
            {
                if (!SecurePath.IsValidFileName(filename))
                    throw FileError.InvalidCharacters(filename);

                string safePath = SecurePath.Sanitize(filename);
                
                if (!File.Exists(safePath))
                {
                    LogWarning($"File not found: {filename}");
                    return Array.Empty<string>();
                }

                if (!SecurePath.IsFileSizeValid(safePath))
                {
                    LogWarning($"File too large (max {SecurePath.MaxFileSize / 1024 / 1024} MB)");
                    return Array.Empty<string>();
                }

                return SecurePath.SafeReadAllLines(safePath);
            }
            catch (Exception ex)
            {
                LogError("Error reading file", ex);
                return Array.Empty<string>();
            }
        }

        private void GetFileInfo(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                LogWarning("Filename cannot be empty");
                return;
            }
            
            try
            {
                if (!SecurePath.IsValidFileName(filename))
                    throw FileError.InvalidCharacters(filename);

                string safePath = SecurePath.Sanitize(filename);
                
                if (File.Exists(safePath))
                {
                    var info = SecurePath.GetSafeFileInfo(safePath);
                    LogInfo($"File: {info.Name}");
                    Console.WriteLine($"   Size: {FormatFileSize(info.Size)}");
                    Console.WriteLine($"   Created: {info.CreationTime}");
                    Console.WriteLine($"   Modified: {info.LastWriteTime}");
                    Console.WriteLine($"   Read-only: {info.IsReadOnly}");
                }
                else
                {
                    LogWarning($"File not found: {filename}");
                }
            }
            catch (Exception ex)
            {
                LogError("Error getting file info", ex);
            }
        }

        private void CopyFile(string source, string dest)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(dest))
            {
                LogWarning("Source and destination cannot be empty");
                return;
            }
            
            try
            {
                if (!SecurePath.IsValidFileName(source) || !SecurePath.IsValidFileName(dest))
                    throw FileError.InvalidCharacters("Invalid filename");

                string safeSource = SecurePath.Sanitize(source);
                string safeDest = SecurePath.Sanitize(dest);

                // Verify both paths are within working directory
                string baseDir = Environment.CurrentDirectory;
                if (!safeSource.StartsWith(baseDir) || !safeDest.StartsWith(baseDir))
                {
                    throw new UnauthorizedAccessException("Cannot copy files outside working directory");
                }

                if (!File.Exists(safeSource))
                {
                    LogWarning($"Source file not found: {source}");
                    return;
                }

                if (!SecurePath.IsFileSizeValid(safeSource))
                {
                    LogWarning("Source file too large to copy");
                    return;
                }

                if (!CheckDiskSpace(safeDest))
                    return;

                File.Copy(safeSource, safeDest, true);
                LogInfo($"Copied: {Path.GetFileName(safeSource)} → {Path.GetFileName(safeDest)}");
            }
            catch (Exception ex)
            {
                LogError("Error copying file", ex);
            }
        }

        private void MoveFile(string source, string dest)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(dest))
            {
                LogWarning("Source and destination cannot be empty");
                return;
            }
            
            try
            {
                if (!SecurePath.IsValidFileName(source) || !SecurePath.IsValidFileName(dest))
                    throw FileError.InvalidCharacters("Invalid filename");

                string safeSource = SecurePath.Sanitize(source);
                string safeDest = SecurePath.Sanitize(dest);

                // Verify both paths are within working directory
                string baseDir = Environment.CurrentDirectory;
                if (!safeSource.StartsWith(baseDir) || !safeDest.StartsWith(baseDir))
                {
                    throw new UnauthorizedAccessException("Cannot move files outside working directory");
                }

                if (!File.Exists(safeSource))
                {
                    LogWarning($"Source file not found: {source}");
                    return;
                }

                if (File.Exists(safeDest))
                {
                    LogWarning($"Destination already exists: {dest}");
                    return;
                }

                if (!CheckDiskSpace(safeDest))
                    return;

                File.Move(safeSource, safeDest);
                LogInfo($"Moved: {Path.GetFileName(safeSource)} → {Path.GetFileName(safeDest)}");
            }
            catch (Exception ex)
            {
                LogError("Error moving file", ex);
            }
        }

        public override void Dispose()
        {
            Close();
            base.Dispose();
        }
    }
}
