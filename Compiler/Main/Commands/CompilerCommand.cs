using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Fminusminus.CodeGen;
using Fminusminus.Main.UI;
using Fminusminus.Optimizer;

namespace Fminusminus.Main.Commands
{
    /// <summary>
    /// Handle 'compile' command - generates native .NET executable
    /// </summary>
    public static class CompileCommand
    {
        public static int Execute(string[] args)
        {
            string filename = "";
            string output = "";
            string optLevel = "o1";
            bool helpRequested = false;

            // Parse command line options
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-o":
                    case "--output":
                        if (i + 1 < args.Length) 
                            output = args[++i];
                        else
                            ErrorHandler.Warning("Missing output filename");
                        break;
                        
                    case "-opt":
                    case "--optimization":
                        if (i + 1 < args.Length) 
                            optLevel = args[++i];
                        else
                            ErrorHandler.Warning("Missing optimization level");
                        break;
                        
                    case "-h":
                    case "--help":
                        helpRequested = true;
                        break;
                        
                    default:
                        if (filename == "" && !args[i].StartsWith("-"))
                            filename = args[i];
                        else if (output == "" && !args[i].StartsWith("-"))
                            output = args[i];
                        else
                            ErrorHandler.Warning($"Unknown option: {args[i]}");
                        break;
                }
            }

            if (helpRequested || filename == "")
            {
                ShowHelp();
                return helpRequested ? 0 : 1;
            }

            if (output == "")
                output = Path.ChangeExtension(filename, ".exe");

            // Validate input file
            if (!File.Exists(filename))
            {
                ErrorHandler.Warning($"File not found: {filename}");
                return 1;
            }

            if (!filename.EndsWith(".f--", StringComparison.OrdinalIgnoreCase))
            {
                ErrorHandler.Warning($"File must have .f-- extension: {filename}");
                return 1;
            }

            // Check file size
            var fileInfo = new FileInfo(filename);
            if (fileInfo.Length > 10 * 1024 * 1024) // 10MB limit
            {
                ErrorHandler.Warning($"File too large ({fileInfo.Length / 1024 / 1024} MB). Maximum allowed: 10 MB");
                return 1;
            }

            // Validate output path
            try
            {
                string outputDir = Path.GetDirectoryName(output);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Test write permission
                using (File.Create(output, 1, FileOptions.DeleteOnClose)) { }
            }
            catch (Exception ex)
            {
                ErrorHandler.Warning($"Cannot write to output path: {ex.Message}");
                return 1;
            }

            ErrorHandler.Info($"Compiling {filename} to {output}\n");

            // Map optimization string to enum
            var optMap = new Dictionary<string, CodeGenerator.OptimizationLevel>(StringComparer.OrdinalIgnoreCase)
            {
                ["o0"] = CodeGenerator.OptimizationLevel.O0,
                ["o1"] = CodeGenerator.OptimizationLevel.O1,
                ["o2"] = CodeGenerator.OptimizationLevel.O2,
                ["o3"] = CodeGenerator.OptimizationLevel.O3,
                ["none"] = CodeGenerator.OptimizationLevel.O0,
                ["basic"] = CodeGenerator.OptimizationLevel.O1,
                ["aggressive"] = CodeGenerator.OptimizationLevel.O2,
                ["max"] = CodeGenerator.OptimizationLevel.O3
            };

            if (!optMap.ContainsKey(optLevel))
            {
                ErrorHandler.Warning($"Unknown optimization level: {optLevel}");
                Console.WriteLine("   Available levels: o0, o1, o2, o3");
                return 1;
            }

            var optEnum = optMap[optLevel];

            // Read source code
            string code = File.ReadAllText(filename);

            // Lexing
            Console.WriteLine("\u001b[33m[1/4] Lexing...\u001b[0m");
            List<Token> tokens;
            try
            {
                var lexer = new Lexer(code);
                tokens = lexer.ScanTokens();
                ErrorHandler.Success($"Found {tokens.Count} tokens");
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

            // Parsing
            Console.WriteLine("\u001b[33m[2/4] Parsing...\u001b[0m");
            ProgramNode ast;
            try
            {
                var parser = new Parser(tokens);
                ast = parser.Parse();
                ErrorHandler.Success($"AST generated");
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

            // Optimization
            Console.WriteLine($"\u001b[33m[3/4] Optimizing (level {optLevel})...\u001b[0m");
            if (optEnum != CodeGenerator.OptimizationLevel.O0)
            {
                try
                {
                    var optimizer = new AstOptimizer((AstOptimizer.OptimizationLevel)optEnum);
                    ast = optimizer.Optimize(ast);
                    Console.WriteLine(optimizer.GetStats());
                }
                catch (Exception ex)
                {
                    ErrorHandler.Warning($"Optimization failed: {ex.Message}. Continuing without optimization.");
                }
            }

            // IL Generation
            Console.WriteLine("\u001b[33m[4/4] Generating IL...\u001b[0m");
            int result;
            try
            {
                var ilGen = new ILGenerator(ast, output, optEnum);
                result = ilGen.GenerateExecutable();
            }
            catch (Exception ex)
            {
                ErrorHandler.Display(ex);
                return 1;
            }

            // Show results
            if (result == 0)
            {
                var outputInfo = new FileInfo(output);
                ErrorHandler.Success($"Compilation successful!");
                Console.WriteLine($"\n📊 Output Statistics:");
                Console.WriteLine($"   • Executable: {output}");
                Console.WriteLine($"   • Size: {outputInfo.Length / 1024} KB");
                Console.WriteLine($"   • Created: {outputInfo.CreationTime}");
                Console.WriteLine($"   • Optimization: {optLevel}");
                Console.WriteLine($"\n🚀 Run with: {output}");
            }

            return result;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("compile - Compile F-- program to .NET executable");
            Console.WriteLine("=================================================");
            Console.WriteLine();
            Console.WriteLine("Usage: fminus compile <filename> [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -o, --output <file>     Output executable path");
            Console.WriteLine("  -opt, --optimization <level>  Optimization level (o0, o1, o2, o3)");
            Console.WriteLine("  -h, --help              Show this help");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  fminus compile hello.f--");
            Console.WriteLine("  fminus compile hello.f-- -o myprogram.exe");
            Console.WriteLine("  fminus compile hello.f-- -opt o2 -o release/program.exe");
            Console.WriteLine();
            Console.WriteLine("Optimization Levels:");
            Console.WriteLine("  o0 - No optimization (fastest compile)");
            Console.WriteLine("  o1 - Basic optimizations (default)");
            Console.WriteLine("  o2 - Aggressive optimizations");
            Console.WriteLine("  o3 - Maximum optimizations (slowest compile)");
        }
    }
}
