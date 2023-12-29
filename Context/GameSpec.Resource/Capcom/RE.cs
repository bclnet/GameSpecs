using GameSpec.Algorithms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GameSpec.Capcom
{
    public static class RE
    {
        static RE()
        {
            var assembly = typeof(RE).Assembly;
            var s = assembly.GetManifestResourceStream("GameSpec.Resource.Capcom.RE.zip");
            var pak = new ZipArchive(s, ZipArchiveMode.Read);
            HashEntries = pak.Entries.ToDictionary(x => x.Name, x => x);
        }

        static readonly IDictionary<string, ZipArchiveEntry> HashEntries;

        static readonly ConcurrentDictionary<string, IDictionary<ulong, string>> HashLookups = new ConcurrentDictionary<string, IDictionary<ulong, string>>();
        public static IDictionary<ulong, string> GetHashLookup(string path) => HashLookups.GetOrAdd(path, x =>
        {
            var hashLookup = new Dictionary<ulong, string>();
            string line;
            using var r = new StreamReader(HashEntries[path]?.Open());
            while ((line = r.ReadLine()) != null)
            {
                var hashLower = MurmurHash3.Hash(line.ToLowerInvariant());
                var hashUpper = MurmurHash3.Hash(line.ToUpperInvariant());
                var hash = (ulong)hashUpper << 32 | hashLower;
                if (hashLookup.TryGetValue(hash, out var collision))
                    Console.WriteLine("[COLLISION]: " + collision + " <-> " + line);
                hashLookup.Add(hash, line);
            }
            return hashLookup;
        });
    }
}