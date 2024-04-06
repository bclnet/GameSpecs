using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Data
{
    public static class DIDTable
    {
        public class Row : IEquatable<Row>
        {
            public uint SetupId;
            public uint MotionTableId;
            public uint SoundTableId;
            public uint CombatTableId;

            public bool Equals(Row table) => SetupId.Equals(table.SetupId);
            public override int GetHashCode() => SetupId.GetHashCode();
        }

        public readonly static Dictionary<uint, Row> Table = new Dictionary<uint, Row>();

        public static void Load()
        {
            var data = new StreamReader(typeof(DIDTable).Assembly.GetManifestResourceStream($"GameX.WbB.Data.DIDTable.txt")).ReadToEnd();
            foreach (var line in File.ReadAllLines(data))
            {
                if (line.StartsWith("#")) continue; // comment
                var pieces = line.Split(',');
                if (pieces.Length != 4) throw new Exception($"DIDTables.Load() - failed to parse {line}");
                var row = new Row
                {
                    SetupId = pieces[0].Length > 0 ? Convert.ToUInt32(pieces[0], 16) : 0,
                    MotionTableId = pieces[1].Length > 0 ? Convert.ToUInt32(pieces[1], 16) : 0,
                    SoundTableId = pieces[2].Length > 0 ? Convert.ToUInt32(pieces[2], 16) : 0,
                    CombatTableId = pieces[3].Length > 0 ? Convert.ToUInt32(pieces[3], 16) : 0
                };
                Table.Add(row.SetupId, row);
            }
        }
    }
}
