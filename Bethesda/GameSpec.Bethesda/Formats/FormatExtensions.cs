﻿using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Bethesda.Formats
{
    /// <summary>
    /// FormatExtensions
    /// </summary>
    public static class FormatExtensions
    {
        static Task<object> NiFactory(BinaryReader r, FileSource f, PakFile s) { var file = new NiFile(Path.GetFileNameWithoutExtension(f.Path)); file.Read(r); return Task.FromResult((object)file); }

        // object factory
        internal static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) GetObjectFactoryFactory(this FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                ".dds" => (0, BinaryDds.Factory),
                ".nif" => (0, NiFactory),
                _ => (0, null),
            };
    }
}