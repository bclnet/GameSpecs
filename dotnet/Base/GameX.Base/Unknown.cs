using System.IO;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using GameX.Formats;

namespace GameX.Unknown
{
    /// <summary>
    /// UnknownFamily
    /// </summary>
    /// <seealso cref="GameX.Family" />
    public class UnknownFamily : Family
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownFamily"/> class.
        /// </summary>
        internal UnknownFamily() : base() { }
        public UnknownFamily(JsonElement elem) : base(elem) { }
    }

    /// <summary>
    /// UnknownPakFile
    /// </summary>
    /// <seealso cref="GameX.Formats.PakFile" />
    public class UnknownPakFile : PakFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownPakFile" /> class.
        /// </summary>
        /// <param name="state">The game.</param>
        public UnknownPakFile(PakState state) : base(state) => Name = "Unknown";
        //public override void Dispose() { }

        public override int Count => 0;
        public override void Closing() { }
        public override void Opening() { }
        public override bool Contains(object path) => false;
        public override (PakFile, FileSource) GetFileSource(object path, bool throwOnError = true) => throw new NotImplementedException();
        public override Task<Stream> LoadFileData(object path, FileOption option = default, bool throwOnError = true) => throw new NotImplementedException();
        public override Task<T> LoadFileObject<T>(object path, FileOption option = default, bool throwOnError = true) => throw new NotImplementedException();
    }
}