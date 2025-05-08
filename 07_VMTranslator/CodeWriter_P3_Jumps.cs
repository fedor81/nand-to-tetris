using System;

namespace VMTranslator;

public partial class CodeWriter
{
	/// <summary>
	/// Транслирует инструкции: label, goto, if-goto
	/// </summary>
	private bool TryWriteProgramFlowCode(VmInstruction instruction, string moduleName)
	{
		if (instruction.Args.Length != 1)
			return false;

		var label = instruction.Args[0];

		return instruction.Name switch
		{
			"label" => WriteLabel(label),
			"goto" => WriteGoTo(label),
			"if-goto" => WriteIfGoTo(label),
			_ => false,
		};
	}

	private bool WriteIfGoTo(string label)
	{
		WritePopToD();
		WriteAsm(
			$"@{label}",
			"D; JNE"
		);
		return true;
	}

	private bool WriteGoTo(string label)
	{
		WriteAsm(
			$"@{label}",
			"0; JMP"
		);
		return true;
	}

	private bool WriteLabel(string label)
	{
		WriteAsm($"({label})");
		return true;
	}
}
