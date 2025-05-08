using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace JackCompiling.Tests
{
    public class CodeWriter_P4_ArraysAndStrings_Tests
    {

        [TestCase("class Main { function int main(){ return true + false + null; } }",
            TestName = "010 Constants",
            ExpectedResult = -1)]
        [TestCase("class Main { " +
                  "function int main(){ var String s; let s = \"Hello\"; return s.length(); } }",
            TestName = "150 Return string length",
            ExpectedResult = 5)]
        [TestCase("class Main { " +
                  "function int main(){ var String s; let s = \"Hello\"; return s.charAt(1); } }",
            TestName = "151 Return string char",
            ExpectedResult = 101)]
        [TestCase("class Main { " +
                  "function int main(){ var String s; let s = \"\"; do s.appendChar(42); return s.charAt(s.length()-1); } }",
            TestName = "152 Return string char 2",
            ExpectedResult = 42)]
        [TestCase("class Main { " +
                  "function int main(){ " +
                  "var Array a; " +
                  "let a = Array.new(5); " +
                  "return a[0]; } }",
            TestName = "160 read arr[i]",
            ExpectedResult = 0)]
        [TestCase("class Main { " +
            "field int x; " +
            "field int y; " +
            "method void setY(int yy){ let y=yy; return; } " +
            "function int main(){ " +
                "var Array a; " +
                "var Main m; " +
                "let a = Array.new(5); " +
                "let m = a; " +
                "do m.setY(42); " +
            "return a[1]; } }",
            TestName = "161 read arr[i]",
            ExpectedResult = 42)]
        [TestCase("class Main { " +
                  "function int main(){ " +
                  "var Array a; " +
                  "let a = Array.new(5); " +
                  "let a[1]=42; " +
                  "return a[1]; } }",
            TestName = "162 read arr[i] after write to arr[i]",
            ExpectedResult = 42)]
        [TestCase("class Main { " +
                  "function int main(){ " +
                  "var Array a; " +
                  "let a = Array.new(5); " +
                  "let a[1]=42; " +
                  "let a[2] = a[1]+1; " +
                  "return a[2]; } }",
            TestName = "170 arr[2] = arr[1]+1",
            ExpectedResult = 43)]
        [TestCase("class Main { " +
                  "function int main(){ " +
                  "var Array a; " +
                  "let a = Array.new(5); " +
                  "let a[2]=2; " +
                  "let a[2] = a[2] * a[2]; " +
                  "return a[2]; } }",
            TestName = "175 arr[2] = arr[2]*arr[2]",
            ExpectedResult = 4)]
        public short CompileAndRunMain(params string[] jackModules)
        {
            var vmCode = Compile(jackModules);
            var vmEmulator = new VmEmulator();
            vmEmulator.Load(vmCode);
            vmEmulator.CallMain();
            return vmEmulator.StackFrame.Stack.Peek();
        }

        [TestCase("01-ExpressionLessSquare")]
        [TestCase("02-ArrayTest")]
        [TestCase("03-Square")]
        public void CompileRealSamples(string directory)
        {
            foreach (var jackFile in Directory.GetFiles(Path.Combine("Tests", directory), "*.jack"))
            {
                Console.WriteLine(jackFile);
                var jackCode = File.ReadAllText(jackFile);
                var vmCode = Compile(jackCode);
                File.WriteAllLines(Path.ChangeExtension(jackFile, ".vm"), vmCode);
            }
        }

        private IReadOnlyList<string> Compile(params string[] jackCode)
        {
            var cw = new CodeWriter();
            foreach (var code in jackCode)
            {
                var tokenizer = new Tokenizer(code);
                var parser = new Parser(tokenizer);
                var classNode = parser.ReadClass();
                cw.WriteClass(classNode);
                Console.WriteLine(cw.GetResult());
            }
            return cw.ResultVmCode;
        }
    }
}
