using GameSpec.Formats;
using GameSpec.Metadata;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Verdata : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Verdata(r));

        #region Headers

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Patch
        {
            public static (string, int) Struct = ("<5i", sizeof(Patch));
            public int File;
            public int Index;
            public int Lookup;
            public int Length;
            public int Extra;
        }

        #endregion

        public Stream Stream;
        public IDictionary<int, Patch[]> Patches;

        public static Binary_Verdata Empty = new Binary_Verdata
        {
            Stream = Stream.Null,
            Patches = new Dictionary<int, Patch[]>(),
        };

        Binary_Verdata() { }
        public Binary_Verdata(BinaryReader r)
        {
            Stream = r.BaseStream;
            Patches = r.ReadL32SArray<Patch>(Patch.Struct).GroupBy(x => x.File).ToDictionary(x => x.Key, x => x.ToArray());
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Version Data" }),
                new MetaInfo("Verdata", items: new List<MetaInfo> {
                    new MetaInfo($"Stream: {Stream}"),
                    new MetaInfo($"Patches: {Patches.Count}"),
                })
            };
            return nodes;
        }
    }
}
