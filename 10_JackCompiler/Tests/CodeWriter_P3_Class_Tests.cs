using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using static JackCompiling.Tests.JackCompilerHelper;

namespace JackCompiling.Tests
{
    public class CodeWriter_P3_Class_Tests
    {
        [TestCase("class Main { function int main(){ return 42; } }",
            TestName = "010 Trivial",
            ExpectedResult = 42)]
        [TestCase("class Main { function int main(){ var int x; let x = 7; return x; } }", 
            TestName="020 Return local variable",
            ExpectedResult = 7)]
        [TestCase("class Main { " +
                  "function int main(){ " +
                  "  var int x, r; " +
                  "  let x = 7; " +
                  "  let r = 0; " +
                  "  if (x > 0) { let r = 1; } " +
                  "  return r; } }",
            TestName = "030 If",
            ExpectedResult = 1)]
        [TestCase("class Main { " +
                  "function int main(){ " +
                  "var int x, r; " +
                  "let x = 7; " +
                  "let r = 0; " +
                  "if (x < 0) { let r = 1; } " +
                  "else { let r = 2; } " +
                  "return r; } }",
            TestName = "040 If-Else",
            ExpectedResult = 2)]
        [TestCase("class Main { " +
                  "function int main(){ " +
                  "var int x, r; " +
                  "let x = 3; " +
                  "let r = 0; " +
                  "while (x > 0) { " +
                  "     let r = r+x; " +
                  "     let x = x-1; } " +
                  "return r; } }",
            TestName = "050 While",
            ExpectedResult = 6)]
        [TestCase("class Main { " +
                  "function int f(){ return 42;} " +
                  "function int main(){ return Main.f(); } }",
            TestName = "060 Call function from the same class",
            ExpectedResult = 42)]
        [TestCase(
            "class A { " +
            "function int f(){ return 42;} }", 
            "class Main { " +
            "function int main(){ return A.f(); } }",
            TestName = "070 Call function from another class",
            ExpectedResult = 42)]
        [TestCase("class Main { " +
                  "function int f(int x){ return x;} " +
                  "function int main(){ return Main.f(42); } }",
            TestName = "080 Call function with argument",
            ExpectedResult = 42)]
        [TestCase("class Main { " +
                  "function int f(int x, int y){ return x-y;} " +
                  "function int main(){ return Main.f(5, 3); } }",
            TestName = "090 Call function with many arguments",
            ExpectedResult = 2)]
        [TestCase("class Main { " +
                  "constructor Main new() {return this;} " +
                  "method int m(){ return 142; } " +
                  "function int main(){ " +
                  "var Main obj; " +
                  "let obj = Main.new(); " +
                  "return obj.m(); } }",
            TestName = "100 Call object method",
            ExpectedResult = 142)]
        [TestCase("class Main { " +
                  "field int x; " +
                  "constructor Main new(int ax) {let x = ax; return this;} " +
                  "method int m(){ return x; } " +
                  "function int main(){ " +
                  "  var Main obj; " +
                  "  let obj = Main.new(43); " +
                  "  return obj.m(); } }",
            TestName = "110 Return field value",
            ExpectedResult = 43)]
        [TestCase("class Main { " +
                  "field int f;" +
                  "constructor Main new(int af) { let f = af; return this;} " +
                  "method int m(){ let f = f + 1; return f; } " +
                  "method int get(){ return f; } " +
                  "function int main(){ " +
                  "  var Main obj; " +
                  "  let obj = Main.new(41); " +
                  "  do obj.m(); " +
                  "  return obj.get(); } }",
            TestName = "115 Do statement",
            ExpectedResult = 42)]
        [TestCase("class Main { " +
                  "static int x; " +
                  "constructor Main new(int ax) {let x = x + ax; return this;} " +
                  "method int m(){ return x; } " +
                  "function int main(){ " +
                  "  var Main obj; " +
                  "  let obj = Main.new(1); " +
                  "  let obj = Main.new(42); " +
                  "  return obj.m(); } }",
            TestName = "120 Use static",
            ExpectedResult = 43)]
        [TestCase("class Main { " +
                  "constructor Main new() {return this;} " +
                  "method int m(int x){ return x+1; } " +
                  "function int main(){ " +
                  "  var Main obj; " +
                  "  let obj = Main.new(); " +
                  "  return obj.m(40); } }",
            TestName = "130 Call method with argument",
            ExpectedResult = 41)]
        [TestCase(
            "class X { " +
            "   field int x; " +
            "   constructor X new(int ax) {let x = ax; return this;} " +
            "   method int m(){ return 2+m2(); } " +
            "   method int m2(){ return x; } }", 
            "class Main { " +
            "   function int main(){ var X obj; let obj = X.new(42); return obj.m(); } }",
            TestName = "140 Create another object and call method from method",
            ExpectedResult = 44)]
        
        
        [TestCase("class Main { " +
                  "function int main(){ return (-1 + (-2)) - 2; } }",
            TestName = "200 Expression: Simple Arithmetic",
            ExpectedResult = -5)]
        [TestCase("class Main { " +
                  "function int main(){ return (-(-2 * 4)) + (-2 * (-2)) + (2*1) + (1*1); } }",
            TestName = "210 Expression: multiplication",
            ExpectedResult = 15)]
        [TestCase("class Main { " +
                  "function int main(){ return 12/2 + (5/3) + (4/3) + (2/100) + (-5/1); } }",
            TestName = "220 Expression: division",
            ExpectedResult = 3)]
        [TestCase("class Main { " +
                  "function int main(){ return 1<10 + ((1>10) * 2) + ((1=1)*4) + ((~0)*8); } }",
            TestName = "230 Expression: logic (> < = ~)",
            ExpectedResult = -1 - 4 - 8)]
        [TestCase("class Main { function int main(){ " +
                  "return (true | false) + ((true | true)*2) + ((false | false)*4) + ((false | true)*8) " +
                  "+ ((true & false)*16) + ((false & true)*32) + ((true & true)*64) + ((false & false)*128); } }",
            TestName = "230 Expression: logic (& |)",
            ExpectedResult = -(1+2+8+64))]
        public short CompileAndRunMain(params string[] jackModules)
        {
            var vmCode = Compile(jackModules);
            var vmEmulator = new VmEmulator();
            vmEmulator.Load(vmCode);
            vmEmulator.CallMain();
            return vmEmulator.StackFrame.Stack.Peek();
        }

        [Test]
        public void DoStatement()
        {
            var statementsSyntax = ParseStatements("do Math.abs(1);");
            var vmEmulator = RunStatements(null, null, statementsSyntax);
            Assert.AreEqual(0, vmEmulator.StackFrame.Stack.Count, 
                "stack.Size != 0. Stack should be empty after do statement execution");
        }

        [TestCase("01-ExpressionLessSquare")]
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
