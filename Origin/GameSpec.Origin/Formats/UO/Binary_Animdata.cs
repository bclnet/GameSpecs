using GameSpec.Formats;
using GameSpec.Meta;
using GameSpec.Origin.Games.UO;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Animdata : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Animdata(r));

        #region Records

        //ref: http://wpdev.sourceforge.net/docs/guide/node167.html:
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Record
        {
            public static (string, int) Struct = ("<64x4x", sizeof(Record));
            public fixed sbyte Frames[64];
            public byte Unknown;
            public byte FrameCount;
            public byte FrameInterval;
            public byte StartInterval;
        }

        readonly static Record[][] AnimData = new Record[0x0800][];

        public static Record GetResource(int itemID)
        {
            itemID &= Database.ItemIDMask;
            return AnimData[itemID >> 3][itemID & 0x07];
        }

        #endregion

        // file: animdata.mul
        public Binary_Animdata(BinaryReader r)
        {
            var length = r.BaseStream.Length / (4 + (8 * (64 + 4)));
            for (var i = 0; i < length; i++)
            {
                r.Skip(4); // chunk header
                AnimData[i] = r.ReadSArray<Record>(Record.Struct, 8);
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Animdata File" }),
                new MetaInfo("Animdata", items: new List<MetaInfo> {
                    //new MetaInfo($"Default: {Default.GumpID}"),
                    //new MetaInfo($"Table: {Table.Count}"),
                })
            };
            return nodes;
        }
    }
}
