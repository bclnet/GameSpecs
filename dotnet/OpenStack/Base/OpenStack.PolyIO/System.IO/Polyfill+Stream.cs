namespace System.IO
{
    public static partial class Polyfill
    {
        #region Read

        public static byte[] ReadAllBytes(this Stream stream)
        {
            using var s = new MemoryStream();
            var oldPosition = stream.Position;
            stream.Position = 0;
            stream.CopyTo(s);
            stream.Position = oldPosition;
            return s.ToArray();
        }

        public static byte[] ReadBytes(this Stream stream, int count) { var data = new byte[count]; stream.Read(data, 0, count); return data; }

        #endregion

        #region Write

        public static void WriteBytes(this Stream stream, byte[] data) => stream.Write(data, 0, data.Length);
        //: TODO Rename me more of CopyTo
        public static void WriteBytes(this Stream stream, BinaryReader r, int count) { var data = r.ReadBytes(count); stream.Write(data, 0, data.Length); }

        #endregion
    }
}