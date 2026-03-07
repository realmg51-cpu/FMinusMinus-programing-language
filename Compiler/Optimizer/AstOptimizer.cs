using System;
using System.Collections.Generic;
using System.Linq;

namespace Fminusminus.Optimizer
{
    /// <summary>
    /// Optimization levels for F-- code
    /// </summary>
    public enum OptimizationLevel
    {
        O0, // No optimization
        O1, // Basic optimizations (constant folding, dead code elimination)
        O2, // Aggressive optimizations (inlining, loop optimization)
        O3  // Maximum optimizations (all of the above + more)
    }

    /// <summary>
    /// AST Optimizer for F-- programming language
    /// </summary>
    public class AstOptimizer
    {
        private readonly OptimizationLevel _level;
        private int _optimizationsApplied;
        private readonly Dictionary<string, object> _constants = new();

        public AstOptimizer(OptimizationLevel level = OptimizationLevel.O1)
        {
            _level = level;
        }

        /// <summary>
        /// Optimize the AST
        /// </summary>
        public ProgramNode Optimize(ProgramNode ast)
        {
            if (ast == null)
                throw new ArgumentNullException(nameof(ast));

            Console.WriteLine($"🔧 Optimizing AST (level {_level})...");
            _optimizationsApplied = 0;

            var optimizedAst = ast;

            // Apply optimizations based on level
            switch (_level)
            {
                case OptimizationLevel.O0:
                    // No optimizations
                    break;

                case OptimizationLevel.O1:
                    optimizedAst = OptimizeO1(ast);
                    break;

                case OptimizationLevel.O2:
                    optimizedAst = OptimizeO2(ast);
                    break;

                case OptimizationLevel.O3:
                    optimizedAst = OptimizeO3(ast);
                    break;
            }

            Console.WriteLine($"✅ Applied {_optimizationsApplied} optimizations");
            return optimizedAst;
        }

        #region O1 Optimizations (Basic)

        private ProgramNode OptimizeO1(ProgramNode ast)
        {
            var program = (ProgramNode)CloneNode(ast);

            if (program.StartBlock != null)
            {
                // Constant folding
                program.StartBlock.Statements = ConstantFolding(program.StartBlock.Statements);
                
                // Dead code elimination
                program.StartBlock.Statements = DeadCodeElimination(program.StartBlock.Statements);
                
                // Remove redundant statements
                program.StartBlock.Statements = RemoveRedundantStatements(program.StartBlock.Statements);
            }

            return program;
        }

        /// <summary>
        /// Constant folding - evaluate constant expressions at compile time
        /// </summary>
        private List<StatementNode> ConstantFolding(List<StatementNode> statements)
        {
            var result = new List<StatementNode>();

            foreach (var stmt in statements)
            {
                switch (stmt)
                {
                    case AssignmentNode assign:
                        if (assign.Value != null)
                        {
                            assign.Value = FoldExpression(assign.Value);
                            
                            // Track constants for later use
                            if (assign.Value is NumberLiteralNode num)
                            {
                                _constants[assign.VariableName] = num.Value;
                                _optimizationsApplied++;
                            }
                            else if (assign.Value is StringLiteralNode str)
                            {
                                _constants[assign.VariableName] = str.Value;
                                _optimizationsApplied++;
                            }
                            else if (assign.Value is BooleanLiteralNode boolVal)
                            {
                                _constants[assign.VariableName] = boolVal.Value;
                                _optimizationsApplied++;
                            }
                        }
                        result.Add(assign);
                        break;

                    case PrintlnStatementNode println:
                        if (println.Expression != null)
                            println.Expression = FoldExpression(println.Expression);
                        result.Add(println);
                        break;

                    case PrintStatementNode print:
                        if (print.Expression != null)
                            print.Expression = FoldExpression(print.Expression);
                        result.Add(print);
                        break;

                    default:
                        result.Add(stmt);
                        break;
                }
            }

            return result;
        }

