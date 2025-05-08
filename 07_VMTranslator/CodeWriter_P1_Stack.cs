using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Транслирует инструкции:
    /// * push [segment] [index] — записывает на стек значение взятое из ячейки [index] сегмента [segment].
    /// * pop [segment] [index] — снимает со стека значение и записывает его в ячейку [index] сегмента [segment].
    ///
    /// Сегменты:
    /// * constant — виртуальный сегмент, по индексу [index] содержит значение [index]
    /// * local — начинается в памяти по адресу Ram[LCL]
    /// * argument — начинается в памяти по адресу Ram[ARG]
    /// * this — начинается в памяти по адресу Ram[THIS]
    /// * that — начинается в памяти по адресу Ram[THAT]
    /// * pointer - по индексу 0, содержит значение Ram[THIS], а по индексу 1 — значение Ram[THAT] 
    /// * temp - начинается в памяти по адресу 5
    /// * static — хранит значения по адресу, который ассемблер выделит переменной @{moduleName}.{index}
    /// </summary>
    /// <returns>
    /// true − если это инструкция работы со стеком, иначе — false.
    /// Если метод возвращает false, он не должен менять ResultAsmCode
    /// </returns>
    private bool TryWriteStackCode(VmInstruction instruction, string moduleName)
    {
        if (instruction.Args.Length != 2)
            return false;

        var segment = instruction.Args[0];
        var index = int.Parse(instruction.Args[1]);

        return instruction.Name switch
        {
            "push" => WritePush(segment, index, moduleName),
            "pop" => WritePop(segment, index, moduleName),
            _ => false
        };
    }

    private bool WritePush(string segment, int index, string moduleName)
    {
        switch (segment)
        {
            case "constant":
                WritePushConstantSegment(index);
                break;
            case "local":
                WritePushSegment("LCL", index);
                break;
            case "argument":
                WritePushSegment("ARG", index);
                break;
            case "this":
                WritePushSegment("THIS", index);
                break;
            case "that":
                WritePushSegment("THAT", index);
                break;
            case "pointer" when index == 0:
                WritePushPointerSegment("THIS");
                break;
            case "pointer" when index == 1:
                WritePushPointerSegment("THAT");
                break;
            case "temp":
                WritePushTempSegment(index);
                break;
            case "static":
                WritePushStaticSegment(index, moduleName);
                break;
            default:
                return false;
        }
        return true;
    }

    private bool WritePop(string segment, int index, string moduleName)
    {
        switch (segment)
        {
            case "local":
                WritePopSegment("LCL", index, moduleName);
                break;
            case "argument":
                WritePopSegment("ARG", index, moduleName);
                break;
            case "this":
                WritePopSegment("THIS", index, moduleName);
                break;
            case "that":
                WritePopSegment("THAT", index, moduleName);
                break;
            case "pointer" when index == 0:
                WritePopPointerSegment("THIS");
                break;
            case "pointer" when index == 1:
                WritePopPointerSegment("THAT");
                break;
            case "temp":
                WritePopTempSegment(index, moduleName);
                break;
            case "static":
                WritePopStaticSegment(index, moduleName);
                break;
            default:
                return false;
        }
        return true;
    }


    /// <summary>
    /// Генерирует код, для сохранения значения D регистра в стек
    /// </summary>
    private void WritePushD()
    {
        WriteAsm(
            "@SP    // Push D",
            "A=M",
            "M=D",
            "@SP",
            "M=M+1"
        );
    }

    /// <summary>
    /// Генерирует код, для извлечения из стека значения в D регистр
    /// </summary>
    private void WritePopToD()
    {
        WriteAsm(
            "@SP    // Pop To D",
            "M=M-1",
            "@SP",
            "A=M",
            "D=M"
        );
    }

    private void WritePushStaticSegment(int index, string moduleName)
    {
        WriteAsm(
            // D = RAM[moduleName.index]
            $"@{moduleName}.{index}",
            "D=M"
        );
        WritePushD();
    }

    private void WritePopStaticSegment(int index, string moduleName)
    {
        WritePopToD();
        WriteAsm(
            // RAM[moduleName.index] = D
            $"@{moduleName}.{index}",
            "M=D"
        );
    }

    private void WritePushConstantSegment(int number)
    {
        if (number == -32768)
        {
            WriteAsm("@32767", "D=A", "D=-D", "D=D-1");
        }
        else
        {
            WriteAsm(
                $"@{Math.Abs(number)}",
                "D=A"
            );

            if (number < 0)
                WriteAsm(
                    "D=-D"
                );
        }

        WritePushD();
    }

    private void WritePushTempSegment(int index)
    {
        WriteSetDToTempSegmentAddress(index);
        WriteAsm(
            // D = RAM[D]
            "A=D",
            "D=M"
        );
        WritePushD();
    }

    private void WritePopTempSegment(int index, string moduleName)
    {
        WriteSetDToTempSegmentAddress(index);
        WriteSaveDToTempVariable(moduleName);
        WritePopToD();
        WriteSetAddressByTempVariableToD(moduleName);
    }

    /// <param name="segment">THIS или THAT</param>
    private void WritePushPointerSegment(string segment)
    {
        // D = RAM[segment]
        WriteAsm(
            $"@{segment}",
            "D=M"
        );
        WritePushD();
    }

    /// <param name="segment">THIS или THAT</param>
    private void WritePopPointerSegment(string segment)
    {
        WritePopToD();
        WriteAsm(
            // segment = D
            $"@{segment}",
            "M=D"
        );
    }

    /// <summary>
    /// Генерирует код для установки регистра D = RAM[segment] + index
    /// </summary>
    private void WriteSetDToSegmentAddress(string segment, int index)
    {
        WriteAsm(
            $"@{segment}",
            "D=M",
            $"@{index}",
            "D=D+A"
         );
    }

    /// <summary>
    /// Генерирует код для установки значения регистра D = 5 + index
    /// </summary>
    private void WriteSetDToTempSegmentAddress(int index)
    {
        WriteAsm(
            $"@5",
            "D=A",
            $"@{index}",
            "D=D+A"
         );
    }

    private void WritePushSegment(string segment, int index)
    {
        WriteSetDToSegmentAddress(segment, index);
        WriteAsm(
            "A=D",
            "D=M"
        );
        WritePushD();
    }

    private void WritePopSegment(string segment, int index, string moduleName)
    {
        WriteSetDToSegmentAddress(segment, index);
        WriteSaveDToTempVariable(moduleName);
        WritePopToD();
        WriteSetAddressByTempVariableToD(moduleName);
    }

    /// <summary>
    /// Устанавивает RAM[moduleName.temp] = D
    /// </summary>
    private void WriteSetAddressByTempVariableToD(string moduleName)
    {
        WriteAsm(
            $"@{moduleName}.temp",
            "A=M",
            "M=D"
        );
    }

    /// <summary>
    /// Генерирует код для помещения регистра D во временную переменную: moduleName.temp = D
    /// </summary>
    private void WriteSaveDToTempVariable(string moduleName)
    {
        WriteAsm(
            $"@{moduleName}.temp",
            "M=D"
        );
    }
}