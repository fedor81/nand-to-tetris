using NUnit.Framework;

namespace Assembler.Tests
{
    [TestFixture]
    public class HackTranslatorTests
    {
        [TestCase("M=A", "1110110000001000")]
        [TestCase("D=M", "1111110000010000")]
        [TestCase("A=D", "1110001100100000")]
        [TestCase("AMD=D", "1110001100111000")]
        [TestCase("D;JMP", "1110001100000111")]
        [TestCase("D;JLT", "1110001100000100")]
        [TestCase("D;JGE", "1110001100000011")]
        [TestCase("D;JEQ", "1110001100000010")]
        [TestCase("D;JNE", "1110001100000101")]
        [TestCase("D=0", "1110101010010000")]
        [TestCase("D=1", "1110111111010000")]
        [TestCase("D=-1", "1110111010010000")]
        [TestCase("D=D+M", "1111000010010000")]
        [TestCase("D=D-A", "1110010011010000")]
        [TestCase("D=D&M", "1111000000010000")]
        [TestCase("D=D|A", "1110010101010000")]
        [TestCase("AMD=0;JLE", "1110101010111110")]
        [TestCase("D=D+1;JGT", "1110011111010001")]
        [TestCase("0;JMP", "1110101010000111")]
        public void CInstruction(string text, string expectedCode)
        {
            var translator = new HackTranslator();
            var code = translator.CInstructionToCode(text);
            Assert.AreEqual(expectedCode, code);
        }

        [TestCase("@0", "0000000000000000")]
        [TestCase("@1", "0000000000000001")]
        [TestCase("@2", "0000000000000010")]
        [TestCase("@32767", "0111111111111111")]
        [TestCase("@B", "0000000000000001")]
        [TestCase("@abc", "0000000000101010")]
        public void AInstruction(string text, string expectedCode)
        {
            var translator = new HackTranslator();
            var symbolTable = new Dictionary<string, int>
            {
                {"abc", 42},
                {"B", 1},
            };
            var code = translator.AInstructionToCode(text, symbolTable);
            Assert.AreEqual(expectedCode, code);
        }

        [Test]
        public void AInstructionWithVariables()
        {
            var translator = new HackTranslator();
            var symbolTable = new Dictionary<string, int>();
            Assert.AreEqual("0000000000010000", translator.AInstructionToCode("@x", symbolTable));
            Assert.AreEqual("0000000000010001", translator.AInstructionToCode("@y", symbolTable));
            Assert.AreEqual("0000000000010000", translator.AInstructionToCode("@x", symbolTable));
            Assert.AreEqual("0000000000010010", translator.AInstructionToCode("@ZZZ", symbolTable));
            Assert.AreEqual("0000000000010001", translator.AInstructionToCode("@y", symbolTable));
            Assert.AreEqual("0000000000010010", translator.AInstructionToCode("@ZZZ", symbolTable));
        }

        [TestCase("Add.asm")]
        [TestCase("Max.asm")]
        [TestCase("Rect.asm")]
        [TestCase("Pong.asm")]
        public void TranslateSampleProgram(string filename)
        {
            var asmFile = Path.Combine("Samples", filename);
            var asmCode = File.ReadAllLines(asmFile);
            var actualHack = Program.TranslateAsmToHack(asmCode);
            var hackFile = Path.ChangeExtension(asmFile, ".hack");
            var expectedHack = File.ReadAllLines(hackFile);
            Assert.AreEqual(expectedHack, actualHack);
        }
    }
}
