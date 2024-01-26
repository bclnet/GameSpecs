using GameSpec.Formats;
using GameSpec.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Body : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Body(r.ToStream()));

        #region Records

        public class Record
        {
            public int OrigBody;
            public int NewBody;
            public int NewHue;
        }

        static readonly Dictionary<int, Record> Records = new Dictionary<int, Record>();

        public static void TranslateBodyAndHue(ref int body, ref int hue)
        {
            if (Records.TryGetValue(body, out var bte))
            {
                body = bte.NewBody;
                if (hue == 0) hue = bte.NewHue;
            }
        }

        #endregion

        // file: Body.def
        public Binary_Body(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                if ((line = line.Trim()).Length == 0 || line.StartsWith("#")) continue;
                try
                {
                    int index1 = line.IndexOf("{"), index2 = line.IndexOf("}");
                    var origBody = Convert.ToInt32(line[..index1]);
                    var newBody1 = line.Substring(index1 + 1, index2 - index1 - 1);
                    var newHue = Convert.ToInt32(line[(index2 + 1)..]);
                    var indexOf = newBody1.IndexOf(',');
                    if (indexOf > -1) newBody1 = newBody1[..indexOf].Trim();
                    var newBody = Convert.ToInt32(newBody1);
                    var id = Convert.ToInt32(origBody);
                    Records[origBody] = new Record
                    {
                        OrigBody = origBody,
                        NewBody = newBody,
                        NewHue = newHue,
                    };
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
