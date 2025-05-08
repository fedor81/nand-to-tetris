using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace VMTranslator.Tests;

[TestFixture]
public class ParserTests
{
    private readonly Parser parser = new();

    [TestCase("add", 1, "add", new string[0])]
    [TestCase("\n\nadd\n\n", 3, "add", new string[0])]
    [TestCase("// comment!\nadd", 2, "add", new string[0])]
    [TestCase(" add ", 1, "add", new string[0])]
    [TestCase(" //comment\n\n  add", 3, "add", new string[0])]
    [TestCase("add //comment", 1, "add", new string[0])]
    [TestCase("add//comment", 1, "add", new string[0])]
    [TestCase("   //   comment  \n   add    //   comment  ", 2, "add", new string[0])]
    [TestCase("push constant 1", 1, "push", new []{"constant", "1"})]
    [TestCase("  //   comment  \n  push  constant     1  //  comment  \n\n", 2, "push", new[] { "constant", "1" })]
    [TestCase("cmd  a1 a2 a3 a4 a5", 1, "cmd", new[] { "a1", "a2", "a3", "a4", "a5" })]
    public void ParseSingleCommand(string program, int expectedLineNumber, string expectedName, string[] expectedArgs)
    {
        var parsed = parser.Parse(program.Split("\n")).Single();
        Assert.AreEqual(expectedName, parsed.Name);
        Assert.AreEqual(expectedArgs, parsed.Args);
        Assert.AreEqual(expectedLineNumber, parsed.LineNumber);
    }

    [Test]
    public void ParseMultipleLines()
    {
        var input = @"// Test file

            // Pushes and adds two constants.
            push constant 7 // seven!
            push constant 8   //eight!
            
            add
            
            ";
        var parsed = parser.Parse(input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
        Assert.That(parsed, Is.EqualTo(new[]
        {
            new VmInstruction(4, "push", "constant", "7"),
            new VmInstruction(5, "push", "constant", "8"),
            new VmInstruction(7, "add"),
        }));
    }

    [TestCaseSource(nameof(GetAllVmFiles))]
    public void ParseSampleFile(string vmFile)
    {
        var inputLines = File.ReadAllLines(vmFile);
        var parsed = parser.Parse(inputLines);
        Assert.That(parsed, Is.Not.Empty);
        var line = 0;
        foreach (var instruction in parsed)
        {
            while (line < inputLines.Length && !inputLines[line].Trim().StartsWith(instruction.ToString()))
                line++;
            if (line >= inputLines.Length)
                Assert.Fail($"Parsed instruction [{instruction}] not present in source file :(");
        }
    }

    public static IEnumerable<string> GetAllVmFiles()
    {
        return Directory.EnumerateFiles("Tests", "*.vm", SearchOption.AllDirectories);
    }
}
