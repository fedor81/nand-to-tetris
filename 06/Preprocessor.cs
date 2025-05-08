using System.Collections.Generic;
using System.Linq;
using System;

namespace Assembler
{
	public class Preprocessor
	{
		/// <summary>
		/// Преобразует нестандартные макро-инструкции в инструкции обычного языка ассемблера.
		/// </summary>
		public string[] PreprocessAsm(string[] instructions)
		{
			var asmCode = new List<string>();
			for (int i = 0; i < instructions.Length; i++)
			{
				var instr = instructions[i];
				try
				{
					TranslateInstruction(instr, asmCode);
				}
				catch (Exception e)
				{
					throw new FormatException($"Can't parse at line {i + 1}: {instr}", e);
				}
			}

			return asmCode.ToArray();
		}


		public void TranslateInstruction(string instruction, List<string> asmCode)
		{
			var queueInstructions = new Queue<string>();
			queueInstructions.Enqueue(instruction);

			// Проход по всем трансляторам инструкций
			foreach (var translator in translators)
			{
				var count = queueInstructions.Count;

				// Каждый транслятор получает по одной инструкций из очереди
				for (int i = 0; i < count; i++)
				{
					// Транслятор возвращает список получившихся инструкций
					foreach (var newInstruction in translator.TryTranstale(queueInstructions.Dequeue()))
						queueInstructions.Enqueue(newInstruction);
				}
			}

			asmCode.AddRange(queueInstructions);
		}


		private readonly List<IInstructionTranslator> translators = new()
		{
			new DRegisterJumpTranslator(),
			new SquareBracketsTranslator(),
		};
	}
}
