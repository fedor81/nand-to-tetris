using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler
{
    public class SquareBracketsTranslator : IInstructionTranslator
    {
        public string[] TryTranstale(string instruction)
        {
            var leftBracket = instruction.IndexOf('[');
            var rightBracket = instruction.IndexOf(']');

            if (leftBracket == -1 || rightBracket == -1)
                return new string[] { instruction };

            string address = instruction.Substring(leftBracket + 1, rightBracket - leftBracket - 1);

            return new string[] {
                "@" + address,
                instruction.Replace($"[{address}]", "")
            };
        }
    }

}
