using NUnit.Framework;

namespace Assembler.Tests;

[TestFixture]
public class PreprocessorTests
{
    [TestCase("AD=M")]
    [TestCase("D=M+D")]
    [TestCase("D;JMP")]
    [TestCase("@123")]
    [TestCase("D=D+1;JGT")]
    [TestCase("0;JMP")]
    public void DoNotChangeUsualInstructions(string text)
    {
        Assert.AreEqual(text, Preprocess(text));
    }

    [TestCase("M[12]=0", ExpectedResult = "@12 M=0")]
    [TestCase("AD=M[42]", ExpectedResult = "@42 AD=M")]
    [TestCase("D=M[SCREEN]", ExpectedResult = "@SCREEN D=M")]
    [TestCase("D=M[myVar]+D", ExpectedResult = "@myVar D=M+D")]
    [TestCase("D=!M[myVar]", ExpectedResult = "@myVar D=!M")]
    [TestCase("M[i]=M[i]+1", ExpectedResult = "@i M=M+1")]
    [TestCase("ADM[i]=M[i]+D", ExpectedResult = "@i ADM=M+D")]
    [TestCase("D;JMP[SOME_PLACE]", ExpectedResult = "@SOME_PLACE D;JMP")]
    [TestCase("D;JGT[END]", ExpectedResult = "@END D;JGT")]
    public string GenerateAInstructionBySquareBrackets(string text)
    {
        return Preprocess(text);
    }

    [TestCase("JMP", ExpectedResult = "0;JMP")]
    [TestCase("JNE", ExpectedResult = "D;JNE")]
    [TestCase("JGT", ExpectedResult = "D;JGT")]
    [TestCase("JMP[SOME_PLACE]", ExpectedResult = "@SOME_PLACE 0;JMP")]
    [TestCase("JLE[END]", ExpectedResult = "@END D;JLE")]
    [TestCase("JLT[12]", ExpectedResult = "@12 D;JLT")]
    public string TranslateShortJumpSyntax(string text)
    {
        return Preprocess(text);
    }

    [TestCase("Add.masm")]
    [TestCase("Max.masm")]
    [TestCase("Rect.masm")]
    public void PreprocessSampleProgram(string filename)
    {
        var masmFile = Path.Combine("Samples", filename);
        var macroAsmCode = File.ReadAllLines(masmFile);
        var asmCode = new Preprocessor().PreprocessAsm(macroAsmCode);
        var asmFile = Path.ChangeExtension(masmFile, ".asm");
        var parser = new Parser();
        var expectedAsmCode = parser.RemoveWhitespacesAndComments(File.ReadAllLines(asmFile));
        Assert.AreEqual(expectedAsmCode, asmCode);
    }

    private static string Preprocess(string text)
    {
        var res = new List<string>();
        new Preprocessor().TranslateInstruction(text, res);
        return string.Join(" ", res);
    }
}
