using System;
using System.IO;
using System.Linq;

namespace Fminusminus.Utils.Security
{
    /// <summary>
    /// Secure path handling to prevent path traversal attacks
    /// </summary>
    public static class SecurePath
    {
        /// <summary>
        /// Maximum file size to read (10 MB)
        /// </summary>
        public const long MaxFileSize = 10 * 1024 * 1024;
        
        /// <summary>
        /// Maximum lines to read from a file
        /// </summary>
        public const int MaxLines = 10000;

        /// <summary>
        /// Sanitize and validate a file path
        /// </summary>
        public static string Sanitize(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be empty");

            // Block path traversal attempts
            if (path.Contains(".."))
                throw new UnauthorizedAccessException("Path traversal attacks are not allowed");

            // Block absolute paths
            if (Path.IsPathRooted(path))
                throw new UnauthorizedAccessException("Absolute paths are not allowed");

            // Get full path and ensure it's within current directory
            var baseDir = Environment.CurrentDirectory;
            var fullPath = Path.GetFullPath(Path.Combine(baseDir, path));

            if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Cannot access files outside working directory");

            return fullPath;
        }

        /// <summary>
        /// Validate filename for invalid characters
        /// </summary>
        public static bool IsValidFileName(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            return !filename.Any(c => invalidChars.Contains(c));
        }

        /// <summary>
        /// Check if file size is within limits
        /// </summary>
        public static bool IsFileSizeValid(string filePath)
        {
            try
            {
                var info = new FileInfo(filePath);
                return info.Length <= MaxFileSize;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Safe file reading with size limits
        /// </summary>
        public static string[] SafeReadAllLines(string filePath, int maxLines = MaxLines)
        {
            var lines = new System.Collections.Generic.List<string>();
            
            using var reader = new StreamReader(filePath);
            for (int i = 0; i < maxLines; i++)
            {
                if (reader.EndOfStream) break;
                lines.Add(reader.ReadLine() ?? "");
            }

            return lines.ToArray();
        }

        /// <summary>
        /// Safe file reading as text with size limits
        /// </summary>
        public static string SafeReadAllText(string filePath)
        {
            using var reader = new StreamReader(filePath);
            var buffer = new char[MaxFileSize];
            int bytesRead = reader.Read(buffer, 0, (int)Math.Min(MaxFileSize, int.MaxValue));
            
            return new string(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Get safe file info without exposing full path
        /// </summary>
        public static FileInfoSafe GetSafeFileInfo(string filePath)
        {
            var info = new FileInfo(filePath);
            return new FileInfoSafe
            {
                Name = info.Name,
                Size = info.Length,
                CreationTime = info.CreationTime,
                LastWriteTime = info.LastWriteTime,
                IsReadOnly = info.IsReadOnly
            };
        }

        /// <summary>
        /// Safe file information (no full path exposure)
        /// </summary>
        public class FileInfoSafe
        {
            public string Name { get; set; } = string.Empty;
            public long Size { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime LastWriteTime { get; set; }
            public bool IsReadOnly { get; set; }
        }
    }
}
