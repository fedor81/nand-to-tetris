﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler
{
    public interface IInstructionTranslator
    {
        string[] TryTranstale(string instruction);
    }
}
