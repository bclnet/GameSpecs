using System.IO;

namespace OpenStack.Graphics.DirectX
{
    public static class DxtUtil
    {
        internal static byte[] DecompressDxt1(byte[] data, int width, int height)
        {
            using MemoryStream s = new MemoryStream(data);
            return DecompressDxt1(s, width, height);
        }

        internal static byte[] DecompressDxt1(Stream s, int width, int height)
        {
            var data = new byte[width * height * 4];
            using BinaryReader sr = new BinaryReader(s);
            int blockCountX = (width + 3) / 4, blockCountY = (height + 3) / 4;
            for (var y = 0; y < blockCountY; y++)
                for (var x = 0; x < blockCountX; x++) DecompressDxt1Block(sr, x, y, blockCountX, width, height, data);
            return data;
        }

        private static void DecompressDxt1Block(BinaryReader sr, int x, int y, int blockCountX, int width, int height, byte[] data)
        {
            var c0 = sr.ReadUInt16();
            var c1 = sr.ReadUInt16();
            ConvertRgb565ToRgb888(c0, out byte r0, out byte g0, out byte b0);
            ConvertRgb565ToRgb888(c1, out byte r1, out byte g1, out byte b1);
            var lookupTable = sr.ReadUInt32();
            for (var blockY = 0; blockY < 4; blockY++)
                for (var blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 0, g = 0, b = 0, a = 255;
                    var index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;
                    if (c0 > c1)
                        switch (index)
                        {
                            case 0: r = r0; g = g0; b = b0; break;
                            case 1: r = r1; g = g1; b = b1; break;
                            case 2: r = (byte)((2 * r0 + r1) / 3); g = (byte)((2 * g0 + g1) / 3); b = (byte)((2 * b0 + b1) / 3); break;
                            case 3: r = (byte)((r0 + 2 * r1) / 3); g = (byte)((g0 + 2 * g1) / 3); b = (byte)((b0 + 2 * b1) / 3); break;
                        }
                    else
                        switch (index)
                        {
                            case 0: r = r0; g = g0; b = b0; break;
                            case 1: r = r1; g = g1; b = b1; break;
                            case 2: r = (byte)((r0 + r1) / 2); g = (byte)((g0 + g1) / 2); b = (byte)((b0 + b1) / 2); break;
                            case 3: r = 0; g = 0; b = 0; a = 0; break;
                        }

                    int px = (x << 2) + blockX, py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        var offset = ((py * width) + px) << 2;
                        data[offset] = r;
                        data[offset + 1] = g;
                        data[offset + 2] = b;
                        data[offset + 3] = a;
                    }
                }
        }

        static byte[] DecompressDxt3(byte[] data, int width, int height)
        {
            using MemoryStream s = new MemoryStream(data);
            return DecompressDxt3(s, width, height);
        }

        static byte[] DecompressDxt3(Stream s, int width, int height)
        {
            var data = new byte[width * height * 4];
            using BinaryReader r = new BinaryReader(s);
            int blockCountX = (width + 3) / 4, blockCountY = (height + 3) / 4;
            for (var y = 0; y < blockCountY; y++)
                for (var x = 0; x < blockCountX; x++) DecompressDxt3Block(r, x, y, blockCountX, width, height, data);
            return data;
        }

