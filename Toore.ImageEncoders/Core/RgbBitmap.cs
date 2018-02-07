using System;
using System.Collections.Generic;

namespace Toore.ImageEncoders.Core
{
    /// <summary>
    /// A RGB encoded pixel.
    /// </summary>
    public struct RgbColor : IRgbColor
    {
        /// <summary>Gets the red component value of the color.</summary>
        /// <returns>The red component byte value.</returns>
        public byte Red { get; }

        /// <summary>Gets the green component value of the color.</summary>
        /// <returns>The green component byte value.</returns>
        public byte Green { get; }

        /// <summary>Gets the blue component value of the color.</summary>
        /// <returns>The blue component byte value.</returns>
        public byte Blue { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="RgbColor" /> with red, green and blue components.
        /// </summary>
        /// <param name="red">The red component value.</param>
        /// <param name="green">The green component value.</param>
        /// <param name="blue">The blue component value.</param>
        public RgbColor(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }
    }

    /// <summary>
    /// A RGB encoded bitmap.
    /// </summary>
    public class RgbBitmap : IRgbBitmap
    {
        private readonly IList<IRgbColor> _image;

        /// <summary>The image is a rectangular pixel array, with pixels appearing left-to-right within each row, and rows appearing top-to-bottom.</summary>
        /// <returns>An array of pixels.</returns>
        public IEnumerable<IRgbColor> Image => _image;

        /// <summary>Gets the width of the bitmap.</summary>
        /// <returns>The width of the bitmap.</returns>
        public int Width { get; }

        /// <summary>Gets the height of the bitmap.</summary>
        /// <returns>The height of the bitmap.</returns>
        public int Height { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RgbBitmap" /> class with pixels, width and height.
        /// </summary>
        /// <param name="image">The image is a rectangular pixel array, with pixels appearing left-to-right within each row, and rows appearing top-to-bottom.</param>
        /// <param name="width">The width of the bitmap.</param>
        /// <param name="height">The height of the bitmap.</param>
        public RgbBitmap(IList<IRgbColor> image, int width, int height)
        {
            if (width < 1)
            {
                throw new ArgumentOutOfRangeException($"{nameof(width)}", width, "Parameter must be greater than zero.");
            }

            if (height < 1)
            {
                throw new ArgumentOutOfRangeException($"{nameof(height)}", height, "Parameter must be greater than zero.");
            }

            if (image.Count != width * height)
            {
                throw new InvalidOperationException("Actual image size does not correspond to desired image size");
            }

            _image = image;
            Width = width;
            Height = height;
        }
    }
}