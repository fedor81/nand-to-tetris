using System.Collections.Generic;

namespace JackCompiling.Tests
{
    public class JackCompilerHelper
    {
        public static StatementsSyntax ParseStatements(string jackCode)
        {
            return new Parser(new Tokenizer(jackCode)).ReadStatements();
        }
        
        public static ExpressionSyntax ParseExpression(string jackCode)
        {
            return new Parser(new Tokenizer(jackCode)).ReadExpression();
        }

        public static short EvalExpression(Dictionary<string, VarInfo>? classSymbols, Dictionary<string, VarInfo>? locals,
            ExpressionSyntax expressionSyntax)
        {
            var codeWriter = new CodeWriter(classSymbols, locals);
            codeWriter.WriteExpression(expressionSyntax);
            var vmEmulator = VmEmulator.LoadTestCode(codeWriter.ResultVmCode);
            int lineIndex = 0;
            vmEmulator.ExecuteFunctionBody(ref lineIndex);
            return vmEmulator.StackFrame.Stack.Pop();
        }

        public static VmEmulator RunStatements(Dictionary<string, VarInfo>? classSymbols, Dictionary<string, VarInfo>? locals,
            StatementsSyntax statementsSyntax)
        {
            var codeWriter = new CodeWriter(classSymbols, locals);
            codeWriter.WriteStatements(statementsSyntax);
            var vmEmulator = new VmEmulator();
            vmEmulator.StackFrame = new StackFrame(new short[5], new short[5], 100, 0, null);
            vmEmulator.Load(codeWriter.ResultVmCode);
            int lineIndex = 0;
            vmEmulator.ExecuteFunctionBody(ref lineIndex);
            return vmEmulator;
        }
    }
}
