using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Вставляет вызов функции Sys.init без аргументов
    /// </summary>
    public void WriteSysInitCall()
    {
        TryWriteFunctionCallCode(new VmInstruction(0, "call", "Sys.init", "0"));
    }

    private readonly Dictionary<string, int> funcReturnNumber = new Dictionary<string, int>();
    private readonly static string endFrame = "endFrameTemp";
    private readonly static string returnAddr = "retAddrTemp";
    private readonly static string tempVariable = "tempTemp";

    /// <summary>
    /// Транслирует инструкции: call, function, return
    /// </summary>
    private bool TryWriteFunctionCallCode(VmInstruction instruction)
    {
        return instruction.Name switch
        {
            "call" => WriteCallFunction(instruction.Args[0], int.Parse(instruction.Args[1])),
            "function" => WriteDeclarationFunction(instruction.Args[0], int.Parse(instruction.Args[1])),
            "return" => WriteReturn(),
            "tailrec" => WriteTailrec(instruction.Args[0], int.Parse(instruction.Args[1])),
            _ => false
        };
    }

    private bool WriteTailrec(string funcName, int argsCount)
    {
        WriteAsm(
            // endFrame = LCL
            "@LCL",
            "D=M",
            $"@{endFrame}",
            "M=D"
        );

        // Запишем во временную переменну ARG + argsConts
        if (argsCount == 0)
            WriteAsm("@ARG", "D=M");
        else
            WriteAsm($"@{argsCount}", "D=A", "@ARG", "D=M+D");
        WriteAsm($"@{tempVariable}", "M=D");

        // Теперь скопируем схраненные значения регистров по адресу ARG + argsCount
        for (var offset = 5; 0 < offset; offset--)
        {
            WriteAsm(
                // D = RAM[endFrame - offset]
                $"@{endFrame}",
                "D=M",
                $"@{offset}",
                "A=D-A",
                "D=M",
                // RAM[temp] = D
                $"@{tempVariable}",
                "A=M",
                "M=D"
            );
        }

        // Скопируем значения аргументов в адреса начинающиеся с AGR
        for (var offset = argsCount; 0 < offset; offset--)
        {
            WritePopToD();
            WriteAsm(

                // RAM[ARG + offset] = D
                $"@{tempVariable}",
                "A=M",
                "M=D"
            );
        }

        WriteAsm(
            // ARG = SP - 5 - nArgs
            "@SP",
            "D=M",
            "@5",
            "D=D-A",
            $"@{argsCount}",
            "D=D-A",
            "@ARG",
            "M=D",

            // LCL = SP
            "@SP",
            "D=M",
            "@LCL",
            "M=D",

            $"@{name}", // Переход
            "0; JMP",
            $"({returnLabel})" // Адрес возврата
        );

        return true;
    }

    private bool WriteReturn()
    {
        WriteAsm(
            // endFrame = LCL
            "@LCL",
            "D=M",
            $"@{endFrame}",
            "M=D"
        );
        WriteSetVariableByEndFrame(returnAddr, offset: 5);

        WritePopToD();
        WriteAsm(
            // RAM[ARG] = D
            "@ARG",
            "A=M",
            "M=D",

            // SP = ARG + 1
            "@ARG",
            "D=M+1",
            "@SP",
            "M=D"
        );

        WriteSetVariableByEndFrame("THAT", offset: 1);
        WriteSetVariableByEndFrame("THIS", offset: 2);
        WriteSetVariableByEndFrame("ARG", offset: 3);
        WriteSetVariableByEndFrame("LCL", offset: 4);
        WriteAsm($"@{returnAddr}", "A=M", "0; JMP");    // goto retAddr

        return true;
    }

    private void WriteSetVariableByEndFrame(string variable, int offset)
    {
        // variable = RAM[endFrame - offset]
        WriteAsm(
            $"@{endFrame}",
            "D=M",
            $"@{offset}",
            "A=D-A",
            "D=M",
            $"@{variable}",
            "M=D"
        );
    }

    private bool WriteDeclarationFunction(string name, int localsCount)
    {
        WriteAsm($"({name})");

        for (var i = 0; i < localsCount; i++)
        {
            WriteAsm("D=0");
            WritePushD();
        }

        return true;
    }

    private string GetReturnLabel(string funcName)
    {
        if (!funcReturnNumber.TryGetValue(funcName, out var returnNumber))
            returnNumber = 0;

        funcReturnNumber[funcName] = returnNumber + 1;
        return $"{funcName}$ret.{returnNumber}";
    }

    private bool WriteCallFunction(string name, int argsCount)
    {
        var returnLabel = GetReturnLabel(name);

        WriteAsm($"@{returnLabel}", "D=A"); // Push return address
        WritePushD();

        WritePushVariable("LCL");
        WritePushVariable("ARG");
        WritePushVariable("THIS");
        WritePushVariable("THAT");

        WriteAsm(
            // ARG = SP - 5 - nArgs
            "@SP",
            "D=M",
            "@5",
            "D=D-A",
            $"@{argsCount}",
            "D=D-A",
            "@ARG",
            "M=D",

            // LCL = SP
            "@SP",
            "D=M",
            "@LCL",
            "M=D",

            $"@{name}", // Переход
            "0; JMP",
            $"({returnLabel})" // Адрес возврата
        );

        return true;
    }

    private void WritePushVariable(string name)
    {
        WriteAsm(
            $"@{name}",
            "D=M"
        );
        WritePushD();
    }
}
