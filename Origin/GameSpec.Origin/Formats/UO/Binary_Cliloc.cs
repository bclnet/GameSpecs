using GameSpec.Formats;
using GameSpec.Metadata;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Cliloc : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Cliloc(r));

        public Hashtable Table = new Hashtable();

        public Binary_Cliloc(BinaryReader r)
        {
            var length = r.BaseStream.Length;
            r.Skip(6);
            while (r.BaseStream.Position < length)
            {
                var id = r.ReadUInt32();
                var text = r.Skip(1).ReadL16AString();
                Table[id] = text;
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Language File" }),
                new MetaInfo("Cliloc", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Table.Count}"),
                })
            };
            return nodes;
        }
    }
}