        private ExpressionNode FoldExpression(ExpressionNode expr)
        {
            // Handle binary operations
            if (expr is BinaryOperationNode binary)
            {
                binary.Left = FoldExpression(binary.Left);
                binary.Right = FoldExpression(binary.Right);

                // Try to fold constants
                if (binary.Left is NumberLiteralNode leftNum && binary.Right is NumberLiteralNode rightNum)
                {
                    _optimizationsApplied++;
                    return FoldNumberOperation(leftNum, binary.Operator, rightNum);
                }
                
                if (binary.Left is StringLiteralNode leftStr && binary.Right is StringLiteralNode rightStr)
                {
                    if (binary.Operator == TokenType.PLUS)
                    {
                        _optimizationsApplied++;
                        return new StringLiteralNode 
                        { 
                            Value = leftStr.Value + rightStr.Value,
                            IsInterpolated = leftStr.IsInterpolated || rightStr.IsInterpolated
                        };
                    }
                }
            }

            // Replace variable with constant if known
            if (expr is VariableNode var && _constants.ContainsKey(var.Name))
            {
                var constant = _constants[var.Name];
                _optimizationsApplied++;
                
                return constant switch
                {
                    double d => new NumberLiteralNode { Value = d },
                    int i => new NumberLiteralNode { Value = i },
                    string s => new StringLiteralNode { Value = s },
                    bool b => new BooleanLiteralNode { Value = b },
                    _ => expr
                };
            }

            return expr;
        }

        private NumberLiteralNode FoldNumberOperation(NumberLiteralNode left, TokenType op, NumberLiteralNode right)
        {
            double result = op switch
            {
                TokenType.PLUS => left.Value + right.Value,
                TokenType.MINUS => left.Value - right.Value,
                TokenType.STAR => left.Value * right.Value,
                TokenType.SLASH when right.Value != 0 => left.Value / right.Value,
                TokenType.PERCENT when right.Value != 0 => left.Value % right.Value,
                _ => throw new InvalidOperationException($"Cannot fold operation {op}")
            };

            return new NumberLiteralNode { Value = result };
        }

        /// <summary>
        /// Dead code elimination - remove code that never executes
        /// </summary>
        private List<StatementNode> DeadCodeElimination(List<StatementNode> statements)
        {
            var result = new List<StatementNode>();
            bool hasReturn = false;

            foreach (var stmt in statements)
            {
                // If we already have a return/end, subsequent statements are dead
                if (hasReturn)
                {
                    _optimizationsApplied++;
                    continue;
                }

                if (stmt is ReturnStatementNode || stmt is EndStatementNode)
                {
                    hasReturn = true;
                }

                result.Add(stmt);
            }

            return result;
        }

        /// <summary>
        /// Remove redundant statements (assignments to self, etc.)
        /// </summary>
        private List<StatementNode> RemoveRedundantStatements(List<StatementNode> statements)
        {
            var result = new List<StatementNode>();
            var lastAssignment = new Dictionary<string, int>();

            for (int i = 0; i < statements.Count; i++)
            {
                if (statements[i] is AssignmentNode assign)
                {
                    // Check if this is a self-assignment (x = x)
                    if (assign.Value is VariableNode var && var.Name == assign.VariableName)
                    {
                        _optimizationsApplied++;
                        continue; // Skip self-assignment
                    }

                    // Check if variable is assigned multiple times without being used
                    if (lastAssignment.ContainsKey(assign.VariableName))
                    {
                        // Mark previous assignment as potentially redundant
                        // (We'll need data flow analysis for this, skipping for now)
                    }

                    lastAssignment[assign.VariableName] = result.Count;
                    result.Add(assign);
                }
                else
                {
                    result.Add(statements[i]);
                }
            }

            return result;
        }

        #endregion

        #region O2 Optimizations (Aggressive)

        private ProgramNode OptimizeO2(ProgramNode ast)
        {
            var program = OptimizeO1(ast);

            if (program.StartBlock != null)
            {
                // Inline constants
                program.StartBlock.Statements = InlineConstants(program.StartBlock.Statements);
                
                // Merge consecutive print statements
                program.StartBlock.Statements = MergePrintStatements(program.StartBlock.Statements);
                
                // Simple loop unrolling
                program.StartBlock.Statements = UnrollLoops(program.StartBlock.Statements);
            }

            return program;
        }

