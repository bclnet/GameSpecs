using GameSpec.Formats;
using GameSpec.Meta;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_MobType : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_MobType(r.ToStream()));

        #region Records

        public enum MobType
        {
            Null = -1,
            Monster = 0,
            Animal = 1,
            Humanoid = 2
        }

        struct Record
        {
            public string Flags;
            public MobType AnimationType;

            public Record(string type, string flags)
            {
                Flags = flags;
                AnimationType = type switch
                {
                    "MONSTER" => MobType.Monster,
                    "ANIMAL" => MobType.Animal,
                    "SEA_MONSTER" => MobType.Monster,
                    "HUMAN" => MobType.Humanoid,
                    "EQUIPMENT" => MobType.Humanoid,
                    _ => MobType.Null,
                };
            }
        }

        public static MobType AnimationTypeXXX(int bodyID) => Records[bodyID].AnimationType;

        static readonly Dictionary<int, Record> Records = new Dictionary<int, Record>();

        #endregion

        // file: mobtypes.txt
        public Binary_MobType(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                var data = line.Replace("   ", "\t").Split('\t');
                var bodyID = int.Parse(data[0]);
                Records[bodyID] = new Record(data[1].Trim(), data[2].Trim());
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "MobType File" }),
                new MetaInfo("MobType", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Records.Count}"),
                })
            };
            return nodes;
        }
    }
}
