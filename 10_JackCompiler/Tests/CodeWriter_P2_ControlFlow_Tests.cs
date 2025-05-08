using System.Collections.Generic;
using NUnit.Framework;
using static JackCompiling.Tests.JackCompilerHelper;

namespace JackCompiling.Tests
{
    public class CodeWriter_P2_ControlFlow_Tests
    {
        [TestCase("", 0)]
        [TestCase("let x=1;", 1)]
        [TestCase("let x=42;", 42)]
        [TestCase("let x=1+(2*3);", 7)]
        [TestCase("let arg=123; let x=arg;", 123)]
        [TestCase("let staticField=123; let x=staticField;", 123)]
        [TestCase("let instanceField=123; let x=instanceField;", 123)]
        [TestCase("if(true){let x=1;}", 1)]
        [TestCase("if(false){let x=1;}", 0)]
        [TestCase("if(false){let x=1;}else{let x=2;}", 2)]
        [TestCase("if(true){let x=1;}else{let x=2;}", 1)]
        [TestCase("if(true){let x = 42; let x=1; } else { let x = 42; let x=2;}", 1)]
        [TestCase("if(true | false){let x = 42; }", 42)]
        [TestCase("if(true & false){let x = 42; }", 0)]
        [TestCase("if(x > -1){let x = 42; }", 42)]
        [TestCase("if(x > 1){let x = 42; }", 0)]
        [TestCase("while(false) {let x=2;}", 0)]
        [TestCase("while(x < 1) {let x=x+1;}", 1)]
        [TestCase("while(x < 12) {let x=x+1;}", 12)]
        [TestCase("while(x < 12) {let x=x+1;} while(x > 10) {let x=x-1;}", 10)]
        [TestCase("while(y < 10) { let z = 0; while (z < 11){ let z = z + 1; let x = x + 1; } let y=y+1;}", 110)]
        [TestCase("if(false){let x=1;}else{let x=2;} if(true){let x=x+10;}else{let x=x+20;}", 12)]
        [TestCase("if(false){let x=1;}else{ if(true){let x=2;}else{let x=3;} }", 2)]
        public void EvalStatements(string jackCode, int expectedX)
        {
            var statementsSyntax = ParseStatements(jackCode);
            var classSymbols = new Dictionary<string, VarInfo>();
            classSymbols.Add("instanceField", new VarInfo(0, VarKind.Field, "int"));
            classSymbols.Add("staticField", new VarInfo(1, VarKind.Static, "int"));
            var locals = new Dictionary<string, VarInfo>();
            locals.Add("x", new VarInfo(2, VarKind.Local, "int"));
            locals.Add("y", new VarInfo(3, VarKind.Local, "int"));
            locals.Add("z", new VarInfo(4, VarKind.Local, "int"));
            locals.Add("arg", new VarInfo(3, VarKind.Parameter, "int"));
            var vmEmulator = RunStatements(classSymbols, locals, statementsSyntax);
            var actualX = vmEmulator.StackFrame.Locals[2];
            Assert.AreEqual(expectedX, actualX);
        }
        
        [Test]
        public void LetStatement_AssignsToArg()
        {
            var statementsSyntax = ParseStatements("let arg=123;");
            var locals = new Dictionary<string, VarInfo>();
            locals.Add("arg", new VarInfo(3, VarKind.Parameter, "int"));
            var vmEmulator = RunStatements(null, locals, statementsSyntax);
            Assert.AreEqual(123, vmEmulator.StackFrame.Args[3]);
        }

        [Test]
        public void LetStatement_AssignsToField()
        {
            var statementsSyntax = ParseStatements("let instanceField=124;");
            var classVars = new Dictionary<string, VarInfo>();
            classVars.Add("instanceField", new VarInfo(2, VarKind.Field, "int"));
            var vmEmulator = RunStatements(classVars, null, statementsSyntax);
            Assert.AreEqual(124, vmEmulator.Heap[vmEmulator.StackFrame.ThisAddress + 2]);
        }

        [Test]
        public void LetStatement_AssignsToStatic()
        {
            var statementsSyntax = ParseStatements("let staticField=125;");
            var classVars = new Dictionary<string, VarInfo>();
            classVars.Add("staticField", new VarInfo(3, VarKind.Static, "int"));
            var vmEmulator = RunStatements(classVars, null, statementsSyntax);
            Assert.AreEqual(125, vmEmulator.Statics[3]);
        }
    }
}