        /// <summary>
        /// Inline constants where possible
        /// </summary>
        private List<StatementNode> InlineConstants(List<StatementNode> statements)
        {
            var result = new List<StatementNode>();
            var constants = new Dictionary<string, ExpressionNode>();

            foreach (var stmt in statements)
            {
                if (stmt is AssignmentNode assign && assign.Value is NumberLiteralNode)
                {
                    // Keep the assignment but also track constant
                    constants[assign.VariableName] = assign.Value;
                    result.Add(assign);
                }
                else if (stmt is PrintlnStatementNode println && println.Expression is VariableNode var 
                         && constants.ContainsKey(var.Name))
                {
                    // Inline constant in print
                    _optimizationsApplied++;
                    println.Expression = constants[var.Name];
                    result.Add(println);
                }
                else
                {
                    result.Add(stmt);
                }
            }

            return result;
        }

        /// <summary>
        /// Merge consecutive print/println statements
        /// </summary>
        private List<StatementNode> MergePrintStatements(List<StatementNode> statements)
        {
            var result = new List<StatementNode>();
            int i = 0;

            while (i < statements.Count)
            {
                if (i < statements.Count - 1)
                {
                    // Check for println() followed by print()
                    if (statements[i] is PrintlnStatementNode println1 && 
                        statements[i + 1] is PrintStatementNode print)
                    {
                        // Merge: println("Hello") + print("World") -> println("Hello\nWorld")
                        if (println1.Expression is StringLiteralNode str1 && 
                            print.Expression is StringLiteralNode str2)
                        {
                            _optimizationsApplied++;
                            result.Add(new PrintlnStatementNode
                            {
                                Expression = new StringLiteralNode
                                {
                                    Value = str1.Value + "\\n" + str2.Value
                                }
                            });
                            i += 2;
                            continue;
                        }
                    }

                    // Check for multiple prints
                    if (statements[i] is PrintStatementNode print1 && 
                        statements[i + 1] is PrintStatementNode print2)
                    {
                        if (print1.Expression is StringLiteralNode str1 && 
                            print2.Expression is StringLiteralNode str2)
                        {
                            _optimizationsApplied++;
                            result.Add(new PrintStatementNode
                            {
                                Expression = new StringLiteralNode
                                {
                                    Value = str1.Value + str2.Value
                                }
                            });
                            i += 2;
                            continue;
                        }
                    }
                }

                result.Add(statements[i]);
                i++;
            }

            return result;
        }

        /// <summary>
        /// Simple loop unrolling (for patterns that look like loops)
        /// </summary>
        private List<StatementNode> UnrollLoops(List<StatementNode> statements)
        {
            // This is a simplified version - real loop detection would need more analysis
            var result = new List<StatementNode>();

            for (int i = 0; i < statements.Count; i++)
            {
                // Look for patterns that might be loops (multiple similar statements)
                if (i < statements.Count - 2)
                {
                    // Check for repeated pattern
                    if (AreStatementsSimilar(statements[i], statements[i + 1]) &&
                        AreStatementsSimilar(statements[i], statements[i + 2]))
                    {
                        // Could be a loop of 3 iterations
                        _optimizationsApplied++;
                        result.Add(statements[i]);
                        result.Add(statements[i + 1]);
                        result.Add(statements[i + 2]);
                        i += 2; // Skip next two since we added them
                        continue;
                    }
                }

                result.Add(statements[i]);
            }

            return result;
        }

        private bool AreStatementsSimilar(StatementNode a, StatementNode b)
        {
            // Simple similarity check - same type
            return a.GetType() == b.GetType();
        }

        #endregion

        #region O3 Optimizations (Maximum)

        private ProgramNode OptimizeO3(ProgramNode ast)
        {
            var program = OptimizeO2(ast);

            if (program.StartBlock != null)
            {
                // Reorder statements for better performance
                program.StartBlock.Statements = ReorderStatements(program.StartBlock.Statements);
                
                // Algebraic simplification
                program.StartBlock.Statements = AlgebraicSimplification(program.StartBlock.Statements);
                
                // Multiple optimization passes
                for (int pass = 0; pass < 3; pass++)
                {
                    program.StartBlock.Statements = ConstantFolding(program.StartBlock.Statements);
                    program.StartBlock.Statements = DeadCodeElimination(program.StartBlock.Statements);
                }
            }

            return program;
        }

        /// <summary>
        /// Reorder statements for better cache locality
        /// </summary>
        private List<StatementNode> ReorderStatements(List<StatementNode> statements)
        {
            // Group variable declarations together
            var assignments = new List<AssignmentNode>();
            var others = new List<StatementNode>();

            foreach (var stmt in statements)
            {
                if (stmt is AssignmentNode assign)
                    assignments.Add(assign);
                else
                    others.Add(stmt);
            }

            var result = new List<StatementNode>();
            result.AddRange(assignments);
            result.AddRange(others);

            if (assignments.Count > 0)
                _optimizationsApplied++;

            return result;
        }

