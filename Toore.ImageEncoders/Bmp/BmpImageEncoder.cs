using System;
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
        public byte[] Encode(IRgbBitmap bitmap)
        {
            // https://en.wikipedia.org/wiki/BMP_file_format

            const int bitsPerPixel = 24;
            var pixelArray = GetPixelArray(bitmap, bitsPerPixel);
            var rawBitmapDataSize = (uint)pixelArray.Length;
            var dibHeader = GetDibHeader(rawBitmapDataSize, (uint)bitmap.Width, (uint)bitmap.Height, bitsPerPixel);

            const int bitmapHeaderSize = 14;
            var bmpFileSize = (uint)(rawBitmapDataSize + dibHeader.Length + bitmapHeaderSize);
            var bmpHeader = GetBmpHeader(bmpFileSize);

            var bmpImage = bmpHeader
                .Concat(dibHeader)
                .Concat(pixelArray)
                .ToArray();

            return bmpImage;
        }

        private static byte[] GetPixelArray(IRgbBitmap bitmap, int bitsPerPixel)
        {
            var sourceRowSize = bitmap.Width * 3;
            var sourcePixelData = bitmap.Image.ToRgbEncodedBytes().ToArray();

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

        private static byte[] GetBmpHeader(UInt32 bmpFileSize)
        {
            byte[] bmpHeader =
                {
                    0x42, 0x4D, // "BM"
                    0x00, 0x00, 0x00, 0x00, // Size of the bmp file
                    0x00, 0x00, // Unused
                    0x00, 0x00, // Unused
                    0x36, 0x00, 0x00, 0x00, // Offset where the pixel array (bitmap data) can be found
                };

            bmpHeader[2] = (byte)bmpFileSize;
            bmpHeader[3] = (byte)(bmpFileSize >> 8);
            bmpHeader[4] = (byte)(bmpFileSize >> 16);
            bmpHeader[5] = (byte)(bmpFileSize >> 24);

            return bmpHeader;
        }

        private static byte[] GetDibHeader(UInt32 rawBitmapDataSize, uint width, uint height, int bitsPerPixel)
        {
            byte[] dibHeader =
                {
                    // DIB header
                    0x28, 0x00, 0x00, 0x00, // Number of bytes in the DIB header (from this point)
                    0x00, 0x00, 0x00, 0x00, // width
                    0x00, 0x00, 0x00, 0x00, // height
                    0x01, 0x00, // Number of color planes
                    0x18, 0x00, // Number of bits per pixel
                    0x00, 0x00, 0x00, 0x00, // BI_RGB, no pixel array compression used
                    0x00, 0x00, 0x00, 0x00, // Size of the raw bitmap data (including padding)
                    0x13, 0x0B, 0x00, 0x00, // Print resolution of the image,
                    0x13, 0x0B, 0x00, 0x00, // 72 DPI × 39.3701 inches per meter yields 2834.6472
                    0x00, 0x00, 0x00, 0x00, // Number of colors in the palette
                    0x00, 0x00, 0x00, 0x00, // 0 means all colors are important
                };

            WriteUInt(4, width);
            WriteUInt(8, height);
            WriteUShort(12, (ushort)bitsPerPixel);
            WriteUInt(20, rawBitmapDataSize);

            return dibHeader;

            void WriteUInt(int index, uint value)
            {
                dibHeader[index] = (byte)value;
                dibHeader[index + 1] = (byte)(value >> 8);
                dibHeader[index + 2] = (byte)(value >> 16);
                dibHeader[index + 3] = (byte)(value >> 24);
            }

            void WriteUShort(int index, ushort value)
            {
                dibHeader[index] = (byte)value;
                dibHeader[index + 1] = (byte)(value >> 8);
            }
        }
    }
}