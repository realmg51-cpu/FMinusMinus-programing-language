using System;
using System.Collections.Generic;
using System.IO;

namespace FSharpMinus.Compiler
{
    /// <summary>
    /// Interpreter cho F-- - Thực thi AST nodes
    /// </summary>
    public class Interpreter
    {
        private Dictionary<string, object> _variables = new();
        private string _currentFile = "";
        private long _memoryLeft = 1024; // Simulate 1GB memory
        private bool _inFileBlock = false;
        private List<string> _output = new();

        /// <summary>
        /// Thực thi chương trình từ AST
        /// </summary>
        public int Execute(ProgramNode program)
        {
            if (program.StartBlock == null)
            {
                throw new RuntimeException("No start() block found");
            }

            foreach (var statement in program.StartBlock.Statements)
            {
                ExecuteStatement(statement);
            }

            return 0; // Default return code
        }

        private void ExecuteStatement(StatementNode statement)
        {
            switch (statement)
            {
                case PrintlnStatementNode println:
                    ExecutePrintln(println);
                    break;

                case ReturnStatementNode ret:
                    ExecuteReturn(ret);
                    break;

                case IOStatementNode io:
                    ExecuteIO(io);
                    break;

                case MemoryStatementNode memory:
                    ExecuteMemory(memory);
                    break;

                case AtBlockNode atBlock:
                    ExecuteAtBlock(atBlock);
                    break;

                case AssignmentNode assignment:
                    ExecuteAssignment(assignment);
                    break;

                case FunctionCallNode funcCall:
                    ExecuteFunctionCall(funcCall);
                    break;

                default:
                    throw new RuntimeException($"Unknown statement type: {statement?.GetType().Name}");
            }
        }

        private void ExecutePrintln(PrintlnStatementNode println)
        {
            string output = println.Value;

            if (println.IsInterpolated)
            {
                // Xử lý string interpolation: $"Hello {var}"
                output = InterpolateString(println.Value);
            }

            // Xử lý theo context
            if (_inFileBlock && !string.IsNullOrEmpty(_currentFile))
            {
                // Ghi vào file
                File.AppendAllText(_currentFile, output + Environment.NewLine);
            }
            else
            {
                // In ra console
                Console.WriteLine(output);
                _output.Add(output);
            }
        }

        private string InterpolateString(string template)
        {
            // Xử lý đơn giản: tìm {var} và thay thế
            var result = template;
            foreach (var variable in _variables)
            {
                result = result.Replace($"{{{variable.Key}}}", variable.Value?.ToString());
            }
            return result;
        }

        private void ExecuteReturn(ReturnStatementNode ret)
        {
            if (ret.ReturnCode != 0)
            {
                throw new RuntimeException($"Program exited with code: {ret.ReturnCode}");
            }
        }

        private void ExecuteIO(IOStatementNode io)
        {
            switch (io.Operation)
            {
                case "cfile":
                    if (io.Parameters.Count > 0)
                    {
                        string fileName = io.Parameters[0].Value;
                        _currentFile = fileName.EndsWith(".txt") ? fileName : fileName + ".txt";
                        Console.WriteLine($"Created file: {_currentFile}");
                    }
                    break;

                case "println":
                    if (io.Parameters.Count > 0)
                    {
                        string content = io.Parameters[0].Value;
                        if (!string.IsNullOrEmpty(_currentFile))
                        {
                            File.AppendAllText(_currentFile, content + Environment.NewLine);
                        }
                    }
                    break;

                case "save":
                    if (!string.IsNullOrEmpty(_currentFile))
                    {
                        // Mặc định lưu vào C:\
                        string path = @"C:\" + _currentFile;
                        // Copy file hiện tại (trong thực tế cần xử lý phức tạp hơn)
                        Console.WriteLine($"Saved to: {path}");
                    }
                    break;

                default:
                    throw new RuntimeException($"Unknown IO operation: {io.Operation}");
            }
        }

        private void ExecuteMemory(MemoryStatementNode memory)
        {
            switch (memory.Property)
            {
                case "memoryleft":
                    Console.WriteLine($"Memory left: {_memoryLeft} MB");
                    break;

                case "memoryused":
                    Console.WriteLine($"Memory used: {1024 - _memoryLeft} MB");
                    break;

                case "memorytotal":
                    Console.WriteLine($"Total memory: 1024 MB");
                    break;

                default:
                    throw new RuntimeException($"Unknown memory property: {memory.Property}");
            }
        }

        private void ExecuteAtBlock(AtBlockNode atBlock)
        {
            string previousFile = _currentFile;
            bool previousInBlock = _inFileBlock;

            _currentFile = atBlock.FileName.Trim('"');
            _inFileBlock = true;

            Console.WriteLine($"Working in file: {_currentFile}");

            foreach (var statement in atBlock.Statements)
            {
                ExecuteStatement(statement);
            }

            _currentFile = previousFile;
            _inFileBlock = previousInBlock;
        }

        private void ExecuteAssignment(AssignmentNode assignment)
        {
            _variables[assignment.VariableName] = assignment.Value.Value;
            Console.WriteLine($"Variable set: {assignment.VariableName} = {assignment.Value.Value}");
        }

        private void ExecuteFunctionCall(FunctionCallNode funcCall)
        {
            // Mở rộng sau này
            Console.WriteLine($"Function call: {funcCall.FunctionName}");
        }

        /// <summary>
        /// Lấy output đã thu thập (cho testing)
        /// </summary>
        public List<string> GetOutput() => _output;
    }

    /// <summary>
    /// Exception cho runtime errors
    /// </summary>
    public class RuntimeException : Exception
    {
        public RuntimeException(string message) : base(message) { }
    }
}
