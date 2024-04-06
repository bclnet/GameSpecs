using GameX.Formats;
using GameX.Meta;
using GameX.Origin.Structs.UO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace GameX.Origin.Formats.UO
{
    #region ServerBinary_BodyTable

    public unsafe class ServerBinary_BodyTable : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new ServerBinary_BodyTable(r.ToStream()));

        // file: Data/bodyTable.cfg
        public ServerBinary_BodyTable(StreamReader r)
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

    #endregion

    #region ServerBinary_Container

    public unsafe class ServerBinary_Container : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new ServerBinary_Container(r.ToStream()));

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
        public ServerBinary_Container(StreamReader r)
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

    #endregion
}
