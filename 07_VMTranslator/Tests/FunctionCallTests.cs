using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using System.Linq;

namespace VMTranslator.Tests;

[TestFixture]
public class FunctionCallTests
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

    [TestCase("f", 0)]
    [TestCase("function.with.long.name", 5)]
    public void Call(string funcName, int argsCount)
    {
        var program = $"call {funcName} {argsCount}|function {funcName} 0";
        var emulator = program.Split("|").LoadVmCodeToEmulator(withMemoryInit:true).EmulateTicks(100);
        var ram = emulator.Ram;
        var sp = ram[0];
        var lcl = ram[1];
        var arg = ram[2];
        Assert.That(sp, Is.EqualTo(VmInitialization.Sp + 5)); // stack pointer. 5: retAddr, LCL, ARG, THIS, THAT
        Assert.That(arg, Is.EqualTo(VmInitialization.Sp - argsCount)); // new ARG value
        Assert.That(lcl, Is.EqualTo(sp)); // new LCL value
        Assert.That(ram[sp - 5], Is.Not.EqualTo(0)); // returnAddress pushed on stack
        var frame = new[] { ram[sp - 1], ram[sp - 2], ram[sp - 3], ram[sp - 4] };
        var expectedFrame = new[] { VmInitialization.Local, VmInitialization.Argument, VmInitialization.This, VmInitialization.That };
        Assert.That(frame, Is.EquivalentTo(expectedFrame)); // callee frame pushed on stack
    }

    [TestCase("f", 0)]
    [TestCase("Math.sin", 1)]
    [TestCase("Math.atan2", 2)]
    public void Function(string funcName, int localVariablesCount)
    {
        var program = $"function {funcName} {localVariablesCount}";
        var emulator = program.Split("|").LoadVmCodeToEmulator().EmulateTicks(int.MaxValue);
        var ram = emulator.Ram;
        var sp = ram[0];
        var lcl = ram[1];
        Assert.That(lcl, Is.EqualTo(VmInitialization.Local)); // LCL should not changed
        Assert.That(sp, Is.EqualTo(VmInitialization.Sp + localVariablesCount)); // stack pointer
        for (int i = 0; i < localVariablesCount; i++)
        {
            Assert.That(ram[sp-1-i], Is.EqualTo(0)); // default 0
        }
    }

    [TestCase("f", 0, 0)]
    [TestCase("func", 2, 3)]
    public void Return(string funcName, int argsCount, int localVariablesCount)
    {
        var pushArgCommands = Enumerable.Range(0, argsCount).Select(i => $"push constant {i}");
        var pushArgs = string.Join("|", pushArgCommands);
        var program = $"{pushArgs} | call {funcName} {argsCount} | goto END | function {funcName} {localVariablesCount} | push constant 42 | return | label END";
        var emulator = program.Split("|").LoadVmCodeToEmulator().EmulateTicks(int.MaxValue);
        var ram = emulator.Ram;
        var sp = ram[0];

        // Stack contains single element — function result
        Assert.That(sp, Is.EqualTo(VmInitialization.Sp + 1)); // stack pointer
        Assert.That(ram[sp-1], Is.EqualTo(42));

        // segments are restored to initial values
        Assert.That(ram[1], Is.EqualTo(VmInitialization.Local));
        Assert.That(ram[2], Is.EqualTo(VmInitialization.Argument));
        Assert.That(ram[3], Is.EqualTo(VmInitialization.This));
        Assert.That(ram[4], Is.EqualTo(VmInitialization.That));
    }

    [Test]
    public void ReturnWithNestedCalls()
    {
        var program = "push constant 1 | call f 1 | goto END | function f 0 | push constant 2 | call g 1 | return | function g 0 | push constant 3 | return | label END";
        var emulator = program.Split("|").LoadVmCodeToEmulator().EmulateTicks(int.MaxValue);
        var ram = emulator.Ram;
        var sp = ram[0];

        // Stack contains single element — function result
        Assert.That(sp, Is.EqualTo(VmInitialization.Sp + 1)); // stack pointer
        Assert.That(ram[sp - 1], Is.EqualTo(3));

        // segments are restored to initial values
        Assert.That(ram[1], Is.EqualTo(VmInitialization.Local));
        Assert.That(ram[2], Is.EqualTo(VmInitialization.Argument));
        Assert.That(ram[3], Is.EqualTo(VmInitialization.This));
        Assert.That(ram[4], Is.EqualTo(VmInitialization.That));
    }

    [TestCase("SimpleFunction")]
    public void SingleFileSampleHaveSameResultAsInCmpFile(string sampleName)
    {
        var path = $"Tests/FunctionCalls/{sampleName}/{sampleName}.vm";
        var vmCode = File.ReadAllLines(path);
        var emulator = vmCode.LoadVmCodeToEmulator(withMemoryInit: false);
        emulator.InitMemoryFromTstFile(Path.ChangeExtension(path, "tst"));
        emulator.EmulateTicks(10000);
        emulator.ShouldBeSameAsInCmpFile(Path.ChangeExtension(path, "cmp"));
    }

    [TestCase("FibonacciElement")]
    [TestCase("StaticsTest")]
    [TestCase("NestedCall")]
    public void DirectoryModeSampleHaveSameResultAsInCmpFile(string sampleName)
    {
        var path = $"Tests/FunctionCalls/{sampleName}";
        var resultAsmCode = new List<string>();
        var codeWriter = new CodeWriter(resultAsmCode);
        codeWriter.WriteMemoryInitialization();
        codeWriter.WriteSysInitCall();
        foreach (var vmFile in Directory.GetFiles(path, "*.vm"))
            codeWriter.WriteModuleFromFile(vmFile);
        var hack = resultAsmCode.ToArray().AsmCodeToHack();
        var emulator = new HackEmulator(hack);
        emulator.InitMemoryFromTstFile(Path.Combine(path, sampleName + ".tst"));
        emulator.EmulateTicks(10000);
        emulator.ShouldBeSameAsInCmpFile(Path.Combine(path, sampleName + ".cmp"));
    }
}
