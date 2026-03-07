using System;
using System.IO;
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
            if (args.Length < 2)
            {
                ErrorHandler.Warning("Missing filename");
                HelpCommand.Show();
                return 1;
            }

            string filename = args[1];
            string output = args.Length > 2 ? args[2] : Path.ChangeExtension(filename, ".exe");
            string optLevel = args.Length > 3 ? args[3] : "o1";
            
            if (!File.Exists(filename))
            {
                ErrorHandler.Warning($"File not found: {filename}");
                return 1;
            }

            string code = File.ReadAllText(filename);
            
            ErrorHandler.Info($"Compiling {filename} to {output}\n");

            // Map optimization string to enum
            var optMap = new Dictionary<string, AstOptimizer.OptimizationLevel>(StringComparer.OrdinalIgnoreCase)
            {
                ["o0"] = AstOptimizer.OptimizationLevel.O0,
                ["o1"] = AstOptimizer.OptimizationLevel.O1,
                ["o2"] = AstOptimizer.OptimizationLevel.O2,
                ["o3"] = AstOptimizer.OptimizationLevel.O3
            };

            if (!optMap.ContainsKey(optLevel))
            {
                ErrorHandler.Warning($"Unknown optimization level: {optLevel}");
                Console.WriteLine("   Available levels: o0, o1, o2, o3");
                return 1;
            }

            var optEnum = optMap[optLevel];

            Console.WriteLine("\u001b[33m[1/3] Parsing...\u001b[0m");
            var lexer = new Lexer(code);
            var tokens = lexer.ScanTokens();
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            ErrorHandler.Success($"AST generated");

            Console.WriteLine("\u001b[33m[2/3] Optimizing...\u001b[0m");
            // Optimization happens inside ILGenerator

            Console.WriteLine("\u001b[33m[3/3] Generating IL...\u001b[0m");
            var ilGen = new ILGenerator(ast, output, optEnum);
            int result = ilGen.GenerateExecutable();

            if (result == 0)
            {
                ErrorHandler.Success($"Compilation successful!");
                Console.WriteLine($"\n🚀 Run with: {output}");
            }

            return result;
        }
    }
}
