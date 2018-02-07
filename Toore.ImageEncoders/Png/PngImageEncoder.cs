using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Toore.ImageEncoders.Core;

namespace Toore.ImageEncoders.Png
{
    public class PngImageEncoder
    {
        private readonly ICrcCalculator _crcCalculator;

        /// <summary>
        /// Initializes a new instance of <see cref="PngImageEncoder"/>.
        /// </summary>
        /// <param name="crcCalculator"></param>
        public PngImageEncoder(ICrcCalculator crcCalculator)
        {
            _crcCalculator = crcCalculator;
        }

        /// <summary>
        /// Encodes pixel information into a Portable Networks Graphics (PNG) image file.
        /// </summary>
        /// <param name="bitmap">A RGB bitmap.</param>
        /// <returns>A png image file</returns>
        public IEnumerable<byte> Encode(IRgbBitmap bitmap)
        {
            // https://en.wikipedia.org/wiki/Portable_Network_Graphics
            // https://tools.ietf.org/html/rfc2083

            return GetPngFileHeader()
                .Concat(GetIHDRChunk(bitmap))
                .Concat(GetIDATChunk(bitmap))
                .Concat(GetIENDChunk());
        }

        private static IEnumerable<byte> GetPngFileHeader()
        {
            return new byte[]
                {
                    0x89,
                    0x50, 0x4E, 0x47, // "PNG"
                    0x0D, 0x0A,
                    0x1A,
                    0x0A
                };
        }

        private IEnumerable<byte> GetIHDRChunk(IRgbBitmap bitmap)
        {
            var chunkData = GetWidthBytes(bitmap)
                .Concat(GetHeightBytes(bitmap))
                .Concat(new[]
                    {
                        (byte)BitDepth.BitsPerSampleIs8,
                        (byte)ColorType.ColorUsed,
                        (byte)CompressionMethod.DeflateInflateCompressionWith32KSlidingWindow,
                        (byte)FilterMethod.AdaptiveFilteringWithFiveBasicFilterTypes,
                        (byte)InterlaceMethod.NoInterlace
                    })
                .ToList();

            return CreateChunk(ChunkType.IHDR, chunkData);
        }

        private IEnumerable<byte> GetIDATChunk(IRgbBitmap bitmap)
        {
            var imageData = bitmap.Image
                .Chunk(bitmap.Width)
                .Select(EncodeScanLine)
                .Select(FilterImageData)
                .SelectMany(x => x)
                .ToList();

            var compressedImageData = CompressImageData(imageData).ToList();

            return CreateChunk(ChunkType.IDAT, compressedImageData);
        }

        private IEnumerable<byte> GetIENDChunk()
        {
            return CreateChunk(ChunkType.IEND, new byte[0]);
        }

        private static IEnumerable<byte> GetWidthBytes(IRgbBitmap bitmap)
        {
            return GetUIntMsb((uint)bitmap.Width);
        }

        private static IEnumerable<byte> GetHeightBytes(IRgbBitmap bitmap)
        {
            return GetUIntMsb((uint)bitmap.Height);
        }

        private static IEnumerable<byte> EncodeScanLine(IEnumerable<IRgbColor> colors)
        {
            return colors.ToRgbEncodedBytes();
        }

        private static IEnumerable<byte> FilterImageData(IEnumerable<byte> imageData)
        {
            return new[] { (byte)FilterType.None }
                .Concat(imageData);
        }

        private static IEnumerable<byte> CompressImageData(IList<byte> imageData)
        {
            var compressedImageData = DeflateCompress(imageData);

            const byte deflateCompression = 8;
            const byte checkHeaderBits = 31 - deflateCompression * 256 % 31;
            const byte flags = checkHeaderBits;

            return new[] { deflateCompression, flags }
                .Concat(compressedImageData)
                .Concat(CreateAdler32Checksum(imageData));
        }

        private static IEnumerable<byte> DeflateCompress(IEnumerable<byte> imageData)
        {
            using (var compressedStream = new MemoryStream())
            {
                using (var sourceStream = new MemoryStream(imageData.ToArray()))
                using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress))
                {
                    sourceStream.CopyTo(deflateStream);
                }

                return compressedStream.ToArray();
            }
        }

        private static IEnumerable<byte> CreateAdler32Checksum(IEnumerable<byte> imageData)
        {
            ushort s1 = 1;
            ushort s2 = 0;

            foreach (var b in imageData)
            {
                const ushort modulo = 65521;
                s1 += (ushort)(b % modulo);
                s2 += (ushort)(s1 % modulo);
            }

            return GetUIntMsb(s2, s1);
        }

        private IEnumerable<byte> CreateChunk(ChunkType chunkType, IList<byte> chunkData)
        {
            var chunkCrc = CalculateCrcData(((byte[])chunkType).Concat(chunkData));

            return GetChunkDataLengthBytes(chunkData)
                .Concat((byte[])chunkType)
                .Concat(chunkData)
                .Concat(chunkCrc);
        }

        private static IEnumerable<byte> GetChunkDataLengthBytes(IEnumerable<byte> chunkData)
        {
            return GetUIntMsb((uint)chunkData.Count());
        }

        private IEnumerable<byte> CalculateCrcData(IEnumerable<byte> data)
        {
            var crc = _crcCalculator.CalculateCrc(data);

            return GetUIntMsb(crc);
        }

        private static IEnumerable<byte> GetUIntMsb(uint value)
        {
            yield return (byte)(value >> 24);
            yield return (byte)(value >> 16);
            yield return (byte)(value >> 8);
            yield return (byte)value;
        }

        private static IEnumerable<byte> GetUIntMsb(ushort mostSignificant, ushort leastSignificant)
        {
            yield return (byte)(mostSignificant >> 8);
            yield return (byte)mostSignificant;
            yield return (byte)(leastSignificant >> 8);
            yield return (byte)leastSignificant;
        }
    }

    internal enum BitDepth
    {
        BitsPerSampleIs8 = 8
    }

    internal enum ColorType
    {
        ColorUsed = 2
    }

    internal enum CompressionMethod
    {
        DeflateInflateCompressionWith32KSlidingWindow = 0
    }

    internal enum FilterMethod
    {
        AdaptiveFilteringWithFiveBasicFilterTypes = 0
    }

    internal enum InterlaceMethod
    {
        NoInterlace = 0
    }

    internal enum FilterType
    {
        None = 0
    }

    internal class ChunkType
    {
        private readonly byte _first;
        private readonly byte _second;
        private readonly byte _third;
        private readonly byte _fourth;

        public static readonly ChunkType IHDR = new ChunkType(73, 72, 68, 82);
        public static readonly ChunkType IDAT = new ChunkType(73, 68, 65, 84);
        public static readonly ChunkType IEND = new ChunkType(73, 69, 78, 68);

        public static explicit operator byte[](ChunkType chunkType)
        {
            return new[]
                {
                    chunkType._first, chunkType._second, chunkType._third, chunkType._fourth
                };
        }

        private ChunkType(byte first, byte second, byte third, byte fourth)
        {
            _first = first;
            _second = second;
            _third = third;
            _fourth = fourth;
        }
    }
}