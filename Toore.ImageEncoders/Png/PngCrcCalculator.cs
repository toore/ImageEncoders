using System.Collections.Generic;

namespace Toore.ImageEncoders.Png
{
    /// <summary>
    /// CRC calculator inspired by ANSI-C source code from: https://tools.ietf.org/html/rfc2083#page-94
    /// </summary>
    public class PngCrcCalculator : ICrcCalculator
    {
        private readonly IDictionary<byte, uint> _crcValues = new Dictionary<byte, uint>(256);

        public uint CalculateCrc(IEnumerable<byte> data)
        {
            uint crc = 0xffffffff;
            foreach (var b in data)
            {
                crc = GetCrc((byte)((crc ^ b) & 0xff)) ^ (crc >> 8);
            }

            return crc ^ 0xffffffff;
        }

        private uint GetCrc(byte value)
        {
            if (_crcValues.ContainsKey(value))
            {
                return _crcValues[value];
            }

            uint crc = value;
            for (var k = 0; k < 8; k++)
            {
                if ((crc & 1) == 1)
                {
                    crc = 0xedb88320 ^ (crc >> 1);
                }
                else
                {
                    crc = crc >> 1;
                }
            }

            _crcValues[value] = crc;
            return crc;
        }
    }
}