using System.Text;

namespace System.Security.Cryptography
{
    public static class Polyfill
    {
        #region HashAlgorithm

        public static int TransformBlock(this HashAlgorithm source, string value)
        {
            var value2 = Encoding.UTF8.GetBytes(value);
            return source.TransformBlock(value2, 0, value2.Length, value2, 0);
        }
        public static int TransformBlock(this HashAlgorithm source, byte[] value)
            => source.TransformBlock(value, 0, value.Length, value, 0);

        public static byte[] ToFinalHash(this HashAlgorithm source)
        {
            source.TransformFinalBlock(new byte[0], 0, 0);
            return source.Hash;
        }

        #endregion
    }
}