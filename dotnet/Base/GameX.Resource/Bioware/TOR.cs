using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ZstdNet;

namespace GameX.Bioware
{
    public static class TOR
    {
        class HashRecord
        {
            public string Hash1 { get; set; }
            public string Hash2 { get; set; }
            public string Path { get; set; }
        }

        class HashRecordMap : ClassMap<HashRecord>
        {
            public HashRecordMap()
            {
                Map(m => m.Hash1).Index(0);
                Map(m => m.Hash2).Index(1);
                Map(m => m.Path).Index(2);
            }
        }

        static TOR()
        {
            var assembly = typeof(TOR).Assembly;
            using var s = assembly.GetManifestResourceStream("GameX.Bioware.Resource.TOR.zst");
            using var r = new StreamReader(new DecompressionStream(s));
            using var csv = new CsvReader(r, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false, Delimiter = "#" });
            csv.Context.RegisterClassMap<HashRecordMap>();
            HashLookup = csv.GetRecords<HashRecord>().ToDictionary(x => ulong.Parse(x.Hash1 + x.Hash2, NumberStyles.HexNumber), x => x.Path);
        }

        public static readonly IDictionary<ulong, string> HashLookup;
    }
}