using System;
using System.Collections.Generic;

namespace Fminusminus
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;
        private readonly List<string> _errors = new();

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public ProgramNode Parse()
        {
            var program = new ProgramNode();
            
            try
            {
                // Bắt buộc phải có: import computer
                if (!Match(TokenType.IMPORT))
                    Error("Missing 'import' keyword");
                
                if (!Match(TokenType.COMPUTER))
                    Error("Expected 'computer' after 'import'");
                
                program.HasImportComputer = true;
                
                // Có thể có newline sau import
                while (Match(TokenType.NEWLINE)) { }
                
                // Bắt buộc phải có: start()
                if (!Match(TokenType.START))
                    Error("Missing 'start()' entry point");
                
                if (!Match(TokenType.LPAREN))
                    Error("Expected '(' after 'start'");
                
                if (!Match(TokenType.RPAREN))
                    Error("Expected ')' after 'start('");
                
                // Parse start block
                program.StartBlock = ParseStartBlock();
                
                // Kiểm tra còn token thừa
                if (Peek().Type != TokenType.EOF)
                    Error($"Unexpected token after end block: {Peek().Lexeme}");
            }
            catch (Exception ex)
            {
                _errors.Add($"fmm001: {ex.Message}");
            }
            
            if (_errors.Count > 0)
                throw new Exception(string.Join("\n", _errors));
            
            return program;
        }

        private StartBlockNode ParseStartBlock()
        {
            var block = new StartBlockNode();
            
            if (!Match(TokenType.LBRACE))
                Error("Expected '{' to start block");
            
            // Parse statements inside block
            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    block.Statements.Add(stmt);
            }
            
            if (!Match(TokenType.RBRACE))
                Error("Expected '}' to end block");
            
            // Kiểm tra bắt buộc phải có return và end
            bool hasReturn = false;
            bool hasEnd = false;
            
            foreach (var stmt in block.Statements)
            {
                if (stmt is ReturnStatementNode) hasReturn = true;
                if (stmt is EndStatementNode) hasEnd = true;
            }
            
            if (!hasReturn)
                Error("Missing required 'return()' statement");
            
            if (!hasEnd)
                Error("Missing required 'end()' statement");
            
            // Kiểm tra return phải là statement cuối cùng trước end
            var lastStmts = block.Statements.GetRange(
                Math.Max(0, block.Statements.Count - 2), 
                Math.Min(2, block.Statements.Count)
            );
            
            if (lastStmts.Count >= 2)
            {
                if (!(lastStmts[lastStmts.Count - 2] is ReturnStatementNode))
                    Error("'return()' must be the last statement before 'end()'");
                
                if (!(lastStmts[lastStmts.Count - 1] is EndStatementNode))
                    Error("'end()' must be the final statement");
            }
            
            block.HasReturn = hasReturn;
            block.HasEnd = hasEnd;
            
            return block;
        }

        private StatementNode ParseStatement()
        {
            // Skip newlines
            while (Match(TokenType.NEWLINE)) { }
            
            if (IsAtEnd()) return null;
            
            switch (Peek().Type)
            {
                case TokenType.PRINTLN:
                    return ParsePrintln();
                    
                case TokenType.PRINT:
                    return ParsePrint();
                    
                case TokenType.RETURN:
                    return ParseReturn();
                    
                case TokenType.END:
                    return ParseEnd();
                    
                case TokenType.IDENTIFIER:
                    return ParseIdentifierStatement();
                    
                case TokenType.AT:
                    return ParseAtBlock();
                    
                case TokenType.IO:
                    return ParseIOStatement();
                    
                case TokenType.MEMORY:
                    return ParseMemoryStatement();
                    
                case TokenType.COMMENT:
                    Advance();
                    return null;
                    
                default:
                    Error($"Unexpected token: {Peek().Lexeme}");
                    Advance();
                    return null;
            }
        }

        private PrintlnStatementNode ParsePrintln()
        {
            Advance(); // consume println
            var node = new PrintlnStatementNode();
            
            if (!Match(TokenType.LPAREN))
                Error("Expected '(' after println");
            
            node.Expression = ParseExpression();
            
            if (!Match(TokenType.RPAREN))
                Error("Expected ')' after expression");
            
            return node;
        }

        private PrintStatementNode ParsePrint()
        {
            Advance(); // consume print
            var node = new PrintStatementNode();
            
            if (!Match(TokenType.LPAREN))
                Error("Expected '(' after print");
            
            node.Expression = ParseExpression();
            
            if (!Match(TokenType.RPAREN))
                Error("Expected ')' after expression");
            
            return node;
        }

        private ReturnStatementNode ParseReturn()
        {
            Advance(); // consume return
            var node = new ReturnStatementNode();
            
            if (!Match(TokenType.LPAREN))
                Error("Expected '(' after return");
            
            if (Check(TokenType.NUMBER))
            {
                node.ReturnCode = (int)Peek().Literal;
                Advance();
            }
            else
            {
                Error("Expected return code number");
            }
            
            if (!Match(TokenType.RPAREN))
                Error("Expected ')' after return code");
            
            return node;
        }

        private EndStatementNode ParseEnd()
        {
            Advance(); // consume end
            var node = new EndStatementNode();
            
            if (!Match(TokenType.LPAREN))
                Error("Expected '(' after end");
            
            if (!Match(TokenType.RPAREN))
                Error("Expected ')' after end(");
            
            return node;
        }

        private StatementNode ParseIdentifierStatement()
        {
            string identifier = Peek().Lexeme;
            Advance();
            
            if (Match(TokenType.ASSIGN))
            {
                var node = new AssignmentNode { VariableName = identifier };
                node.Value = ParseExpression();
                return node;
            }
            
            Error($"Unexpected identifier '{identifier}'");
            return null;
        }

        private AtBlockNode ParseAtBlock()
        {
            Advance(); // consume at
            var node = new AtBlockNode();
            
            node.FileName = ParseExpression();
            
            if (!Match(TokenType.LBRACE))
                Error("Expected '{' after filename");
            
            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    node.Statements.Add(stmt);
            }
            
            if (!Match(TokenType.RBRACE))
                Error("Expected '}' to end at block");
            
            return node;
        }

        private IOStatementNode ParseIOStatement()
        {
            Advance(); // consume io
            var node = new IOStatementNode();
            
            if (!Match(TokenType.DOT))
                Error("Expected '.' after io");
            
            if (Check(TokenType.IDENTIFIER))
            {
                node.Operation = Peek().Lexeme;
                Advance();
            }
            else
            {
                Error("Expected IO operation after io.");
            }
            
            if (Match(TokenType.LPAREN))
            {
                while (!Check(TokenType.RPAREN) && !IsAtEnd())
                {
                    node.Parameters.Add(ParseExpression());
                    Match(TokenType.COMMA);
                }
                
                if (!Match(TokenType.RPAREN))
                    Error("Expected ')' after parameters");
            }
            
            return node;
        }

        private MemoryStatementNode ParseMemoryStatement()
        {
            Advance(); // consume memory
            var node = new MemoryStatementNode();
            
            if (!Match(TokenType.DOT))
                Error("Expected '.' after memory");
            
            if (Check(TokenType.IDENTIFIER))
            {
                node.Property = Peek().Lexeme;
                Advance();
            }
            else
            {
                Error("Expected memory property after memory.");
            }
            
            return node;
        }

        private ExpressionNode ParseExpression()
        {
            if (Check(TokenType.STRING))
            {
                var node = new StringLiteralNode { 
                    Value = Peek().Literal.ToString(),
                    IsInterpolated = false
                };
                Advance();
                return node;
            }
            
            if (Check(TokenType.STRING_INTERPOLATED))
            {
                var node = new StringLiteralNode { 
                    Value = Peek().Literal.ToString(),
                    IsInterpolated = true
                };
                Advance();
                return node;
            }
            
            if (Check(TokenType.NUMBER))
            {
                var node = new NumberLiteralNode { 
                    Value = Convert.ToDouble(Peek().Literal)
                };
                Advance();
                return node;
            }
            
            if (Check(TokenType.IDENTIFIER))
            {
                var node = new VariableNode { Name = Peek().Lexeme };
                Advance();
                return node;
            }
            
            Error("Expected expression");
            return null;
        }

        private bool Match(TokenType type)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
            return false;
        }

        private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;
        private Token Peek() => _tokens[_current];
        private Token Previous() => _tokens[_current - 1];
        private bool IsAtEnd() => Peek().Type == TokenType.EOF;
        
        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private void Error(string message)
        {
            if (!IsAtEnd())
            {
                var token = Peek();
                throw new Exception($"Line {token.Line}, Column {token.Column}: {message}");
            }
            else
            {
                throw new Exception($"At end of file: {message}");
            }
        }
    }
}
