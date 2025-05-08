using System;
using NUnit.Framework;
using System.Collections.Generic;
using static JackCompiling.Tests.JackCompilerHelper;

namespace JackCompiling.Tests
{
    [TestFixture]
    public class CodeWriter_P1_NumExpressions_Tests
    {
        [TestCase("42", ExpectedResult = 42)]
        [TestCase("-1", ExpectedResult = -1)]
        [TestCase("~0", ExpectedResult = -1)]
        [TestCase("~1234", ExpectedResult = ~1234)]
        [TestCase("2+2", ExpectedResult = 4)]
        [TestCase("123-100", ExpectedResult = 23)]
        [TestCase("1 + 2 + 3", ExpectedResult = 6)]
        [TestCase("(1)", ExpectedResult = 1)]
        [TestCase("(1+2)*3", ExpectedResult = 9)]
        [TestCase("3*(1+2)", ExpectedResult = 9)]
        [TestCase("(3+4)*(1+2)", ExpectedResult = 21)]
        [TestCase("false", ExpectedResult = 0)]
        [TestCase("true", ExpectedResult = -1)]
        [TestCase("true | false", ExpectedResult = -1)]
        [TestCase("false | false", ExpectedResult = 0)]
        [TestCase("false & true", ExpectedResult = 0)]
        [TestCase("true & true", ExpectedResult = -1)]
        [TestCase("~false", ExpectedResult = -1)]
        [TestCase("~true", ExpectedResult = 0)]
        [TestCase("2*3", ExpectedResult = 6)]
        [TestCase("10/2", ExpectedResult = 5)]
        [TestCase("10<1", ExpectedResult = 0)]
        [TestCase("10>1", ExpectedResult = -1)]
        [TestCase("(1+10)>10", ExpectedResult = -1)]
        [TestCase("(1+2+3) = 6", ExpectedResult = -1)]
        [TestCase("(1<2)*10", ExpectedResult = -10)]
        [TestCase("-1*5", ExpectedResult = -5)]
        [TestCase("5*-1", ExpectedResult = -5)]
        public int TestConstantExpression(string jackExpression)
        {
            var expressionSyntax = ParseExpression(jackExpression);
            return EvalExpression(null, null, expressionSyntax);
        }
        
        [TestCase("arg+1", ExpectedResult = 124)]
        [TestCase("x+2", ExpectedResult = 2325)]
        [TestCase("arg+arg", ExpectedResult = 246)]
        [TestCase("x+arg", ExpectedResult = 2446)]
        public int UseLocals(string jackExpression)
        {
            const short argValue = 123;
            const int argIndex = 2;
            const short localValue = 2323;
            const int localIndex = 1;
            var expressionSyntax = ParseExpression(jackExpression);
            Dictionary<string,VarInfo>? locals = new Dictionary<string, VarInfo>();
            locals.Add("arg", new VarInfo(argIndex, VarKind.Parameter, "int"));
            locals.Add("x", new VarInfo(localIndex, VarKind.Local, "int"));

            var codeWriter = new CodeWriter(null, locals);
            codeWriter.WriteExpression(expressionSyntax);
            
            var vmEmulator = VmEmulator.LoadTestCode(codeWriter.ResultVmCode);
            vmEmulator.StackFrame.Args[argIndex] = argValue;
            vmEmulator.StackFrame.Locals[localIndex] = localValue;
            int lineIndex = 0;
            vmEmulator.ExecuteFunctionBody(ref lineIndex);
            return vmEmulator.StackFrame.Stack.Pop();
        }
        [TestCase("instanceField+2", ExpectedResult = 102)]
        [TestCase("staticField+3", ExpectedResult = 203)]
        [TestCase("instanceField+staticField", ExpectedResult = 300)]
        public int UseFields(string jackExpression)
        {
            const short instanceFieldValue = 100;
            const short staticFieldValue = 200;
            const int instanceFieldIndex = 1;
            const int staticFieldIndex = 5;
            var expressionSyntax = ParseExpression(jackExpression);
            var classSymbols = new Dictionary<string, VarInfo>();
            classSymbols.Add("staticField", new VarInfo(staticFieldIndex, VarKind.Static, "int"));
            classSymbols.Add("instanceField", new VarInfo(instanceFieldIndex, VarKind.Field, "int"));

            var codeWriter = new CodeWriter(classSymbols);
            codeWriter.WriteExpression(expressionSyntax);
            
            var vmEmulator = VmEmulator.LoadTestCode(codeWriter.ResultVmCode);
            vmEmulator.Heap[vmEmulator.StackFrame.ThisAddress+instanceFieldIndex] = instanceFieldValue;
            vmEmulator.Statics[staticFieldIndex] = staticFieldValue;
            int lineIndex = 0;
            vmEmulator.ExecuteFunctionBody(ref lineIndex);
            return vmEmulator.StackFrame.Stack.Pop();
        }

        /// <summary>
        /// В первой задаче не нужно реализовывать генерацию кода для массивов, строк, вызовов функций и т.п.
        /// однако и TryWriteNumericTerm на таких нереализованных синтаксических конструкциях должен возвращать false, 
        /// чтобы их обработка досталась другим функциям, которые вы реализуете в следующих задачах
        /// </summary>
        [TestCase("staticField[0]", "pop pointer 1")]
        [TestCase("f(1)", "call Main.f 2")]
        [TestCase("null", "push constant 0")]
        [TestCase("this", "push pointer 0")]
        [TestCase("Main.f(1)", "call Main.f 1")]
        [TestCase("\"hello\"", "call String.new 1")]
        public void DoNotGenerateCodeForUnknownTermsOrGenerateRightCode(string code, string expectedInstruction)
        {
            var expressionSyntax = ParseExpression(code);
            var classSymbols = new Dictionary<string, VarInfo>();
            classSymbols.Add("staticField", new VarInfo(0, VarKind.Static, "int"));
            var codeWriter = new CodeWriter(classSymbols);
            try
            {
                codeWriter.WriteExpression(expressionSyntax);
            }
            catch (Exception)
            {
                return;
            }
            Assert.That(codeWriter.ResultVmCode, Contains.Item(expectedInstruction));
        }
    }
}
