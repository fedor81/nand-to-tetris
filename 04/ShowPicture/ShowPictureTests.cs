using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ShowPicture
{

    [TestFixture]
    public class ShowPictureTests
    {
        private const int ramSize = 32 * 256;
        private const int ramBaseAddress = 0x4000;

        [TestCase]
        public void MyTest()
        {
            var pixels = new bool[,]
            {
            // {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
            };

            var asm = ShowPictureTask.GenerateShowPictureCode(pixels);
            var hack = Assembler.Program.TranslateAsmToHack(asm);
            Assert.AreEqual(true, true);
        }

        [TestCase("Wolf")]
        [TestCase("Wizzard")]
        public void ShowPicture_MatchesMemory(string name)
        {
            var pixels = ImageHelpers.LoadImagePixels($"{name}.png");
            var asm = ShowPictureTask.GenerateShowPictureCode(pixels);
            File.WriteAllLines($"{name}.asm", asm);
            var hack = Assembler.Program.TranslateAsmToHack(asm);
            var emulator = new HackEmulator(hack);
            emulator.Ticks(1000000);

            AssertMemoryIs(emulator, ReadMemory($"{name}.mem.txt"));
        }

        private static void AssertMemoryIs(HackEmulator emulator, short[] memory)
        {
            Assert.That(memory.Length, Is.EqualTo(ramSize));
            for (var i = 0; i < ramSize; i++)
            {
                var addr = ramBaseAddress + i;
                // Check elements explicitly instead of a single assert for easier debugging.
                Assert.That(
                    emulator.Ram[addr],
                    Is.EqualTo(memory[i]),
                    $"Expected memory at {addr} to be {memory[i]}, it's {emulator.Ram[addr]} instead"
                );
            }
        }

        private static short[] ReadMemory(string filename)
        {
            var memory = File.ReadAllText(filename)
                .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(short.Parse)
                .ToArray();
            return memory;
        }
    }
}