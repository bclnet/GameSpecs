using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Formats
{
    /// <summary>
    /// StreamPakFile
    /// </summary>
    /// <seealso cref="GameX.Formats.BinaryPakFile" />
    public class StreamPakFile : BinaryPakFile
    {
        readonly AbstractHost Host;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamPakFile" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="state">The state.</param>
        /// <param name="address">The host.</param>
        public StreamPakFile(Func<Uri, string, AbstractHost> factory, PakState state, Uri address = null) : base(state, new PakBinaryCanStream())
        {
            UseReader = false;
            if (address != null) Host = factory(address, state.Path);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamPakFile" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="state">The state.</param>
        public StreamPakFile(BinaryPakFile parent, PakState state) : base(state, new PakBinaryCanStream())
        {
            UseReader = false;
            Files = parent.Files;
        }

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="tag">The tag.</param>
        public override async Task Read(BinaryReader _, object tag)
        {
            // http pak
            if (Host != null)
            {
                var files = Files = new List<FileSource>();
                var set = await Host.GetSetAsync() ?? throw new NotSupportedException(".set not found");
                foreach (var item in set) files.Add(new FileSource { Path = item });
                return;
            }

            // read pak
            var path = PakPath;
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            var setPath = System.IO.Path.Combine(path, ".set");
            if (File.Exists(setPath)) using (var r = new BinaryReader(File.Open(setPath, FileMode.Open, FileAccess.Read, FileShare.Read))) await PakBinary.Stream.Read(this, r, "Set");
            var metaPath = System.IO.Path.Combine(path, ".meta");
            if (File.Exists(metaPath)) using (var r = new BinaryReader(File.Open(setPath, FileMode.Open, FileAccess.Read, FileShare.Read))) await PakBinary.Stream.Read(this, r, "Meta");
            var rawPath = System.IO.Path.Combine(path, ".raw");
            if (File.Exists(rawPath)) using (var r = new BinaryReader(File.Open(rawPath, FileMode.Open, FileAccess.Read, FileShare.Read))) await PakBinary.Stream.Read(this, r, "Raw");
        }

        /// <summary>
        /// Writes the asynchronous.
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="tag">The tag.</param>
        /// <exception cref="NotSupportedException"></exception>
        public override async Task Write(BinaryWriter _, object tag)
        {
            // http pak
            if (Host != null) throw new NotSupportedException();

            // write pak
            var path = PakPath;
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path)) Directory.CreateDirectory(path);
            var setPath = System.IO.Path.Combine(path, ".set");
            using (var w = new BinaryWriter(new FileStream(setPath, FileMode.Create, FileAccess.Write))) await PakBinary.Stream.Write(this, w, "Set");
            var metaPath = System.IO.Path.Combine(path, ".meta");
            using (var w = new BinaryWriter(new FileStream(metaPath, FileMode.Create, FileAccess.Write))) await PakBinary.Stream.Write(this, w, "Meta");
            var rawPath = System.IO.Path.Combine(path, ".raw");
            if (FilesRawSet != null && FilesRawSet.Count > 0) using (var w = new BinaryWriter(new FileStream(rawPath, FileMode.Create, FileAccess.Write))) await PakBinary.Stream.Write(this, w, "Raw");
            else if (File.Exists(rawPath)) File.Delete(rawPath);
        }

        /// <summary>
        /// Reads the file data asynchronous.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public override async Task<Stream> ReadData(BinaryReader r, FileSource file, FileOption option = default)
        {
            var path = file.Path;
            // http pak
            if (Host != null) return await Host.GetFileAsync(path);

            // read pak
            path = System.IO.Path.Combine(PakPath, path);
            return File.Exists(path) ? File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
        }

        /// <summary>
        /// Writes the file data asynchronous.
        /// </summary>
        /// <param name="w">The w.</param>
        /// <param name="file">The file.</param>
        /// <param name="data">The data.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public override Task WriteData(BinaryWriter w, FileSource file, Stream data, FileOption option = default) => throw new NotSupportedException();
    }
}