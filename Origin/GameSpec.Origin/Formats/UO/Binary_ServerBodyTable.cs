using GameSpec.Formats;
using GameSpec.Meta;
using GameSpec.Origin.Games.UO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_ServerBodyTable : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_ServerBodyTable(r.ToStream()));

        // file: Data/bodyTable.cfg
        public Binary_ServerBodyTable(StreamReader r)
        {
            Body.Types = new BodyType[0x1000];
            string line;
            while ((line = r.ReadLine()) != null)
            {
                if (line.Length == 0 || line.StartsWith("#")) continue;
                var split = line.Split('\t');
                if (int.TryParse(split[0], out var bodyID) && Enum.TryParse(split[1], true, out BodyType type) && bodyID >= 0 && bodyID < Body.Types.Length) Body.Types[bodyID] = type;
                else
                {
                    Console.WriteLine("Warning: Invalid bodyTable entry:");
                    Console.WriteLine(line);
                }
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Bodytable config" }),
                new MetaInfo("Bodytable", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Body.Types.Length}"),
                })
            };
            return nodes;
        }
    }
}
