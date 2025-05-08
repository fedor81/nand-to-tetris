using System;
using System.Collections.Generic;
using System.IO;

namespace VMTranslator;

// Код класса с транслятором разбит на несколько файлов
// с помощью ключевого слова partial в объявлении класса
// Это позволяет объявлять методы и поля одного класса в разных файлах.
public partial class CodeWriter
{
    public readonly List<string> ResultAsmCode;
    private readonly Parser parser = new();

    /// <param name="resultAsmCode">В этот список транслятор должен добавлять сгенерированный код на ассемблере</param>
    public CodeWriter(List<string> resultAsmCode)
    {
        ResultAsmCode = resultAsmCode;
    }

    /// <summary>
    /// Транслирует одну инструкцию.
    /// Вспомогательный метод для реализации методов трансляции целого модуля.
    /// </summary>
    public virtual void WriteInstruction(VmInstruction instruction, string moduleName)
    {
        WriteAsm($"// {instruction}");
        var success = TryWriteStackCode(instruction, moduleName)
                      || TryWriteLogicAndArithmeticCode(instruction)
                      || TryWriteProgramFlowCode(instruction, moduleName)
                      || TryWriteFunctionCallCode(instruction);
        if (!success)
            throw new FormatException($"Unknown instruction [{instruction}]");
    }

    // Транслирует все строки кода внутри модуля moduleName.
    public void WriteModule(string moduleName, string[] vmLines)
    {
        foreach (var line in parser.Parse(vmLines))
        {
            WriteInstruction(line, moduleName);
        }
    }

    // Транслирует код для виртуальной машины из файла filename.
    public void WriteModuleFromFile(string filename)
    {
        WriteModule(Path.GetFileNameWithoutExtension(filename), File.ReadAllLines(filename));
    }

    /// <summary>
    /// Вспомогательный метод для удобства реализации остальных методов
    /// Используйте его в других задачах, когда понадобится.
    /// </summary>
    private void WriteAsm(params string[] instructions)
    {
        ResultAsmCode.AddRange(instructions);
    }
}
