using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Toore.ImageEncoders.Core;
using Toore.ImageEncoders.Png;
using Xunit;

namespace Toore.ImageEncodersTests
{
    public abstract class PngImageEncoderTests
    {
        private readonly PngImageEncoder _sut;

        protected PngImageEncoderTests()
        {
            _sut = PngImageEncoderFactory.Create();
        }

        public class Header : PngImageEncoderTests
        {
            [Fact]
            public void Asserts_png_file_signature()
            {
                var pngMandatoryFileSignature = new byte[]
                    {
                        137, 80, 78, 71, 13, 10, 26, 10
                    };

                var bitmap = _sut.Encode(new RgbBitmap(new List<IRgbColor> { new RgbColor() }, 1, 1));

                bitmap.Take(8)
                    .Should()
                    .BeEquivalentTo(pngMandatoryFileSignature);
            }

            [Fact]
            public void Generates_test_image()
            {
                var pixels = new List<IRgbColor>
                    {
                        new RgbColor(0, 0, 255),
                        new RgbColor(0, 255, 0),
                        new RgbColor(255, 0, 0),
                        new RgbColor(255, 255, 255),
                    };

                var bitmap = _sut.Encode(new RgbBitmap(pixels, 2, 2));

                File.WriteAllBytes("test.png", bitmap.ToArray());
            }
        }

        public class IHDRChunk : PngImageEncoderTests
        {
            private readonly byte[] _IHDRChunkType = { 73, 72, 68, 82 };
            private const int WidthLength = 4;
            private const int HeightLength = 4;
            private const int BitDepthLength = 1;
            private const int ColorTypeLength = 1;
            private const int CompressionMethodLength = 1;
            private const int FilterMethodLength = 1;
            private const int ÍnterlaceMethodLength = 1;


            [Fact]
            public void Chunk_exists()
            {
                var bitmap = _sut.Encode(new RgbBitmap(new List<IRgbColor> { new RgbColor() }, 1, 1));

                var chunkBeginning = bitmap.SkipUntilPattern(_IHDRChunkType);

                chunkBeginning.Should().NotBeEmpty();
            }

            [Fact]
            public void Chunk_length_is_13_bytes()
            {
                var bitmap = _sut.Encode(new RgbBitmap(new List<IRgbColor> { new RgbColor() }, 1, 1));
                var uintMsbEncodedLength13 = new byte[] { 0, 0, 0, 13 };
                var IHDRLengthAndTypePattern = uintMsbEncodedLength13.Concat(_IHDRChunkType).ToList();

                var chunkBeginning = bitmap.SkipUntilPattern(IHDRLengthAndTypePattern);

                chunkBeginning.Should().NotBeEmpty();
            }

            [Fact]
            public void Assert_width()
            {
                var bitmap = _sut.Encode(new RgbBitmapBuilder().Width(10).Height(1).FillPixels().Create());

                var width = GetIHDRChunk(bitmap)
                    .Take(4)
                    .ToList();

                width.GetUIntMsb().Should().Be(10);
            }

            [Fact]
            public void Assert_height()
            {
                var bitmap = _sut.Encode(new RgbBitmapBuilder().Width(1).Height(10).FillPixels().Create());

                var height = GetIHDRChunk(bitmap)
                    .Skip(WidthLength)
                    .Take(4)
                    .ToList();

                height.GetUIntMsb().Should().Be(10);
            }

            [Fact]
            public void Assert_bit_depth()
            {
                const int expectedBitDepth = 8;

                var bitmap = _sut.Encode(new RgbBitmapBuilder().Width(1).Height(10).FillPixels().Create());

                var bitDepth = GetIHDRChunk(bitmap)
                    .Skip(WidthLength + HeightLength)
                    .ToList();

                bitDepth.First().Should().Be(expectedBitDepth);
            }

            [Fact]
            public void Assert_color_type()
            {
                const int colorIsUsed = 2;

                var bitmap = _sut.Encode(new RgbBitmapBuilder().Width(1).Height(10).FillPixels().Create());

                var bitDepth = GetIHDRChunk(bitmap)
                    .Skip(WidthLength + HeightLength + BitDepthLength)
                    .ToList();

                bitDepth.First().Should().Be(colorIsUsed);
            }

            [Fact]
            public void Assert_compression_method()
            {
                const int deflateInflateCompressionWith32KSlidingWindow = 0;

                var bitmap = _sut.Encode(new RgbBitmapBuilder().Width(1).Height(10).FillPixels().Create());

                var bitDepth = GetIHDRChunk(bitmap)
                    .Skip(WidthLength + HeightLength + BitDepthLength + ColorTypeLength)
                    .ToList();

                bitDepth.First().Should().Be(deflateInflateCompressionWith32KSlidingWindow);
            }

            [Fact]
            public void Assert_filter_method()
            {
                const int adaptiveFilteringWithFiveBasicFilterTypes = 0;

                var bitmap = _sut.Encode(new RgbBitmapBuilder().Width(1).Height(10).FillPixels().Create());

                var bitDepth = GetIHDRChunk(bitmap)
                    .Skip(WidthLength + HeightLength + BitDepthLength + ColorTypeLength + CompressionMethodLength)
                    .ToList();

                bitDepth.First().Should().Be(adaptiveFilteringWithFiveBasicFilterTypes);
            }

            [Fact]
            public void Assert_interlace_method()
            {
                const int noInterlace = 0;

                var bitmap = _sut.Encode(new RgbBitmapBuilder().Width(1).Height(10).FillPixels().Create());

                var bitDepth = GetIHDRChunk(bitmap)
                    .Skip(WidthLength + HeightLength + BitDepthLength + ColorTypeLength + CompressionMethodLength + FilterMethodLength)
                    .ToList();

                bitDepth.First().Should().Be(noInterlace);
            }

            private IEnumerable<byte> GetIHDRChunk(IEnumerable<byte> bitmap)
            {
                return bitmap.SkipUntilPattern(_IHDRChunkType).Skip(4).Take(13);
            }
        }
    }

    public class RgbBitmapBuilder
    {
        private int _width;
        private int _height;
        private IList<IRgbColor> _pixels;

        public IRgbBitmap Create()
        {
            return new RgbBitmap(_pixels, _width, _height);
        }

        public RgbBitmapBuilder Width(int width)
        {
            _width = width;
            return this;
        }

        public RgbBitmapBuilder Height(int height)
        {
            _height = height;
            return this;
        }

        public RgbBitmapBuilder FillPixels()
        {
            _pixels = Enumerable.Repeat<IRgbColor>(new RgbColor(), _width * _height).ToList();
            return this;
        }
    }

    public static class ListExtensions
    {
        public static uint GetUIntMsb(this IList<byte> data)
        {
            if (data.Count != 4)
            {
                throw new ArgumentException("Length must be 4", $"{nameof(data)}");
            }

            return (uint)(data[0] >> 24) +
                   (uint)(data[1] >> 16) +
                   (uint)(data[2] >> 8) +
                   data[3];
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> SkipUntilPattern<T>(this IEnumerable<T> source, IList<T> sequence)
        {
            var rest = source.SkipWhile(x => !x.Equals(sequence.First()))
                .ToList();

            while (rest.Any())
            {
                if (rest.Take(sequence.Count).SequenceEqual(sequence))
                {
                    return rest;
                }

                rest = rest.SkipWhile(x => !x.Equals(sequence.First()))
                    .ToList();
            }

            return Enumerable.Empty<T>();
        }
    }
}