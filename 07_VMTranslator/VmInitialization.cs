using System;
using System.Collections.Generic;

namespace VMTranslator;

public static class VmInitialization
{
    public const int Sp = 256;
    public const int Local = 300;
    public const int Argument = 400;
    public const int This = 3000;
    public const int That = 3010;

    private readonly static Dictionary<string, int> RegisterValue = new()
    {
        {"SP", Sp },
        {"LCL", Local},
        {"ARG", Argument},
        {"THIS", This},
        {"THAT", That},
    };


    /// <summary>
    /// Генерирует код инициализации значения регистров SP, LCL, ARG, THIS, THAT в их начальные значения (константы выше)
    /// </summary>
    public static void WriteMemoryInitialization(this CodeWriter translator)
    {
        foreach (var (register, value) in RegisterValue)
        {
            translator.ResultAsmCode.AddRange(new string[] {
                $"@{value}",
                "D=A",
                $"@{register}",
                "M=D"
            });
        }
    }
}
