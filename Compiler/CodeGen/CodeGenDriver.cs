using System;
using System.IO;
using Fminusminus.Optimizer;

namespace Fminusminus.CodeGen
{
    /// <summary>
    /// Driver for code generation - handles multiple output formats and file writing
    /// </summary>
    public class CodeGenDriver
    {
        private readonly ProgramNode _ast;
        private readonly CodeGenerator.TargetPlatform _target;
        private readonly CodeGenerator.OptimizationLevel _optLevel;
        private readonly bool _saveToFile;

        public CodeGenDriver(ProgramNode ast, 
                            CodeGenerator.TargetPlatform target = CodeGenerator.TargetPlatform.CIL,
                            CodeGenerator.OptimizationLevel opt = CodeGenerator.OptimizationLevel.O1,
                            bool saveToFile = true)
        {
            _ast = ast;
            _target = target;
            _optLevel = opt;
            _saveToFile = saveToFile;
        }

        public string Generate()
        {
            Console.WriteLine($"🚀 Generating code for {_target} (optimization level {_optLevel})...");

            var generator = new CodeGenerator(_ast, _target, _optLevel);
            var code = generator.Generate();

            if (_saveToFile)
            {
                SaveToFile(code);
            }

            return code;
        }

        private void SaveToFile(string code)
        {
            string filename = GetFilename();
            File.WriteAllText(filename, code);
            Console.WriteLine($"💾 Saved to {filename}");
        }

        private string GetFilename()
        {
            string extension = _target switch
            {
                CodeGenerator.TargetPlatform.CIL => ".il",
                CodeGenerator.TargetPlatform.C => ".c",
                CodeGenerator.TargetPlatform.JavaScript => ".js",
                CodeGenerator.TargetPlatform.Python => ".py",
                CodeGenerator.TargetPlatform.Fminus => ".f--",
                _ => ".txt"
            };

            return $"output{extension}";
        }

        public void PrintStats()
        {
            Console.WriteLine($"📊 Target: {_target}");
            Console.WriteLine($"📊 Optimization: {_optLevel}");
        }
    }
}
