using System.Collections.Generic;

namespace Fminusminus
{
    /// <summary>
    /// IO list file node
    /// </summary>
    public class IOListNode : StatementNode
    {
        public ExpressionNode Path { get; set; }
        public bool UseOSPath { get; set; }
        
        public override void Print(int indent = 0)
        {
            string pathInfo = UseOSPath ? "OS.path" : Path?.ToString() ?? ".";
            Console.WriteLine($"{new string(' ', indent)}IO.LISTFILE({pathInfo})");
        }
    }
}
