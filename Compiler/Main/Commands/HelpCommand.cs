using System;

namespace Fminusminus.Main.Commands
{
    /// <summary>
    /// Handle help display
    /// </summary>
    public static class HelpCommand
    {
        public static int Execute(string[]? args = null)
        {
            Show();
            return 0;
        }

        public static void Show()
        {
            bool noColor = Environment.GetEnvironmentVariable("FMINUS_NO_COLOR") == "1";
            
            if (!noColor)
                Console.ForegroundColor = ConsoleColor.Cyan;
                
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     F-- Programming Language - Complete Usage Guide     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            
            if (!noColor)
                Console.ResetColor();
            
            Console.WriteLine();

            // Basic commands
            if (!noColor) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📌 BASIC COMMANDS:");
            if (!noColor) Console.ResetColor();
            Console.WriteLine("  run <file>           Run an F-- program");
            Console.WriteLine("  ast <file>           Display Abstract Syntax Tree");
            Console.WriteLine("  compile <file> [options]  Compile to .NET executable");
            Console.WriteLine("  codegen <file> [target] [opt]  Generate code for other platforms");
            Console.WriteLine("  version, -v          Show version information");
            Console.WriteLine("  help, -h             Show this help message");
            Console.WriteLine();

            // Compile options
            if (!noColor) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🛠️  COMPILE OPTIONS:");
            if (!noColor) Console.ResetColor();
            Console.WriteLine("  -o, --output <file>     Output executable path");
            Console.WriteLine("  -opt, --optimization <level>  Optimization level");
            Console.WriteLine("      Examples:");
            Console.WriteLine("        fminus compile hello.f-- -o myapp.exe");
            Console.WriteLine("        fminus compile hello.f-- -opt o2");
            Console.WriteLine();

            // Codegen targets
            if (!noColor) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🎯 CODEGEN TARGETS:");
            if (!noColor) Console.ResetColor();
            Console.WriteLine("  cil        .NET Common Intermediate Language (default)");
            Console.WriteLine("  c          C programming language");
            Console.WriteLine("  js         JavaScript");
            Console.WriteLine("  py         Python");
            Console.WriteLine("  f--        F-- itself (self-hosting)");
            Console.WriteLine();

            // Optimization levels
            if (!noColor) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚡ OPTIMIZATION LEVELS:");
            if (!noColor) Console.ResetColor();
            Console.WriteLine("  o0         No optimization (fastest compile)");
            Console.WriteLine("  o1         Basic optimizations (default)");
            Console.WriteLine("  o2         Aggressive optimizations");
            Console.WriteLine("  o3         Maximum optimizations (slowest compile)");
            Console.WriteLine();

            // Package management
            if (!noColor) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📦 PACKAGE MANAGEMENT:");
            if (!noColor) Console.ResetColor();
            Console.WriteLine("  fminus package list           List available packages");
            Console.WriteLine("  fminus package info <name>    Show package info");
            Console.WriteLine("  (More package commands coming soon)");
            Console.WriteLine();

            // Examples
            if (!noColor) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📋 EXAMPLES:");
            if (!noColor) Console.ResetColor();
            Console.WriteLine("  fminus run examples/hello.f--");
            Console.WriteLine("  fminus ast examples/hello.f--");
            Console.WriteLine("  fminus compile hello.f-- -o bin/program.exe");
            Console.WriteLine("  fminus codegen examples/hello.f-- c");
            Console.WriteLine("  fminus codegen examples/hello.f-- js o2");
            Console.WriteLine("  fminus codegen examples/hello.f-- py");
            Console.WriteLine();

            // Platform support
            if (!noColor) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("💻 PLATFORM SUPPORT:");
            if (!noColor) Console.ResetColor();
            Console.WriteLine("  Windows     ✓ Full support");
            Console.WriteLine("  Linux       ✓ Full support");
            Console.WriteLine("  macOS       ✓ Full support");
            Console.WriteLine();

            // Exit codes
            if (!noColor) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📊 EXIT CODES:");
            if (!noColor) Console.ResetColor();
            Console.WriteLine("  0         Success");
            Console.WriteLine("  1         General error");
            Console.WriteLine("  2         Syntax error");
            Console.WriteLine("  3         File not found");
            Console.WriteLine("  4         Permission denied");
            Console.WriteLine("  130       Interrupted by user (Ctrl+C)");
            Console.WriteLine();

            // Environment variables
            if (!noColor) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🔧 ENVIRONMENT VARIABLES:");
            if (!noColor) Console.ResetColor();
            Console.WriteLine("  FMINUS_HOME         F-- installation directory");
            Console.WriteLine("  FMINUS_DEBUG        Enable debug output (set to 1)");
            Console.WriteLine("  FMINUS_NO_COLOR     Disable colored output (set to 1)");
            Console.WriteLine("  FMINUS_LOG_LEVEL    Log level (error, warn, info, debug)");
            Console.WriteLine();

            // Troubleshooting
            if (!noColor) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🔍 TROUBLESHOOTING:");
            if (!noColor) Console.ResetColor();
            Console.WriteLine("  • Check fminus-error.log for detailed errors");
            Console.WriteLine("  • Use --verbose flag for more output");
            Console.WriteLine("  • Set FMINUS_DEBUG=1 for debug mode");
            Console.WriteLine("  • Report issues at:");
            Console.WriteLine("    https://github.com/realmg51-cpu/F--Programming-Language/issues");
            Console.WriteLine();

            // Project structure
            if (!noColor) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📁 PROJECT STRUCTURE:");
            if (!noColor) Console.ResetColor();
            Console.WriteLine("  Compiler/     Source code");
            Console.WriteLine("  examples/     Example programs");
            Console.WriteLine("  docs/         Documentation");
            Console.WriteLine("  tests/        Unit tests");
            Console.WriteLine();

            // Resources
            if (!noColor) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🌐 RESOURCES:");
            if (!noColor) Console.ResetColor();
            Console.WriteLine("  GitHub: https://github.com/realmg51-cpu/F--Programming-Language");
            Console.WriteLine("  NuGet: https://www.nuget.org/packages/Fminusminus");
            Console.WriteLine("  Discord: https://discord.gg/fminus (coming soon)");
            Console.WriteLine();

            // Version info
            if (!noColor) Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"F-- v{GetVersion()} - Created by RealMG (13 tuổi)");
            if (!noColor) Console.ResetColor();
        }

        private static string GetVersion()
        {
            try
            {
                var assembly = typeof(HelpCommand).Assembly;
                var version = assembly.GetName().Version;
                return version?.ToString() ?? "2.0.0-alpha1";
            }
            catch
            {
                return "2.0.0-alpha1";
            }
        }
    }
}
