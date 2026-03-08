using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Fminusminus.Main.Commands;
using Fminusminus.Main.UI;

namespace Fminusminus.Main
{
    public static class Program
    {
        private static readonly string Version = GetVersion();
        private static readonly bool DebugMode = Environment.GetEnvironmentVariable("FMINUS_DEBUG") == "1";
        private static readonly bool NoColor = Environment.GetEnvironmentVariable("FMINUS_NO_COLOR") == "1";

        static Program()
        {
            // Handle Ctrl+C
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("\n\n⚠ Interrupted by user");
                e.Cancel = true;
                Environment.Exit(130);
            };

            // Disable colors if requested
            if (NoColor)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            // Check for updates in background
            if (!DebugMode)
            {
                _ = Task.Run(CheckForUpdates);
            }
        }

        public static int Main(string[] args)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Show logo (adaptive)
                if (Console.WindowWidth >= 50)
                    Logo.Display(showTip: true);
                else
                    Logo.DisplaySmall();

                if (DebugMode)
                {
                    Console.WriteLine($"\n🔧 Debug mode enabled");
                    Console.WriteLine($"📁 Working dir: {Environment.CurrentDirectory}");
                    Console.WriteLine($"📦 Arguments: {string.Join(", ", args)}");
                    Console.WriteLine($"🔢 Count: {args.Length}");
                }

                if (args.Length == 0)
                {
                    HelpCommand.Show();
                    return 0;
                }

                string command = args[0].ToLower();
                
                int result = (command, args) switch
                {
                    ("run", _) => RunCommand.Execute(args),
                    ("ast", _) => AstCommand.Execute(args),
                    ("codegen", _) => CodeGenCommand.Execute(args),
                    ("compile", _) => CompileCommand.Execute(args),
                    ("build", _) => CompileCommand.Execute(args), // Alias for compile
                    ("version", _) or ("--version", _) or ("-v", _) => VersionCommand.Execute(),
                    ("help", _) or ("--help", _) or ("-h", _) => HelpCommand.Execute(),
                    _ when File.Exists(command) && command.EndsWith(".f--", StringComparison.OrdinalIgnoreCase) 
                        => RunCommand.Execute(new[] { "run", command }),
                    _ => UnknownCommand(command)
                };

                stopwatch.Stop();

                if (DebugMode)
                {
                    Console.WriteLine($"\n⏱️  Execution time: {stopwatch.ElapsedMilliseconds} ms");
                    Console.WriteLine($"📊 Exit code: {result}");
                }

                return result;
            }
            catch (AggregateException ex)
            {
                ErrorHandler.DisplayMultiple(ex);
                return 1;
            }
            catch (Exception ex)
            {
                ErrorHandler.Display(ex);
                return 1;
            }
        }

        private static int UnknownCommand(string command)
        {
            ErrorHandler.Warning($"Unknown command: '{command}'");
            Console.WriteLine();
            Console.WriteLine("Did you mean:");
            Console.WriteLine("  • run     - Run an F-- program");
            Console.WriteLine("  • ast     - Show syntax tree");
            Console.WriteLine("  • compile - Compile to executable");
            Console.WriteLine("  • codegen - Generate code for other languages");
            Console.WriteLine("  • help    - Show help");
            Console.WriteLine();
            Console.WriteLine($"Or try: fminus run {command} (if it's a filename)");
            return 1;
        }

        private static async Task CheckForUpdates()
        {
            try
            {
                string lastCheckFile = Path.Combine(Path.GetTempPath(), "fminus_last_check.txt");
                
                // Check once per day
                if (File.Exists(lastCheckFile))
                {
                    var lastCheck = File.GetLastWriteTime(lastCheckFile);
                    if ((DateTime.Now - lastCheck).TotalDays < 1)
                        return;
                }

                File.WriteAllText(lastCheckFile, DateTime.Now.ToString());

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Fminusminus-CLI");

                // Simple version check - in production, parse JSON properly
                var response = await client.GetStringAsync("https://api.nuget.org/v3-flatcontainer/fminusminus/index.json");
                
                // This is simplified - you'd need proper JSON parsing
                if (response.Contains("2.1.0"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n✨ New version available: v2.1.0");
                    Console.WriteLine("   Run: dotnet tool update -g fminusminus");
                    Console.ResetColor();
                }
            }
            catch
            {
                // Silent fail - don't bother user with network errors
            }
        }

        private static string GetVersion()
        {
            try
            {
                var assembly = typeof(Program).Assembly;
                var version = assembly.GetName().Version;
                return version?.ToString() ?? "2.0.0";
            }
            catch
            {
                return "2.0.0";
            }
        }
    }
}
