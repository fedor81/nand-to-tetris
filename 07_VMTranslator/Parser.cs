using System;
using System.Collections.Generic;
using System.Linq;

namespace VMTranslator;

public class Parser
{
    /// <summary>
    /// Читает список строк, пропускает строки, не являющиеся инструкциями,
    /// и возвращает массив инструкций
    /// </summary>
    public VmInstruction[] Parse(string[] vmLines)
    {
        var vmInstructions = new List<VmInstruction>();

        for (var i = 0; i < vmLines.Length; i++)
        {
            var lineParts = SplitAndDeleteCommentsAndSpaces(vmLines[i]);

            if (lineParts.Length > 0)
                vmInstructions.Add(new VmInstruction(i + 1, lineParts[0], lineParts[1..]));
        }

        return vmInstructions.ToArray();
    }

    private string[] SplitAndDeleteCommentsAndSpaces(string line)
    {
        var commentIndex = line.IndexOf("//");

        if (commentIndex != -1)
            line = line[..commentIndex];

        return line.Split(' ').Where(x => x.Length > 0).ToArray();
    }
}
