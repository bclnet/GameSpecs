using GameSpec.Formats;
using GameSpec.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_ServerContainer : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_ServerContainer(r.ToStream()));

        #region Records

        public class ContainerData
        {
            public int GumpID;
            public Vector4<int> Bounds;
            public int DropSound;
        }

        static ContainerData Default = new ContainerData { GumpID = 0x3C, Bounds = new Vector4<int>(44, 65, 142, 94), DropSound = 0x48 };
        static readonly Dictionary<int, ContainerData> Records = new Dictionary<int, ContainerData>();

        public static ContainerData Get(int itemID)
        {
            Records.TryGetValue(itemID, out var data);
            if (data != null) return data;
            else return Default;
        }

        #endregion

        // file: Data/containers.cfg
        public Binary_ServerContainer(StreamReader r)
        {
            Default = null;
            string line;
            while ((line = r.ReadLine()) != null)
            {
                if (line.Length == 0 || line.StartsWith("#")) continue;
                try
                {
                    var split = line.Split('\t');
                    if (split.Length >= 3)
                    {
                        var rect = split[1].Split(' ');
                        if (rect.Length < 4) continue;
                        var data = new ContainerData
                        {
                            GumpID = ConvertX.ToInt32(split[0]),
                            Bounds = new Vector4<int>(ConvertX.ToInt32(rect[0]), ConvertX.ToInt32(rect[1]), ConvertX.ToInt32(rect[2]), ConvertX.ToInt32(rect[3])),
                            DropSound = ConvertX.ToInt32(split[2]),
                        };
                        Default ??= data;
                        if (split.Length >= 4)
                        {
                            var ids = split[3].Split(',');
                            for (var i = 0; i < ids.Length; i++)
                            {
                                var id = ConvertX.ToInt32(ids[i]);
                                if (Records.ContainsKey(id)) Console.WriteLine(@"Warning: double ItemID entry in Data\containers.cfg");
                                else Records[id] = data;
                            }
                        }
                    }
                }
                catch { }
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Container File" }),
                new MetaInfo("Container", items: new List<MetaInfo> {
                    new MetaInfo($"Default: {Default.GumpID}"),
                    new MetaInfo($"Table: {Records.Count}"),
                })
            };
            return nodes;
        }
    }
}
