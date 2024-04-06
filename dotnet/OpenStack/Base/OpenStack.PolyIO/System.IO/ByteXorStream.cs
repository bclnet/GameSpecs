namespace System.IO
{
    public class ByteXorStream : Stream
    {
        public Stream Stream;
        public byte Byte;

        public ByteXorStream(Stream stream, byte @byte)
        {
            Stream = stream;
            Byte = @byte;
        }

        public override void Flush() => Stream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = Stream.Read(buffer, offset, count);
            for (var i = 0; i < read; i++) buffer[i] ^= Byte;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin) => Stream.Seek(offset, origin);

        public override void SetLength(long value) => Stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (var i = 0; i < count; i++) buffer[offset + i] ^= Byte;
            Stream.Write(buffer, offset, count);
        }

        public override bool CanRead => Stream.CanRead;

        public override bool CanSeek => Stream.CanSeek;

        public override bool CanWrite => Stream.CanWrite;

        public override long Length => Stream.Length;

        public override long Position
        {
            get => Stream.Position;
            set => Stream.Position = value;
        }
    }
}
