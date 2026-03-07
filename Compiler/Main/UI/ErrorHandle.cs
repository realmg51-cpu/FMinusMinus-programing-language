using System;
using System.IO;
using System.Linq;
using Fminusminus.Errors;

namespace Fminusminus.Main.UI
{
    /// <summary>
    /// Handle error display
    /// </summary>
    public static class ErrorHandler
    {
        private static readonly string _logFile = "fminus_error.log";
        
        static ErrorHandler()
        {
            try
            {
                // Create log file if it doesn't exist
                if (!File.Exists(_logFile))
                    File.WriteAllText(_logFile, $"--- F-- Error Log Started at {DateTime.Now} ---\n");
            }
            catch
            {
                // Ignore logging errors
            }
        }
        
        private static void LogToFile(string level, string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level}: {message}\n";
                File.AppendAllText(_logFile, logEntry);
            }
            catch
            {
                // Fail silently - can't log if file system fails
            }
        }
        
        public static void Display(Exception ex)
        {
            if (ex == null)
            {
                DisplayUnknown();
                return;
            }
            
            // Log to file
            LogToFile("ERROR", ex.ToString());
            
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                
                switch (ex)
                {
                    case SyntaxError syntaxError:
                        Console.WriteLine($"\n❌ Syntax Error at line {syntaxError.Line}, column {syntaxError.Column}:");
                        Console.WriteLine($"   {syntaxError.Message}");
                        if (!string.IsNullOrEmpty(syntaxError.Token))
                            Console.WriteLine($"   Token: '{syntaxError.Token}'");
                        break;
                        
                    case AggregateException aggEx:
                        DisplayMultiple(aggEx);
                        return; // DisplayMultiple đã xử lý màu
                        
                    case FileNotFoundException fileError:
                        Console.WriteLine($"\n❌ File Error: {ex.Message}");
                        Console.WriteLine($"   File: {fileError.FileName}");
                        break;
                        
                    case UnauthorizedAccessException:
                        Console.WriteLine($"\n❌ Access Error: {ex.Message}");
                        Console.WriteLine("   You don't have permission to access this resource.");
                        break;
                        
                    case ArgumentException argEx:
                        Console.WriteLine($"\n❌ Argument Error: {ex.Message}");
                        Console.WriteLine($"   Parameter: {argEx.ParamName}");
                        break;
                        
                    default:
                        Console.WriteLine($"\n❌ Error: {ex.Message}");
                        if (ex.InnerException != null)
                            Console.WriteLine($"   → {ex.InnerException.Message}");
                        break;
                }
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
        
        private static void DisplayUnknown()
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n❌ Unknown error occurred");
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
        
        public static void DisplayMultiple(AggregateException ex)
        {
            if (ex == null) return;
            
            LogToFile("ERROR", $"Multiple errors: {ex.InnerExceptions.Count}");
            
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Multiple errors occurred ({ex.InnerExceptions.Count}):");
                
                void PrintException(Exception e, int depth = 0)
                {
                    string prefix = new string(' ', depth * 2) + "• ";
                    
                    if (e is SyntaxError syntaxError)
                    {
                        Console.WriteLine($"{prefix}[Line {syntaxError.Line}] {syntaxError.Message}");
                    }
                    else
                    {
                        Console.WriteLine($"{prefix}{e.Message}");
                    }
                    
                    if (e.InnerException != null)
                        PrintException(e.InnerException, depth + 1);
                }
                
                for (int i = 0; i < ex.InnerExceptions.Count; i++)
                {
                    Console.WriteLine($"\n   Error #{i + 1}:");
                    PrintException(ex.InnerExceptions[i], 1);
                }
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
        
        public static void DisplayCompilerError(string message, int line, int column, string code = "")
        {
            LogToFile("COMPILER ERROR", $"Line {line}:{column} - {message}");
            
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Compiler Error at line {line}, column {column}:");
                Console.WriteLine($"   {message}");
                
                // Hiển thị dòng code bị lỗi
                if (!string.IsNullOrEmpty(code))
                {
                    string[] lines = code.Split('\n');
                    if (line - 1 < lines.Length)
                    {
                        Console.WriteLine($"\n   {lines[line - 1]}");
                        Console.WriteLine($"   {new string(' ', Math.Max(0, column - 1))}^");
                    }
                }
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
        
        public static void Warning(string message)
        {
            LogToFile("WARNING", message);
            
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n⚠ Warning: {message}");
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
        
        public static void Info(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] ℹ Info: {message}");
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
        
        public static void Success(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✅ {message}");
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
        
        public static void Debug(string message)
        {
#if DEBUG
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\n🔍 DEBUG: {message}");
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
#endif
        }
    }
}
