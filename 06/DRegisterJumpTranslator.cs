using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler
{
    public class DRegisterJumpTranslator : IInstructionTranslator
    {
        public string[] TryTranstale(string instruction)
        {
            if (instruction.Length >= 3)
            {
                var jump = instruction[..3];

                if (jump == "JMP")
                    return new string[] { "0;" + instruction };
                else if (jumpInstructions.Contains(jump))
                    return new string[] { "D;" + instruction };
            }

            return new string[] { instruction };
        }

        private static HashSet<string> jumpInstructions = new()
        {
            { "JGT" },
            { "JEQ" },
            { "JGE" },
            { "JLT" },
            { "JNE" },
            { "JLE" },
        };
    }
}
