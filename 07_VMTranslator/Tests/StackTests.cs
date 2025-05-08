using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace VMTranslator.Tests;

[TestFixture]
public class StackTests
{

    [Test]
    public void DoNotWriteCodeForUnknownInstruction()
    {
        var vmCode = new string[] { "abc 1 2 3" };
        var result = new List<string>();
        var codeWriter = new CodeWriter(result);
        Assert.That(() => codeWriter.WriteModule("test", vmCode), Throws.Exception);
        Assert.AreEqual(result.Count, 0);
    }
    
    [TestCase("", new int[] { })]
    [TestCase("push constant 0", new[] { 0 })]
    [TestCase("push constant -32768", new[] { -32768 })]
    [TestCase("push constant -32", new[] { -32})]
    [TestCase("push constant 1", new[] { 1 })]
    [TestCase("push constant 1|push constant 2", new[] { 1, 2 })]
    [TestCase("push constant 1|push constant 2|push constant 3", new[] { 1, 2, 3 })]
    [TestCase("push constant 1|push constant 2|push constant 3|pop local 0", new[] { 1, 2 })]
    [TestCase("push constant 1|push constant 2|push constant 3|pop local 0|pop local 1| push constant 4", new[] { 1, 4 })]
    [TestCase("push constant 1|pop local 1|push constant 2", new[] { 2 })]
    public void PushPopWithConstants(string program, int[] stackValues)
    {
        var emulator = program.Split("|").LoadVmCodeToEmulator(withMemoryInit: true).EmulateTicks(100);
        Assert.That(emulator.Ram[0], Is.EqualTo(256 + stackValues.Length));
        for (int i = 0; i < stackValues.Length; i++)
            Assert.That(emulator.Ram[256 + i], Is.EqualTo(stackValues[i]));
    }

    [TestCase("local", 0, 10, 300)]
    [TestCase("local", 1, 12, 301)]
    [TestCase("argument", 0, 20, 400)]
    [TestCase("argument", 2, 23, 402)]
    [TestCase("this", 0, 30, 3000)]
    [TestCase("this", 3, 33, 3003)]
    [TestCase("that", 0, 40, 3010)]
    [TestCase("that", 4, 44, 3014)]
    public void PopShouldStoreValueInMemory(string segment, int indexInSegment, int value, int expectedDestinationAddress)
    {
        var program = $"push constant {value}|pop {segment} {indexInSegment}".Split("|");
        var emulator = program.LoadVmCodeToEmulator(withMemoryInit: true).EmulateTicks(100);
        Assert.That(emulator.Ram[expectedDestinationAddress], Is.EqualTo(value));
    }

    [Test]
    public void PointerSegmentChangesThisAndThat()
    {
        var program = "push constant 123|push constant 456|pop pointer 1|pop pointer 0".Split("|");
        var emulator = program.LoadVmCodeToEmulator(withMemoryInit: true).EmulateTicks(100);
        Assert.That(emulator.Ram[3], Is.EqualTo(123));
        Assert.That(emulator.Ram[4], Is.EqualTo(456));
    }

    [Test]
    public void TempSegmentMapsToMemoryStartingFrom5()
    {
        var program = "push constant 123|push constant 456|pop temp 1|pop temp 0".Split("|");
        var emulator = program.LoadVmCodeToEmulator(withMemoryInit: true).EmulateTicks(100);
        Assert.That(emulator.Ram[5], Is.EqualTo(123));
        Assert.That(emulator.Ram[6], Is.EqualTo(456));
    }

    [Test]
    public void StaticSegmentStoreValueInNewGlobalVariables()
    {
        var program = "push constant 123|push constant 456|pop static 1|pop static 0".Split("|");
        var emulator = program.LoadVmCodeToEmulator(withMemoryInit: true).EmulateTicks(100);
        Assert.That(emulator.Ram[16..18], Is.EquivalentTo(new[] { 123, 456 }));
    }

    [TestCase("local", 0, 10)]
    [TestCase("local", 1, 12)]
    [TestCase("argument", 0, 20)]
    [TestCase("argument", 2, 23)]
    [TestCase("this", 0, 30)]
    [TestCase("this", 3, 33)]
    [TestCase("that", 0, 40)]
    [TestCase("that", 4, 44)]
    [TestCase("static", 0, 50)]
    [TestCase("static", 5, 55)]
    [TestCase("pointer", 0, 60)]
    [TestCase("pointer", 1, 61)]
    [TestCase("temp", 0, 71)]
    [TestCase("temp", 3, 73)]
    public void PushPopWithSegments(string segment, int indexInSegment, int value)
    {
        // Кладет константу на стек, потом достает её в указанный индекс указанного сегмента,
        // потом из этого же индекса этого же сегмента дважды кладет на стек.
        // Проверяет, что в стеке два элемента и оба равны переданной константе
        var program =
            $"push constant {value}|pop {segment} {indexInSegment}|push {segment} {indexInSegment}|push {segment} {indexInSegment}"
                .Split("|");
        var emulator = program.LoadVmCodeToEmulator(withMemoryInit: true).EmulateTicks(100);
        Assert.That(emulator.Ram[0], Is.EqualTo(258));
        Assert.That(emulator.Ram[256], Is.EqualTo(value));
        Assert.That(emulator.Ram[257], Is.EqualTo(value));
    }
}
