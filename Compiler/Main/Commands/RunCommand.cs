using System;
using System.IO;
using Fminusminus.Main.UI;

namespace Fminusminus.Main.Commands
{
    /// <summary>
    /// Handle 'run' command
    /// </summary>
    public static class RunCommand
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
            
            if (!File.Exists(filename))
            {
                ErrorHandler.Warning($"File not found: {filename}");
                return 1;
            }

            ErrorHandler.Info($"Running: {filename}\n");
            
            string code = File.ReadAllText(filename);
            
            // Lexer
            Console.WriteLine("\u001b[33m[1/4] Lexing...\u001b[0m");
            var lexer = new Lexer(code);
            var tokens = lexer.ScanTokens();
            ErrorHandler.Success($"Found {tokens.Count} tokens");
            
            // Parser
            Console.WriteLine("\u001b[33m[2/4] Parsing...\u001b[0m");
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            ErrorHandler.Success($"AST generated");
            
            // Interpreter
            Console.WriteLine("\u001b[33m[3/4] Interpreting...\u001b[0m");
            var interpreter = new Interpreter();
            int result = interpreter.Execute(ast);
            
            // Result
            Console.WriteLine("\u001b[33m[4/4] Done!\u001b[0m");
            
            if (result == 0)
            {
                ErrorHandler.Success($"Program completed with exit code: {result}");
            }
            else
            {
                ErrorHandler.Warning($"Program completed with exit code: {result}");
            }
            
            return result;
        }
    }
}
