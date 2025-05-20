namespace Assembler
{
    public class Parser
    {
        /// <summary>
        /// Удаляет все комментарии и пустые строки из программы. Удаляет все пробелы из команд.
        /// </summary>
        /// <param name="asmLines">Строки ассемблерного кода</param>
        /// <returns>Только значащие строки строки ассемблерного кода без комментариев и лишних пробелов</returns>
        public string[] RemoveWhitespacesAndComments(string[] asmLines)
        {
            return asmLines.Select(line => line.Replace(" ", ""))
                .Select(line =>
                    {
                        var commentIndex = line.IndexOf("//");

                        return commentIndex != -1 ?
                            line = line[..commentIndex] : line;
                    })
                .Where(line => !string.IsNullOrEmpty(line))
                .ToArray();
        }
    }
}