        /// <summary>
        /// Algebraic simplification (x + 0 = x, x * 1 = x, etc.)
        /// </summary>
        private List<StatementNode> AlgebraicSimplification(List<StatementNode> statements)
        {
            var result = new List<StatementNode>();

            foreach (var stmt in statements)
            {
                if (stmt is AssignmentNode assign && assign.Value is BinaryOperationNode binary)
                {
                    assign.Value = SimplifyAlgebraic(binary);
                    result.Add(assign);
                }
                else if (stmt is PrintlnStatementNode println && println.Expression is BinaryOperationNode binary)
                {
                    println.Expression = SimplifyAlgebraic(binary);
                    result.Add(println);
                }
                else
                {
                    result.Add(stmt);
                }
            }

            return result;
        }

        private ExpressionNode SimplifyAlgebraic(BinaryOperationNode binary)
        {
            // x + 0 = x
            if (binary.Operator == TokenType.PLUS)
            {
                if (IsZero(binary.Right))
                {
                    _optimizationsApplied++;
                    return binary.Left;
                }
                if (IsZero(binary.Left))
                {
                    _optimizationsApplied++;
                    return binary.Right;
                }
            }

            // x * 1 = x
            if (binary.Operator == TokenType.STAR)
            {
                if (IsOne(binary.Right))
                {
                    _optimizationsApplied++;
                    return binary.Left;
                }
                if (IsOne(binary.Left))
                {
                    _optimizationsApplied++;
                    return binary.Right;
                }
            }

            // x * 0 = 0
            if (binary.Operator == TokenType.STAR)
            {
                if (IsZero(binary.Left) || IsZero(binary.Right))
                {
                    _optimizationsApplied++;
                    return new NumberLiteralNode { Value = 0 };
                }
            }

            return binary;
        }

        private bool IsZero(ExpressionNode expr)
        {
            return expr is NumberLiteralNode num && num.Value == 0;
        }

        private bool IsOne(ExpressionNode expr)
        {
            return expr is NumberLiteralNode num && num.Value == 1;
        }

        #endregion

        #region Node Cloning

        private AstNode CloneNode(AstNode node)
        {
            return node switch
            {
                ProgramNode p => CloneProgram(p),
                StartBlockNode s => CloneStartBlock(s),
                PrintlnStatementNode p => ClonePrintln(p),
                PrintStatementNode p => ClonePrint(p),
                ReturnStatementNode r => CloneReturn(r),
                EndStatementNode e => CloneEnd(e),
                AssignmentNode a => CloneAssignment(a),
                PackageCallNode pc => ClonePackageCall(pc),
                AtBlockNode a => CloneAtBlock(a),
                IOStatementNode io => CloneIO(io),
                ComputerStatementNode c => CloneComputer(c),
                MemoryStatementNode m => CloneMemory(m),
                StringLiteralNode s => CloneString(s),
                NumberLiteralNode n => CloneNumber(n),
                VariableNode v => CloneVariable(v),
                BooleanLiteralNode b => CloneBoolean(b),
                NullLiteralNode n => CloneNull(n),
                BinaryOperationNode b => CloneBinary(b),
                _ => throw new NotSupportedException($"Cannot clone node type: {node.GetType()}")
            };
        }

        private ProgramNode CloneProgram(ProgramNode original)
        {
            return new ProgramNode
            {
                ImportedPackages = new List<string>(original.ImportedPackages),
                HasImportComputer = original.HasImportComputer,
                StartBlock = original.StartBlock != null ? CloneStartBlock(original.StartBlock) : null,
                Line = original.Line,
                Column = original.Column
            };
        }

        private StartBlockNode CloneStartBlock(StartBlockNode original)
        {
            return new StartBlockNode
            {
                Statements = original.Statements.Select(s => (StatementNode)CloneNode(s)).ToList(),
                HasReturn = original.HasReturn,
                HasEnd = original.HasEnd,
                Line = original.Line,
                Column = original.Column
            };
        }

