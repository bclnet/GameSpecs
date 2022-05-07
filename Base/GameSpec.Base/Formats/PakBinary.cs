using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    public class PakBinary
    {
        /// <summary>
        /// ReadState
        /// </summary>
        public enum ReadStage { _Set, _Meta, _Raw, File }

        /// <summary>
        /// WriteState
        /// </summary>
        public enum WriteStage { _Set, _Meta, _Raw, Header, File }

        /// <summary>
        /// The file
        /// </summary>
        public readonly static PakBinary Stream = new PakBinaryCanStream();

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="r">The r.</param>
        /// <param name="stage">The stage.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage) => throw new NotSupportedException();

        /// <summary>
        /// Writes the asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="w">The w.</param>
        /// <param name="stage">The stage.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task WriteAsync(BinaryPakFile source, BinaryWriter w, WriteStage stage) => throw new NotSupportedException();

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="r">The r.</param>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null) => throw new NotSupportedException();

        /// <summary>
        /// Writes the asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="w">The w.</param>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <param name="data">The data.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task WriteDataAsync(BinaryPakFile source, BinaryWriter w, FileMetadata file, Stream data, DataOption option = 0, Action<FileMetadata, string> exception = null) => throw new NotSupportedException();

        /// <summary>
        /// Processes this instance.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="NotSupportedException"></exception>
        public virtual void Process(BinaryPakFile source) { }
}
}