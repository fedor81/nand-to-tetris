using NUnit.Framework;

namespace Assembler.Tests
{
    [TestFixture]
    public class ParserTests
    {
        [TestCase("", new string[0])]
        [TestCase("// comment", new string[0])]
        [TestCase("\n\n", new string[0])]
        [TestCase("@a", new[] { "@a" })]
        [TestCase("@123", new[] { "@123" })]
        [TestCase("(abc)", new[] { "(abc)" })]
        [TestCase("D=M+D", new[] { "D=M+D" })]
        [TestCase("@a//sdfsdf", new[] { "@a" })]
        [TestCase("@123//sdfadsf", new[] { "@123" })]
        [TestCase("(abc)//asdfasf", new[] { "(abc)" })]
        [TestCase("D=M+D// sdadf", new[] { "D=M+D" })]
        [TestCase("  @a  //  sdf  sdf", new[] { "@a" })]
        [TestCase("  @123   //  sdf  adsf", new[] { "@123" })]
        [TestCase("  (abc)  //  asd  fasf", new[] { "(abc)" })]
        [TestCase("  D  =  M  +  D  // sda  df", new[] { "D=M+D" })]
        [TestCase(" \nD=M", new[] { "D=M" })]
        [TestCase(" // hello\n (asd)\n@asd // assd\n\n\nD  =M+  D  // sda  df\n\n", new[] { "(asd)", "@asd", "D=M+D" })]
        public void Clean(string text, string[] expectedLines)
        {
            var parser = new Parser();
            var instructions = parser.RemoveWhitespacesAndComments(text.Split("\n"));
            Assert.AreEqual(expectedLines, instructions);
        }
    }
}
