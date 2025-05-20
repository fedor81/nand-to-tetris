namespace Assembler
{
    public static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine($"Usage: {Path.GetFileName(Environment.ProcessPath)} <file>.asm");
                Console.WriteLine("Compiles <file>.asm to <file>.hack ");
                Environment.Exit(1);
                return;
            }

            var asmFile = args[0];
            var hackFile = Path.ChangeExtension(asmFile, ".hack");
            var lines = File.ReadAllLines(asmFile);
            var hackLines = TranslateAsmToHack(lines);
            File.WriteAllLines(hackFile, hackLines);
        }

        public static string[] TranslateAsmToHack(string[] lines)
        {
            var parser = new Parser();
            var analyzer = new SymbolAnalyzer();
            var translator = new HackTranslator();
            var preprocessor = new Preprocessor();

            var withoutCommentsAndEmptyLines = parser.RemoveWhitespacesAndComments(lines);
            var pureAsm = preprocessor.PreprocessAsm(withoutCommentsAndEmptyLines);
            var symbolTable = analyzer.CreateSymbolsTable(pureAsm, out var withoutLabels);
            return translator.TranslateAsmToHack(withoutLabels, symbolTable);
        }
    }
}
