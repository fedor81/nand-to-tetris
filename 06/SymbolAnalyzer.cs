using System.Collections.Generic;
using System.Dynamic;

namespace Assembler
{
    public class SymbolAnalyzer
    {
        private Dictionary<string, int> GetPredefinedSymbols()
        {
            var predefinedSymbols = new Dictionary<string, int>()
            {
                { "SCREEN", 16384 },
                { "KBD", 24576 },
                { "SP", 0 },
                { "LCL", 1 },
                { "ARG", 2 },
                { "THIS", 3 },
                { "THAT", 4 },
            };

            for (var i = 0; i < 16; i++)
                predefinedSymbols.Add("R" + i, i);

            return predefinedSymbols;
        }


        /// <summary>
        /// Находит все метки в ассемблерном коде, удаляет их из кода и вносит их адреса в таблицу символов.
        /// </summary>
        /// <param name="instructionsWithLabels">Ассемблерный код, возможно, содержащий метки</param>
        /// <param name="instructionsWithoutLabels">Ассемблерный код без меток</param>
        /// <returns>
        /// Таблица символов, содержащая все стандартные предопределенные символы (R0−R15, SCREEN, ...),
        /// а также все найденные в программе метки.
        /// </returns>
        public Dictionary<string, int> CreateSymbolsTable(string[] instructionsWithLabels,
            out string[] instructionsWithoutLabels)
        {
            var symbols = GetPredefinedSymbols();
            var counter = 0;

            instructionsWithoutLabels = instructionsWithLabels.Where(instruction => 
            {
                if (instruction.StartsWith('('))
                {
                    var label = instruction[1..^1];
                    symbols[label] = counter;
                    return false;
                }
                else
                    counter++;
                return true;
            }).ToArray();

            return symbols;
        }
    }
}
