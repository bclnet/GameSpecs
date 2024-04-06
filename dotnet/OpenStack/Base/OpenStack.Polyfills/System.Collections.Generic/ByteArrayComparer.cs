using System.Linq;

namespace System.Collections.Generic
{
    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public static ByteArrayComparer Default = new ByteArrayComparer();
        public bool Equals(byte[] left, byte[] right) => left == null || right == null ? left == right : left.SequenceEqual(right);
        public int GetHashCode(byte[] key) => (key ?? throw new ArgumentNullException(nameof(key))).Sum(b => b);
    }
}