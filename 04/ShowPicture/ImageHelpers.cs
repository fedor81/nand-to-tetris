using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ShowPicture
{
    internal static class ImageHelpers
    {
        public static bool[,] LoadImagePixels(string filename) {
            bool[,] pixels;
            using (Image<Rgba32> image = Image.Load<Rgba32>(filename)) 
            {
                pixels = new bool[image.Height, image.Width];
                image.ProcessPixelRows(accessor =>
                {
                    for (int row = 0; row < accessor.Height; row++)
                    {
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(row);
                        for (int column = 0; column < pixelRow.Length; column++)
                        {
                            ref Rgba32 pixel = ref pixelRow[column];
                            pixels[row, column] = new Color(pixel) != Color.White;
                        }
                    }
                });
            }
            return pixels;
        }
    }
}
