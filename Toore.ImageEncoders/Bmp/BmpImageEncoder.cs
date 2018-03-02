using System;
using System.Collections.Generic;
using System.Linq;
using Toore.ImageEncoders.Core;

namespace Toore.ImageEncoders.Bmp
{
    public class BmpImageEncoder
    {
        /// <summary>
        /// Encodes pixel information into a bitmap image file.
        /// </summary>
        /// <param name="bitmap">A RGB bitmap.</param>
        /// <returns>A bitmap image file</returns>
        public IEnumerable<byte> Encode(IRgbBitmap bitmap)
        {
            // https://en.wikipedia.org/wiki/BMP_file_format

            const int bitsPerPixel = 24;
            var pixelArray = GetPixelArray(bitmap, bitsPerPixel);
            var rawBitmapDataSize = (uint)pixelArray.Length;
            var dibHeader = GetDibHeader(rawBitmapDataSize, (uint)bitmap.Width, (uint)bitmap.Height, bitsPerPixel).ToList();

            const int bitmapHeaderSize = 14;
            var bmpFileSize = (uint)(rawBitmapDataSize + dibHeader.Count + bitmapHeaderSize);
            var bmpHeader = GetBmpHeader(bmpFileSize);

            return bmpHeader
                .Concat(dibHeader)
                .Concat(pixelArray);
        }

        private static byte[] GetPixelArray(IRgbBitmap bitmap, int bitsPerPixel)
        {
            var sourceRowSize = bitmap.Width * 3;
            var sourcePixelData = bitmap.Pixels.ToRgbEncodedBytes().ToArray();

            var destinationRowSize = (bitsPerPixel * bitmap.Width + 31) / 32 * 4;
            var pixelArraySize = destinationRowSize * bitmap.Height;
            var pixelArray = new byte[pixelArraySize];

            for (var sourceRowIndex = 0; sourceRowIndex < bitmap.Height; sourceRowIndex++)
            {
                var sourceOffset = sourceRowIndex * bitmap.Width * 3;
                var destinationOffset = (bitmap.Height - sourceRowIndex - 1) * destinationRowSize;

                Buffer.BlockCopy(sourcePixelData, sourceOffset, pixelArray, destinationOffset, sourceRowSize);
            }

            return pixelArray;
        }

        private static IEnumerable<byte> GetBmpHeader(UInt32 bmpFileSize)
        {
            const byte B = 0x42;
            const byte M = 0x4D;
            var bmpIdentifier = new[] { B, M };
            var unusedBytes = new byte[4];
            var pixelArrayOffset = GetUIntLsb(54);

            return bmpIdentifier
                .Concat(GetUIntLsb(bmpFileSize))
                .Concat(unusedBytes)
                .Concat(pixelArrayOffset);
        }

        private static IEnumerable<byte> GetDibHeader(UInt32 rawBitmapDataSize, uint width, uint height, ushort numberOfBitsPerPixel)
        {
            var dibHeaderSize = GetUIntLsb(40);
            const ushort numberOfColorPlanes = 1;
            var noPixelCompressionUsed = new byte[4];
            const uint resolutionInPixelPerMeter = (uint)(72 * 39.3701);
            const uint horizontalPrintResolution = resolutionInPixelPerMeter;
            const uint verticalPrintResolution = resolutionInPixelPerMeter;
            const uint numberOfColorsInThePalette = 0;
            const uint allColorsAreImportant = 0;

            return dibHeaderSize
                .Concat(GetUIntLsb(width))
                .Concat(GetUIntLsb(height))
                .Concat(GetUShortLsb(numberOfColorPlanes))
                .Concat(GetUShortLsb(numberOfBitsPerPixel))
                .Concat(noPixelCompressionUsed)
                .Concat(GetUIntLsb(rawBitmapDataSize))
                .Concat(GetUIntLsb(horizontalPrintResolution))
                .Concat(GetUIntLsb(verticalPrintResolution))
                .Concat(GetUIntLsb(numberOfColorsInThePalette))
                .Concat(GetUIntLsb(allColorsAreImportant));
        }

        private static IEnumerable<byte> GetUIntLsb(uint value)
        {
            yield return (byte)value;
            yield return (byte)(value >> 8);
            yield return (byte)(value >> 16);
            yield return (byte)(value >> 24);
        }

        private static IEnumerable<byte> GetUShortLsb(ushort value)
        {
            yield return (byte)value;
            yield return (byte)(value >> 8);
        }
    }
}