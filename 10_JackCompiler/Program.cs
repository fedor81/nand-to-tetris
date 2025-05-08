using System;
using System.IO;

namespace JackCompiling
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:\n" +
                                  " JackCompiler <file>.jack" +
                                  " JackCompiler <directory>");
                Console.WriteLine("Translates <file>.jack to <file>.vm");
                Console.WriteLine("           or every *.jack file in <directory> to *.vm");
                Environment.Exit(1);
                return;
            }

            var directoryMode = Directory.Exists(args[0]);

            var jackFiles = directoryMode ? Directory.GetFiles(args[0], "*.jack") : new[] { args[0] };
            foreach (var jackFile in jackFiles)
            {
                var compiler = new JackCompiler();
                compiler.Compile(jackFile);
            }
        }
    }
}
