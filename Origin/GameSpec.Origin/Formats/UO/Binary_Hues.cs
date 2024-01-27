using GameSpec.Formats;
using GameSpec.Meta;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Hues : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Hues(r));

        #region Records

        public class Record
        {
            public int Index;
            public ushort[] Colors;
            public string Name;
            public ushort TableStart;
            public ushort TableEnd;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HueData
        {
            public static (string, int) Struct = ("<?", sizeof(HueData));
            public fixed ushort Colors[32];
            public ushort TableStart;
            public ushort TableEnd;
            public fixed byte Name[20];
        }

        static readonly Record[] Records = new Record[3000];

        #endregion

        // file: hues.mul
        public Binary_Hues(BinaryReader r)
        {
            var blockCount = (int)r.BaseStream.Length / 708;
            if (blockCount > 375) blockCount = 375;

            for (var i = 0; i < blockCount; ++i)
            {
                r.Skip(4);
                r.ReadSArray<HueData>(HueData.Struct, 8);
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Hue File" }),
                new MetaInfo("Hue", items: new List<MetaInfo> {
                    //new MetaInfo($"Hues: {Hues.Length}"),
                    //new MetaInfo($"Data: {Pixels.Length}"),
                })
            };
            return nodes;
        }
    }
}
