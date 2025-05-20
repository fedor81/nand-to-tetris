using System;
using System.Collections.Generic;

namespace ShowPicture
{
    public static class ShowPictureTask
    {
        private const int ScreenAddress = 16384;

        public static string[] GenerateShowPictureCode(bool[,] pixels)
        {
            var countBits = 16;
            var result = new List<string>();

            for (var y = 0; y < pixels.GetLength(1); y++)
            {
                var pixelsCode = 0;

                for (var x = 0; x < pixels.GetLength(0); x++)
                {
                    var pixel = pixels[x, y];

                    if (pixel)
                    {
                        var degree = countBits - x % countBits;
                        pixelsCode += (int)Math.Pow(2, degree);
                    }

                    if ((x + 1) % countBits == 0 && x != 0)
                    {
                        var address = ScreenAddress + y * 32 + x / countBits;
                        result.Add($"@{pixelsCode}");
                        result.Add("D = M");
                        result.Add($"@{address}");
                        result.Add("M = D");
                        pixelsCode = 0;
                    }
                }
            }

            return result.ToArray();
        }

        private static string Format(int pixels, int address)
        {
            return $"@{pixels}\nD = A\n@{address}\nM = D";
        }
    }
}
