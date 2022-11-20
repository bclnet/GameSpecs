namespace GameSpec.IW.Formats
{
    public unsafe class LibX
    {
        public static int CountBytes(byte[] bytes, byte[] needle, int offset)
        {
            int num = 0;
            int num3 = 0;

            var num7 = bytes.Length - 1;
            for (var j = offset; j <= num7; j++)
            {
                if (bytes[j] == needle[num3]) num3++;
                else num3 = 0;
                if (num3 >= needle.Length) { num++; num3 = 0; }
            }
            return num;
        }

        public static int CountBytes(byte[] bytes, byte[] needle, int offset, int endOffset)
        {
            int num = 0;
            int num3 = 0;
            var num8 = endOffset;
            for (var i = offset; i <= num8; i++)
            {
                if (bytes[i] == needle[num3]) num3++;
                else num3 = 0;
                if (num3 >= needle.Length) { num++; num3 = 0; }
            }
            return num;
        }

        public static int FindBytes(byte[] bytes, byte[] needle, int offset)
        {
            int num = 0;
            var num5 = bytes.Length - 1;
            for (var i = offset; i <= num5; i++)
            {
                if (bytes[i] == needle[num]) num++;
                else num = 0;
                if (num >= needle.Length) return i - (needle.Length - 1);
            }
            return -1;
        }
    }
}