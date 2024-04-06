using System.Text;

namespace GameX.Algorithms
{
    public static class MurmurHash2
    {
        const uint M = 0x5bd1e995;
        const int R = 24;

        public static uint Hash(string data, uint seed) => Hash(Encoding.ASCII.GetBytes(data), seed);
        public static uint Hash(byte[] data, uint seed)
        {
            var l = data.Length;
            if (l == 0) return 0;

            var h = seed ^ (uint)l;
            var i = 0;
            while (l >= 4)
            {
                var k = (uint)(data[i++] | data[i++] << 8 | data[i++] << 16 | data[i++] << 24);
                k *= M; k ^= k >> R; k *= M;
                h *= M; h ^= k; l -= 4;
            }

            switch (l)
            {
                case 3: h ^= (ushort)(data[i++] | data[i++] << 8); h ^= (uint)(data[i] << 16); h *= M; break;
                case 2: h ^= (ushort)(data[i++] | data[i] << 8); h *= M; break;
                case 1: h ^= data[i]; h *= M; break;
                default: break;
            }
            h ^= h >> 13;
            h *= M;
            h ^= h >> 15;
            return h;
        }
    }
}
