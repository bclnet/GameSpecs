using ICSharpCode.SharpZipLib.Zip.Compression;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.IW.Zone
{
    public enum ZSTREAM
    {
        TEMP,
        PHYSICAL,
        RUNTIME,
        VIRTUAL,
        LARGE,
        CALLBACK,
        VERTEX,
        INDEX
    }

    public unsafe class ZStream : IDisposable
    {
        public const uint zero = 0;
        public const uint pad = 0xFFFFFFFF;

        public const int ALIGN_TO_1 = 0;
        public const int ALIGN_TO_2 = 1;
        public const int ALIGN_TO_4 = 3;
        public const int ALIGN_TO_8 = 7;
        public const int ALIGN_TO_16 = 15;
        public const int ALIGN_TO_32 = 31;
        public const int ALIGN_TO_64 = 63;
        public const int ALIGN_TO_128 = 127;

        int size;
        int offset;
        int location;
        byte[] origin;
        int maxsize;
        ZSTREAM curStream;
        int[] streamOffsets = new int[8];
        Stack<ZSTREAM> streamStack = new Stack<ZSTREAM>();

        unsafe struct XZoneMemory
        {
            public int zoneSize;
            public int unk1;
            public fixed int streams[8];
        }

        static XZoneMemory memory = new XZoneMemory(); // { 0, 0, { 0, 0, 0, 0, 0, 0, 0, 0 } };

        public ZStream(int scriptStrings, int assets)
        {
            const int SIZE = 0x10000000;
            origin = new byte[SIZE];
            location = 0;
            size = SIZE;
            offset = 0;
            maxsize = 0;

            pushStream(ZSTREAM.TEMP);

            fixed (XZoneMemory* _ = &memory) write(curStream, (byte*)_, sizeof(XZoneMemory), 1);

            write((byte*)&scriptStrings, 4, 1);
            write(scriptStrings > 0 ? pad : zero, 4, 1);

            write((byte*)&assets, 4, 1);
            write(assets > 0 ? pad : zero, 4, 1);
        }

        public void Dispose() { }

        public void resize(int newsize)
        {
            if (newsize == -1) newsize = maxsize;
            if (newsize < maxsize) return;

            var newdata = new byte[newsize];
            Array.Copy(origin, newdata, newsize);

            origin = newdata;
            size = newsize;
            location = offset;
        }

        public int getsize() => size;

        public int write(string val, int size, int count) => write(curStream, null, size, count);
        public int write(int val, int size, int count) => write(curStream, (byte*)val, size, count);
        public int write(uint val, int size, int count) => write(curStream, (byte*)val, size, count);
        public int write(byte* str, int size, int count) => write(curStream, (byte*)str, size, count);
        public int write(char* str, int size, int count) => write(curStream, (byte*)str, size, count);
        public int write(ZSTREAM stream, byte* str, int size, int count)
        {
            if (stream == ZSTREAM.RUNTIME)
            {
                streamOffsets[(int)stream] += size * count; // stay up to date on those streams
                return count;
            }

            if ((size * count) + location > this.size) resize(this.size + size * count + 2048);

            fixed (byte* _ = origin) Buffer.MemoryCopy(str, _ + location, origin.Length, size * count);
            offset += size * count;
            location += size * count;

            if (offset > maxsize) maxsize = offset;

            streamOffsets[(int)stream] += size * count; // stay up to date on those streams

            return count;
        }

        public int write(ZSTREAM stream, int value, int count)
        {
            var ret = 0;
            for (var i = 0; i < count; i++)
                ret += write(stream, (byte*)&value, 4, 1);
            return ret;
        }

        public Span<byte> at => origin.AsSpan(location);

        public byte[] data => origin;

        public void writetofile(Stream file)
        {
            file.Write(origin, 0, size);
            //fwrite(origin, size, 1, file);
        }

        public byte[] compressZlib() => GameX.Formats.Compression.CompressZlib(origin, size);

        public int getStreamOffset(ZSTREAM stream) => streamOffsets[(int)stream];

        public void updateStreamOffsetHeader()
        {
            fixed (byte* _ = data)
            {
                var mem = (XZoneMemory*)_;
                mem->zoneSize = getsize() - 39; // data length
                for (var i = 0; i < 8; i++)
                    mem->streams[i] = streamOffsets[i];
            }
        }

        public void pushStream(ZSTREAM stream)
        {
            streamStack.Push(curStream);
            curStream = stream;
        }

        public void popStream()
        {
            if (streamStack.Count == 0) Console.WriteLine("Tried to pop stream when no streams present on the stack!");
            curStream = streamStack.Pop();
        }

        public void align(int alignment)
            => streamOffsets[(int)curStream] = ~alignment & (alignment + streamOffsets[(int)curStream]);

        public void increaseStreamPos(int amt)
            => streamOffsets[(int)curStream] += amt;
    }
}