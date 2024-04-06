using System;
using System.IO;

namespace Compression
{
    internal class Lzss
    {
        internal class BinaryReaderE : BinaryReader
        {
            public BinaryReaderE(Stream stream) : base(stream) { }
            public override short ReadInt16() { var numArray = ReadBytes(2); Array.Reverse((Array)numArray); return BitConverter.ToInt16(numArray, 0); }
            public override int ReadInt32() { var numArray = ReadBytes(4); Array.Reverse((Array)numArray); return BitConverter.ToInt32(numArray, 0); }
            public override uint ReadUInt32() { var numArray = ReadBytes(4); Array.Reverse((Array)numArray); return BitConverter.ToUInt32(numArray, 0); }
            public uint ReadUInt() => base.ReadUInt32();
        }

        int uncompressedSize;
        BinaryReaderE stream;
        const short DICT_SIZE = 4096;
        const short MIN_MATCH = 3;
        const short MAX_MATCH = 18;
        byte[] output;
        byte[] dictionary;
        short NR;
        short DO;
        short DI;
        int OI;

        public Lzss(BinaryReaderE stream, int uncompressedSize)
        {
            this.stream = stream;
            this.uncompressedSize = uncompressedSize;
        }

        bool LastByte() => stream.BaseStream.Position == stream.BaseStream.Length;

        void ClearDict()
        {
            for (var index = 0; index < DICT_SIZE; ++index) dictionary[index] = 32;
            DI = DICT_SIZE - MAX_MATCH;
        }

        byte ReadByte() { ++NR; return stream.ReadByte(); }

        byte[] ReadBytes(int bytes) => stream.ReadBytes(bytes);

        void WriteByte(byte b)
        {
            output[OI++] = b;
            dictionary[DI++ % DICT_SIZE] = b;
        }

        void WriteBytes(byte[] bytes)
        {
            foreach (var num in bytes)
            {
                if (OI >= uncompressedSize) break;
                output[OI++] = num;
            }
        }

        byte ReadDict() => dictionary[DO++ % DICT_SIZE];

        short ReadInt16() => stream.ReadInt16();

        void ReadBlock(int N)
        {
            NR = 0;
            if (N < 0) WriteBytes(ReadBytes(N * -1));
            else
            {
                ClearDict();
                while (NR < N && !LastByte())
                {
                    var num1 = ReadByte();
                    if (NR >= N || LastByte()) break;
                    for (var index1 = 0; index1 < 8; ++index1)
                    {
                        if (num1 % 2 == 1)
                        {
                            WriteByte(ReadByte());
                            if (NR >= N) return;
                        }
                        else
                        {
                            if (NR >= N) return;
                            DO = ReadByte();
                            if (NR >= N) return;
                            var num2 = ReadByte();
                            DO |= (short)((num2 & 240) << 4);
                            var num3 = ((int)num2 & 15) + MIN_MATCH;
                            for (var index2 = 0; index2 < num3; ++index2) WriteByte(ReadDict());
                        }
                        num1 >>= 1;
                        if (LastByte()) return;
                    }
                }
            }
        }

        public byte[] Decompress()
        {
            output = new byte[uncompressedSize];
            dictionary = new byte[DICT_SIZE];
            while (!LastByte())
            {
                var N = ReadInt16();
                if (N != 0) ReadBlock(N);
                else break;
            }
            return output;
        }
    }
}
