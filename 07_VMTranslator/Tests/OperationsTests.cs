using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace VMTranslator.Tests;

[TestFixture]
public class OperationsTests
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

    [TestCase("push constant 42", 42)]
    [TestCase("push constant 1", 1)]
    [TestCase("push constant 0", 0)]

    [TestCase("push constant 50|push constant 51|add", 101)]
    [TestCase("push constant 50|push constant 51|sub", -1)]
    [TestCase("push constant 42|neg", -42)]

    [TestCase("push constant 1|push constant 2|lt", -1)]
    [TestCase("push constant 1|push constant 2|gt", 0)]
    [TestCase("push constant 1|push constant 2|eq", 0)]
    [TestCase("push constant 11|push constant 2|lt", 0)]
    [TestCase("push constant 11|push constant 2|gt", -1)]
    [TestCase("push constant 11|push constant 2|eq", 0)]
    [TestCase("push constant 2|push constant 2|lt", 0)]
    [TestCase("push constant 2|push constant 2|gt", 0)]
    [TestCase("push constant 2|push constant 2|eq", -1)]

    [TestCase("push constant 1|not", -2)]
    [TestCase("push constant 1|inc", 2)]
    [TestCase("push constant 3|dec", 2)]
    [TestCase("push constant 0|push constant 255|or", 255)]
    [TestCase("push constant 3|push constant 0|or", 3)]
    [TestCase("push constant 0|push constant 0|or", 0)]
    [TestCase("push constant 255|push constant 256|or", 511)]
    [TestCase("push constant 0|push constant 255|and", 0)]
    [TestCase("push constant 255|push constant 0|and", 0)]
    [TestCase("push constant 0|push constant 0|and", 0)]
    [TestCase("push constant 255|push constant 127|and", 127)]
    public void ArithmeticAndLogic(string program, int expectedResult)
    {
        var emulator = program.Split("|").LoadVmCodeToEmulator().EmulateTicks(100);
        Assert.That(emulator.Ram[0], Is.EqualTo(257));
        Assert.That(emulator.Ram[256], Is.EqualTo(expectedResult));
    }

    [TestCase("SimpleAdd")]
    [TestCase("StackTest")]
    public void StackArithmetic(string testName)
    {
        var path = $"Tests/StackArithmetic/{testName}/{testName}.vm";
        var vmCode = File.ReadAllLines(path);
        var emulator = vmCode.LoadVmCodeToEmulator();
        emulator.EmulateTicks(600);
        emulator.ShouldBeSameAsInCmpFile(Path.ChangeExtension(path, ".cmp"));
    }

    [TestCase("BasicTest")]
    [TestCase("PointerTest")]
    [TestCase("StaticTest")]
    public void MemoryAccessTests(string testName)
    {
        var path = $"Tests/MemoryAccess/{testName}/{testName}.vm";
        var vmCode = File.ReadAllLines(path);
        var emulator = vmCode.LoadVmCodeToEmulator(withMemoryInit: true);
        emulator.EmulateTicks(600);
        emulator.ShouldBeSameAsInCmpFile(Path.ChangeExtension(path, ".cmp"));
    }
}
