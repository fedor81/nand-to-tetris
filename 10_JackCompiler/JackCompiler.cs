using System.IO;

namespace JackCompiling
{
    public class JackCompiler
    {
        public void Compile(string jackFile)
        {
            var text = File.ReadAllText(jackFile);
            var tokenizer = new Tokenizer(text);
            var parser = new Parser(tokenizer);
            var clazz = parser.ReadClass();

            var writer = new XmlSyntaxWriter();
            writer.Write(clazz);
            File.WriteAllLines(Path.ChangeExtension(jackFile, ".xml"), writer.GetResult());

            var codeWriter = new CodeWriter();
            codeWriter.WriteClass(clazz);
            File.WriteAllLines(Path.ChangeExtension(jackFile, ".vm"), codeWriter.ResultVmCode);
        }
    }
}
