using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace VMTranslator.Tests;

public static class VmTestsHelper
{
    public static string[] AsmCodeToHack(this IEnumerable<string> asmCode)
    {
        return Assembler.Program.TranslateAsmToHack(asmCode.ToArray());
    }

    public static string[] VmCodeToHack(this string[] vmCode)
    {
        var resultAsmCode = new List<string>();
        var vmCodeWriter = new CodeWriter(resultAsmCode);
        vmCodeWriter.WriteModule("main", vmCode);
        return resultAsmCode.AsmCodeToHack();
    }

    public static HackEmulator LoadVmCodeToEmulator(this string[] vmCode, bool withMemoryInit = true)
    {
        var emulator = new HackEmulator(vmCode.VmCodeToHack());
        if (withMemoryInit)
        {
            emulator.Ram[0] = VmInitialization.Sp;
            emulator.Ram[1] = VmInitialization.Local;
            emulator.Ram[2] = VmInitialization.Argument;
            emulator.Ram[3] = VmInitialization.This;
            emulator.Ram[4] = VmInitialization.That;
        }
        return emulator;
    }

    public static void InitMemoryFromTstFile(this HackEmulator emulator, string tstFilePath)
    {
        var lines = File.ReadAllLines(tstFilePath)
            .Where(line => line.StartsWith("set RAM["))
            .Select(line => line.Split(new[] { ' ', '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries))
            .ToList();
        foreach (var line in lines)
        {
            var addr = int.Parse(line[2]);
            var value = short.Parse(line[3]);
            emulator.Ram[addr] = value;
        }
    }

    public static void ShouldBeSameAsInCmpFile(this HackEmulator emulator, string cmpFile)
    {
        var cmpFileContent = File.ReadAllLines(cmpFile);
        for (int i = 0; i < cmpFileContent.Length; i += 2)
        {
            var addresses = cmpFileContent[i].Split("|", StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim().Replace("RAM[", "").Replace("]", "")).Select(int.Parse).ToArray();
            var expectedValues = cmpFileContent[i + 1].Split("|", StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim()).Select(short.Parse).ToArray();
            foreach (var (address, expectedValue) in addresses.Zip(expectedValues))
                Assert.That(emulator.Ram[address], Is.EqualTo(expectedValue));
        }
    }
}
