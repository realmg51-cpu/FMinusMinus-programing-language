using System.Collections.Generic;

namespace Fminusminus
{
    /// <summary>
    /// Computer system information node
    /// </summary>
    public class ComputerStatementNode : StatementNode
    {
        public string Property { get; set; }      // systeminfo, version, etc
        public string Operation { get; set; }      // get, set, etc
        public List<ExpressionNode> Parameters { get; set; } = new();

        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}COMPUTER.{Property}({Operation})");
            foreach (var param in Parameters)
                param.Print(indent + 2);
        }
    }
}
