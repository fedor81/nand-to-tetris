using System.Collections.Generic;
using NUnit.Framework;

namespace VMTranslator.Tests;

[TestFixture]
public class InitializationTests
{
    [Test]
    public void MemoryInitialization()
    {
        var resultAsmCode = new List<string>();
        var codeWriter = new CodeWriter(resultAsmCode);
        codeWriter.WriteMemoryInitialization();
        var hack = Assembler.Program.TranslateAsmToHack(resultAsmCode.ToArray());
        var emulator = new HackEmulator(hack);
        emulator.EmulateTicks(100);
        Assert.That(emulator.Ram[0], Is.EqualTo(256));
        Assert.That(emulator.Ram[1], Is.EqualTo(300));
        Assert.That(emulator.Ram[2], Is.EqualTo(400));
        Assert.That(emulator.Ram[3], Is.EqualTo(3000));
        Assert.That(emulator.Ram[4], Is.EqualTo(3010));
    }
}
