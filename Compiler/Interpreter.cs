using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fminusminus
{
    public class Interpreter
    {
        private Dictionary<string, object> _variables = new();
        private string _currentFile = null;
        private bool _inFileBlock = false;
        private List<string> _fileContent = new();
        
        // Memory simulation
        private long _totalMemory = 1024; // MB
        private long _usedMemory = 256;    // MB
        private long _memoryLeft => _totalMemory - _usedMemory;
        
        public int Execute(ProgramNode program)
        {
            if (!program.HasImportComputer)
                throw new Exception("Missing 'import computer'");
            
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
                    // Return handled by caller
                    break;
                    
                case EndStatementNode end:
                    // End just marks the end
                    break;
                    
                case AssignmentNode assign:
                    ExecuteAssignment(assign);
                    break;
                    
                case IOStatementNode io:
                    ExecuteIO(io);
                    break;
                    
                case AtBlockNode atBlock:
                    ExecuteAtBlock(atBlock);
                    break;
                    
                case MemoryStatementNode memory:
                    ExecuteMemory(memory);
                    break;
                    
                default:
                    throw new Exception($"Unknown statement type: {statement.GetType().Name}");
            }
        }

        private void ExecutePrintln(PrintlnStatementNode println)
        {
            string output = EvaluateExpression(println.Expression);
            
            if (_inFileBlock && _currentFile != null)
            {
                _fileContent.Add(output);
            }
            else
            {
                Console.WriteLine(output);
            }
        }

        private void ExecutePrint(PrintStatementNode print)
        {
            string output = EvaluateExpression(print.Expression);
            
            if (_inFileBlock && _currentFile != null)
            {
                _fileContent.Add(output);
            }
            else
            {
                Console.Write(output);
            }
        }

        private void ExecuteAssignment(AssignmentNode assign)
        {
            string value = EvaluateExpression(assign.Value);
            _variables[assign.VariableName] = value;
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
                    
                default:
                    throw new Exception($"Unknown IO operation: {io.Operation}");
            }
        }

        private void ExecuteCreateFile(IOStatementNode io)
        {
            if (io.Parameters.Count < 1)
                throw new Exception("io.cfile requires filename parameter");
            
            string fileName = EvaluateExpression(io.Parameters[0]);
            
            // Xử lý path parameter nếu có
            string path = "";
            if (io.Parameters.Count > 1 && io.Parameters[1] is VariableNode pathVar && pathVar.Name == "path")
            {
                if (io.Parameters.Count > 2)
                    path = EvaluateExpression(io.Parameters[2]);
            }
            
            // Mặc định lưu ở thư mục hiện tại
            _currentFile = path == "" ? fileName : Path.Combine(path, fileName);
            if (!_currentFile.EndsWith(".txt"))
                _currentFile += ".txt";
            
            Console.WriteLine($"Created file: {_currentFile}");
        }

        private void ExecuteFilePrint(IOStatementNode io)
        {
            if (_currentFile == null)
                throw new Exception("No file opened. Use io.cfile first.");
            
            if (io.Parameters.Count < 1)
                throw new Exception($"io.{io.Operation} requires content parameter");
            
            string content = EvaluateExpression(io.Parameters[0]);
            _fileContent.Add(content);
        }

        private void ExecuteFileSave(IOStatementNode io)
        {
            if (_currentFile == null)
                throw new Exception("No file to save. Use io.cfile first.");
            
            // Xử lý path parameter nếu có
            string savePath = _currentFile;
            if (io.Parameters.Count > 0)
            {
                string path = EvaluateExpression(io.Parameters[0]);
                if (Directory.Exists(path))
                {
                    savePath = Path.Combine(path, Path.GetFileName(_currentFile));
                }
                else
                {
                    savePath = path;
                }
            }
            
            try
            {
                File.WriteAllLines(savePath, _fileContent);
                Console.WriteLine($"File saved: {savePath}");
                
                // Clear file content after save
                _fileContent.Clear();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving file: {ex.Message}");
            }
        }

        private void ExecuteAtBlock(AtBlockNode atBlock)
        {
            string fileName = EvaluateExpression(atBlock.FileName);
            string previousFile = _currentFile;
            bool previousInBlock = _inFileBlock;
            var previousContent = _fileContent;
            
            _currentFile = fileName;
            _inFileBlock = true;
            _fileContent = new List<string>();
            
            foreach (var statement in atBlock.Statements)
            {
                ExecuteStatement(statement);
            }
            
            // Restore previous state
            _currentFile = previousFile;
            _inFileBlock = previousInBlock;
            _fileContent = previousContent;
        }

        private void ExecuteMemory(MemoryStatementNode memory)
        {
            long value = memory.Property switch
            {
                "memoryleft" => _memoryLeft,
                "memoryused" => _usedMemory,
                "memorytotal" => _totalMemory,
                _ => throw new Exception($"Unknown memory property: {memory.Property}")
            };
            
            if (_inFileBlock && _currentFile != null)
            {
                _fileContent.Add(value.ToString());
            }
            else
            {
                Console.WriteLine($"{memory.Property}: {value} MB");
            }
            
            // Simulate memory usage
            if (memory.Property == "memoryused")
                _usedMemory = Math.Min(_totalMemory, _usedMemory + 1);
        }

        private string EvaluateExpression(ExpressionNode expr)
        {
            switch (expr)
            {
                case StringLiteralNode str:
                    if (str.IsInterpolated)
                        return InterpolateString(str.Value);
                    return str.Value;
                    
                case NumberLiteralNode num:
                    return num.Value.ToString();
                    
                case VariableNode var:
                    if (_variables.TryGetValue(var.Name, out object value))
                        return value.ToString();
                    throw new Exception($"Undefined variable: {var.Name}");
                    
                default:
                    throw new Exception($"Cannot evaluate expression: {expr?.GetType().Name}");
            }
        }

        private string InterpolateString(string template)
        {
            var result = new StringBuilder();
            int i = 0;
            
            while (i < template.Length)
            {
                if (template[i] == '{')
                {
                    int j = i + 1;
                    int braceCount = 1;
                    
                    while (j < template.Length && braceCount > 0)
                    {
                        if (template[j] == '{') braceCount++;
                        if (template[j] == '}') braceCount--;
                        j++;
                    }
                    
                    if (braceCount == 0)
                    {
                        string varName = template.Substring(i + 1, j - i - 2).Trim();
                        string value = EvaluateExpression(new VariableNode { Name = varName });
                        result.Append(value);
                        i = j;
                    }
                    else
                    {
                        result.Append(template[i]);
                        i++;
                    }
                }
                else
                {
                    result.Append(template[i]);
                    i++;
                }
            }
            
            return result.ToString();
        }
    }
}

