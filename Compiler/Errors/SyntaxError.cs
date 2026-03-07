using System;

namespace Fminusminus.Errors
{
    /// <summary>
    /// Syntax errors with exact position reporting
    /// </summary>
    public class SyntaxError : Exception
    {
        public int Line { get; }
        public int Column { get; }
        public string Symbol { get; }
        public string ErrorCode { get; }
        public string SourceLine { get; set; }

        public SyntaxError(string message, int line, int col, string symbol, string code = "FMM100") 
            : base($"[{code}] {message}")
        {
            Line = line;
            Column = col;
            Symbol = symbol;
            ErrorCode = code;
        }

        /// <summary>
        /// Unexpected symbol error
        /// </summary>
        public static SyntaxError UnexpectedSymbol(int line, int col, char symbol)
        {
            return new SyntaxError(
                $"Unexpected symbol '{symbol}'",
                line, col, symbol.ToString(),
                "FMM101"
            );
        }

        /// <summary>
        /// Missing token error
        /// </summary>
        public static SyntaxError MissingToken(int line, int col, string expected)
        {
            return new SyntaxError(
                $"Expected '{expected}'",
                line, col, "",
                "FMM102"
            );
        }

        /// <summary>
        /// Invalid token error
        /// </summary>
        public static SyntaxError InvalidToken(int line, int col, string token)
        {
            return new SyntaxError(
                $"Invalid token '{token}'",
                line, col, token,
                "FMM103"
            );
        }

        /// <summary>
        /// Unterminated string error
        /// </summary>
        public static SyntaxError UnterminatedString(int line, int col)
        {
            return new SyntaxError(
                "Unterminated string literal",
                line, col, "\"",
                "FMM104"
            );
        }

        /// <summary>
        /// Invalid number format error
        /// </summary>
        public static SyntaxError InvalidNumber(int line, int col, string number)
        {
            return new SyntaxError(
                $"Invalid number format '{number}'",
                line, col, number,
                "FMM105"
            );
        }

        /// <summary>
        /// Get formatted error message with arrow pointing to error
        /// </summary>
        public string GetFormattedMessage(string[] lines)
        {
            if (lines == null || lines.Length < Line)
                return Message;

            string errorLine = lines[Line - 1];
            string arrow = new string(' ', Math.Max(0, Column - 1)) + "^";
            
            return $@"
{Message}
{errorLine}
{arrow}
at line {Line}, column {Column}";
        }
    }
}
