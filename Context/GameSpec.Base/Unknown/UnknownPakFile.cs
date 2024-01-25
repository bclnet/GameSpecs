﻿using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Unknown
{
    /// <summary>
    /// UnknownPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.PakFile" />
    public class UnknownPakFile : PakFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="name">The name.</param>
        public UnknownPakFile(FamilyGame game, string name) : base(null, game, null, name, null) { }
        public UnknownPakFile(FamilyGame game, IFileSystem fileSystem, FamilyGame.Edition edition, string filePath, object tag = default) : base(fileSystem, game, edition, "Unknown") { }
        //public override void Dispose() { }

        public override int Count => 0;
        public override void Closing() { }
        public override void Opening() { }
        public override bool Contains(object path) => false;
        public override Task<Stream> LoadFileData(object path, FileOption option = default) => throw new NotImplementedException();
        public override Task<T> LoadFileObject<T>(object path, FileOption option = default) => throw new NotImplementedException();
    }
}