using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Fminusminus.CodeGen;
using Fminusminus.Main.UI;

namespace Fminusminus.Main.Commands
{
    /// <summary>
    /// Handle 'codegen' command
    /// </summary>
    public static class CodeGenCommand
    {
        public static int Execute(string[] args)
        {
            if (args.Length < 2)
            {
                ErrorHandler.Warning("Missing filename");
                ShowHelp();
                return 1;
            }

            string filename = args[1];
            string target = "cil";
            string optLevel = "o1";
            string? outputFile = null;
            
            // Parse additional options
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i] == "--target" || args[i] == "-t")
                {
                    if (i + 1 < args.Length) target = args[++i];
                }
                else if (args[i] == "--opt" || args[i] == "-O")
                {
                    if (i + 1 < args.Length) optLevel = args[++i];
                }
                else if (args[i] == "--output" || args[i] == "-o")
                {
                    if (i + 1 < args.Length) outputFile = args[++i];
                }
                else if (args[i] == "--help" || args[i] == "-h")
                {
                    ShowHelp();
                    return 0;
                }
            }
            
            // Validate file
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

            ErrorHandler.Info($"Reading {filename}...");
            string code = File.ReadAllText(filename);
            ErrorHandler.Success($"Read {code.Length} characters");

            // Lexer
            ErrorHandler.Info("[1/4] Lexing...");
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

            // Parser
            ErrorHandler.Info("[2/4] Parsing...");
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
                ErrorHandler.Warning($"Unknown target: {target}");
                Console.WriteLine("   Available targets: cil, c, js, py, f--");
                return 1;
            }

            if (!optMap.ContainsKey(optLevel))
            {
                ErrorHandler.Warning($"Unknown optimization level: {optLevel}");
                Console.WriteLine("   Available levels: o0, o1, o2, o3");
                return 1;
            }

            var targetEnum = targetMap[target];
            var optEnum = optMap[optLevel];

            // Check if target supports optimization
            if (optEnum != CodeGenerator.OptimizationLevel.O0 && targetEnum == CodeGenerator.TargetPlatform.Fminus)
            {
                ErrorHandler.Warning($"Optimization level {optLevel} may not be fully supported for F-- target");
            }

            ErrorHandler.Info($"[3/4] Optimizing (level {optLevel})...");
            ErrorHandler.Info($"[4/4] Generating {target} code...");

            // Generate code
            string generatedCode;
            try
            {
                var driver = new CodeGenDriver(ast, targetEnum, optEnum, saveToFile: true, outputFilename: outputFile);
                generatedCode = driver.Generate();
                ErrorHandler.Success($"Code generation successful!");
            }
            catch (Exception ex)
            {
                ErrorHandler.Display(ex);
                return 1;
            }

            // Show preview
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

            // Show stats
            Console.WriteLine($"\n\u001b[33m📊 Generation Statistics:\u001b[0m");
            Console.WriteLine($"   • Lines of code: {lines.Length}");
            Console.WriteLine($"   • File size: {generatedCode.Length} bytes");
            if (outputFile != null)
            {
                Console.WriteLine($"   • Output: {outputFile}");
            }

            return 0;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("codegen - Generate code for other platforms");
            Console.WriteLine("============================================");
            Console.WriteLine();
            Console.WriteLine("Usage: fminus codegen <filename> [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -t, --target <target>  Target platform (cil, c, js, py, f--)");
            Console.WriteLine("  -O, --opt <level>      Optimization level (o0, o1, o2, o3)");
            Console.WriteLine("  -o, --output <file>    Output file name");
            Console.WriteLine("  -h, --help              Show this help");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  fminus codegen hello.f-- -t c");
            Console.WriteLine("  fminus codegen hello.f-- -t js -O o2");
            Console.WriteLine("  fminus codegen hello.f-- -t py -o output.py");
            Console.WriteLine("  fminus codegen hello.f-- -t f-- -O o0");
            Console.WriteLine();
            Console.WriteLine("Note: Optimization levels may have different effects per target");
        }
    }
}
