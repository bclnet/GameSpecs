using GameX.Algorithms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GameX.Capcom
{
    public static class RE
    {
        static readonly IDictionary<string, ZipArchiveEntry> HashEntries;
        static RE()
        {
            var assembly = typeof(RE).Assembly;
            var s = assembly.GetManifestResourceStream("GameX.Resource.Capcom.RE.zip");
            var pak = new ZipArchive(s, ZipArchiveMode.Read);
            HashEntries = pak.Entries.ToDictionary(x => x.Name, x => x);
        }

        static readonly ConcurrentDictionary<string, IDictionary<ulong, string>> HashLookups = new ConcurrentDictionary<string, IDictionary<ulong, string>>();
        public static IDictionary<ulong, string> GetHashLookup(string path) => HashLookups.GetOrAdd(path, x =>
        {
            var value = new Dictionary<ulong, string>();
            string line;
            using var r = new StreamReader(HashEntries[path].Open());
            while ((line = r.ReadLine()) != null)
            {
                var hashLower = MurmurHash3.Hash(line.ToLowerInvariant());
                var hashUpper = MurmurHash3.Hash(line.ToUpperInvariant());
                var hash = (ulong)hashUpper << 32 | hashLower;
                if (value.TryGetValue(hash, out var collision))
                    Console.WriteLine("[COLLISION]: " + collision + " <-> " + line);
                value.Add(hash, line);
            }
            return value;
        });
    }
}