using System;
using System.IO;
using System.Collections.Generic;
using Fminusminus.CodeGen;

namespace Fminusminus
{
    class Program
    {
        static int Main(string[] args)
        {
            PrintLogo();

            if (args.Length == 0)
            {
                ShowHelp();
                return 1;
            }

            string command = args[0].ToLower();
            
            try
            {
                switch (command)
                {
                    case "run":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("Error: Missing filename");
                            return 1;
                        }
                        return RunFile(args[1]);
                        
                    case "ast":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("Error: Missing filename");
                            return 1;
                        }
                        return ShowAST(args[1]);
                        
                    case "codegen":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("Error: Missing filename");
                            return 1;
                        }
                        string target = args.Length > 2 ? args[2] : "cil";
                        string optLevel = args.Length > 3 ? args[3] : "o1";
                        return GenerateCode(args[1], target, optLevel);
                        
                    case "--version":
                    case "-v":
                        ShowVersion();
                        return 0;
                        
                    case "--help":
                    case "-h":
                        ShowHelp();
                        return 0;
                        
                    default:
                        // Assume it's a filename (run mode)
                        return RunFile(args[0]);
                }
            }
            catch (AggregateException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n❌ Multiple errors occurred:");
                foreach (var inner in ex.InnerExceptions)
                {
                    Console.WriteLine($"   • {inner.Message}");
                }
                Console.ResetColor();
                return 1;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        static void PrintLogo()
        {
            Console.WriteLine(@"
    ╔══════════════════════════════════════╗
    ║  ███████╗  ███╗   ███╗  ██╗██╗      ║
    ║  ██╔════╝  ████╗ ████║  ██║██║      ║
    ║  █████╗    ██╔████╔██║  ██║██║      ║
    ║  ██╔══╝    ██║╚██╔╝██║  ██║██║      ║
    ║  ██║       ██║ ╚═╝ ██║  ██║██║      ║
    ║  ╚═╝       ╚═╝     ╚═╝  ╚═╝╚═╝      ║
    ║                                      ║
    ║     F-- PROGRAMMING LANGUAGE         ║
    ║        Version 2.0.0.0-alpha1        ║
    ║     Created by RealMG (13 tuổi)      ║
    ║    Contributors: chaunguyen12477     ║
    ╚══════════════════════════════════════╝
            ");
        }

        static int RunFile(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine($"fmm004: File not found: {filename}");
                return 1;
            }

            Console.WriteLine($"\u001b[36m▶ Running: {filename}\u001b[0m\n");
            
            string code = File.ReadAllText(filename);
            
            // Lexer
            Console.WriteLine("\u001b[33m[1/4] Lexing...\u001b[0m");
            var lexer = new Lexer(code);
            var tokens = lexer.ScanTokens();
            Console.WriteLine($"      ✓ Found {tokens.Count} tokens");
            
            // Parser
            Console.WriteLine("\u001b[33m[2/4] Parsing...\u001b[0m");
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            Console.WriteLine($"      ✓ AST generated");
            
            // Interpreter
            Console.WriteLine("\u001b[33m[3/4] Interpreting...\u001b[0m");
            var interpreter = new Interpreter();
            int result = interpreter.Execute(ast);
            
            // Result
            Console.WriteLine("\u001b[33m[4/4] Done!\u001b[0m");
            
            if (result == 0)
            {
                Console.WriteLine($"\n\u001b[32m✅ Program completed with exit code: {result}\u001b[0m");
            }
            else
            {
                Console.WriteLine($"\n\u001b[33m⚠ Program completed with exit code: {result}\u001b[0m");
            }
            
            return result;
        }

        static int ShowAST(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine($"fmm004: File not found: {filename}");
                return 1;
            }

            string code = File.ReadAllText(filename);
            
            var lexer = new Lexer(code);
            var tokens = lexer.ScanTokens();
            
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            
            Console.WriteLine("\n\u001b[36m=== Abstract Syntax Tree ===\u001b[0m\n");
            ast.Print();
            
            return 0;
        }

        static int GenerateCode(string filename, string target, string optLevel)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine($"fmm004: File not found: {filename}");
                return 1;
            }

            string code = File.ReadAllText(filename);
            
            Console.WriteLine($"\u001b[36m▶ Generating code for {filename}\u001b[0m\n");
            
            // Lexer
            Console.WriteLine("\u001b[33m[1/4] Lexing...\u001b[0m");
            var lexer = new Lexer(code);
            var tokens = lexer.ScanTokens();
            Console.WriteLine($"      ✓ Found {tokens.Count} tokens");
            
            // Parser
            Console.WriteLine("\u001b[33m[2/4] Parsing...\u001b[0m");
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            Console.WriteLine($"      ✓ AST generated");
            
            // Map target string to enum
            var targetMap = new Dictionary<string, CodeGenerator.TargetPlatform>(StringComparer.OrdinalIgnoreCase)
            {
                ["cil"] = CodeGenerator.TargetPlatform.CIL,
                ["il"] = CodeGenerator.TargetPlatform.CIL,
                ["c"] = CodeGenerator.TargetPlatform.C,
                ["js"] = CodeGenerator.TargetPlatform.JavaScript,
                ["javascript"] = CodeGenerator.TargetPlatform.JavaScript,
                ["py"] = CodeGenerator.TargetPlatform.Python,
                ["python"] = CodeGenerator.TargetPlatform.Python,
                ["f--"] = CodeGenerator.TargetPlatform.Fminus,
                ["fminus"] = CodeGenerator.TargetPlatform.Fminus
            };

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

            if (!targetMap.ContainsKey(target))
            {
                Console.WriteLine($"\u001b[31m❌ Unknown target: {target}\u001b[0m");
                Console.WriteLine("   Available targets: cil, c, js, py, f--");
                return 1;
            }

            if (!optMap.ContainsKey(optLevel))
            {
                Console.WriteLine($"\u001b[31m❌ Unknown optimization level: {optLevel}\u001b[0m");
                Console.WriteLine("   Available levels: o0, o1, o2, o3");
                return 1;
            }

            var targetEnum = targetMap[target];
            var optEnum = optMap[optLevel];

            Console.WriteLine($"\u001b[33m[3/4] Optimizing (level {optLevel})...\u001b[0m");
            Console.WriteLine($"\u001b[33m[4/4] Generating {target} code...\u001b[0m");

            var driver = new CodeGenDriver(ast, targetEnum, optEnum, saveToFile: true);
            var generatedCode = driver.Generate();
            
            Console.WriteLine($"\n\u001b[32m✅ Code generation successful!\u001b[0m");
            
            // Show preview of generated code
            Console.WriteLine("\n\u001b[36m=== Generated Code Preview ===\u001b[0m");
            var lines = generatedCode.Split('\n');
            int previewLines = Math.Min(10, lines.Length);
            for (int i = 0; i < previewLines; i++)
            {
                Console.WriteLine($"  {lines[i]}");
            }
            if (lines.Length > 10)
            {
                Console.WriteLine($"  ... ({lines.Length - 10} more lines)");
            }

            return 0;
        }

        static void ShowVersion()
        {
            Console.WriteLine("F-- Programming Language v2.0.0.0-alpha1");
            Console.WriteLine("Copyright (c) 2026 RealMG");
            Console.WriteLine("License: MIT");
            Console.WriteLine("\n\"The backward step of humanity, but forward step in creativity!\"");
            Console.WriteLine("\nContributors:");
            Console.WriteLine("  • realmg51-cpu (Creator, 13 years old)");
            Console.WriteLine("  • chaunguyen12477-cmyk (Contributor)");
        }

        static void ShowHelp()
        {
            Console.WriteLine("F-- Programming Language - Usage Guide");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("📌 COMMANDS:");
            Console.WriteLine();
            Console.WriteLine("  run <file>           Run an F-- program");
            Console.WriteLine("  ast <file>           Display Abstract Syntax Tree");
            Console.WriteLine("  codegen <file> [target] [opt]  Generate code for other platforms");
            Console.WriteLine("  --version, -v        Show version information");
            Console.WriteLine("  --help, -h           Show this help message");
            Console.WriteLine();
            Console.WriteLine("🎯 CODEGEN TARGETS:");
            Console.WriteLine("  cil        .NET Common Intermediate Language (default)");
            Console.WriteLine("  c          C programming language");
            Console.WriteLine("  js         JavaScript");
            Console.WriteLine("  py         Python");
            Console.WriteLine("  f--        F-- itself (self-hosting)");
            Console.WriteLine();
            Console.WriteLine("⚡ OPTIMIZATION LEVELS:");
            Console.WriteLine("  o0         No optimization");
            Console.WriteLine("  o1         Basic optimizations (default)");
            Console.WriteLine("  o2         Aggressive optimizations");
            Console.WriteLine("  o3         Maximum optimizations");
            Console.WriteLine();
            Console.WriteLine("📋 EXAMPLES:");
            Console.WriteLine("  fminus run examples/hello.f--");
            Console.WriteLine("  fminus ast examples/hello.f--");
            Console.WriteLine("  fminus codegen examples/hello.f-- c");
            Console.WriteLine("  fminus codegen examples/hello.f-- js o2");
            Console.WriteLine("  fminus codegen examples/hello.f-- py");
            Console.WriteLine();
            Console.WriteLine("📁 PROJECT STRUCTURE:");
            Console.WriteLine("  Compiler/     Source code");
            Console.WriteLine("  examples/     Example programs");
            Console.WriteLine("  docs/         Documentation");
            Console.WriteLine();
            Console.WriteLine("🌐 RESOURCES:");
            Console.WriteLine("  GitHub: https://github.com/realmg51-cpu/F--Programming-Language");
            Console.WriteLine("  NuGet: https://www.nuget.org/packages/Fminusminus");
            Console.WriteLine("  Discord: https://discord.gg/fminus (coming soon)");
        }

        static void RunInteractive()
        {
            Console.WriteLine("\n🔮 F-- Interactive Mode (REPL) - Coming Soon!");
            Console.WriteLine("   For now, use 'fminus run <file>' to execute programs.\n");
        }
    }
}
