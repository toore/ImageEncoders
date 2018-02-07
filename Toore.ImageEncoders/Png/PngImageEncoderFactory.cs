namespace Toore.ImageEncoders.Png
{
    public static class PngImageEncoderFactory
    {
        public static PngImageEncoder Create()
        {
            return new PngImageEncoder(new PngCrcCalculator());
        }
    }
}