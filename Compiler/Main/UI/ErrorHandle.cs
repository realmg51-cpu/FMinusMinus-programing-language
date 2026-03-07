using System;

namespace Fminusminus.Main.UI
{
    /// <summary>
    /// Handle error display
    /// </summary>
    public static class ErrorHandler
    {
        public static void Display(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.ResetColor();
        }

        public static void DisplayMultiple(AggregateException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n❌ Multiple errors occurred:");
            foreach (var inner in ex.InnerExceptions)
            {
                Console.WriteLine($"   • {inner.Message}");
            }
            Console.ResetColor();
        }

        public static void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n⚠ Warning: {message}");
            Console.ResetColor();
        }

        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nℹ Info: {message}");
            Console.ResetColor();
        }

        public static void Success(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✅ {message}");
            Console.ResetColor();
        }
    }
}
