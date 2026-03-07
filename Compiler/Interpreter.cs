using Fminusminus.Utils.Package;

private void ExecuteStatement(StatementNode statement)
{
    switch (statement)
    {
        case PrintlnStatementNode println:
            if (println.Expression is StringLiteralNode str)
            {
                if (str.IsInterpolated)
                    Computer.PrintLn(str.Value, _variables);
                else
                    Computer.PrintLn(str.Value);
            }
            else if (println.Expression is VariableNode var)
            {
                var value = Computer.GetVariable(var.Name)?.ToString() ?? "";
                Computer.PrintLn(value);
            }
            break;
            
        case PrintStatementNode print:
            if (print.Expression is StringLiteralNode str)
            {
                Computer.Print(str.Value);
            }
            break;
            
        case ReturnStatementNode ret:
            // Handled by caller
            break;
            
        case AssignmentNode assign:
            if (assign.Value is StringLiteralNode strVal)
            {
                Computer.SetVariable(assign.VariableName, strVal.Value);
            }
            else if (assign.Value is NumberLiteralNode numVal)
            {
                Computer.SetVariable(assign.VariableName, numVal.Value);
            }
            break;
            
        case ComputerCallNode computer:
            ExecuteComputerCall(computer);
            break;
            
        case AtBlockNode atBlock:
            ExecuteAtBlock(atBlock);
            break;
    }
}

private void ExecuteComputerCall(ComputerCallNode computer)
{
    switch (computer.Method)
    {
        case "GetInfo":
            var result = Computer.GetInfo();
            Computer.PrintLn(result);
            break;
            
        case "GetOSPath":
            var path = Computer.GetOSPath();
            Computer.SetVariable("OS_PATH", path);
            break;
            
        case "CreateFile":
            if (computer.Arguments.Count > 0 && 
                computer.Arguments[0] is StringLiteralNode filename)
            {
                string path = computer.Arguments.Count > 1 && 
                             computer.Arguments[1] is StringLiteralNode pathNode 
                             ? pathNode.Value : null;
                ComputerIO.CreateFile(filename.Value, path);
            }
            break;
            
        case "ListFiles":
            string dir = computer.Arguments.Count > 0 && 
                        computer.Arguments[0] is StringLiteralNode dirNode 
                        ? dirNode.Value : ".";
            ComputerIO.ListFiles(dir);
            break;
            
        case "BeginWrite":
            if (computer.Arguments.Count > 0 && 
                computer.Arguments[0] is StringLiteralNode fileNode)
            {
                ComputerIO.BeginWrite(fileNode.Value);
            }
            break;
            
        case "WriteLine":
            if (computer.Arguments.Count > 0 && 
                computer.Arguments[0] is StringLiteralNode contentNode)
            {
                ComputerIO.WriteLine(contentNode.Value);
            }
            break;
            
        case "EndWrite":
            ComputerIO.EndWrite();
            break;
    }
}
