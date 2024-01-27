using GameSpec.Formats;
using GameSpec.Meta;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_StringTable : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_StringTable(r));

        #region Records

        public static Hashtable Records = new Hashtable();

        #endregion

        // file: Cliloc.enu
        public Binary_StringTable(BinaryReader r)
        {
            var length = r.BaseStream.Length;
            r.Skip(6);
            while (r.BaseStream.Position < length)
            {
                var id = r.ReadUInt32();
                var text = r.Skip(1).ReadL16AString();
                Records[id] = text;
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Cliloc Language File" }),
                new MetaInfo("Cliloc", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Records.Count}"),
                })
            };
            return nodes;
        }
    }
}
