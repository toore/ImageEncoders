using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toore.ImageEncoders.Core;
using Toore.ImageEncoders.Png;
using Xunit;

namespace Toore.ImageEncodersTests
{
    public class PngImageEncoderTests
    {
        private readonly PngImageEncoder _sut;

        public PngImageEncoderTests()
        {
            _sut = PngImageEncoderFactory.Create();
        }

        [Fact]
        public void Asserts_image()
        {
            var pixels = new List<IRgbColor>
                {
                    new RgbColor(0, 0, 255),
                    new RgbColor(0, 255, 0),
                    new RgbColor(255, 0, 0),
                    new RgbColor(255, 255, 255),
                };

            var bitmap = _sut.Encode(new RgbBitmap(pixels, 2, 2));

            //var fileStream = File.OpenWrite("test.png");
            //var binaryWriter = new BinaryWriter(fileStream);
            //binaryWriter.Write(bitmap.ToArray());

            File.WriteAllBytes("test.png", bitmap.ToArray());
        }
    }
}