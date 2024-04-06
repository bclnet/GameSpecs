namespace System.IO
{
    /// <summary>
    /// A <see cref="PartialInputStream"/> is an <see cref="InflaterInputStream"/>
    /// whose data is only a part or subsection of a file.
    /// </summary>
    public class PartialInputStream : Stream
    {
        Stream _baseStream;
        readonly long _start;
        readonly long _length;
        long _readPos;
        readonly long _end;

        /// <summary>
        /// Initialise a new instance of the <see cref="PartialInputStream"/> class.
        /// </summary>
        /// <param name="source">The <see cref="Stream"/> containing the underlying stream to use for IO.</param>
        /// <param name="start">The start of the partial data.</param>
        /// <param name="length">The length of the partial data.</param>
        public PartialInputStream(Stream source, long start, long length)
        {
            _start = start;
            _length = length;
            _baseStream = source;
            _readPos = start;
            _end = start + length;
        }

        /// <summary>
        /// Read a byte from this stream.
        /// </summary>
        /// <returns>Returns the byte read or -1 on end of stream.</returns>
        public override int ReadByte()
        {
            // -1 is the correct value at end of stream.
            if (_readPos >= _end) return -1;
            lock (_baseStream) { _baseStream.Seek(_readPos++, SeekOrigin.Begin); return _baseStream.ReadByte(); }
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="System.ArgumentException">The sum of offset and count is larger than the buffer length. </exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="System.ArgumentNullException">buffer is null. </exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">offset or count is negative. </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_baseStream)
            {
                if (count > _end - _readPos)
                {
                    count = (int)(_end - _readPos);
                    if (count == 0) return 0;
                }
                // Protect against Stream implementations that throw away their buffer on every Seek (for example, Mono FileStream)
                if (_baseStream.Position != _readPos) _baseStream.Seek(_readPos, SeekOrigin.Begin);
                var readCount = _baseStream.Read(buffer, offset, count);
                if (readCount > 0) _readPos += readCount;
                return readCount;
            }
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="System.NotSupportedException">The stream does not support writing. </exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="System.ArgumentNullException">buffer is null. </exception>
        /// <exception cref="System.ArgumentException">The sum of offset and count is greater than the buffer length. </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">offset or count is negative. </exception>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type <see cref="System.IO.SeekOrigin"></see> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            var newPos = _readPos;
            switch (origin)
            {
                case SeekOrigin.Begin: newPos = _start + offset; break;
                case SeekOrigin.Current: newPos = _readPos + offset; break;
                case SeekOrigin.End: newPos = _end + offset; break;
            }

            if (newPos < _start) throw new ArgumentException("Negative position is invalid");
            else if (newPos > _end) throw new IOException("Cannot seek past end");
            _readPos = newPos;
            return _readPos;
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="System.IO.IOException">An I/O error occurs. </exception>
        public override void Flush() { } // Nothing to do.

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <value></value>
        /// <returns>The current position within the stream.</returns>
        /// <exception cref="System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="System.NotSupportedException">The stream does not support seeking. </exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get => _readPos - _start;
            set
            {
                var newPos = _start + value;
                if (newPos < _start) throw new ArgumentException("Negative position is invalid");
                else if (newPos > _end) throw new InvalidOperationException("Cannot seek past end");
                _readPos = newPos;
            }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <value></value>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Length => _length;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value>false</value>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite => false;

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value>true</value>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek => true;

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value>true.</value>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead => true;

        /// <summary>
        /// Gets a value that determines whether the current stream can time out.
        /// </summary>
        /// <value></value>
        /// <returns>A value that determines whether the current stream can time out.</returns>
        public override bool CanTimeout => _baseStream.CanTimeout;
    }
}
