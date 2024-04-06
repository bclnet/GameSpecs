using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Data
{
    public static class LocationTable
    {
        public enum TeleportLocationType
        {
            Undef,
            Town,
            Dungeon,
            POI
        }

        public class Row
        {
            public string Name;
            public string Type;
            public string Location;
            public TeleportLocationType LocationType;

            public bool Contains(string str)
                => Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1
                    || Type.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1
                    || Location.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1;
        }

        public readonly static Dictionary<string, Row> Table = new Dictionary<string, Row>();

        public static void Load()
        {
            var data = new StreamReader(typeof(DIDTable).Assembly.GetManifestResourceStream($"GameX.WbB.Data.LocationTable.txt")).ReadToEnd();
            foreach (var line in File.ReadAllLines(data))
            {
                if (line.StartsWith("#")) continue; // comment
                var pieces = line.Split(new string[] { " | " }, StringSplitOptions.None);
                if (pieces.Length < 3) throw new Exception($"Location.Load() - failed to parse {line}");

                var row = new Row
                {
                    Name = pieces[0],
                    Type = pieces[1],
                    Location = pieces[2],
                    LocationType = Enum.TryParse(pieces[1], out TeleportLocationType z) ? z : default,
                };
                Table.Add(row.Name, row);
            }
        }
    }
}
