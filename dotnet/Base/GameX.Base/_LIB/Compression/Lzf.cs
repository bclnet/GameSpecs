using System;

namespace Compression
{
    internal class Lzf
    {
        public static int Decompress(byte[] input, byte[] output)
        {
            int i = 0;
            int o = 0;
            int inputLength = input.Length;
            int outputLength = output.Length;
            while (i < inputLength)
            {
                var control = (uint)input[i++];
                if (control < (1 << 5))
                {
                    var length = (int)(control + 1);
                    if (o + length > outputLength)
                        throw new InvalidOperationException();

                    Array.Copy(input, i, output, o, length);
                    i += length;
                    o += length;
                }
                else
                {
                    var length = (int)(control >> 5);
                    if (length == 7)
                        length += input[i++];
                    length += 2;

                    var offset = (int)((control & 0x1F) << 8);
                    offset |= input[i++];
                    if (o + length > outputLength)
                        throw new InvalidOperationException();
                    offset = o - 1 - offset;
                    if (offset < 0)
                        throw new InvalidOperationException();

                    var block = Math.Min(length, o - offset);
                    Array.Copy(output, offset, output, o, block);
                    o += block;
                    offset += block;
                    length -= block;

                    while (length > 0)
                    {
                        output[o++] = output[offset++];
                        length--;
                    }
                }
            }
            return o;
        }
    }
}
