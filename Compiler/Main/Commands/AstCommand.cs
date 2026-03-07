using System;
using System.IO;
using System.Linq;
using Fminusminus.Main.UI;

namespace Fminusminus.Main.Commands
{
    /// <summary>
    /// Handle 'ast' command
    /// </summary>
    public static class AstCommand
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
            string? outputFile = null;
            bool jsonOutput = false;
            
            // Parse additional options
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i] == "--output" || args[i] == "-o")
                {
                    if (i + 1 < args.Length)
                    {
                        outputFile = args[i + 1];
                        i++;
                    }
                }
                else if (args[i] == "--json" || args[i] == "-j")
                {
                    jsonOutput = true;
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
            ErrorHandler.Info("Lexing...");
            List<Token> tokens;
            try
            {
                var lexer = new Lexer(code);
                tokens = lexer.ScanTokens();
                ErrorHandler.Success($"Generated {tokens.Count} tokens");
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
            ErrorHandler.Info("Parsing...");
            ProgramNode ast;
            try
            {
                var parser = new Parser(tokens);
                ast = parser.Parse();
                ErrorHandler.Success("AST generated successfully");
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

            // Output
            if (outputFile != null)
            {
                try
                {
                    using var writer = new StreamWriter(outputFile);
                    var originalOut = Console.Out;
                    Console.SetOut(writer);
                    
                    if (jsonOutput)
                    {
                        // Giả sử có method ToJson()
                        // writer.WriteLine(ast.ToJson());
                        writer.WriteLine("{\"error\": \"JSON output not implemented yet\"}");
                    }
                    else
                    {
                        Console.WriteLine("\n=== Abstract Syntax Tree ===\n");
                        ast.Print();
                    }
                    
                    Console.SetOut(originalOut);
                    ErrorHandler.Success($"AST saved to {outputFile}");
                }
                catch (Exception ex)
                {
                    ErrorHandler.Display(ex);
                    return 1;
                }
            }
            else
            {
                if (jsonOutput)
                {
                    // Console.WriteLine(ast.ToJson());
                    Console.WriteLine("{\"error\": \"JSON output not implemented yet\"}");
                }
                else
                {
                    Console.WriteLine("\n\u001b[36m=== Abstract Syntax Tree ===\u001b[0m\n");
                    ast.Print();
                    
                    // Show stats
                    Console.WriteLine($"\n\u001b[33m📊 AST Statistics:\u001b[0m");
                    Console.WriteLine($"   • Imported packages: {ast.ImportedPackages.Count}");
                    Console.WriteLine($"   • Start block statements: {ast.StartBlock?.Statements.Count ?? 0}");
                    Console.WriteLine($"   • Has return: {ast.StartBlock?.HasReturn}");
                }
            }
            
            return 0;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("ast - Display Abstract Syntax Tree");
            Console.WriteLine("====================================");
            Console.WriteLine();
            Console.WriteLine("Usage: fminus ast <filename> [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -o, --output <file>  Save AST to file");
            Console.WriteLine("  -j, --json           Output as JSON");
            Console.WriteLine("  -h, --help           Show this help");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  fminus ast hello.f--");
            Console.WriteLine("  fminus ast hello.f-- -o ast.txt");
            Console.WriteLine("  fminus ast hello.f-- --json");
        }
    }
}
