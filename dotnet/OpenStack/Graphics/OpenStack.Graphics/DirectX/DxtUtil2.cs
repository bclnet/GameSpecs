namespace OpenStack.Graphics.DirectX
{
    public unsafe static class DxtUtil2
    {
        public static void ConvertDxt3ToDtx5(byte[] data, int width, int height, int mipMaps)
        {
            fixed (byte* data_ = data)
            {
                var p = data_;
                int blockCountX = (width + 3) / 4, blockCountY = (height + 3) / 4;
                for (var y = 0; y < blockCountY; y++)
                    for (var x = 0; x < blockCountX; x++)
                        ConvertDxt3BlockToDtx5Block(p += 16);
            }
        }

        static void ConvertDxt3BlockToDtx5Block(byte* p)
        {
            byte a0 = p[0], a1 = p[1], a2 = p[1], a3 = p[1], a4 = p[1], a5 = p[1], a6 = p[1], a7 = p[1];
        }
    }
}
