using NUnit.Framework;

namespace Assembler.Tests
{
    [TestFixture]
    public class SymbolAnalyzerTests
    {
        [Test]
        public void EmptyProgram()
        {
            var symbolAnalyzer = new SymbolAnalyzer();
            symbolAnalyzer.CreateSymbolsTable(Array.Empty<string>(), out var result);
            Assert.IsEmpty(result);
        }

        [Test]
        public void HasPredefinedSymbols()
        {
            var symbolAnalyzer = new SymbolAnalyzer();
            var table = symbolAnalyzer.CreateSymbolsTable(Array.Empty<string>(), out var result);
            Assert.That(table, Contains.Key("R0").WithValue(0));
            Assert.That(table, Contains.Key("R1").WithValue(1));
            Assert.That(table, Contains.Key("R2").WithValue(2));
            Assert.That(table, Contains.Key("R14").WithValue(14));
            Assert.That(table, Contains.Key("R15").WithValue(15));
            Assert.That(table, Contains.Key("SP").WithValue(0));
            Assert.That(table, Contains.Key("LCL").WithValue(1));
            Assert.That(table, Contains.Key("ARG").WithValue(2));
            Assert.That(table, Contains.Key("THIS").WithValue(3));
            Assert.That(table, Contains.Key("THAT").WithValue(4));
            Assert.That(table, Contains.Key("SCREEN").WithValue(0x4000));
            Assert.That(table, Contains.Key("KBD").WithValue(0x6000));
            Assert.IsEmpty(result);
        }

        [Test]
        public void ProgramWithoutLabels()
        {
            var symbolAnalyzer = new SymbolAnalyzer();
            var program = "@12 M=A @KBD D=M".Split(" ");
            var table = symbolAnalyzer.CreateSymbolsTable(program, out var result);
            Assert.That(result, Is.EqualTo(program));
        }

        [Test]
        public void CreateSymbolTable()
        {
            var symbolAnalyzer = new SymbolAnalyzer();
            var program = @"
(A)
(BBB_BB)
@A
@A
@BBB_BB
(C)
(DD)
@C
M=A
(EEE)
".Trim();
            var table = symbolAnalyzer.CreateSymbolsTable(program.Split(new []{'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries), out var result);
            Assert.AreEqual("@A @A @BBB_BB @C M=A".Split(" "), result);
            Assert.That(table, Contains.Key("A").WithValue(0));
            Assert.That(table, Contains.Key("BBB_BB").WithValue(0));
            Assert.That(table, Contains.Key("C").WithValue(3));
            Assert.That(table, Contains.Key("EEE").WithValue(5));
            Assert.That(table, Contains.Key("DD").WithValue(3));
            Assert.That(table, Contains.Key("R0").WithValue(0));
            Assert.That(table, Contains.Key("SCREEN").WithValue(0x4000));
            Assert.That(table, Contains.Key("KBD").WithValue(0x6000));
        }
    }
}
