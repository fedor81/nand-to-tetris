using System;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Транслирует инструкции:
    /// * арифметических операция: add sub, neg
    /// * логических операций: eq, gt, lt, and, or, not
    /// </summary>
    /// <returns>true − если это логическая или арифметическая инструкция, иначе — false.</returns>
    private bool TryWriteLogicAndArithmeticCode(VmInstruction instruction)
    {
        return instruction.Args.Length == 0 && instruction.Name switch
        {
            "add" => WriteOperationWithTwoItems('+'),
            "sub" => WriteOperationWithTwoItems('-'),
            "neg" => WriteOperationWithOneItem('-'),
            "eq" => WriteOperationComparison("JEQ"),
            "gt" => WriteOperationComparison("JGT"),
            "lt" => WriteOperationComparison("JLT"),
            "and" => WriteOperationWithTwoItems('&'),
            "or" => WriteOperationWithTwoItems('|'),
            "not" => WriteOperationWithOneItem('!'),
            "inc" => WriteDAndOne("+"),
            "dec" => WriteDAndOne("-"),
            _ => false
        };
    }

    private bool WriteDAndOne(string operation)
    {
        WritePopToD();
        WriteAsm($"D=M{operation}1");
        WritePushD();
        return true;
    }

    private bool WriteOperationWithTwoItems(char operation)
    {
        WriteOperationWithTwoItemsAndSaveToD(operation);
        WritePushD();
        return true;
    }

    private void WriteOperationWithTwoItemsAndSaveToD(char operation)
    {
        WritePopToD();
        WriteAsm(
            "@SP",
            "AM=M-1",
            $"D=M{operation}D"
        );
    }

    private bool WriteOperationWithOneItem(char operation)
    {
        WritePopToD();
        WriteAsm($"D={operation}M");
        WritePushD();
        return true;
    }

    private bool WriteOperationComparison(string jump)
    {
        WriteOperationWithTwoItemsAndSaveToD('-');
        WriteAsm(
            "@SP",
            "A=M",
            "M=-1",
            $"@{ConstructLabel(labelNumber)}",
            $"D; {jump}",
            "@SP",
            "A=M",
            "M=0",
            $"({ConstructLabel(labelNumber)})",
			"@SP",
			"M=M+1"
        );
        labelNumber++;
        return true;
    }

    private int labelNumber = 0;
    private readonly Func<int, string> ConstructLabel = (number) => $"VmLabel{number}";
}
