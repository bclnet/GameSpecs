using GameSpec.Formats;
using GameSpec.Meta;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_MusicDef : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_MusicDef(r.ToStream()));

        #region Records

        public static bool TryGetMusicData(int index, out string name, out bool doesLoop)
        {
            if (Records.ContainsKey(index))
            {
                name = Records[index].Item1;
                doesLoop = Records[index].Item2;
                return true;
            }
            name = null;
            doesLoop = false;
            return false;
        }

        static readonly Dictionary<int, (string, bool)> Records = new Dictionary<int, (string, bool)>();

        #endregion

        // file: Music/Digital/Config.txt
        public Binary_MusicDef(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                var splits = line.Split(new[] { ' ', ',', '\t' });
                if (splits.Length < 2 || splits.Length > 3) continue;
                var index = int.Parse(splits[0]);
                var name = splits[1].Trim();
                var doesLoop = splits.Length == 3 && splits[2] == "loop";
                Records.Add(index, (name, doesLoop));
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Music config" }),
                new MetaInfo("MusicDef", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Records.Count}"),
                })
            };
            return nodes;
        }
    }
}
