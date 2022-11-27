using System.Collections.Generic;
using System.IO;

namespace GameSpec.IW.Formats
{
    public unsafe static class LibX
    {
        //public static int CountBytes(byte[] bytes, byte[] needle, int offset)
        //{
        //    int num = 0;
        //    int num3 = 0;

        //    var num7 = bytes.Length - 1;
        //    for (var j = offset; j <= num7; j++)
        //    {
        //        if (bytes[j] == needle[num3]) num3++;
        //        else num3 = 0;
        //        if (num3 >= needle.Length) { num++; num3 = 0; }
        //    }
        //    return num;
        //}

        //public static int CountBytes(byte[] bytes, byte[] needle, int offset, int endOffset)
        //{
        //    int num = 0;
        //    int num3 = 0;
        //    var num8 = endOffset;
        //    for (var i = offset; i <= num8; i++)
        //    {
        //        if (bytes[i] == needle[num3]) num3++;
        //        else num3 = 0;
        //        if (num3 >= needle.Length) { num++; num3 = 0; }
        //    }
        //    return num;
        //}

        //public static int FindBytes(byte[] bytes, byte[] needle, int offset)
        //{
        //    int num = 0;
        //    var num5 = bytes.Length - 1;
        //    for (var i = offset; i <= num5; i++)
        //    {
        //        if (bytes[i] == needle[num]) num++;
        //        else num = 0;
        //        if (num >= needle.Length) return i - (needle.Length - 1);
        //    }
        //    return -1;
        //}

        public static long[] FindBytes(this BinaryReader br, byte[] needle)
        {
            var list = new List<long>();
            var buffer = new byte[0x100000];
            int read, i, j = 0;
            var position = br.BaseStream.Position;
            while ((read = br.BaseStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                for (i = 0; i < read; i++)
                    if (needle[j] == buffer[i])
                    {
                        j++;
                        if (j == needle.Length) { list.Add(position + i + 1 - needle.Length); j = 0; }
                    }
                    else j = 0;
                position += read;
            }
            return list.ToArray();
        }
    }
}