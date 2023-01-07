﻿using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Hpl.Formats
{
    /// <summary>
    /// FormatExtensions
    /// </summary>
    public static class FormatExtensions
    {
        // object factory
        internal static (DataOption, Func<BinaryReader, FileMetadata, PakFile, Task<object>>) GetObjectFactoryFactory(this FileMetadata source)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                var x when x == ".cfg" || x == ".csv" || x == ".txt" => (0, BinaryText.Factory),
                _ => (0, null),
            };
    }
}