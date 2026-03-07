using System;
using System.Collections.Generic;

namespace Fminusminus
{
    /// <summary>
    /// Base class for all AST nodes
    /// </summary>
    public abstract class AstNode
    {
        public int Line { get; set; }
        public int Column { get; set; }
        
        public virtual void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}{GetType().Name}");
        }
    }

    /// <summary>
    /// Program node - root of AST
    /// </summary>
    public class ProgramNode : AstNode
    {
        public bool HasImportComputer { get; set; }
        public StartBlockNode? StartBlock { get; set; }  // 👈 THÊM ?
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}Program");
            if (HasImportComputer)
                Console.WriteLine($"{new string(' ', indent + 2)}IMPORT computer");
            StartBlock?.Print(indent + 2);
        }
    }

    /// <summary>
    /// Start block: start() { ... }
    /// </summary>
    public class StartBlockNode : AstNode
    {
        public List<StatementNode> Statements { get; set; } = new();
        public bool HasReturn { get; set; }
        public bool HasEnd { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}StartBlock");
            foreach (var stmt in Statements)
                stmt.Print(indent + 2);
        }
    }

    /// <summary>
    /// Base class for statements
    /// </summary>
    public abstract class StatementNode : AstNode { }

    /// <summary>
    /// Print with newline
    /// </summary>
    public class PrintlnStatementNode : StatementNode
    {
        public ExpressionNode? Expression { get; set; }  // 👈 THÊM ?
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}PRINTLN");
            Expression?.Print(indent + 2);
        }
    }

    /// <summary>
    /// Print without newline
    /// </summary>
    public class PrintStatementNode : StatementNode
    {
        public ExpressionNode? Expression { get; set; }  // 👈 THÊM ?
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}PRINT");
            Expression?.Print(indent + 2);
        }
    }

    /// <summary>
    /// Return statement
    /// </summary>
    public class ReturnStatementNode : StatementNode
    {
        public int ReturnCode { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}RETURN {ReturnCode}");
        }
    }

    /// <summary>
    /// End statement
    /// </summary>
    public class EndStatementNode : StatementNode
    {
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}END");
        }
    }

    /// <summary>
    /// Assignment: variable = value
    /// </summary>
    public class AssignmentNode : StatementNode
    {
        public string VariableName { get; set; } = string.Empty;  // 👈 GÁN GIÁ TRỊ MẶC ĐỊNH
        public ExpressionNode? Value { get; set; }  // 👈 THÊM ?
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}ASSIGN {VariableName} =");
            Value?.Print(indent + 2);
        }
    }

    /// <summary>
    /// IO operations
    /// </summary>
    public class IOStatementNode : StatementNode
    {
        public string Operation { get; set; } = string.Empty;  // 👈 GÁN GIÁ TRỊ MẶC ĐỊNH
        public List<ExpressionNode> Parameters { get; set; } = new();
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}IO.{Operation}");
            foreach (var param in Parameters)
                param.Print(indent + 2);
        }
    }

    /// <summary>
    /// Computer system info
    /// </summary>
    public class ComputerStatementNode : StatementNode
    {
        public string Property { get; set; } = string.Empty;  // 👈 GÁN GIÁ TRỊ MẶC ĐỊNH
        public string Operation { get; set; } = string.Empty;  // 👈 GÁN GIÁ TRỊ MẶC ĐỊNH
        public List<ExpressionNode> Parameters { get; set; } = new();
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}COMPUTER.{Property}({Operation})");
            foreach (var param in Parameters)
                param.Print(indent + 2);
        }
    }

    /// <summary>
    /// At block: at "file.txt" { ... }
    /// </summary>
    public class AtBlockNode : StatementNode
    {
        public ExpressionNode? FileName { get; set; }  // 👈 THÊM ?
        public List<StatementNode> Statements { get; set; } = new();
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}AT");
            FileName?.Print(indent + 2);
            Console.WriteLine($"{new string(' ', indent + 2)}BLOCK");
            foreach (var stmt in Statements)
                stmt.Print(indent + 4);
        }
    }

    /// <summary>
    /// Memory access
    /// </summary>
    public class MemoryStatementNode : StatementNode
    {
        public string Property { get; set; } = string.Empty;  // 👈 GÁN GIÁ TRỊ MẶC ĐỊNH
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}MEMORY.{Property}");
        }
    }

    /// <summary>
    /// Base class for expressions
    /// </summary>
    public abstract class ExpressionNode : AstNode { }

    /// <summary>
    /// String literal
    /// </summary>
    public class StringLiteralNode : ExpressionNode
    {
        public string Value { get; set; } = string.Empty;  // 👈 GÁN GIÁ TRỊ MẶC ĐỊNH
        public bool IsInterpolated { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}STRING: \"{Value}\" {(IsInterpolated ? "(interpolated)" : "")}");
        }
    }

    /// <summary>
    /// Number literal
    /// </summary>
    public class NumberLiteralNode : ExpressionNode
    {
        public double Value { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}NUMBER: {Value}");
        }
    }

    /// <summary>
    /// Variable reference
    /// </summary>
    public class VariableNode : ExpressionNode
    {
        public string Name { get; set; } = string.Empty;  // 👈 GÁN GIÁ TRỊ MẶC ĐỊNH
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}VARIABLE: {Name}");
        }
    }
}