        static void DecompressDxt3Block(BinaryReader sr, int x, int y, int blockCountX, int width, int height, byte[] data)
        {
            byte a0 = sr.ReadByte(), a1 = sr.ReadByte(), a2 = sr.ReadByte(), a3 = sr.ReadByte(),
                a4 = sr.ReadByte(), a5 = sr.ReadByte(), a6 = sr.ReadByte(), a7 = sr.ReadByte();
            ushort c0 = sr.ReadUInt16(), c1 = sr.ReadUInt16();
            ConvertRgb565ToRgb888(c0, out var r0, out var g0, out var b0);
            ConvertRgb565ToRgb888(c1, out var r1, out var g1, out var b1);
            var lookupTable = sr.ReadUInt32();
            var alphaIndex = 0;
            for (var blockY = 0; blockY < 4; blockY++)
            {
                for (var blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 0, g = 0, b = 0, a = 0;
                    var index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;
                    switch (alphaIndex)
                    {
                        case 0: a = (byte)((a0 & 0x0F) | ((a0 & 0x0F) << 4)); break;
                        case 1: a = (byte)((a0 & 0xF0) | ((a0 & 0xF0) >> 4)); break;
                        case 2: a = (byte)((a1 & 0x0F) | ((a1 & 0x0F) << 4)); break;
                        case 3: a = (byte)((a1 & 0xF0) | ((a1 & 0xF0) >> 4)); break;
                        case 4: a = (byte)((a2 & 0x0F) | ((a2 & 0x0F) << 4)); break;
                        case 5: a = (byte)((a2 & 0xF0) | ((a2 & 0xF0) >> 4)); break;
                        case 6: a = (byte)((a3 & 0x0F) | ((a3 & 0x0F) << 4)); break;
                        case 7: a = (byte)((a3 & 0xF0) | ((a3 & 0xF0) >> 4)); break;
                        case 8: a = (byte)((a4 & 0x0F) | ((a4 & 0x0F) << 4)); break;
                        case 9: a = (byte)((a4 & 0xF0) | ((a4 & 0xF0) >> 4)); break;
                        case 10: a = (byte)((a5 & 0x0F) | ((a5 & 0x0F) << 4)); break;
                        case 11: a = (byte)((a5 & 0xF0) | ((a5 & 0xF0) >> 4)); break;
                        case 12: a = (byte)((a6 & 0x0F) | ((a6 & 0x0F) << 4)); break;
                        case 13: a = (byte)((a6 & 0xF0) | ((a6 & 0xF0) >> 4)); break;
                        case 14: a = (byte)((a7 & 0x0F) | ((a7 & 0x0F) << 4)); break;
                        case 15: a = (byte)((a7 & 0xF0) | ((a7 & 0xF0) >> 4)); break;
                    }
                    ++alphaIndex;
                    switch (index)
                    {
                        case 0: r = r0; g = g0; b = b0; break;
                        case 1: r = r1; g = g1; b = b1; break;
                        case 2: r = (byte)((2 * r0 + r1) / 3); g = (byte)((2 * g0 + g1) / 3); b = (byte)((2 * b0 + b1) / 3); break;
                        case 3: r = (byte)((r0 + 2 * r1) / 3); g = (byte)((g0 + 2 * g1) / 3); b = (byte)((b0 + 2 * b1) / 3); break;
                    }
                    int px = (x << 2) + blockX, py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        var offset = ((py * width) + px) << 2;
                        data[offset] = r;
                        data[offset + 1] = g;
                        data[offset + 2] = b;
                        data[offset + 3] = a;
                    }
                }
            }
        }

        static byte[] DecompressDxt5(byte[] data, int width, int height)
        {
            using MemoryStream s = new MemoryStream(data);
            return DecompressDxt5(s, width, height);
        }

        static byte[] DecompressDxt5(Stream s, int width, int height)
        {
            var data = new byte[width * height * 4];
            using BinaryReader sr = new BinaryReader(s);
            int blockCountX = (width + 3) / 4, blockCountY = (height + 3) / 4;
            for (var y = 0; y < blockCountY; y++)
                for (var x = 0; x < blockCountX; x++) DecompressDxt5Block(sr, x, y, blockCountX, width, height, data);
            return data;
        }

        static void DecompressDxt5Block(BinaryReader sr, int x, int y, int blockCountX, int width, int height, byte[] data)
        {
            byte alpha0 = sr.ReadByte(), alpha1 = sr.ReadByte();
            var alphaMask = (ulong)sr.ReadByte();
            alphaMask += (ulong)sr.ReadByte() << 8;
            alphaMask += (ulong)sr.ReadByte() << 16;
            alphaMask += (ulong)sr.ReadByte() << 24;
            alphaMask += (ulong)sr.ReadByte() << 32;
            alphaMask += (ulong)sr.ReadByte() << 40;
            ushort c0 = sr.ReadUInt16(), c1 = sr.ReadUInt16();
            ConvertRgb565ToRgb888(c0, out var r0, out var g0, out var b0);
            ConvertRgb565ToRgb888(c1, out var r1, out var g1, out var b1);
            var lookupTable = sr.ReadUInt32();
            for (var blockY = 0; blockY < 4; blockY++)
                for (var blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 0, g = 0, b = 0, a = 255;
                    var index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;
                    var alphaIndex = (uint)((alphaMask >> 3 * (4 * blockY + blockX)) & 0x07);
                    if (alphaIndex == 0) a = alpha0;
                    else if (alphaIndex == 1) a = alpha1;
                    else if (alpha0 > alpha1) a = (byte)(((8 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 7);
                    else if (alphaIndex == 6) a = 0;
                    else if (alphaIndex == 7) a = 0xff;
                    else a = (byte)(((6 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 5);
                    switch (index)
                    {
                        case 0: r = r0; g = g0; b = b0; break;
                        case 1: r = r1; g = g1; b = b1; break;
                        case 2: r = (byte)((2 * r0 + r1) / 3); g = (byte)((2 * g0 + g1) / 3); b = (byte)((2 * b0 + b1) / 3); break;
                        case 3: r = (byte)((r0 + 2 * r1) / 3); g = (byte)((g0 + 2 * g1) / 3); b = (byte)((b0 + 2 * b1) / 3); break;
                    }
                    int px = (x << 2) + blockX, py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        var offset = ((py * width) + px) << 2;
                        data[offset] = r;
                        data[offset + 1] = g;
                        data[offset + 2] = b;
                        data[offset + 3] = a;
                    }
                }
        }

        static void ConvertRgb565ToRgb888(ushort color, out byte r, out byte g, out byte b)
        {
            var temp = (color >> 11) * 255 + 16;
            r = (byte)((temp / 32 + temp) / 32); temp = ((color & 0x07E0) >> 5) * 255 + 32;
            g = (byte)((temp / 64 + temp) / 64); temp = (color & 0x001F) * 255 + 16;
            b = (byte)((temp / 32 + temp) / 32);
        }
    }
}
