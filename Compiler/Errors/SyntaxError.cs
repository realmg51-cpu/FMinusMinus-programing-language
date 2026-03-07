using System;

namespace Fminusminus.Errors
{
    /// <summary>
    /// Syntax error codes
    /// </summary>
    public static class SyntaxErrorCodes
    {
        public const string Generic = "FMM100";
        public const string UnexpectedSymbol = "FMM101";
        public const string MissingToken = "FMM102";
        public const string InvalidToken = "FMM103";
        public const string UnterminatedString = "FMM104";
        public const string InvalidNumber = "FMM105";
        public const string UnterminatedComment = "FMM106";
        public const string InvalidEscape = "FMM107";
        public const string UnexpectedEOF = "FMM108";
    }

    /// <summary>
    /// Syntax errors with exact position reporting
    /// </summary>
    public class SyntaxError : Exception
    {
        public int Line { get; }
        public int Column { get; }
        public string Symbol { get; }
        public string ErrorCode { get; }
        public string? SourceLine { get; set; }

        public SyntaxError(string message, int line, int col, string symbol, string code = SyntaxErrorCodes.Generic, string? sourceLine = null) 
            : base($"[{code}] {message}")
        {
            Line = line;
            Column = col;
            Symbol = symbol;
            ErrorCode = code;
            SourceLine = sourceLine;
        }

        /// <summary>
        /// Unexpected symbol error
        /// </summary>
        public static SyntaxError UnexpectedSymbol(int line, int col, char symbol, string? sourceLine = null)
        {
            return new SyntaxError(
                $"Unexpected symbol '{symbol}'",
                line, col, symbol.ToString(),
                SyntaxErrorCodes.UnexpectedSymbol,
                sourceLine
            );
        }

        /// <summary>
        /// Missing token error
        /// </summary>
        public static SyntaxError MissingToken(int line, int col, string expected, string? sourceLine = null)
        {
            return new SyntaxError(
                $"Expected '{expected}'",
                line, col, "",
                SyntaxErrorCodes.MissingToken,
                sourceLine
            );
        }

        /// <summary>
        /// Invalid token error
        /// </summary>
        public static SyntaxError InvalidToken(int line, int col, string token, string? sourceLine = null)
        {
            return new SyntaxError(
                $"Invalid token '{token}'",
                line, col, token,
                SyntaxErrorCodes.InvalidToken,
                sourceLine
            );
        }

        /// <summary>
        /// Unterminated string error
        /// </summary>
        public static SyntaxError UnterminatedString(int line, int col, string? sourceLine = null)
        {
            return new SyntaxError(
                "Unterminated string literal",
                line, col, "\"",
                SyntaxErrorCodes.UnterminatedString,
                sourceLine
            );
        }

        /// <summary>
        /// Invalid number format error
        /// </summary>
        public static SyntaxError InvalidNumber(int line, int col, string number, string? sourceLine = null)
        {
            return new SyntaxError(
                $"Invalid number format '{number}'",
                line, col, number,
                SyntaxErrorCodes.InvalidNumber,
                sourceLine
            );
        }

        /// <summary>
        /// Unterminated comment error
        /// </summary>
        public static SyntaxError UnterminatedComment(int line, int col, string? sourceLine = null)
        {
            return new SyntaxError(
                "Unterminated comment",
                line, col, "/*",
                SyntaxErrorCodes.UnterminatedComment,
                sourceLine
            );
        }

        /// <summary>
        /// Invalid escape sequence error
        /// </summary>
        public static SyntaxError InvalidEscapeSequence(int line, int col, char sequence, string? sourceLine = null)
        {
            return new SyntaxError(
                $"Invalid escape sequence '\\{sequence}'",
                line, col, $"\\{sequence}",
                SyntaxErrorCodes.InvalidEscape,
                sourceLine
            );
        }

        /// <summary>
        /// Unexpected end of file error
        /// </summary>
        public static SyntaxError UnexpectedEOF()
        {
            return new SyntaxError(
                "Unexpected end of file",
                0, 0, "EOF",
                SyntaxErrorCodes.UnexpectedEOF
            );
        }

        /// <summary>
        /// Get formatted error message with arrow pointing to error
        /// </summary>
        public string GetFormattedMessage()
        {
            if (string.IsNullOrEmpty(SourceLine))
                return Message;

            // Calculate arrow position safely
            int arrowPosition = Math.Min(Column - 1, SourceLine.Length - 1);
            arrowPosition = Math.Max(0, arrowPosition);
            
            string arrow = new string(' ', arrowPosition) + "^";
            
            return $@"
{Message}
{SourceLine}
{arrow}
at line {Line}, column {Column}";
        }

        /// <summary>
        /// Get formatted error message with source lines array
        /// </summary>
        public string GetFormattedMessage(string[] lines)
        {
            if (lines == null || lines.Length < Line)
                return Message;

            SourceLine = lines[Line - 1];
            return GetFormattedMessage();
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(SourceLine))
                return GetFormattedMessage();
            
            return $"[{ErrorCode}] at line {Line}, column {Column}: {Message}";
        }

        public static implicit operator string(SyntaxError error) => error.ToString();
    }
}