        private PrintlnStatementNode ClonePrintln(PrintlnStatementNode original)
        {
            return new PrintlnStatementNode
            {
                Expression = original.Expression != null ? (ExpressionNode)CloneNode(original.Expression) : null,
                Line = original.Line,
                Column = original.Column
            };
        }

        private PrintStatementNode ClonePrint(PrintStatementNode original)
        {
            return new PrintStatementNode
            {
                Expression = original.Expression != null ? (ExpressionNode)CloneNode(original.Expression) : null,
                Line = original.Line,
                Column = original.Column
            };
        }

        private ReturnStatementNode CloneReturn(ReturnStatementNode original)
        {
            return new ReturnStatementNode
            {
                ReturnCode = original.ReturnCode,
                Line = original.Line,
                Column = original.Column
            };
        }

        private EndStatementNode CloneEnd(EndStatementNode original)
        {
            return new EndStatementNode
            {
                Line = original.Line,
                Column = original.Column
            };
        }

        private AssignmentNode CloneAssignment(AssignmentNode original)
        {
            return new AssignmentNode
            {
                VariableName = original.VariableName,
                Value = original.Value != null ? (ExpressionNode)CloneNode(original.Value) : null,
                Line = original.Line,
                Column = original.Column
            };
        }

        private PackageCallNode ClonePackageCall(PackageCallNode original)
        {
            return new PackageCallNode
            {
                PackageName = original.PackageName,
                MethodName = original.MethodName,
                Arguments = original.Arguments.Select(a => (ExpressionNode)CloneNode(a)).ToList(),
                Line = original.Line,
                Column = original.Column
            };
        }

        private AtBlockNode CloneAtBlock(AtBlockNode original)
        {
            return new AtBlockNode
            {
                FileName = original.FileName != null ? (ExpressionNode)CloneNode(original.FileName) : null,
                Statements = original.Statements.Select(s => (StatementNode)CloneNode(s)).ToList(),
                Line = original.Line,
                Column = original.Column
            };
        }

        private IOStatementNode CloneIO(IOStatementNode original)
        {
            return new IOStatementNode
            {
                Operation = original.Operation,
                Parameters = original.Parameters.Select(p => (ExpressionNode)CloneNode(p)).ToList(),
                Line = original.Line,
                Column = original.Column
            };
        }

        private ComputerStatementNode CloneComputer(ComputerStatementNode original)
        {
            return new ComputerStatementNode
            {
                Property = original.Property,
                Operation = original.Operation,
                Line = original.Line,
                Column = original.Column
            };
        }

        private MemoryStatementNode CloneMemory(MemoryStatementNode original)
        {
            return new MemoryStatementNode
            {
                Property = original.Property,
                Line = original.Line,
                Column = original.Column
            };
        }

        private StringLiteralNode CloneString(StringLiteralNode original)
        {
            return new StringLiteralNode
            {
                Value = original.Value,
                IsInterpolated = original.IsInterpolated,
                Line = original.Line,
                Column = original.Column
            };
        }

        private NumberLiteralNode CloneNumber(NumberLiteralNode original)
        {
            return new NumberLiteralNode
            {
                Value = original.Value,
                Line = original.Line,
                Column = original.Column
            };
        }

        private VariableNode CloneVariable(VariableNode original)
        {
            return new VariableNode
            {
                Name = original.Name,
                Line = original.Line,
                Column = original.Column
            };
        }

        private BooleanLiteralNode CloneBoolean(BooleanLiteralNode original)
        {
            return new BooleanLiteralNode
            {
                Value = original.Value,
                Line = original.Line,
                Column = original.Column
            };
        }

        private NullLiteralNode CloneNull(NullLiteralNode original)
        {
            return new NullLiteralNode
            {
                Line = original.Line,
                Column = original.Column
            };
        }

        private BinaryOperationNode CloneBinary(BinaryOperationNode original)
        {
            return new BinaryOperationNode(
                (ExpressionNode)CloneNode(original.Left),
                original.Operator,
                (ExpressionNode)CloneNode(original.Right)
            )
            {
                Line = original.Line,
                Column = original.Column
            };
        }

        #endregion

        /// <summary>
        /// Get optimization statistics
        /// </summary>
        public string GetStats()
        {
            return $@"
📊 Optimizer Statistics:
   Level: {_level}
   Optimizations applied: {_optimizationsApplied}
   Constants tracked: {_constants.Count}";
        }
    }
}
