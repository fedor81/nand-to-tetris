using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using static JackCompiling.TokenType;

namespace JackCompiling.Tests
{
    [TestFixture]
    public class TokenizerTests
    {
        [TestCase("{ ", Symbol, "{")]
        [TestCase("/", Symbol, "/")]
        [TestCase(" /", Symbol, "/")]
        [TestCase("\t/", Symbol, "/")]
        [TestCase("\n/", Symbol, "/")]
        [TestCase("\r/", Symbol, "/")]
        [TestCase("\t\r\n /", Symbol, "/")]
        [TestCase("1 ", IntegerConstant, "1")]
        [TestCase("0 ", IntegerConstant, "0")]
        [TestCase("123", IntegerConstant, "123")]
        [TestCase("123 ", IntegerConstant, "123")]
        [TestCase("\"1\"", StringConstant, "1")]
        [TestCase("\"\" ", StringConstant, "")]
        [TestCase("\" \" ", StringConstant, " ")]
        [TestCase("\"123123\" ", StringConstant, "123123")]
        [TestCase("function", Keyword, "function")]
        [TestCase("var ", Keyword, "var")]
        [TestCase("varName ", Identifier, "varName")]
        [TestCase("id", Identifier, "id")]
        [TestCase("i0_", Identifier, "i0_")]
        [TestCase("i0_0 ", Identifier, "i0_0")]
        public void ReadSingleToken(string text, TokenType expectedType, string expectedValue)
        {
            var tokenizer = new Tokenizer(text);
            var token = tokenizer.TryReadNext()!;
            Assert.AreEqual(expectedType, token.TokenType);
            Assert.AreEqual(expectedValue, token.Value);
        }

        [TestCase("1 > 2", new[] { IntegerConstant, Symbol, IntegerConstant })]
        [TestCase("\"1\"=\"2\"", new[] { StringConstant, Symbol, StringConstant })]
        [TestCase("let abc = 1+2;", new[] { Keyword, Identifier, Symbol, IntegerConstant, Symbol, IntegerConstant, Symbol, })]
        public void ReadTokenSequence(string text, TokenType[] expectedTypes)
        {
            var tokenizer = new Tokenizer(text);
            foreach (var expectedType in expectedTypes)
            {
                var token = tokenizer.TryReadNext()!;
                Console.WriteLine(token.Value);
                Assert.AreEqual(expectedType, token.TokenType);
            }

            Assert.IsNull(tokenizer.TryReadNext());
        }

        [TestCase("1 > 2", new[] { IntegerConstant, Symbol, IntegerConstant })]
        [TestCase("\"1\"=\"2\"", new[] { StringConstant, Symbol, StringConstant })]
        public void PushBackSingleToken(string text, TokenType[] expectedTypes)
        {
            var tokenizer = new Tokenizer(text);
            var tokens = new Stack<Token>();
            foreach (var expectedType in expectedTypes)
            {
                var token = tokenizer.TryReadNext()!;
                tokenizer.PushBack(token);
                token = tokenizer.TryReadNext()!;
                Assert.AreEqual(expectedType, token.TokenType);
                tokens.Push(token);
            }

            Assert.IsNull(tokenizer.TryReadNext());
            while (tokens.Any())
                tokenizer.PushBack(tokens.Pop());
            foreach (var expectedType in expectedTypes)
            {
                var token = tokenizer.TryReadNext()!;
                Assert.AreEqual(expectedType, token.TokenType);
            }
        }

        [TestCase("1 > 2")]
        [TestCase("\"1\"=\"2\"")]
        [TestCase("class Main { function void main() { let a = 1; } }")]
        public void PushBackManyTokens(string text)
        {
            var tokenizer = new Tokenizer(text);
            var tokens = new List<Token>();
            while (true)
            {
                var token = tokenizer.TryReadNext();
                if (token is null) break;
                tokens.Add(token);
            }

            foreach (var token in Enumerable.Reverse(tokens))
                tokenizer.PushBack(token);

            foreach (var token in tokens)
            {
                var readToken = tokenizer.TryReadNext()!;
                Assert.AreEqual(token.TokenType, readToken.TokenType);
                Assert.AreEqual(token.Value, readToken.Value);
            }
        }

        [TestCase("42")]
        [TestCase("//\n42")]
        [TestCase("// 1 2 3 \n42")]
        [TestCase("// 1 2 3 \n   42")]
        [TestCase("   // 1 2 3 \n   42")]
        [TestCase("/**/42")]
        [TestCase("/**/ 42")]
        [TestCase("/**/\n42")]
        [TestCase("/* 1 2 3 */\n42")]
        [TestCase("/* 1 \n2 3 */\n42")]
        [TestCase("/* 1 \n2 3* * */\n42")]
        [TestCase(" /* 1 \n2 3* * */ \n 42")]
        [TestCase("/**/ /*sdf*/ // asd \n42")]
        public void SkipCommentsBeforeToken(string text)
        {
            var tokenizer = new Tokenizer(text);
            Assert.AreEqual(42, tokenizer.TryReadNext()!.IntValue);
        }

        [TestCase("42 //\n")]
        [TestCase("42 //")]
        [TestCase("42 /**/")]
        public void SkipCommentsAtTheEnd(string text)
        {
            var tokenizer = new Tokenizer(text);
            Assert.AreEqual(42, tokenizer.TryReadNext()!.IntValue);
            Assert.IsNull(tokenizer.TryReadNext());
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
            var expectedFilename = Path.Combine(
                Path.GetDirectoryName(jackFile)!,
                Path.GetFileNameWithoutExtension(jackFile) + "T.xml"
            );
            var expectedTokens = XDocument.Load(expectedFilename).Root!.Elements().ToList();
            var tokenizer = new Tokenizer(text);
            foreach (var expectedToken in expectedTokens)
            {
                var token = tokenizer.TryReadNext()!;
                Console.WriteLine(token);
                var expectedType = expectedToken.Name.LocalName;
                var expectedValue = expectedToken.Value[1..^1];
                Assert.AreEqual(expectedType.ToLower(), token.TokenType.ToString().ToLower());
                Assert.AreEqual(expectedValue, token.Value);
            }

            Assert.IsNull(tokenizer.TryReadNext());
        }

        [TestCase]
        public void LineColNumberTest()
        {
            var text = "let x=1; // comment\n/*\ncomment 2\n*/\nlet language = \"jack\" ; ";
            var expectedTokens = new List<Token>() {
                new(Keyword, "let", 1, 1),
                new(Identifier, "x", 1, 5),
                new(Symbol, "=", 1, 6),
                new(IntegerConstant, "1", 1, 7),
                new(Symbol, ";", 1, 8),
                new(Keyword, "let", 5, 1),
                new(Identifier, "language", 5, 5),
                new(Symbol, "=", 5, 14),
                new(StringConstant, "jack", 5, 16),
                new(Symbol, ";", 5, 23),
            };

            var tokenizer = new Tokenizer(text);
            foreach (var expectedToken in expectedTokens)
            {
                var token = tokenizer.TryReadNext()!;
                Console.WriteLine(token);
                Assert.AreEqual(expectedToken, token);
            }

            Assert.IsNull(tokenizer.TryReadNext());
        }
    }
}
