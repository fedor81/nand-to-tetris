using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace JackCompiling.Tests
{
    [TestFixture]
    public class ParserTests
    {
        [TestCase("class X{}", "class[class X { }]")]
        [TestCase("class X{ field int a; }", "class[class X { classVarDec[field int a ;] }]")]
        [TestCase("class X{ static int b; }", "class[class X { classVarDec[static int b ;] }]")]
        [TestCase("class X{ field int f1, f2; static SomeType s1, s2; }", "class[class X { classVarDec[field int f1 , f2 ;] classVarDec[static SomeType s1 , s2 ;] }]")]
        [TestCase("class X{ method int m(){} }",
            "class[class X { subroutineDec[method int m ( parameterList[] ) subroutineBody[{ statements[] }]] }]")]
        [TestCase("class X{ method int m1(){} function SomeType m2(){} constructor X new(){}}",
            "class[class X { " +
            "subroutineDec[method int m1 ( parameterList[] ) subroutineBody[{ statements[] }]] " +
            "subroutineDec[function SomeType m2 ( parameterList[] ) subroutineBody[{ statements[] }]] " +
            "subroutineDec[constructor X new ( parameterList[] ) subroutineBody[{ statements[] }]] " +
            "}]")]
        [TestCase("class X{ method int m(){ var int a;} }",
            "class[class X { subroutineDec[method int m ( parameterList[] ) subroutineBody[{ varDec[var int a ;] statements[] }]] }]")]
        [TestCase("class X{ method int m(){ var boolean v1, v2; var X x;} }",
            "class[class X { subroutineDec[method int m ( parameterList[] ) " +
            "subroutineBody[{ varDec[var boolean v1 , v2 ;] varDec[var X x ;] statements[] }]] }]")]
        public void ReadClassDecl(string text, string expectedTree)
        {
            AssertParserResult(text, p => p.ReadClass(), expectedTree);
        }

        [TestCase("qweqwe Name { }")]
        [TestCase("; Name { }")]
        [TestCase("class Name ; }")]
        [TestCase("class Name { ;")]
        [TestCase("class 42 { }")]
        [TestCase("class Name")]
        [TestCase("class Name { + int x; }")]
        [TestCase("class Name { while int x; }")]
        [TestCase("class Name { 42 int x; }")]
        [TestCase("class Name { var int x; }")]
        [TestCase("class Name { field 42 x; }")]
        [TestCase("class Name { field + x; }")]
        [TestCase("class Name { field int \"x\"; }")]
        [TestCase("class Name { field int while; }")]
        [TestCase("class Name { field int x + }")]
        [TestCase("class Name { function 42 f(){} }")]
        [TestCase("class Name { function int 42(){} }")]
        [TestCase("class Name { function int f[){} }")]
        [TestCase("class Name { function int f(]{} }")]
        [TestCase("class Name { function int f()[} }")]
        [TestCase("class Name { function int f(){] }")]
        public void FailsOfIncorrectClassDecl(string text)
        {
            Assert.Throws<ExpectedException>(() => ParseToCompactSyntaxTree(text, p => p.ReadClass()));
        }

        [TestCase(")", "parameterList[]")]
        [TestCase("int x)", "parameterList[int x]")]
        [TestCase("int x, MyClass y)", "parameterList[int x , MyClass y]")]
        [TestCase("int x, char y, Array z)", "parameterList[int x , char y , Array z]")]
        public void ReadParametersList(string text, string expectedTree)
        {
            AssertParserResult(text, p => p.ReadParameterList(), expectedTree);
        }

        [TestCase("]")]
        [TestCase("+")]
        [TestCase("while x)")]
        [TestCase("int)")]
        [TestCase("42 x)")]
        [TestCase("int x; MyClass y)")]
        [TestCase("int x, MyClass y]")]
        [TestCase("int x, 42 y)")]
        [TestCase("int x, 42)")]
        public void FailOnIncorrectParametersList(string text)
        {
            Assert.Throws<ExpectedException>(() => ParseToCompactSyntaxTree(text, p => p.ReadParameterList()));
        }

        [TestCase("", "statements[]")]
        [TestCase("}", "statements[]")]
        [TestCase("return;", "statements[returnStatement[return ;]]")]
        public void ReadTrivialStatements(string text, string expectedTree)
        {
            AssertParserResult(text, p => p.ReadStatements(), expectedTree);
        }

        [TestCase("12;", "term[12]")]
        [TestCase("true;", "term[true]")]
        [TestCase("\"int true\";", "term[int true]")]
        [TestCase("x", "term[x]")]
        [TestCase("-42", "term[- term[42]]")]
        [TestCase("f()", "term[f ( expressionList[] )]")]
        [TestCase("C.m()", "term[C . m ( expressionList[] )]")]
        public void ReadTermWithoutExpressions(string termCode, string expectedTree)
        {
            AssertParserResult(termCode, p => p.ReadTerm(), expectedTree);
        }

        [TestCase("f(a)", "term[f ( expressionList[expression[term[a]]] )]")]
        [TestCase("C.m(a)", "term[C . m ( expressionList[expression[term[a]]] )]")]
        [TestCase("(42)", "term[( expression[term[42]] )]")]
        [TestCase("~(C.m())", "term[~ term[( expression[term[C . m ( expressionList[] )]] )]]")]
        [TestCase("x[1]", "term[x [ expression[term[1]] ]]")]
        [TestCase("x[y[1]]", "term[x [ expression[term[y [ expression[term[1]] ]]] ]]")]
        [TestCase("(1+2)", "term[( expression[term[1] + term[2]] )]")]
        [TestCase("(1+2*3)", "term[( expression[term[1] + term[2] * term[3]] )]")]
        [TestCase("(-1+~2)", "term[( expression[term[- term[1]] + term[~ term[2]]] )]")]
        [TestCase("~(C.m(a,b,c,d))",
            "term[~ term[( expression[term[C . m ( expressionList[expression[term[a]] , expression[term[b]] , expression[term[c]] , expression[term[d]]] )]] )]]")]
        public void ReadTermWithExpressions(string termCode, string expectedTree)
        {
            AssertParserResult(termCode, p => p.ReadTerm(), expectedTree);
        }


        [TestCase("f()", "f ( expressionList[] )")]
        [TestCase("f(a)", "f ( expressionList[expression[term[a]]] )")]
        [TestCase("M.f(a)", "M . f ( expressionList[expression[term[a]]] )")]
        [TestCase("M.f(a, b)", "M . f ( expressionList[expression[term[a]] , expression[term[b]]] )")]
        public void ReadSubroutineCall(string text, string expectedTree)
        {
            AssertParserResult(text, p => p.ReadSubroutineCall(), expectedTree);
        }

        [TestCase("return;", "statements[returnStatement[return ;]]")]
        [TestCase("return 42;", "statements[returnStatement[return expression[term[42]] ;]]")]
        [TestCase("return a+b;", "statements[returnStatement[return expression[term[a] + term[b]] ;]]")]
        [TestCase("return a[b];", "statements[returnStatement[return expression[term[a [ expression[term[b]] ]]] ;]]")]
        [TestCase("let x = 2;", "statements[letStatement[let x = expression[term[2]] ;]]")]
        [TestCase("let x = a+b;", "statements[letStatement[let x = expression[term[a] + term[b]] ;]]")]
        [TestCase("let x[1] = 2;", "statements[letStatement[let x [ expression[term[1]] ] = expression[term[2]] ;]]")]
        [TestCase("let x = 2; let y = 123;", "statements[letStatement[let x = expression[term[2]] ;] letStatement[let y = expression[term[123]] ;]]")]
        [TestCase("do f();", "statements[doStatement[do f ( expressionList[] ) ;]]")]
        [TestCase("do M.f();", "statements[doStatement[do M . f ( expressionList[] ) ;]]")]
        [TestCase("do func(a, b+c);", "statements[doStatement[do func ( expressionList[expression[term[a]] , expression[term[b] + term[c]]] ) ;]]")]
        [TestCase("do func(a[b+c]);", "statements[doStatement[do func ( expressionList[expression[term[a [ expression[term[b] + term[c]] ]]]] ) ;]]")]
        [TestCase("do Memory.deAlloc(this);", "statements[doStatement[do Memory . deAlloc ( expressionList[expression[term[this]]] ) ;]]")]
        [TestCase("if (true) { }", "statements[ifStatement[if ( expression[term[true]] ) { statements[] }]]")]
        [TestCase("if (true) { } else { }",
            "statements[ifStatement[if ( expression[term[true]] ) { statements[] } else { statements[] }]]")]
        [TestCase("if (a & b) { return 1; } else { do f(); return 2; }",
            "statements[" +
            "ifStatement[if ( expression[term[a] & term[b]] ) { " +
            "statements[returnStatement[return expression[term[1]] ;]] } " +
            "else { " +
            "statements[doStatement[do f ( expressionList[] ) ;] returnStatement[return expression[term[2]] ;]] }]]")]
        [TestCase("while (a & b) { }", "statements[whileStatement[while ( expression[term[a] & term[b]] ) { statements[] }]]")]
        [TestCase("while (true) { do f(); do g(); }",
            "statements[whileStatement[while ( expression[term[true]] ) { " +
            "statements[doStatement[do f ( expressionList[] ) ;] doStatement[do g ( expressionList[] ) ;]] }]]")]
        [TestCase("x = 42;\narray[1] = null;\nf();\nM.f();\nobj.method();", "statements[" +
                "letStatement[let x = expression[term[42]] ;] " +
                "letStatement[let array [ expression[term[1]] ] = expression[term[null]] ;] " +
                "letStatement[let f ( expressionList[] ) ;] " +
                "letStatement[let M . f ( expressionList[] ) ;] " +
                "letStatement[let obj . method ( expressionList[] ) ;]" +
                "]")]
        public void ReadStatements(string text, string expectedTree)
        {
            AssertParserResult(text, p => p.ReadStatements(), expectedTree);
        }

        [TestCase("return")]
        [TestCase("return 1 2")]
        [TestCase("return 1 2;")]
        [TestCase("return +")]
        [TestCase("return a[1);")]
        [TestCase("return a(1];")]
        [TestCase("let;")]
        [TestCase("let")]
        [TestCase("let a")]
        [TestCase("let a;")]
        [TestCase("let a =")]
        [TestCase("let a =;")]
        [TestCase("let a = 42")]
        [TestCase("let a = 42 +")]
        [TestCase("let 42 = 42;")]
        [TestCase("let a + 42;")]
        [TestCase("let while = 42;")]
        [TestCase("do")]
        [TestCase("do;")]
        [TestCase("do f")]
        [TestCase("do f;")]
        [TestCase("do f(;")]
        [TestCase("do f()")]
        [TestCase("do 42();")]
        [TestCase("do while();")]
        [TestCase("if")]
        [TestCase("if a")]
        [TestCase("if (")]
        [TestCase("if (a")]
        [TestCase("if (a)")]
        [TestCase("if (a){")]
        [TestCase("if (){ }")]
        [TestCase("if [a){ }")]
        [TestCase("if (a]{ }")]
        [TestCase("if (a)[ }")]
        [TestCase("if (a){ ]")]
        [TestCase("if (a){ }else")]
        [TestCase("if (a){ }else{")]
        [TestCase("if (a){ }else{]")]
        [TestCase("if (a){ }else[}")]
        [TestCase("while")]
        [TestCase("while (")]
        [TestCase("while (a")]
        [TestCase("while (a)")]
        [TestCase("while (a){")]
        [TestCase("while (){}")]
        [TestCase("while [a){}")]
        [TestCase("while (a]{}")]
        [TestCase("while (a)[}")]
        [TestCase("while (a){]")]
        public void FailOnIncorrectStatements(string text)
        {
            Assert.Throws<ExpectedException>(() => ParseToCompactSyntaxTree(text, p => p.ReadStatements()));
        }


        [TestCase("Tests/01-ExpressionLessSquare/Main.jack")]
        [TestCase("Tests/01-ExpressionLessSquare/Square.jack")]
        [TestCase("Tests/01-ExpressionLessSquare/SquareGame.jack")]
        [TestCase("Tests/02-ArrayTest/Main.jack")]
        [TestCase("Tests/03-Square/Main.jack")]
        [TestCase("Tests/03-Square/Square.jack")]
        [TestCase("Tests/03-Square/SquareGame.jack")]
        public void RealTest(string jackFile)
        {
            var text = File.ReadAllText(jackFile);
            var expectedFilename = Path.ChangeExtension(jackFile, ".xml");
            var tokenizer = new Tokenizer(text);
            var parser = new Parser(tokenizer);
            var classSyntax = parser.ReadClass();
            var writer = new XmlSyntaxWriter();
            writer.Write(classSyntax);
            Console.WriteLine(string.Join("\n", writer.GetResult()));
            Assert.AreEqual(File.ReadAllLines(expectedFilename), writer.GetResult());
        }

        private void AssertParserResult<TRes>(string text, Func<Parser, TRes> parse, string expectedTree)
        {
            var compactSyntax = ParseToCompactSyntaxTree(text, parse);
            Assert.AreEqual(expectedTree, compactSyntax.Single());
        }

        private static IReadOnlyList<string> ParseToCompactSyntaxTree<TRes>(string text, Func<Parser, TRes> parse)
        {
            var tokenizer = new Tokenizer(text);
            var parser = new Parser(tokenizer);
            var treeNode = parse(parser);
            var writer = new CompactSyntaxWriter();
            writer.Write(treeNode);
            Console.WriteLine(string.Join("\n", writer.GetResult()));
            return writer.GetResult();
        }
    }
}
