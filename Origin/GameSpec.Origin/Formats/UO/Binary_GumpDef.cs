using GameSpec.Formats;
using GameSpec.Meta;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_GumpDef : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_GumpDef(r.ToStream()));

        #region Records

        public static bool ItemHasGumpTranslation(int gumpIndex, out int gumpIndexTranslated, out int defaultHue)
        {
            if (Records.TryGetValue(gumpIndex, out var translation))
            {
                gumpIndexTranslated = translation.Item1;
                defaultHue = translation.Item2;
                return true;
            }
            gumpIndexTranslated = 0;
            defaultHue = 0;
            return false;
        }

        static readonly Dictionary<int, (int, int)> Records = new Dictionary<int, (int, int)>();

        #endregion

        // file: gump.def
        public Binary_GumpDef(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                var defs = line.Replace('\t', ' ').Split(' ');
                if (defs.Length != 3) continue;
                var inGump = int.Parse(defs[0]);
                var outGump = int.Parse(defs[1].Replace("{", string.Empty).Replace("}", string.Empty));
                var outHue = int.Parse(defs[2]);
                Records[inGump] = (outGump, outHue);
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Gump Language File" }),
                new MetaInfo("GumpDef", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Records.Count}"),
                })
            };
            return nodes;
        }
    }
}
