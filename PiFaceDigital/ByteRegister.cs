using System;
using System.Collections;

namespace PiFace
{
    public class ByteRegister
    {
        private BitArray bits = new BitArray(8);
        private readonly int bitCount = 8;

        public event Action<int, bool> ValueChange = null;
        public ByteRegister()
        { }

        public bool GetBit(int bit)
        {
            if (bit < 0 || bit >= bitCount)
                throw new ArgumentException($"bit index must be between 0 and {bitCount}");
            return bits[bit];
        }

        public void SetBit(int bit, bool value)
        {
            if (bit < 0 || bit >= bitCount)
                throw new ArgumentException($"bit index must be between 0 and {bitCount}");

            if (bits[bit] != value)
            {
                bits[bit] = value;
                ValueChange?.Invoke(bit, value);
            }
        }

        public byte ToByte()
        {
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }

        public void FromByte(byte init)
        {
            if (init != ToByte())
            {
                BitArray newValue = new(new[] { init });
                for (int i = 0; i < bits.Length; i++)
                {
                    if (newValue[i] != bits[i])
                        ValueChange?.Invoke(i, newValue[i]);
                }

                bits = newValue;
            }
        }
    }
}
