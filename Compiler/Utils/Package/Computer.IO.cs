using System;
using System.Collections.Generic;
using System.IO;
using Fminusminus.Errors;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Computer file I/O utilities for F--
    /// Handles all file operations
    /// </summary>
    public static class ComputerIO
    {
        private static string? _currentFile;
        private static List<string> _fileContent = new();
        private static bool _inFileBlock;

        #region File Creation

        /// <summary>
        /// Create a new file
        /// </summary>
        public static void CreateFile(string filename, string? path = null)
        {
            try
            {
                // Validate filename
                if (string.IsNullOrWhiteSpace(filename))
                    throw FileError.InvalidCharacters(filename);

                if (FileError.HasInvalidCharacters(filename))
                    throw FileError.InvalidCharacters(filename);

                // Construct full path
                _currentFile = string.IsNullOrEmpty(path) 
                    ? filename 
                    : Path.Combine(path, filename);

                if (!_currentFile.EndsWith(".txt"))
                    _currentFile += ".txt";

                Console.WriteLine($"📁 Created file: {_currentFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating file: {ex.Message}");
            }
        }

        #endregion

        #region File Writing

        /// <summary>
        /// Start writing to a file
        /// </summary>
        public static void BeginWrite(string filename)
        {
            _currentFile = filename;
            _inFileBlock = true;
            _fileContent.Clear();
        }

        /// <summary>
        /// Write line to current file
        /// </summary>
        public static void WriteLine(string content)
        {
            if (!_inFileBlock || _currentFile == null)
            {
                Console.WriteLine("⚠️ No file opened for writing");
                return;
            }

            _fileContent.Add(content);
        }

        /// <summary>
        /// Write to current file (no newline)
        /// </summary>
        public static void Write(string content)
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

        /// <summary>
        /// Finish writing and save file
        /// </summary>
        public static void EndWrite()
        {
            if (_currentFile == null || _fileContent.Count == 0)
                return;

            try
            {
                File.WriteAllLines(_currentFile, _fileContent);
                Console.WriteLine($"💾 Saved: {_currentFile}");
            }
            catch (UnauthorizedAccessException)
            {
                throw FileError.AccessDenied(_currentFile);
            }
            catch (PathTooLongException)
            {
                throw FileError.PathTooLong(_currentFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving file: {ex.Message}");
            }
            finally
            {
                _inFileBlock = false;
                _fileContent.Clear();
            }
        }

        #endregion

        #region File Reading

        /// <summary>
        /// List files in directory
        /// </summary>
        public static void ListFiles(string path = ".")
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Console.WriteLine($"❌ Directory not found: {path}");
                    return;
                }

                var files = Directory.GetFiles(path);
                var dirs = Directory.GetDirectories(path);

                Console.WriteLine($"\n📁 Contents of '{path}':");
                Console.WriteLine($"   Total: {dirs.Length} folders, {files.Length} files\n");

                foreach (var dir in dirs)
                    Console.WriteLine($"   📂 {Path.GetFileName(dir)}/");
                foreach (var file in files)
                {
                    var info = new FileInfo(file);
                    Console.WriteLine($"   📄 {Path.GetFileName(file)} ({info.Length} bytes)");
                }
                Console.WriteLine();
            }
            catch (UnauthorizedAccessException)
            {
                throw FileError.AccessDenied(path);
            }
        }

        /// <summary>
        /// Read file content
        /// </summary>
        public static string[] ReadFile(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                    throw FileError.NotFound(filename);

                return File.ReadAllLines(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error reading file: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        #endregion

        #region File Management

        /// <summary>
        /// Delete a file
        /// </summary>
        public static void DeleteFile(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                    Console.WriteLine($"🗑️ Deleted: {filename}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting file: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if file exists
        /// </summary>
        public static bool FileExists(string filename)
        {
            return File.Exists(filename);
        }

        /// <summary>
        /// Get file information
        /// </summary>
        public static void FileInfo(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    Console.WriteLine($"❌ File not found: {filename}");
                    return;
                }

                var info = new FileInfo(filename);
                Console.WriteLine($"\n📄 File: {info.Name}");
                Console.WriteLine($"   Size: {info.Length} bytes");
                Console.WriteLine($"   Created: {info.CreationTime}");
                Console.WriteLine($"   Modified: {info.LastWriteTime}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        #endregion
    }
}
