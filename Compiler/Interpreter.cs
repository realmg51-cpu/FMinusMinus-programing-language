using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Fminusminus.Errors;
using Fminusminus.Utils;

namespace Fminusminus
{
    public partial class Interpreter
    {
        private Dictionary<string, object> _variables = new();
        private string _currentFile = null;
        private bool _inFileBlock = false;
        private List<string> _fileContent = new();
        private SystemInfo _systemInfo = null;

        // Memory simulation
        private long _totalMemory = 1024;
        private long _usedMemory = 256;
        private long _memoryLeft => _totalMemory - _usedMemory;

        public int Execute(ProgramNode program)
        {
            if (!program.HasImportComputer)
                throw new Exception("Missing 'import computer'");
            
            _systemInfo = new SystemInfo();
            
            foreach (var statement in program.StartBlock.Statements)
            {
                ExecuteStatement(statement);
            }
            
            return 0;
        }

        private void ExecuteStatement(StatementNode statement)
        {
            switch (statement)
            {
                case PrintlnStatementNode println:
                    ExecutePrintln(println);
                    break;
                    
                case PrintStatementNode print:
                    ExecutePrint(print);
                    break;
                    
                case ReturnStatementNode ret:
                    // Handled by caller
                    break;
                    
                case EndStatementNode end:
                    // Just marks the end
                    break;
                    
                case AssignmentNode assign:
                    ExecuteAssignment(assign);
                    break;
                    
                case IOStatementNode io:
                    ExecuteIO(io);
                    break;
                    
                case ComputerStatementNode computer:
                    ExecuteComputer(computer);
                    break;
                    
                case AtBlockNode atBlock:
                    ExecuteAtBlock(atBlock);
                    break;
                    
                case MemoryStatementNode memory:
                    ExecuteMemory(memory);
                    break;
                    
                default:
                    throw new Exception($"Unknown statement type: {statement?.GetType().Name}");
            }
        }

        private void ExecuteComputer(ComputerStatementNode computer)
        {
            if (computer.Property == "systeminfo" && computer.Operation == "get")
            {
                _variables["systeminfo"] = _systemInfo;
                Console.WriteLine(_systemInfo.ToString());
            }
        }

        private void ExecuteIO(IOStatementNode io)
        {
            switch (io.Operation)
            {
                case "cfile":
                    ExecuteCreateFile(io);
                    break;
                    
                case "println":
                case "print":
                    ExecuteFilePrint(io);
                    break;
                    
                case "save":
                    ExecuteFileSave(io);
                    break;
                    
                case "listfile":
                    ExecuteListFile(io);
                    break;
                    
                default:
                    throw new Exception($"Unknown IO operation: {io.Operation}");
            }
        }

        private void ExecuteListFile(IOStatementNode io)
        {
            string path = ".";
            
            // Parse parameters
            if (io.Parameters.Count > 0)
            {
                if (io.Parameters[0] is VariableNode var && var.Name == "path")
                {
                    if (io.Parameters.Count > 1)
                    {
                        if (io.Parameters[1] is StringLiteralNode strNode)
                        {
                            path = strNode.Value;
                        }
                        else if (io.Parameters[1] is VariableNode osVar && osVar.Name == "OS" && 
                                 io.Parameters.Count > 2 && io.Parameters[2] is VariableNode pathVar && pathVar.Name == "path")
                        {
                            path = SystemInfo.GetOSPath();
                        }
                    }
                }
            }
            
            try
            {
                // Validate path
                if (string.IsNullOrWhiteSpace(path))
                    throw FileError.NotFound(path);
                
                // Check for invalid characters
                if (FileError.HasInvalidCharacters(path))
                    throw FileError.InvalidCharacters(path);
                
                // Check if directory exists
                if (!Directory.Exists(path))
                    throw FileError.NotFound(path);
                
                // Check access permission
                try
                {
                    Directory.GetFiles(path);
                }
                catch (UnauthorizedAccessException)
                {
                    throw FileError.AccessDenied(path);
                }
                
                var files = Directory.GetFiles(path);
                var dirs = Directory.GetDirectories(path);
                
                Console.WriteLine($"\n📁 Contents of '{path}':");
                Console.WriteLine($"   Total: {dirs.Length} folders, {files.Length} files\n");
                
                foreach (var dir in dirs)
                    Console.WriteLine($"   📂 {Path.GetFileName(dir)}/");
                foreach (var file in files)
                    Console.WriteLine($"   📄 {Path.GetFileName(file)}");
                    
                Console.WriteLine();
            }
            catch (FileError)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error listing directory: {ex.Message}");
            }
        }

        // ... (rest of existing methods)
    }
}
