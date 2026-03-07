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
        public List<string> ImportedPackages { get; set; } = new();
        public StartBlockNode? StartBlock { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}Program");
            foreach (var pkg in ImportedPackages)
                Console.WriteLine($"{new string(' ', indent + 2)}IMPORT {pkg}");
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
        public ExpressionNode? Expression { get; set; }
        
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
        public ExpressionNode? Expression { get; set; }
        
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
    /// Assignment: variable = value
    /// </summary>
    public class AssignmentNode : StatementNode
    {
        public string VariableName { get; set; } = string.Empty;
        public ExpressionNode? Value { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}ASSIGN {VariableName} =");
            Value?.Print(indent + 2);
        }
    }

    /// <summary>
    /// Computer call node
    /// </summary>
    public class ComputerCallNode : StatementNode
    {
        public string PackageName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public List<ExpressionNode> Arguments { get; set; } = new();
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}CALL {PackageName}.{MethodName}()");
            foreach (var arg in Arguments)
                arg.Print(indent + 2);
        }
    }

    /// <summary>
    /// At block: at "file.txt" { ... }
    /// </summary>
    public class AtBlockNode : StatementNode
    {
        public ExpressionNode? FileName { get; set; }
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
    /// Base class for expressions
    /// </summary>
    public abstract class ExpressionNode : AstNode { }

    /// <summary>
    /// String literal
    /// </summary>
    public class StringLiteralNode : ExpressionNode
    {
        public string Value { get; set; } = string.Empty;
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
        public string Name { get; set; } = string.Empty;
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}VARIABLE: {Name}");
        }
    }

    /// <summary>
    /// Boolean literal
    /// </summary>
    public class BooleanLiteralNode : ExpressionNode
    {
        public bool Value { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}BOOL: {Value}");
        }
    }

    /// <summary>
    /// Null literal
    /// </summary>
    public class NullLiteralNode : ExpressionNode
    {
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}NULL");
        }
    }
}
