using System.Collections.Generic;

namespace Toore.ImageEncoders.Core
{
    /// <summary>
    /// A RGB encoded bitmap.
    /// </summary>
    public interface IRgbBitmap
    {
        /// <summary>The image is a rectangular pixel array, with pixels appearing left-to-right within each row, and rows appearing top-to-bottom.</summary>
        /// <returns>An array of pixels.</returns>
        IEnumerable<IRgbColor> Pixels { get; }

        /// <summary>Gets the width of the bitmap.</summary>
        /// <returns>The width of the bitmap.</returns>
        int Width { get; }

        /// <summary>Gets the height of the bitmap.</summary>
        /// <returns>The height of the bitmap.</returns>
        int Height { get; }
    }
}