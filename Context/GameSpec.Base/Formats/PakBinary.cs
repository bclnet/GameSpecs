using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.Formats
{
    public class PakBinary
    {
        /// <summary>
        /// The file
        /// </summary>
        public readonly static PakBinary Stream = new PakBinaryCanStream();

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="r">The r.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag = default) => throw new NotSupportedException();

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="r">The r.</param>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default) => throw new NotSupportedException();

        /// <summary>
        /// Writes the asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="w">The w.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task WriteAsync(BinaryPakFile source, BinaryWriter w, object tag = default) => throw new NotSupportedException();

        /// <summary>
        /// Writes the asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="w">The w.</param>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task WriteDataAsync(BinaryPakFile source, BinaryWriter w, FileSource file, Stream data, FileOption option = default) => throw new NotSupportedException();

        /// <summary>
        /// Processes this instance.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="NotSupportedException"></exception>
        public virtual void Process(BinaryPakFile source) { }

        /// <summary>
        /// handles an exception.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="option">The option.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="NotSupportedException"></exception>
        public static void HandleException(object source, FileOption option, string message)
        {
            Log(message);
            if ((option & FileOption.Supress) != 0) throw new Exception(message);
        }
    }
}