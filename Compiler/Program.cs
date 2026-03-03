using System;
using System.IO;

namespace FSharpMinus.Compiler
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine(@"
   ______
  |  ___|
  | |_ __ _ _ __ ___   __ _ 
  |  _/ _` | '_ ` _ \ / _` |
  | || (_| | | | | | | (_| |
  \_| \__,_|_| |_| |_|\__,_|
            ");
            Console.WriteLine("F-- Compiler v1.4 - The backward step of humanity\n");

            if (args.Length == 0)
            {
                ShowHelp();
                return 1;
            }

            string command = args[0].ToLower();
            
            switch (command)
            {
                case "run":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Error: Missing file name");
                        return 1;
                    }
                    return RunFile(args[1]);
                    
                case "ast":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Error: Missing file name");
                        return 1;
                    }
                    return ShowAST(args[1]);
                    
                case "--version":
                    ShowVersion();
                    return 0;
                    
                case "--help":
                    ShowHelp();
                    return 0;
                    
                default:
                    // Default behavior: run file
                    return RunFile(args[0]);
            }
        }

        static int RunFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"fmm004: File not found - {filePath}");
                return 1;
            }

            try
            {
                string code = File.ReadAllText(filePath);
                
                // Lexer
                Console.WriteLine("[1/4] Lexing...");
                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                Console.WriteLine($"      Found {tokens.Count} tokens");
                
                // Parser
                Console.WriteLine("[2/4] Parsing...");
                var parser = new Parser(tokens);
                var ast = parser.Parse();
                Console.WriteLine($"      Found {ast.Statements.Count} statements");
                
                // Interpreter
                Console.WriteLine("[3/4] Interpreting...");
                var interpreter = new Interpreter();
                var result = interpreter.Execute(ast);
                
                // Kết quả
                Console.WriteLine("[4/4] Done!");
                Console.WriteLine($"Exit code: {result}");
                
                return result;
            }
            catch (LexerException ex)
            {
                Console.WriteLine($"fmm002: {ex.Message}");
                return 1;
            }
            catch (ParseException ex)
            {
                Console.WriteLine($"fmm001: {ex.Message}");
                return 1;
            }
            catch (RuntimeException ex)
            {
                Console.WriteLine($"fmm003: {ex.Message}");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"fmm099: Unexpected error - {ex.Message}");
                return 1;
            }
        }

        static int ShowAST(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"fmm004: File not found - {filePath}");
                return 1;
            }

            string code = File.ReadAllText(filePath);
            
            // Lexer
            var lexer = new Lexer(code);
            var tokens = lexer.ScanTokens();
            
            // Parser
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            
            // In AST
            Console.WriteLine("\n=== Abstract Syntax Tree ===");
            ast.Print(0);
            
            return 0;
        }

        static void ShowVersion()
        {
            Console.WriteLine("F-- version 1.4.0");
            Console.WriteLine("Copyright (c) 2024 - Lập trình viên 13 tuổi");
            Console.WriteLine("License: MIT");
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: fminus <command> [options]");
            Console.WriteLine("\nCommands:");
            Console.WriteLine("  run <file.f-->     Run F-- program");
            Console.WriteLine("  ast <file.f-->     Show AST tree");
            Console.WriteLine("  --version          Show version");
            Console.WriteLine("  --help             Show this help");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("  fminus hello.f--");
            Console.WriteLine("  fminus run hello.f--");
            Console.WriteLine("  fminus ast hello.f--");
        }
    }
}
