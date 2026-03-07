using System;
using System.Collections.Generic;
using Fminusminus.Errors;

namespace Fminusminus
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;
        private readonly List<SyntaxError> _errors = new();

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public ProgramNode Parse()
        {
            var program = new ProgramNode();
            
            try
            {
                SkipCommentsAndNewlines();

                // Parse all import statements
                while (Check(TokenType.IMPORT))
                {
                    if (!Match(TokenType.IMPORT))
                        throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "import");
                    
                    if (!Match(TokenType.IDENTIFIER))
                        throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "package name");
                    
                    string packageName = Previous().Lexeme;
                    program.ImportedPackages.Add(packageName);
                    
                    SkipCommentsAndNewlines();
                }
                
                if (!program.ImportedPackages.Contains("computer"))
                    throw new SyntaxError("Program must import 'computer' package", 1, 1, "");
                
                SkipCommentsAndNewlines();
                
                if (!Match(TokenType.START))
                    throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "start");
                
                if (!Match(TokenType.LPAREN))
                    throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 5, "(");
                
                if (!Match(TokenType.RPAREN))
                    throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, ")");
                
                program.StartBlock = ParseStartBlock();
                
                SkipCommentsAndNewlines();
                
                if (Peek().Type != TokenType.EOF)
                    throw SyntaxError.UnexpectedSymbol(Peek().Line, Peek().Column, Peek().Lexeme[0]);
            }
            catch (SyntaxError ex)
            {
                _errors.Add(ex);
                if (_errors.Count > 1)
                    throw new AggregateException($"Found {_errors.Count} syntax errors", _errors);
                throw;
            }
            
            return program;
        }

        private StartBlockNode ParseStartBlock()
        {
            var block = new StartBlockNode();
            
            SkipCommentsAndNewlines();
            
            if (!Match(TokenType.LBRACE))
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "{");
            
            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    block.Statements.Add(stmt);
                
                SkipCommentsAndNewlines();
            }
            
            if (!Match(TokenType.RBRACE))
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "}");
            
            // Check for required return statement
            bool hasReturn = false;
            foreach (var stmt in block.Statements)
            {
                if (stmt is ReturnStatementNode)
                {
                    hasReturn = true;
                    break;
                }
            }
            
            if (!hasReturn)
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "return()");
            
            block.HasReturn = hasReturn;
            
            return block;
        }

        private void SkipCommentsAndNewlines()
        {
            while (!IsAtEnd())
            {
                if (Match(TokenType.NEWLINE))
                    continue;
                if (Match(TokenType.COMMENT))
                    continue;
                break;
            }
        }

        private StatementNode? ParseStatement()
        {
            SkipCommentsAndNewlines();
            
            if (IsAtEnd()) return null;
            
            var token = Peek();
            
            try
            {
                switch (token.Type)
                {
                    case TokenType.PRINTLN:
                        return ParsePrintln();
                        
                    case TokenType.PRINT:
                        return ParsePrint();
                        
                    case TokenType.RETURN:
                        return ParseReturn();
                        
                    case TokenType.IDENTIFIER:
                        // Check if it's a package call (identifier.identifier)
                        if (PeekNext().Type == TokenType.DOT)
                        {
                            return ParsePackageCall();
                        }
                        return ParseIdentifierStatement();
                        
                    case TokenType.AT:
                        return ParseAtBlock();
                        
                    case TokenType.COMMENT:
                        Advance();
                        return null;
                        
                    default:
                        throw SyntaxError.UnexpectedSymbol(token.Line, token.Column, token.Lexeme[0]);
                }
            }
            catch (SyntaxError ex)
            {
                _errors.Add(ex);
                while (!Check(TokenType.NEWLINE) && !IsAtEnd()) Advance();
                return null;
            }
        }

        private PrintlnStatementNode ParsePrintln()
        {
            Advance();
            var node = new PrintlnStatementNode();
            
            if (!Match(TokenType.LPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 6, "(");
            
            node.Expression = ParseExpression();
            
            if (!Match(TokenType.RPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, ")");
            
            return node;
        }

        private PrintStatementNode ParsePrint()
        {
            Advance();
            var node = new PrintStatementNode();
            
            if (!Match(TokenType.LPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 5, "(");
            
            node.Expression = ParseExpression();
            
            if (!Match(TokenType.RPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, ")");
            
            return node;
        }

        private ReturnStatementNode ParseReturn()
        {
            Advance();
            var node = new ReturnStatementNode();
            
            if (!Match(TokenType.LPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 6, "(");
            
            if (Check(TokenType.NUMBER))
            {
                if (Peek().Literal == null)
                    throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "number value");
                    
                node.ReturnCode = Convert.ToInt32(Peek().Literal);
                Advance();
            }
            else
            {
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "number");
            }
            
            if (!Match(TokenType.RPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, ")");
            
            return node;
        }

        private ComputerCallNode ParsePackageCall()
        {
            var node = new ComputerCallNode();
            
            // Package name
            node.PackageName = Peek().Lexeme;
            Advance();
            
            if (!Match(TokenType.DOT))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + node.PackageName.Length, ".");
            
            // Method name
            if (Check(TokenType.IDENTIFIER))
            {
                node.MethodName = Peek().Lexeme;
                Advance();
            }
            else
            {
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "method name");
            }
            
            // Arguments
            if (Match(TokenType.LPAREN))
            {
                while (!Check(TokenType.RPAREN) && !IsAtEnd())
                {
                    var expr = ParseExpression();
                    if (expr == null)
                        throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "expression");
                        
                    node.Arguments.Add(expr);
                    Match(TokenType.COMMA);
                }
                
                if (!Match(TokenType.RPAREN))
                    throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, ")");
            }
            
            return node;
        }

        private StatementNode? ParseIdentifierStatement()
        {
            string identifier = Peek().Lexeme;
            int line = Peek().Line;
            int col = Peek().Column;
            Advance();
            
            if (Match(TokenType.ASSIGN))
            {
                var node = new AssignmentNode { VariableName = identifier };
                node.Value = ParseExpression();
                return node;
            }
            
            throw SyntaxError.InvalidToken(line, col, identifier);
        }

        private AtBlockNode ParseAtBlock()
        {
            Advance();
            var node = new AtBlockNode();
            
            node.FileName = ParseExpression();
            
            if (!(node.FileName is StringLiteralNode))
                throw new SyntaxError("Filename must be a string", 
                    node.FileName!.Line, node.FileName.Column, "");
            
            SkipCommentsAndNewlines();
            
            if (!Match(TokenType.LBRACE))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, "{");
            
            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    node.Statements.Add(stmt);
                
                SkipCommentsAndNewlines();
            }
            
            if (!Match(TokenType.RBRACE))
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "}");
            
            return node;
        }

        private ExpressionNode? ParseExpression()
        {
            if (Check(TokenType.STRING))
            {
                var node = new StringLiteralNode { 
                    Value = Peek().Literal?.ToString() ?? "",
                    IsInterpolated = false
                };
                Advance();
                return node;
            }
            
            if (Check(TokenType.STRING_INTERPOLATED))
            {
                var node = new StringLiteralNode { 
                    Value = Peek().Literal?.ToString() ?? "",
                    IsInterpolated = true
                };
                Advance();
                return node;
            }
            
            if (Check(TokenType.NUMBER))
            {
                if (Peek().Literal == null)
                    throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "number value");
                    
                var node = new NumberLiteralNode { 
                    Value = Convert.ToDouble(Peek().Literal)
                };
                Advance();
                return node;
            }
            
            if (Check(TokenType.IDENTIFIER))
            {
                // Check for boolean literals
                string lexeme = Peek().Lexeme;
                if (lexeme == "true" || lexeme == "false")
                {
                    var node = new BooleanLiteralNode { Value = lexeme == "true" };
                    Advance();
                    return node;
                }
                
                if (lexeme == "null")
                {
                    Advance();
                    return new NullLiteralNode();
                }
                
                var varNode = new VariableNode { Name = lexeme };
                Advance();
                return varNode;
            }
            
            throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "expression");
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
        
        private Token PeekNext() 
        {
            int nextIndex = _current + 1;
            if (nextIndex < _tokens.Count)
                return _tokens[nextIndex];
            return _tokens[_current];
        }
        
        private Token Previous() 
        { 
            if (_current == 0) 
                throw new InvalidOperationException("Cannot get previous token at start of stream");
            return _tokens[_current - 1]; 
        }
        
        private bool IsAtEnd() => Peek().Type == TokenType.EOF;
        
        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }
    }
}
