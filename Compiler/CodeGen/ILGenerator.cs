using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Fminusminus.Optimizer;

namespace Fminusminus.CodeGen
{
    /// <summary>
    /// IL Code Generator for F--
    /// Uses System.Reflection.Emit to generate MSIL code
    /// Creates actual .NET executables that run as fast as C#!
    /// </summary>
    public class ILGenerator
    {
        private readonly ProgramNode _ast;
        private readonly string _outputPath;
        private readonly AstOptimizer.OptimizationLevel _optLevel;
        
        // Dynamic assembly
        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _moduleBuilder;
        private TypeBuilder _typeBuilder;
        private MethodBuilder _methodBuilder;
        private ILGenerator _il;
        
        // Symbol table for variables
        private Dictionary<string, LocalBuilder> _locals = new();
        private Dictionary<string, FieldBuilder> _fields = new();

        public ILGenerator(ProgramNode ast, string outputPath = "a.exe", 
                          AstOptimizer.OptimizationLevel optLevel = AstOptimizer.OptimizationLevel.O1)
        {
            _ast = ast;
            _outputPath = outputPath;
            _optLevel = optLevel;
        }

        /// <summary>
        /// Generate executable from AST
        /// </summary>
        public int GenerateExecutable()
        {
            Console.WriteLine($"⚡ Generating executable: {_outputPath}");
            Console.WriteLine($"🔧 Optimization level: {_optLevel}");

            try
            {
                // Apply optimizations if enabled
                var optimizedAst = _ast;
                if (_optLevel > AstOptimizer.OptimizationLevel.O0)
                {
                    var optimizer = new AstOptimizer(_optLevel);
                    optimizedAst = optimizer.Optimize(_ast);
                }

                // Create dynamic assembly
                CreateAssembly();
                
                // Generate code from AST
                GenerateProgram(optimizedAst);
                
                // Save assembly
                _assemblyBuilder.Save(_outputPath);
                
                Console.WriteLine($"✅ Successfully generated: {_outputPath}");
                Console.WriteLine($"📦 File size: {new FileInfo(_outputPath).Length} bytes");
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Compilation failed: {ex.Message}");
                return 1;
            }
        }

        private void CreateAssembly()
        {
            // Assembly name
            string assemblyName = Path.GetFileNameWithoutExtension(_outputPath);
            var name = new AssemblyName(assemblyName);
            
            // Create assembly builder
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                name, 
                AssemblyBuilderAccess.Save
            );
            
            // Create module builder
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(
                assemblyName, 
                _outputPath
            );
            
            // Create main class
            _typeBuilder = _moduleBuilder.DefineType(
                "Program",
                TypeAttributes.Public | TypeAttributes.Class
            );
            
            // Create main method
            _methodBuilder = _typeBuilder.DefineMethod(
                "Main",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(int),
                new[] { typeof(string[]) }
            );
            
            // Get IL generator
            _il = _methodBuilder.GetILGenerator();
        }

        private void GenerateProgram(ProgramNode program)
        {
            if (!program.HasImportComputer)
            {
                _il.Emit(OpCodes.Ldc_I4_1);
                _il.Emit(OpCodes.Ret);
                return;
            }

            if (program.StartBlock != null)
            {
                foreach (var stmt in program.StartBlock.Statements)
                {
                    GenerateStatement(stmt);
                }
            }

            // Default return 0 if no return statement found
            _il.Emit(OpCodes.Ldc_I4_0);
            _il.Emit(OpCodes.Ret);

            // Create type
            var type = _typeBuilder.CreateType();
            
            // Set entry point
            _assemblyBuilder.SetEntryPoint(_methodBuilder, PEFileKinds.ConsoleApplication);
        }

        private void GenerateStatement(StatementNode stmt)
        {
            switch (stmt)
            {
                case PrintlnStatementNode println:
                    GeneratePrintln(println);
                    break;
                    
                case PrintStatementNode print:
                    GeneratePrint(print);
                    break;
                    
                case ReturnStatementNode ret:
                    GenerateReturn(ret);
                    break;
                    
                case EndStatementNode end:
                    // End does nothing in IL - just a marker
                    break;
                    
                case AssignmentNode assign:
                    GenerateAssignment(assign);
                    break;
                    
                case MemoryStatementNode memory:
                    GenerateMemory(memory);
                    break;
                    
                case IOStatementNode io:
                    GenerateIO(io);
                    break;
                    
                case ComputerStatementNode computer:
                    GenerateComputer(computer);
                    break;
            }
        }

        private void GeneratePrintln(PrintlnStatementNode println)
        {
            // Get Console.WriteLine method
            var consoleWriteLine = typeof(Console).GetMethod(
                "WriteLine", 
                new[] { typeof(string) }
            ) ?? throw new InvalidOperationException("Console.WriteLine not found");

            // Load string argument
            if (println.Expression is StringLiteralNode str)
            {
                if (str.IsInterpolated)
                {
                    // Handle interpolation - for now, just use as is
                    _il.Emit(OpCodes.Ldstr, InterpolateString(str.Value));
                }
                else
                {
                    _il.Emit(OpCodes.Ldstr, str.Value);
                }
            }
            else if (println.Expression is VariableNode var)
            {
                // Load variable value
                if (_locals.TryGetValue(var.Name, out var local))
                {
                    _il.Emit(OpCodes.Ldloc, local);
                }
                else
                {
                    // Assume string variable
                    _il.Emit(OpCodes.Ldstr, $"{{{var.Name}}}");
                }
            }
            else
            {
                _il.Emit(OpCodes.Ldstr, "");
            }

            // Call Console.WriteLine
            _il.Emit(OpCodes.Call, consoleWriteLine);
        }

