using System.Collections.Generic;

namespace Toore.ImageEncoders.Core
{
    public static class RgbColorExtensions
    {
        /// <summary>
        /// Encodes an array of <see cref="RgbColor" /> to bytes.
        /// </summary>
        /// <param name="colors">The array of colors.</param>
        /// <returns>An array of bytes, where every <see cref="RgbColor" /> is encoded into three bytes [R, G, B].</returns>
        public static IEnumerable<byte> ToRgbEncodedBytes(this IEnumerable<IRgbColor> colors)
        {
            foreach (var color in colors)
            {
                yield return color.Red;
                yield return color.Green;
                yield return color.Blue;
            }
        }
    }
}