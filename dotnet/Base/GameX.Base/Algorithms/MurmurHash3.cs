using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameX.Algorithms
{
    public static class MurmurHash3
    {
        const uint C1 = 0xcc9e2d51;
        const uint C2 = 0x1b873593;
        const uint Seed = 0xffffffff;

        public static uint Hash(string data) => Hash(Encoding.Unicode.GetBytes(data));
        public static uint Hash(byte[] data)
        {
            uint h = Seed, k, l = 0U;
            using var r = new BinaryReader(new MemoryStream(data));
            var chunk = r.ReadBytes(4);
            while (chunk.Length > 0)
            {
                l += (uint)chunk.Length;
                switch (chunk.Length)
                {
                    case 4:
                        k = (uint)(chunk[0] | chunk[1] << 8 | chunk[2] << 16 | chunk[3] << 24); k *= C1; k = Rotl32(k, 15); k *= C2; h ^= k;
                        h = Rotl32(h, 13); h = h * 5 + 0xe6546b64; break;
                    case 3: k = (uint)(chunk[0] | chunk[1] << 8 | chunk[2] << 16); k *= C1; k = Rotl32(k, 15); k *= C2; h ^= k; break;
                    case 2: k = (uint)(chunk[0] | chunk[1] << 8); k *= C1; k = Rotl32(k, 15); k *= C2; h ^= k; break;
                    case 1: k = (uint)(chunk[0]); k *= C1; k = Rotl32(k, 15); k *= C2; h ^= k; break;
                }
                chunk = r.ReadBytes(4);
            }
            h ^= l;
            h = Fmix(h);
            unchecked { return h; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static uint Rotl32(uint x, byte r) => (x << r) | (x >> (32 - r));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Fmix(uint h)
        {
            h ^= h >> 16; h *= 0x85ebca6b;
            h ^= h >> 13; h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }
    }
}
