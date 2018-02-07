using System.Collections.Generic;

namespace Toore.ImageEncoders.Png
{
    public interface ICrcCalculator
    {
        uint CalculateCrc(IEnumerable<byte> data);
    }
}