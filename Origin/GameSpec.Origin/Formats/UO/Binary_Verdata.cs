using GameSpec.Formats;
using GameSpec.Meta;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Verdata : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Verdata(r, (BinaryPakFile)s));

        public static void Touch(PakFile source)
        {
            if (PakFile != null) return;
            if (source.Contains("verdata.mul")) source.LoadFileObject<Binary_Verdata>("verdata.mul").Wait();
        }

        #region Records

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Patch
        {
            public static (string, int) Struct = ("<5i", sizeof(Patch));
            public int File;
            public int Index;
            public int Offset;
            public int FileSize;
            public int Extra;
        }

        public static Stream ReadData(long offset, int fileSize)
            => PakFile.GetReader().Func(r => new MemoryStream(r.Seek(offset).ReadBytes(fileSize)));

        #endregion

        public static BinaryPakFile PakFile;
        public static IDictionary<int, Patch[]> Patches = new Dictionary<int, Patch[]>();

        public Binary_Verdata(BinaryReader r, BinaryPakFile s)
        {
            PakFile = s;
            Patches = r.ReadL32SArray<Patch>(Patch.Struct).GroupBy(x => x.File).ToDictionary(x => x.Key, x => x.ToArray());
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Version Data" }),
                new MetaInfo("Verdata", items: new List<MetaInfo> {
                    new MetaInfo($"Patches: {Patches.Count}"),
                })
            };
            return nodes;
        }
    }
}
