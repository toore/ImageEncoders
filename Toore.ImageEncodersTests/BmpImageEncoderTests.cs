using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Toore.ImageEncoders.Bmp;
using Toore.ImageEncoders.Core;
using Xunit;

namespace Toore.ImageEncodersTests
{
    public class BmpImageEncoderTests
    {
        private readonly BmpImageEncoder _sut;
        private const int Dword32Bit = 4;
        private readonly List<IRgbColor> _emptyImageData = new List<IRgbColor>(new IRgbColor[0]);

        public BmpImageEncoderTests()
        {
            _sut = new BmpImageEncoder();
        }

        [Fact]
        public void Asserts_bmp_header_identifier()
        {
            var bitmap = _sut.Encode(new RgbBitmap(new List<IRgbColor> { new RgbColor() }, 1, 1));

            bitmap[0].Should().Be((byte)'B');
            bitmap[1].Should().Be((byte)'M');
        }

        [Fact]
        public void Asserts_bmp_header_file_size()
        {
            var bitmap = _sut.Encode(new RgbBitmap(CreateRgbColors(512), 128, 4));

            // 54 + 512*3 = 1590 = 0x0636
            bitmap.SubBuffer(2, Dword32Bit).ShouldBeEquivalentTo(new byte[] { 0x36, 0x06, 0x00, 0x00 });
        }

        [Fact]
        public void Asserts_width()
        {
            var bitmap = _sut.Encode(new RgbBitmap(CreateRgbColors(256), 256, 1));

            bitmap.SubBuffer(18, Dword32Bit).ShouldBeEquivalentTo(new byte[] { 0, 1, 0, 0 });
        }

        [Fact]
        public void Asserts_height()
        {
            var bitmap = _sut.Encode(new RgbBitmap(CreateRgbColors(65536), 1, 65536));

            bitmap.SubBuffer(22, Dword32Bit).ShouldBeEquivalentTo(new byte[] { 0, 0, 1, 0 });
        }

        [Fact]
        public void Asserts_pixel_array_order()
        {
            var red = new byte[] { 255, 0, 0 };
            var white = new byte[] { 255, 255, 255 };
            var blue = new byte[] { 0, 0, 255 };
            var green = new byte[] { 0, 255, 0 };
            var padding = new byte[] { 0, 0 };
            const int rgbSize = 3;
            var pixels = new List<IRgbColor>
                {
                    new RgbColor(0, 0, 255),
                    new RgbColor(0, 255, 0),
                    new RgbColor(255, 0, 0),
                    new RgbColor(255, 255, 255)
                };

            var bitmap = _sut.Encode(new RgbBitmap(pixels, 2, 2));

            bitmap.SubBuffer(54, rgbSize).ShouldBeEquivalentTo(red);
            bitmap.SubBuffer(57, rgbSize).ShouldBeEquivalentTo(white);
            bitmap.SubBuffer(60, padding.Length).ShouldBeEquivalentTo(padding);
            bitmap.SubBuffer(62, rgbSize).ShouldBeEquivalentTo(blue);
            bitmap.SubBuffer(65, rgbSize).ShouldBeEquivalentTo(green);
            bitmap.SubBuffer(68, padding.Length).ShouldBeEquivalentTo(padding);
        }

        [Fact]
        public void Width_can_not_be_less_than_1()
        {
            Action act = () => _sut.Encode(new RgbBitmap(_emptyImageData, 0, 1));

            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Height_can_not_be_less_than_1()
        {
            Action act = () => _sut.Encode(new RgbBitmap(_emptyImageData, 1, 0));

            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        private static List<IRgbColor> CreateRgbColors(int size)
        {
            return Enumerable.Repeat(new RgbColor(), size)
                .Cast<IRgbColor>()
                .ToList();
        }
    }

    public static class ArrayExtensions
    {
        public static byte[] SubBuffer(this byte[] buffer, int index, int length)
        {
            var subBuffer = new byte[length];
            Buffer.BlockCopy(buffer, index, subBuffer, 0, length);
            return subBuffer;
        }
    }
}