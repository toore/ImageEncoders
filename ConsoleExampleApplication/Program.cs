using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toore.ImageEncoders.Bmp;
using Toore.ImageEncoders.Core;
using Toore.ImageEncoders.Png;

namespace ConsoleExampleApplication
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var pixels = new List<IRgbColor>
                {
                    new RgbColor(0, 0, 255),
                    new RgbColor(0, 255, 0),
                    new RgbColor(255, 0, 0),
                    new RgbColor(255, 255, 255),
                };
            var rgbBitmap = new RgbBitmap(pixels, 2, 2);

            var pngImageEncoder = PngImageEncoderFactory.Create();
            File.WriteAllBytes("test.png", pngImageEncoder.Encode(rgbBitmap).ToArray());

            var bmpImageEncoder = new BmpImageEncoder();
            File.WriteAllBytes("test.bmp", bmpImageEncoder.Encode(rgbBitmap).ToArray());
        }
    }
}