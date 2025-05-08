using System;
using System.Collections.Generic;
using System.IO;

namespace VMTranslator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:\n" +
                                  $" {Path.GetFileName(Environment.ProcessPath)} <file>.vm" +
                                  $" {Path.GetFileName(Environment.ProcessPath)} <directory>");
                Console.WriteLine("Translates <file>.vm   to <file>.asm");
                Console.WriteLine("           <directory> to <directory>/<directory>.asm");
                Environment.Exit(1);
                return;
            }

            var resultAsmCode = new List<string>();
            var codeWriter = new CodeWriter(resultAsmCode);
            if (File.Exists(args[0]))
            {
                Console.WriteLine($"Translate single file {args[0]}");
                codeWriter.WriteModuleFromFile(args[0]);
                var asmFile = Path.ChangeExtension(args[0], ".asm");
                File.WriteAllLines(asmFile, resultAsmCode);
            }
            else
            {
                Console.WriteLine($"Translate directory {args[0]}");
                codeWriter.WriteMemoryInitialization();
                codeWriter.WriteSysInitCall();
                foreach (var vmFile in Directory.GetFiles(args[0], "*.vm"))
                    codeWriter.WriteModuleFromFile(vmFile);
                var asmFile = Path.Combine(args[0], Path.GetFileName(Path.GetFullPath(args[0])) + ".asm");
                File.WriteAllLines(asmFile, resultAsmCode);
            }

            // Uncomment these lines if you want to execute your program after translation to assembly code
            //var hack = Assembler.Program.TranslateAsmToHack(asmLines.ToArray());
            //var emulator = new HackEmulator(hack);
            //emulator.Ticks(int.MaxValue);
            //Console.WriteLine(emulator);
        }
    }
}