        private void GeneratePrint(PrintStatementNode print)
        {
            // Get Console.Write method
            var consoleWrite = typeof(Console).GetMethod(
                "Write", 
                new[] { typeof(string) }
            ) ?? throw new InvalidOperationException("Console.Write not found");

            // Load string argument
            if (print.Expression is StringLiteralNode str)
            {
                _il.Emit(OpCodes.Ldstr, str.Value);
            }
            else
            {
                _il.Emit(OpCodes.Ldstr, "");
            }

            // Call Console.Write
            _il.Emit(OpCodes.Call, consoleWrite);
        }

        private void GenerateReturn(ReturnStatementNode ret)
        {
            _il.Emit(OpCodes.Ldc_I4, ret.ReturnCode);
            _il.Emit(OpCodes.Ret);
        }

        private void GenerateAssignment(AssignmentNode assign)
        {
            if (!_locals.ContainsKey(assign.VariableName))
            {
                // Create local variable
                var local = _methodBuilder.GetILGenerator().DeclareLocal(typeof(string));
                _locals[assign.VariableName] = local;
            }

            var localVar = _locals[assign.VariableName];

            if (assign.Value is StringLiteralNode str)
            {
                _il.Emit(OpCodes.Ldstr, str.Value);
                _il.Emit(OpCodes.Stloc, localVar);
            }
            else if (assign.Value is NumberLiteralNode num)
            {
                // Convert number to string for simplicity
                _il.Emit(OpCodes.Ldstr, num.Value.ToString());
                _il.Emit(OpCodes.Stloc, localVar);
            }
        }

        private void GenerateMemory(MemoryStatementNode memory)
        {
            var consoleWriteLine = typeof(Console).GetMethod(
                "WriteLine", 
                new[] { typeof(string) }
            )!;

            // Simulate memory info
            string memoryInfo = memory.Property switch
            {
                "memoryleft" => $"Memory left: 1024 MB",
                "memoryused" => $"Memory used: 256 MB",
                "memorytotal" => $"Total memory: 1280 MB",
                _ => "Unknown memory property"
            };

            _il.Emit(OpCodes.Ldstr, memoryInfo);
            _il.Emit(OpCodes.Call, consoleWriteLine);
        }

        private void GenerateIO(IOStatementNode io)
        {
            var consoleWriteLine = typeof(Console).GetMethod(
                "WriteLine", 
                new[] { typeof(string) }
            )!;

            switch (io.Operation)
            {
                case "cfile":
                    _il.Emit(OpCodes.Ldstr, $"📁 Creating file...");
                    _il.Emit(OpCodes.Call, consoleWriteLine);
                    break;
                    
                case "save":
                    _il.Emit(OpCodes.Ldstr, $"💾 File saved");
                    _il.Emit(OpCodes.Call, consoleWriteLine);
                    break;
                    
                case "listfile":
                    _il.Emit(OpCodes.Ldstr, $"📂 Listing files...");
                    _il.Emit(OpCodes.Call, consoleWriteLine);
                    
                    // Simple directory listing
                    var files = Directory.GetFiles(".");
                    foreach (var file in files)
                    {
                        _il.Emit(OpCodes.Ldstr, $"  📄 {Path.GetFileName(file)}");
                        _il.Emit(OpCodes.Call, consoleWriteLine);
                    }
                    break;
            }
        }

        private void GenerateComputer(ComputerStatementNode computer)
        {
            var consoleWriteLine = typeof(Console).GetMethod(
                "WriteLine", 
                new[] { typeof(string) }
            )!;

            if (computer.Property == "systeminfo" && computer.Operation == "get")
            {
                _il.Emit(OpCodes.Ldstr, $"OS: {Environment.OSVersion}");
                _il.Emit(OpCodes.Call, consoleWriteLine);
                
                _il.Emit(OpCodes.Ldstr, $"Machine: {Environment.MachineName}");
                _il.Emit(OpCodes.Call, consoleWriteLine);
                
                _il.Emit(OpCodes.Ldstr, $"CPU Cores: {Environment.ProcessorCount}");
                _il.Emit(OpCodes.Call, consoleWriteLine);
                
                _il.Emit(OpCodes.Ldstr, $".NET: {Environment.Version}");
                _il.Emit(OpCodes.Call, consoleWriteLine);
            }
        }

        private string InterpolateString(string template)
        {
            // Simple interpolation - replace {var} with actual values
            // In real implementation, would need proper variable resolution
            return template.Replace("{", "").Replace("}", "");
        }
    }
}
