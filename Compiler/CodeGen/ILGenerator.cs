using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Fminusminus;
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
        private readonly CodeGenerator.OptimizationLevel _optLevel;
        
        // Dynamic assembly
        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _moduleBuilder;
        private TypeBuilder _typeBuilder;
        private MethodBuilder _methodBuilder;
        private System.Reflection.Emit.ILGenerator _il;
        
        // Symbol table for variables
        private Dictionary<string, LocalBuilder> _locals = new();
        private Dictionary<string, FieldBuilder> _fields = new();
        
        // Helper fields
        private LocalBuilder _arrayLocal;

        public ILGenerator(ProgramNode ast, string outputPath = "a.exe", 
                          CodeGenerator.OptimizationLevel optLevel = CodeGenerator.OptimizationLevel.O1)
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
                if (_optLevel > CodeGenerator.OptimizationLevel.O0)
                {
                    var optimizer = new AstOptimizer((AstOptimizer.OptimizationLevel)_optLevel);
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
                    
                case PackageCallNode pkg:
                    GeneratePackageCall(pkg);
                    break;
                    
                case AtBlockNode atBlock:
                    GenerateAtBlock(atBlock);
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
                _il.Emit(OpCodes.Ldstr, str.Value);
            }
            else if (println.Expression is VariableNode var)
            {
                // Load variable value
                if (_locals.TryGetValue(var.Name, out var local))
                {
                    _il.Emit(OpCodes.Ldloc, local);
                    // Ensure it's string
                    var toString = typeof(object).GetMethod("ToString");
                    _il.Emit(OpCodes.Callvirt, toString);
                }
                else
                {
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
                // Create local variable using existing ILGenerator
                var local = _il.DeclareLocal(typeof(object));
                _locals[assign.VariableName] = local;
            }

            var localVar = _locals[assign.VariableName];

            switch (assign.Value)
            {
                case StringLiteralNode str:
                    _il.Emit(OpCodes.Ldstr, str.Value);
                    break;
                    
                case NumberLiteralNode num:
                    _il.Emit(OpCodes.Ldc_R8, num.Value);
                    _il.Emit(OpCodes.Box, typeof(double));
                    break;
                    
                case BooleanLiteralNode boolVal:
                    _il.Emit(boolVal.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Box, typeof(bool));
                    break;
                    
                case NullLiteralNode:
                    _il.Emit(OpCodes.Ldnull);
                    break;
                    
                default:
                    _il.Emit(OpCodes.Ldstr, "");
                    break;
            }
            
            _il.Emit(OpCodes.Stloc, localVar);
        }

        private void GenerateMemory(MemoryStatementNode memory)
        {
            var consoleWriteLine = typeof(Console).GetMethod(
                "WriteLine", 
                new[] { typeof(string) }
            )!;

            // Get real memory info using SystemInfo class
            var getTotalMemory = typeof(GC).GetMethod("GetTotalMemory", new[] { typeof(bool) })!;
            
            _il.Emit(OpCodes.Ldc_I4_0); // false - don't force collection
            _il.Emit(OpCodes.Call, getTotalMemory);
            
            // Format memory string
            _il.Emit(OpCodes.Ldstr, "{0} bytes");
            _il.Emit(OpCodes.Box, typeof(long));
            var formatMethod = typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object) })!;
            _il.Emit(OpCodes.Call, formatMethod);
            _il.Emit(OpCodes.Call, consoleWriteLine);
        }

        private void GenerateIO(IOStatementNode io)
        {
            var consoleWriteLine = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })!;
            var directoryGetFiles = typeof(Directory).GetMethod("GetFiles", new[] { typeof(string) })!;
            var stringFormat = typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object) })!;

            switch (io.Operation.ToLower())
            {
                case "listfile":
                    _il.Emit(OpCodes.Ldstr, ".");
                    _il.Emit(OpCodes.Call, directoryGetFiles);
                    
                    var filesArray = _il.DeclareLocal(typeof(string[]));
                    _il.Emit(OpCodes.Stloc, filesArray);
                    
                    // Create index variable
                    var index = _il.DeclareLocal(typeof(int));
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Stloc, index);
                    
                    var loopStart = _il.DefineLabel();
                    var loopEnd = _il.DefineLabel();
                    
                    _il.MarkLabel(loopStart);
                    
                    // Check if index < array.Length
                    _il.Emit(OpCodes.Ldloc, index);
                    _il.Emit(OpCodes.Ldloc, filesArray);
                    _il.Emit(OpCodes.Ldlen);
                    _il.Emit(OpCodes.Conv_I4);
                    _il.Emit(OpCodes.Bge, loopEnd);
                    
                    // Load and print file name
                    _il.Emit(OpCodes.Ldloc, filesArray);
                    _il.Emit(OpCodes.Ldloc, index);
                    _il.Emit(OpCodes.Ldelem_Ref);
                    
                    _il.Emit(OpCodes.Ldstr, "  📄 {0}");
                    _il.Emit(OpCodes.Ldloc, filesArray);
                    _il.Emit(OpCodes.Ldloc, index);
                    _il.Emit(OpCodes.Ldelem_Ref);
                    _il.Emit(OpCodes.Call, stringFormat);
                    _il.Emit(OpCodes.Call, consoleWriteLine);
                    
                    // index++
                    _il.Emit(OpCodes.Ldloc, index);
                    _il.Emit(OpCodes.Ldc_I4_1);
                    _il.Emit(OpCodes.Add);
                    _il.Emit(OpCodes.Stloc, index);
                    
                    _il.Emit(OpCodes.Br, loopStart);
                    _il.MarkLabel(loopEnd);
                    break;
                    
                default:
                    _il.Emit(OpCodes.Ldstr, $"IO operation: {io.Operation}");
                    _il.Emit(OpCodes.Call, consoleWriteLine);
                    break;
            }
        }

        private void GenerateComputer(ComputerStatementNode computer)
        {
            var consoleWriteLine = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })!;

            if (computer.Property == "systeminfo")
            {
                // OS Version
                _il.Emit(OpCodes.Ldstr, $"OS: {Environment.OSVersion}");
                _il.Emit(OpCodes.Call, consoleWriteLine);
                
                // Machine Name
                _il.Emit(OpCodes.Ldstr, $"Machine: {Environment.MachineName}");
                _il.Emit(OpCodes.Call, consoleWriteLine);
                
                // Processor Count
                _il.Emit(OpCodes.Ldstr, $"CPU Cores: {Environment.ProcessorCount}");
                _il.Emit(OpCodes.Call, consoleWriteLine);
                
                // .NET Version
                _il.Emit(OpCodes.Ldstr, $".NET: {Environment.Version}");
                _il.Emit(OpCodes.Call, consoleWriteLine);
            }
            else
            {
                _il.Emit(OpCodes.Ldstr, $"Computer.{computer.Property}");
                _il.Emit(OpCodes.Call, consoleWriteLine);
            }
        }

        private void GeneratePackageCall(PackageCallNode pkg)
        {
            var consoleWriteLine = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })!;
            
            _il.Emit(OpCodes.Ldstr, $"📦 Calling {pkg.PackageName}.{pkg.MethodName}()");
            _il.Emit(OpCodes.Call, consoleWriteLine);
            
            foreach (var arg in pkg.Arguments)
            {
                if (arg is StringLiteralNode str)
                {
                    _il.Emit(OpCodes.Ldstr, $"   Arg: {str.Value}");
                    _il.Emit(OpCodes.Call, consoleWriteLine);
                }
            }
        }

        private void GenerateAtBlock(AtBlockNode atBlock)
        {
            var consoleWriteLine = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })!;
            
            if (atBlock.FileName is StringLiteralNode str)
            {
                _il.Emit(OpCodes.Ldstr, $"📌 Executing block from: {str.Value}");
                _il.Emit(OpCodes.Call, consoleWriteLine);
            }
            
            // Generate statements in the block
            foreach (var stmt in atBlock.Statements)
            {
                GenerateStatement(stmt);
            }
        }
    }
}
