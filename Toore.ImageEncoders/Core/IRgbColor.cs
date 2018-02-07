namespace Toore.ImageEncoders.Core
{
    /// <summary>
    /// A RGB encoded pixel.
    /// </summary>
    public interface IRgbColor
    {
        /// <summary>Gets the red component value of the color.</summary>
        /// <returns>The red component byte value.</returns>
        byte Red { get; }

        /// <summary>Gets the green component value of the color.</summary>
        /// <returns>The green component byte value.</returns>
        byte Green { get; }

        /// <summary>Gets the blue component value of the color.</summary>
        /// <returns>The blue component byte value.</returns>
        byte Blue { get; }
    }
}