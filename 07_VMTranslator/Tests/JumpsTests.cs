using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace VMTranslator.Tests;

[TestFixture]
public class JumpsTests
{
    [Test]
    public void DoNotWriteCodeForUnknownInstruction()
    {
        var vmCode = new[] { "abc 1 2 3" };
        var result = new List<string>();
        var codeWriter = new CodeWriter(result);
        Assert.That(() => codeWriter.WriteModule("test", vmCode), Throws.Exception);
        Assert.AreEqual(result.Count, 0);
    }

    [TestCase("label L1|push constant 1", 1)]
    [TestCase("push constant 42|label L1", 42)]
    [TestCase("goto END|push constant 1|label END|push constant 2", 2)]
    [TestCase("goto L1|label L1|push constant 1|goto L3|label L2|push constant 2|label L3", 1)]
    [TestCase("label L0|goto L1|label L1|push constant 1|goto L3|label L2|push constant 2|label L3", 1)]
    public void LabelAndGoto(string program, short expectedStackTop)
    {
        var emulator = program.Split("|").LoadVmCodeToEmulator().EmulateTicks(100);
        Assert.That(emulator.Ram[0], Is.EqualTo(257));
        Assert.That(emulator.Ram[256], Is.EqualTo(expectedStackTop));
    }

    [TestCase("push constant 0|if-goto THEN|push constant 1|goto END|label THEN|push constant 2|label END", 1)]
    [TestCase("push constant 0|not|if-goto THEN|push constant 1|goto END|label THEN|push constant 2|label END", 2)]
    [TestCase("push constant 0|not|if-goto L1|goto END|label L1|push constant 0|if-goto END|push constant 1|label END", 1)]
    [TestCase("push constant 0|not|if-goto L1|goto END|label L1|push constant 42|push constant 0|not|if-goto END|neg|label END", 42)]
    public void IfGoto(string program, short expectedStackTop)
    {
        var emulator = program.Split("|").LoadVmCodeToEmulator().EmulateTicks(100);
        Assert.That(emulator.Ram[0], Is.EqualTo(257));
        Assert.That(emulator.Ram[256], Is.EqualTo(expectedStackTop));
    }

    [TestCase("BasicLoop")]
    [TestCase("FibonacciSeries")]
    public void RunSamplesAndCompareWithCmpFiles(string sampleName)
    {
        var path = $"Tests/ProgramFlow/{sampleName}/{sampleName}.vm";
        var vmCode = File.ReadAllLines(path);
        var emulator = vmCode.LoadVmCodeToEmulator(withMemoryInit:true);
        emulator.InitMemoryFromTstFile(Path.ChangeExtension(path, "tst"));
        emulator.EmulateTicks(2000);
        emulator.ShouldBeSameAsInCmpFile(Path.ChangeExtension(path, "cmp"));
    }
}
