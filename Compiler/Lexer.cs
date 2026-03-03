using System;
using System.Collections.Generic;
using System.Text;

namespace FSharpMinus.Compiler
{
    /// <summary>
    /// Lexer cho F-- 
    /// Chuyển đổi code string thành danh sách tokens
    /// "Thoải mái nhưng vẫn chuẩn chỉnh!"
    /// </summary>
    public class Lexer
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new();
        private int _start = 0;
        private int _current = 0;
        private int _line = 1;
        private int _column = 1;
        private readonly List<string> _errors = new();

        // Từ khóa của F-- (thoải mái thêm sau này)
        private static readonly Dictionary<string, TokenType> _keywords = new()
        {
            // Từ khóa cơ bản
            { "import", TokenType.IMPORT },
            { "using", TokenType.USING },
            { "namespace", TokenType.NAMESPACE },
            { "start", TokenType.START },
            { "return", TokenType.RETURN },
            { "at", TokenType.AT },
            
            // Câu lệnh
            { "println", TokenType.PRINTLN },
            { "io", TokenType.IO },
            { "memory", TokenType.MEMORY },
            
            // Kiểu dữ liệu
            { "true", TokenType.TRUE },
            { "false", TokenType.FALSE },
            { "null", TokenType.NULL },
            
            // Điều khiển (cho tương lai)
            { "if", TokenType.IF },
            { "else", TokenType.ELSE },
            { "while", TokenType.WHILE },
            { "for", TokenType.FOR },
            { "break", TokenType.BREAK },
            { "continue", TokenType.CONTINUE },
            
            // Function (cho tương lai)
            { "func", TokenType.FUNC },
            { "return", TokenType.RETURN },
        };

