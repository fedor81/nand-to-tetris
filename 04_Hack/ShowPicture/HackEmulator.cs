using System;
using System.Text;

namespace ShowPicture
{
    public class HackEmulator
    {
        private readonly string[] rom;
        public short[] Ram { get; private set; } = new short[24577];
        public short D { get; private set; }
        public short A { get; private set; }

        public short M
        {
            get => Ram[A];
            set => Ram[A] = value;
        }

        public int Pc { get; private set; }


        public HackEmulator(string[] rom, short ramInitValue = 0)
        {
            this.rom = rom;
            if (ramInitValue != 0)
                for (int i = 0; i < Ram.Length; i++)
                    Ram[i] = ramInitValue;
        }

        public void Ticks(int ticksCount)
        {
            for (int i = 0; i < ticksCount; i++)
            {
                if (Pc >= rom.Length) break;
                Tick();
            }
        }

        public void Tick()
        {
            var instr = rom[Pc];
            if (instr[0] == '0')
            {
                A = Convert.ToInt16(instr, 2);
                Pc += 1;
            }
            else
            {
                var a = instr[3];
                var comp = instr[4..10];
                var dest = instr[10..13];
                var jump = instr[13..16];
                var compResult = Compute(comp, a == '1' ? M : A);
                SetDest(dest, compResult);
                Jump(jump, compResult);
            }
        }

        private void Jump(string jump, short compResult)
        {
            var shouldJump =
                jump[0] == '1' && compResult < 0 ||
                jump[1] == '1' && compResult == 0 ||
                jump[2] == '1' && compResult > 0;
            Pc = shouldJump ? A : Pc + 1;
        }

        private void SetDest(string dest, short value)
        {
            if (dest[2] == '1') M = value;
            if (dest[1] == '1') D = value;
            if (dest[0] == '1') A = value;
        }

        private short Compute(string comp, short am)
        {
            var x = comp[0] == '1' ? 0 : D;
            x = comp[1] == '1' ? ~x : x;
            var y = comp[2] == '1' ? 0 : am;
            y = comp[3] == '1' ? ~y : y;
            var res = comp[4] == '1' ? x + y : x & y;
            res = comp[5] == '1' ? ~res : res;
            return (short)res;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"PC: {Pc}: {rom[Pc]}");
            sb.AppendLine($"D: {D}    A: {A}     M: {SafeGetM() ?? "[A is not valid address]"}");
            sb.AppendLine("RAM (zero registers are omitted):");
            for (int i = 0; i < Ram.Length; i++)
            {
                if (i < 20 || Ram[i] != 0)
                {
                    sb.Append($"RAM[{i}] = {Ram[i]}");
                    if (i == Ram[0]) Console.Write(" <- SP");
                    if (i == Ram[1]) Console.Write(" <- LCL");
                    if (i == Ram[2]) Console.Write(" <- ARG");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private string SafeGetM()
        {
            if (A >= 0 && A < Ram.Length)
                return Ram[A].ToString();
            else
                return null;
        }
    }
}
