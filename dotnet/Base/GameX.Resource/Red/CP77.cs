using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GameX.Red
{
    public static class CP77
    {
        class HashRecord
        {
            public string String { get; set; }
            public string Hash { get; set; }
        }

        static CP77()
        {
            var assembly = typeof(CP77).Assembly;
            using var s = assembly.GetManifestResourceStream("GameX.Resource.Red.CP77.zip");
            var pak = new ZipArchive(s, ZipArchiveMode.Read);
            using var r = new StreamReader(pak.GetEntry("CP77/hashes.csv")?.Open());
            using var csv = new CsvReader(r, CultureInfo.InvariantCulture);
            HashLookup = csv.GetRecords<HashRecord>().ToDictionary(x => ulong.Parse(x.Hash), x => x.String);
        }

        public static readonly IDictionary<ulong, string> HashLookup;
    }
}