        public Lexer(string source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <summary>
        /// Quét toàn bộ source code và sinh tokens
        /// </summary>
        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line, _column));

            if (_errors.Count > 0)
            {
                throw new LexerException(string.Join("\n", _errors));
            }

            return _tokens;
        }

        /// <summary>
        /// Quét một token
        /// </summary>
        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                // Single-character tokens
                case '(': AddToken(TokenType.LPAREN); break;
                case ')': AddToken(TokenType.RPAREN); break;
                case '{': AddToken(TokenType.LBRACE); break;
                case '}': AddToken(TokenType.RBRACE); break;
                case '[': AddToken(TokenType.LBRACKET); break;
                case ']': AddToken(TokenType.RBRACKET); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case ':': AddToken(TokenType.COLON); break;
                case '?': AddToken(TokenType.QUESTION); break;

                // Operators
                case '+': AddToken(CheckNext('+') ? TokenType.PLUS_PLUS : TokenType.PLUS); break;
                case '-': 
                    if (CheckNext('-'))
                        AddToken(TokenType.MINUS_MINUS);
                    else if (CheckNext('>'))
                        AddToken(TokenType.ARROW);
                    else
                        AddToken(TokenType.MINUS);
                    break;
                case '*': AddToken(TokenType.STAR); break;
                case '/': 
                    if (Match('/'))
                    {
                        // Comment đến hết dòng
                        while (Peek() != '\n' && !IsAtEnd())
                            Advance();
                        
                        // Lưu comment để có thể giữ lại nếu muốn
                        string comment = _source.Substring(_start + 2, _current - _start - 2);
                        AddToken(TokenType.COMMENT, comment);
                    }
                    else if (Match('*'))
                    {
                        // Block comment /* */
                        BlockComment();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case '%': AddToken(TokenType.PERCENT); break;

                // Comparison
                case '=':
                    if (CheckNext('='))
                        AddToken(TokenType.EQUAL_EQUAL);
                    else if (CheckNext('>'))
                        AddToken(TokenType.ARROW);
                    else
                        AddToken(TokenType.ASSIGN);
                    break;
                case '!':
                    AddToken(CheckNext('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '<':
                    if (CheckNext('='))
                        AddToken(TokenType.LESS_EQUAL);
                    else if (CheckNext('<'))
                        AddToken(TokenType.LESS_LESS);
                    else
                        AddToken(TokenType.LESS);
                    break;
                case '>':
                    if (CheckNext('='))
                        AddToken(TokenType.GREATER_EQUAL);
                    else if (CheckNext('>'))
                        AddToken(TokenType.GREATER_GREATER);
                    else
                        AddToken(TokenType.GREATER);
                    break;

                // String literals
                case '"':
                    StringLiteral();
                    break;
                
                // String interpolation $""
                case '$':
                    if (CheckNext('"'))
                    {
                        Advance(); // consume "
                        StringInterpolation();
                    }
                    else
                    {
                        AddToken(TokenType.IDENTIFIER, "$"); // Có thể là biến $style
                    }
                    break;

                // Whitespace
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace
                    break;
                case '\n':
                    _line++;
                    _column = 1;
                    AddToken(TokenType.NEWLINE);
                    break;

                default:
                    if (IsDigit(c))
                    {
                        Number();
                    }
                    else if (IsAlpha(c) || c == '_')
                    {
                        Identifier();
                    }
                    else
                    {
                        // Ký tự không hợp lệ
                        _errors.Add($"fmm002: Unexpected character '{c}' at line {_line}, column {_column}");
                    }
                    break;
            }
        }

        /// <summary>
        /// Xử lý string literal "hello"
        /// </summary>
        private void StringLiteral()
        {
            StringBuilder sb = new();

            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                if (Peek() == '\\')
                {
                    // Escape sequences
                    Advance();
                    switch (Peek())
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        default: sb.Append(Peek()); break;
                    }
                }
                else
                {
                    sb.Append(Peek());
                }
                Advance();
            }

            if (IsAtEnd())
            {
                _errors.Add($"fmm002: Unterminated string at line {_line}");
                return;
            }

            // Consume closing "
            Advance();

            AddToken(TokenType.STRING, sb.ToString());
        }

        /// <summary>
        /// Xử lý string interpolation $"Hello {name}"
        /// </summary>
        private void StringInterpolation()
        {
            StringBuilder sb = new();

            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                
                if (Peek() == '{')
                {
                    // Bắt đầu interpolation
                    sb.Append('{');
                    Advance();
                    
                    // Xử lý tên biến bên trong {}
                    while (Peek() != '}' && !IsAtEnd() && Peek() != '\n')
                    {
                        sb.Append(Peek());
                        Advance();
                    }
                    
                    if (Peek() == '}')
                    {
                        sb.Append('}');
                        Advance();
                    }
                }
                else if (Peek() == '\\')
                {
                    // Escape sequences
                    Advance();
                    switch (Peek())
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case '"': sb.Append('"'); break;
                        case '{': sb.Append('{'); break;
                        case '}': sb.Append('}'); break;
                        default: sb.Append(Peek()); break;
                    }
                    Advance();
                }
                else
                {
                    sb.Append(Peek());
                    Advance();
                }
            }

            if (IsAtEnd())
            {
                _errors.Add($"fmm002: Unterminated interpolated string at line {_line}");
                return;
            }

            // Consume closing "
            Advance();

            AddToken(TokenType.STRING_INTERPOLATED, sb.ToString());
        }

        /// <summary>
        /// Xử lý block comment /* */
        /// </summary>
        private void BlockComment()
        {
            int nesting = 1;
            
            while (nesting > 0 && !IsAtEnd())
            {
                if (Peek() == '\n') 
                {
                    _line++;
                    _column = 1;
                }
                
                if (Peek() == '/' && PeekNext() == '*')
                {
                    nesting++;
                    Advance();
                    Advance();
                }
                else if (Peek() == '*' && PeekNext() == '/')
                {
                    nesting--;
                    Advance();
                    Advance();
                }
                else
                {
                    Advance();
                }
            }

            if (nesting > 0)
            {
                _errors.Add($"fmm002: Unterminated block comment at line {_line}");
            }
        }

        /// <summary>
        /// Xử lý số
        /// </summary>
        private void Number()
        {
            while (IsDigit(Peek())) Advance();

            // Look for fractional part
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (IsDigit(Peek())) Advance();
                AddToken(TokenType.FLOAT, double.Parse(_source.Substring(_start, _current - _start)));
            }
            else
            {
                AddToken(TokenType.INTEGER, int.Parse(_source.Substring(_start, _current - _start)));
            }
        }

        /// <summary>
        /// Xử lý identifier và keywords
        /// </summary>
        private void Identifier()
        {
            while (IsAlphaNumeric(Peek()) || Peek() == '$') Advance();

            string text = _source.Substring(_start, _current - _start);
            
            // Kiểm tra xem có phải keyword không
            TokenType type = _keywords.TryGetValue(text, out var keywordType) 
                ? keywordType 
                : TokenType.IDENTIFIER;

            AddToken(type, text);
        }

        /// <summary>
        /// Helper methods
        /// </summary>
        private char Advance()
        {
            _current++;
            _column++;
            return _source[_current - 1];
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            _current++;
            _column++;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _source[_current];
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }

        private bool CheckNext(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;
            return true;
        }

        private bool IsAtEnd() => _current >= _source.Length;

        private bool IsDigit(char c) => c >= '0' && c <= '9';
        private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

        private void AddToken(TokenType type, object? literal = null)
        {
            string text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line, _column - (text.Length)));
        }
    }

    /// <summary>
    /// Định nghĩa các loại token trong F--
    /// </summary>
    public enum TokenType
    {
        // Keywords
        IMPORT, USING, NAMESPACE, START, RETURN, AT,
        PRINTLN, IO, MEMORY,
        TRUE, FALSE, NULL,
        IF, ELSE, WHILE, FOR, BREAK, CONTINUE,
        FUNC,

        // Literals
        IDENTIFIER, STRING, STRING_INTERPOLATED, INTEGER, FLOAT, COMMENT,

        // Single-character tokens
        LPAREN, RPAREN, LBRACE, RBRACE, LBRACKET, RBRACKET,
        COMMA, DOT, SEMICOLON, COLON, QUESTION,

        // Operators
        PLUS, PLUS_PLUS, MINUS, MINUS_MINUS, STAR, SLASH, PERCENT,
        ASSIGN, ARROW,

        // Comparison
        EQUAL_EQUAL, BANG, BANG_EQUAL,
        LESS, LESS_EQUAL, GREATER, GREATER_EQUAL,
        LESS_LESS, GREATER_GREATER,

        // Special
        NEWLINE, EOF
    }

    /// <summary>
    /// Token của F--
    /// </summary>
    public class Token
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public object? Literal { get; }
        public int Line { get; }
        public int Column { get; }

        public Token(TokenType type, string lexeme, object? literal, int line, int column)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            if (Literal != null)
                return $"{Type} '{Lexeme}' = {Literal} (line {Line}, col {Column})";
            return $"{Type} '{Lexeme}' (line {Line}, col {Column})";
        }
    }

    /// <summary>
    /// Exception cho lexer errors
    /// </summary>
    public class LexerException : Exception
    {
        public LexerException(string message) : base(message) { }
    }
}
