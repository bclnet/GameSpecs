using GameSpec.Formats;
using GameSpec.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_BodyTable : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_BodyTable(r.ToStream()));

        #region Records

        public class Record
        {
            public readonly int OldId;
            public readonly int NewId;
            public readonly int NewHue;

            public Record(int oldId, int newId, int newHue)
            {
                OldId = oldId;
                NewId = newId;
                NewHue = newHue;
            }
        }

        static readonly Dictionary<int, Record> Records = new Dictionary<int, Record>();

        //public static void TranslateBodyAndHue(ref int id, ref int hue)
        //{
        //    if (Records.TryGetValue(id, out var bte))
        //    {
        //        id = bte.NewId;
        //        if (hue == 0) hue = bte.NewHue;
        //    }
        //}

        #endregion

        // file: Body.def
        public Binary_BodyTable(StreamReader r)
        {
            while (r.ReadLine() is { } line)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                try
                {
                    var index1 = line.IndexOf("{", StringComparison.Ordinal);
                    var index2 = line.IndexOf("}", StringComparison.Ordinal);
                    
                    var param1 = line[..index1];
                    var param2 = line.Substring(index1 + 1, index2 - index1 - 1);
                    var param3 = line[(index2 + 1)..];

                    var indexOf = param2.IndexOf(',');
                    if (indexOf > -1) param2 = param2[..indexOf].Trim();

                    var oldId = Convert.ToInt32(param1);
                    var newId = Convert.ToInt32(param2);
                    var newHue = Convert.ToInt32(param3);
                    Records[oldId] = new Record(oldId, newId, newHue);
                }
                catch { }
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Body config" }),
                new MetaInfo("Body", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Records.Count}"),
                })
            };
            return nodes;
        }
    }
}
